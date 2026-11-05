import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../providers/report_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/loading_widget.dart';
import '../../widgets/common/stat_card.dart';

class GSTReportsScreen extends StatelessWidget {
  const GSTReportsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final reportProvider = context.watch<ReportProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final gst = reportProvider.taxReport;

    return Scaffold(
      appBar: AppBar(
        title: const Text('GST & Tax Filing Reports'),
        actions: [
          IconButton(
            icon: const Icon(Icons.download),
            tooltip: 'Export GSTR-1 & 3B',
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Exporting GST filing data (GSTR-1 JSON & Excel)...')),
              );
            },
          ),
        ],
      ),
      body: reportProvider.isLoading
          ? const LoadingWidget(message: 'Calculating GST returns...')
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Net Tax Payable Highlight Card
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(20),
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [Color(0xFF0F172A), Color(0xFF1E293B)],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(16),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Net Tax Payable to Government',
                          style: TextStyle(color: Colors.white70, fontSize: 13),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          AppFormatters.formatCurrency(gst.netTaxPayable, symbol: sym),
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 28,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 14),
                        Row(
                          children: [
                            Text(
                              'Output Tax: ${AppFormatters.formatCurrency(gst.totalTaxCollected, symbol: sym)}',
                              style: const TextStyle(color: Colors.white70, fontSize: 12),
                            ),
                            const Text(' • ', style: TextStyle(color: Colors.white70)),
                            Text(
                              'ITC Claimed: ${AppFormatters.formatCurrency(gst.totalInputTax, symbol: sym)}',
                              style: const TextStyle(color: Colors.white70, fontSize: 12),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 20),

                  // CGST, SGST, IGST Breakdown Grid
                  Row(
                    children: [
                      Expanded(
                        child: StatCard(
                          title: 'CGST (Central)',
                          value: AppFormatters.formatCurrency(gst.totalCGST, symbol: sym),
                          icon: Icons.account_balance,
                          color: AppTheme.primaryColor,
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: StatCard(
                          title: 'SGST (State)',
                          value: AppFormatters.formatCurrency(gst.totalSGST, symbol: sym),
                          icon: Icons.location_city,
                          color: AppTheme.secondaryColor,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 10),
                  StatCard(
                    title: 'IGST (Integrated Interstate)',
                    value: AppFormatters.formatCurrency(gst.totalIGST, symbol: sym),
                    icon: Icons.flight_takeoff,
                    color: AppTheme.infoColor,
                  ),
                  const SizedBox(height: 24),

                  // GST Slabs Breakdown Table
                  const Text(
                    'Slab-Wise Tax Collection',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 12),

                  Container(
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(color: AppTheme.borderLight),
                    ),
                    child: ListView.separated(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: gst.breakdown.length,
                      separatorBuilder: (_, __) => const Divider(height: 1),
                      itemBuilder: (ctx, i) {
                        final b = gst.breakdown[i];
                        return Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    b.taxName,
                                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                                  ),
                                  const SizedBox(height: 2),
                                  Text(
                                    '${b.transactionCount} transactions',
                                    style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                                  ),
                                ],
                              ),
                              Text(
                                AppFormatters.formatCurrency(b.amount, symbol: sym),
                                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: AppTheme.primaryColor),
                              ),
                            ],
                          ),
                        );
                      },
                    ),
                  ),
                  const SizedBox(height: 32),
                ],
              ),
            ),
    );
  }
}

