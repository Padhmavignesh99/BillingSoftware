import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'package:fl_chart/fl_chart.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../providers/report_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/loading_widget.dart';
import '../../widgets/common/stat_card.dart';

class ReportsScreen extends StatefulWidget {
  const ReportsScreen({super.key});

  @override
  State<ReportsScreen> createState() => _ReportsScreenState();
}

class _ReportsScreenState extends State<ReportsScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<ReportProvider>().fetchReports();
    });
  }

  @override
  Widget build(BuildContext context) {
    final reportProvider = context.watch<ReportProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final summary = reportProvider.salesSummary;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Sales & Business Reports'),
        actions: [
          IconButton(
            icon: const Icon(Icons.file_download_outlined),
            tooltip: 'Export CSV',
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Downloading sales report spreadsheet...')),
              );
            },
          ),
        ],
      ),
      body: reportProvider.isLoading
          ? const LoadingWidget(message: 'Generating sales reports...')
          : RefreshIndicator(
              onRefresh: () => reportProvider.fetchReports(),
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Period Selector
                    SingleChildScrollView(
                      scrollDirection: Axis.horizontal,
                      child: Row(
                        children: ['Today', 'This Week', 'This Month', 'This Year'].map((p) {
                          final isSel = reportProvider.selectedPeriod == p;
                          return Padding(
                            padding: const EdgeInsets.only(right: 8),
                            child: ChoiceChip(
                              label: Text(p),
                              selected: isSel,
                              selectedColor: AppTheme.primaryColor.withOpacity(0.15),
                              labelStyle: TextStyle(
                                fontWeight: isSel ? FontWeight.bold : FontWeight.normal,
                                color: isSel ? AppTheme.primaryColor : AppTheme.textDark,
                              ),
                              onSelected: (_) => reportProvider.setSelectedPeriod(p),
                            ),
                          );
                        }).toList(),
                      ),
                    ),
                    const SizedBox(height: 16),

                    // Quick Jump to Sub Reports (GST & P&L)
                    Row(
                      children: [
                        Expanded(
                          child: InkWell(
                            onTap: () => context.push('/gst-reports'),
                            borderRadius: BorderRadius.circular(12),
                            child: Container(
                              padding: const EdgeInsets.all(14),
                              decoration: BoxDecoration(
                                color: Colors.blue.shade50,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: Colors.blue.shade200),
                              ),
                              child: Row(
                                children: [
                                  const Icon(Icons.request_quote_outlined, color: AppTheme.primaryColor),
                                  const SizedBox(width: 10),
                                  const Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text('GST Reports', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13)),
                                        Text('Tax filing summary', style: TextStyle(fontSize: 11, color: AppTheme.textMuted)),
                                      ],
                                    ),
                                  ),
                                  const Icon(Icons.arrow_forward_ios, size: 14, color: AppTheme.primaryColor),
                                ],
                              ),
                            ),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: InkWell(
                            onTap: () => context.push('/profit-loss'),
                            borderRadius: BorderRadius.circular(12),
                            child: Container(
                              padding: const EdgeInsets.all(14),
                              decoration: BoxDecoration(
                                color: Colors.green.shade50,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: Colors.green.shade200),
                              ),
                              child: Row(
                                children: [
                                  const Icon(Icons.trending_up, color: AppTheme.successColor),
                                  const SizedBox(width: 10),
                                  const Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text('Profit & Loss', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13)),
                                        Text('P&L balance sheet', style: TextStyle(fontSize: 11, color: AppTheme.textMuted)),
                                      ],
                                    ),
                                  ),
                                  const Icon(Icons.arrow_forward_ios, size: 14, color: AppTheme.successColor),
                                ],
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 20),

                    // Metrics Grid
                    GridView.count(
                      crossAxisCount: 2,
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      crossAxisSpacing: 12,
                      mainAxisSpacing: 12,
                      childAspectRatio: 1.4,
                      children: [
                        StatCard(
                          title: 'Total Sales',
                          value: AppFormatters.formatCurrency(summary.totalSales, symbol: sym),
                          icon: Icons.payments_outlined,
                          color: AppTheme.primaryColor,
                        ),
                        StatCard(
                          title: 'Total Tax Collected',
                          value: AppFormatters.formatCurrency(summary.totalTax, symbol: sym),
                          icon: Icons.receipt_long,
                          color: AppTheme.secondaryColor,
                        ),
                        StatCard(
                          title: 'Total Invoices',
                          value: '${summary.totalInvoices}',
                          icon: Icons.shopping_bag_outlined,
                          color: AppTheme.infoColor,
                        ),
                        StatCard(
                          title: 'Avg Order Value',
                          value: AppFormatters.formatCurrency(summary.averageOrderValue, symbol: sym),
                          icon: Icons.analytics_outlined,
                          color: AppTheme.successColor,
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),

                    // Day-wise Sales Chart
                    Container(
                      padding: const EdgeInsets.all(18),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(16),
                        border: Border.all(color: AppTheme.borderLight),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'Day-Wise Sales Trend',
                            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                          ),
                          const SizedBox(height: 4),
                          const Text(
                            'Daily revenue fluctuations',
                            style: TextStyle(fontSize: 12, color: AppTheme.textMuted),
                          ),
                          const SizedBox(height: 20),
                          SizedBox(
                            height: 180,
                            child: summary.dayWiseSales.isEmpty
                                ? const Center(child: Text('No daily data available'))
                                : LineChart(
                                    LineChartData(
                                      gridData: const FlGridData(show: false),
                                      titlesData: FlTitlesData(
                                        show: true,
                                        topTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                                        rightTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                                        leftTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                                        bottomTitles: AxisTitles(
                                          sideTitles: SideTitles(
                                            showTitles: true,
                                            getTitlesWidget: (val, meta) {
                                              final idx = val.toInt();
                                              if (idx >= 0 && idx < summary.dayWiseSales.length) {
                                                final date = summary.dayWiseSales[idx].date;
                                                return Padding(
                                                  padding: const EdgeInsets.only(top: 8),
                                                  child: Text(
                                                    '${date.day}/${date.month}',
                                                    style: const TextStyle(fontSize: 10, color: AppTheme.textMuted),
                                                  ),
                                                );
                                              }
                                              return const Text('');
                                            },
                                          ),
                                        ),
                                      ),
                                      borderData: FlBorderData(show: false),
                                      lineBarsData: [
                                        LineChartBarData(
                                          spots: List.generate(
                                            summary.dayWiseSales.length,
                                            (i) => FlSpot(i.toDouble(), summary.dayWiseSales[i].amount),
                                          ),
                                          isCurved: true,
                                          color: AppTheme.primaryColor,
                                          barWidth: 3,
                                          dotData: const FlDotData(show: true),
                                          belowBarData: BarAreaData(
                                            show: true,
                                            color: AppTheme.primaryColor.withOpacity(0.12),
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),
                  ],
                ),
              ),
            ),
    );
  }
}

