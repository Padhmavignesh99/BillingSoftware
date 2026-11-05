import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'package:fl_chart/fl_chart.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../providers/dashboard_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/loading_widget.dart';
import '../../widgets/common/stat_card.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<DashboardProvider>().loadDashboardData();
    });
  }

  @override
  Widget build(BuildContext context) {
    final dashboard = context.watch<DashboardProvider>();
    final settings = context.watch<SettingsProvider>();
    final summary = dashboard.summary;
    final charts = dashboard.charts;
    final sym = settings.currencySymbol;

    if (dashboard.isLoading) {
      return const LoadingWidget(message: 'Loading business dashboard...');
    }

    return Scaffold(
      body: RefreshIndicator(
        onRefresh: () => dashboard.loadDashboardData(),
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Welcome Banner & Quick Action Bar
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  gradient: const LinearGradient(
                    colors: [AppTheme.primaryColor, Color(0xFF2563EB)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(16),
                  boxShadow: [
                    BoxShadow(
                      color: AppTheme.primaryColor.withOpacity(0.3),
                      blurRadius: 12,
                      offset: const Offset(0, 4),
                    ),
                  ],
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              "Today's Overview",
                              style: TextStyle(
                                color: Colors.white70,
                                fontSize: 13,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              AppFormatters.formatCurrency(summary.todaySales, symbol: sym),
                              style: const TextStyle(
                                color: Colors.white,
                                fontSize: 28,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ],
                        ),
                        ElevatedButton.icon(
                          onPressed: () => context.go('/billing'),
                          icon: const Icon(Icons.add, size: 18),
                          label: const Text('New Bill'),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.white,
                            foregroundColor: AppTheme.primaryColor,
                            elevation: 0,
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(10),
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    Row(
                      children: [
                        _miniPill(
                          icon: Icons.receipt_outlined,
                          text: '${summary.todayInvoiceCount} Invoices',
                        ),
                        const SizedBox(width: 12),
                        _miniPill(
                          icon: Icons.money_off_outlined,
                          text: '${AppFormatters.formatCurrency(summary.todayExpenses, symbol: sym)} Expenses',
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 20),

              // KPI Cards Grid
              LayoutBuilder(
                builder: (context, constraints) {
                  final isWide = constraints.maxWidth > 600;
                  return GridView.count(
                    crossAxisCount: isWide ? 4 : 2,
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    crossAxisSpacing: 12,
                    mainAxisSpacing: 12,
                    childAspectRatio: isWide ? 1.5 : 1.35,
                    children: [
                      StatCard(
                        title: 'Total Revenue',
                        value: AppFormatters.formatCurrency(summary.totalRevenue, symbol: sym),
                        subtitle: '+12% this month',
                        icon: Icons.trending_up,
                        color: AppTheme.successColor,
                        onTap: () => context.go('/reports'),
                      ),
                      StatCard(
                        title: 'Pending Due',
                        value: AppFormatters.formatCurrency(summary.totalDueAmount, symbol: sym),
                        subtitle: 'Credit collections',
                        icon: Icons.pending_actions,
                        color: AppTheme.warningColor,
                        onTap: () => context.push('/credit-bills'),
                      ),
                      StatCard(
                        title: 'Low Stock Alert',
                        value: '${summary.lowStockProductCount} Items',
                        subtitle: 'Requires reorder',
                        icon: Icons.warning_amber_rounded,
                        color: AppTheme.errorColor,
                        onTap: () => context.go('/inventory'),
                      ),
                      StatCard(
                        title: 'Total Customers',
                        value: '${summary.totalCustomers}',
                        subtitle: 'Active directory',
                        icon: Icons.people_outline,
                        color: AppTheme.secondaryColor,
                        onTap: () => context.push('/customers'),
                      ),
                    ],
                  );
                },
              ),
              const SizedBox(height: 24),

              // Sales Performance Chart Card
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: AppTheme.borderLight),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Sales Performance',
                              style: TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.bold,
                                color: AppTheme.textDark,
                              ),
                            ),
                            SizedBox(height: 2),
                            Text(
                              'Last 7 days revenue vs expenses',
                              style: TextStyle(fontSize: 12, color: AppTheme.textMuted),
                            ),
                          ],
                        ),
                        TextButton(
                          onPressed: () => context.go('/reports'),
                          child: const Text('Full Report'),
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),
                    SizedBox(
                      height: 200,
                      child: charts.salesChart.isEmpty
                          ? const Center(child: Text('No chart data available'))
                          : BarChart(
                              BarChartData(
                                alignment: BarChartAlignment.spaceAround,
                                maxY: 50000,
                                barTouchData: BarTouchData(
                                  enabled: true,
                                  touchTooltipData: BarTouchTooltipData(
                                    getTooltipColor: (_) => AppTheme.textDark,
                                    getTooltipItem: (group, groupIndex, rod, rodIndex) {
                                      final day = charts.salesChart[groupIndex].label;
                                      return BarTooltipItem(
                                        '$day\n₹${rod.toY.toInt()}',
                                        const TextStyle(color: Colors.white, fontSize: 12),
                                      );
                                    },
                                  ),
                                ),
                                titlesData: FlTitlesData(
                                  show: true,
                                  topTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                                  rightTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                                  bottomTitles: AxisTitles(
                                    sideTitles: SideTitles(
                                      showTitles: true,
                                      getTitlesWidget: (value, meta) {
                                        final index = value.toInt();
                                        if (index >= 0 && index < charts.salesChart.length) {
                                          return Padding(
                                            padding: const EdgeInsets.only(top: 8),
                                            child: Text(
                                              charts.salesChart[index].label,
                                              style: const TextStyle(
                                                color: AppTheme.textMuted,
                                                fontSize: 11,
                                                fontWeight: FontWeight.w600,
                                              ),
                                            ),
                                          );
                                        }
                                        return const Text('');
                                      },
                                    ),
                                  ),
                                  leftTitles: AxisTitles(
                                    sideTitles: SideTitles(
                                      showTitles: true,
                                      reservedSize: 42,
                                      getTitlesWidget: (value, meta) {
                                        if (value == 0) return const Text('');
                                        return Text(
                                          '${(value / 1000).toInt()}k',
                                          style: const TextStyle(
                                            color: AppTheme.textMuted,
                                            fontSize: 10,
                                          ),
                                        );
                                      },
                                    ),
                                  ),
                                ),
                                gridData: FlGridData(
                                  show: true,
                                  drawVerticalLine: false,
                                  horizontalInterval: 10000,
                                  getDrawingHorizontalLine: (value) => FlLine(
                                    color: Colors.grey.shade200,
                                    strokeWidth: 1,
                                  ),
                                ),
                                borderData: FlBorderData(show: false),
                                barGroups: List.generate(charts.salesChart.length, (i) {
                                  final sVal = charts.salesChart[i].value;
                                  final eVal = i < charts.expenseChart.length
                                      ? charts.expenseChart[i].value
                                      : 0.0;
                                  return BarChartGroupData(
                                    x: i,
                                    barRods: [
                                      BarChartRodData(
                                        toY: sVal,
                                        color: AppTheme.primaryColor,
                                        width: 14,
                                        borderRadius: const BorderRadius.vertical(top: Radius.circular(4)),
                                      ),
                                      BarChartRodData(
                                        toY: eVal,
                                        color: AppTheme.errorColor.withOpacity(0.7),
                                        width: 14,
                                        borderRadius: const BorderRadius.vertical(top: Radius.circular(4)),
                                      ),
                                    ],
                                  );
                                }),
                              ),
                            ),
                    ),
                    const SizedBox(height: 16),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        _legendItem('Sales Revenue', AppTheme.primaryColor),
                        const SizedBox(width: 24),
                        _legendItem('Expenses', AppTheme.errorColor.withOpacity(0.7)),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),

              // Quick Actions Row
              const Text(
                'Quick Operations',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color: AppTheme.textDark,
                ),
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  _actionChip(
                    context,
                    icon: Icons.receipt_long,
                    label: 'Invoices',
                    route: '/invoices',
                    color: Colors.blue.shade700,
                  ),
                  const SizedBox(width: 10),
                  _actionChip(
                    context,
                    icon: Icons.inventory_2,
                    label: 'Products',
                    route: '/products',
                    color: Colors.indigo.shade700,
                  ),
                  const SizedBox(width: 10),
                  _actionChip(
                    context,
                    icon: Icons.account_balance_wallet,
                    label: 'Expenses',
                    route: '/expenses',
                    color: Colors.purple.shade700,
                  ),
                  const SizedBox(width: 10),
                  _actionChip(
                    context,
                    icon: Icons.people,
                    label: 'Customers',
                    route: '/customers',
                    color: Colors.teal.shade700,
                  ),
                ],
              ),
              const SizedBox(height: 24),
            ],
          ),
        ),
      ),
    );
  }

  Widget _miniPill({required IconData icon, required String text}) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: Colors.white.withOpacity(0.2),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: Colors.white),
          const SizedBox(width: 6),
          Text(
            text,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 12,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  Widget _legendItem(String label, Color color) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 12,
          height: 12,
          decoration: BoxDecoration(
            color: color,
            borderRadius: BorderRadius.circular(3),
          ),
        ),
        const SizedBox(width: 6),
        Text(
          label,
          style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
        ),
      ],
    );
  }

  Widget _actionChip(
    BuildContext context, {
    required IconData icon,
    required String label,
    required String route,
    required Color color,
  }) {
    return Expanded(
      child: InkWell(
        onTap: () => context.push(route),
        borderRadius: BorderRadius.circular(12),
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 14),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: AppTheme.borderLight),
          ),
          child: Column(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: color.withOpacity(0.1),
                  shape: BoxShape.circle,
                ),
                child: Icon(icon, color: color, size: 20),
              ),
              const SizedBox(height: 8),
              Text(
                label,
                style: const TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                  color: AppTheme.textDark,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

