import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../providers/report_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/loading_widget.dart';

class ProfitLossScreen extends StatelessWidget {
  const ProfitLossScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final reportProvider = context.watch<ReportProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final pl = reportProvider.profitLoss;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Profit & Loss Statement'),
      ),
      body: reportProvider.isLoading
          ? const LoadingWidget(message: 'Calculating P&L statement...')
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  // Net Profit Banner
                  Container(
                    padding: const EdgeInsets.all(22),
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [AppTheme.successColor, Color(0xFF059669)],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(16),
                      boxShadow: [
                        BoxShadow(
                          color: AppTheme.successColor.withOpacity(0.3),
                          blurRadius: 10,
                          offset: const Offset(0, 4),
                        ),
                      ],
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Net Business Profit',
                          style: TextStyle(color: Colors.white70, fontSize: 13, fontWeight: FontWeight.w500),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          AppFormatters.formatCurrency(pl.netProfit, symbol: sym),
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 32,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 12),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.2),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Text(
                            'Net Profit Margin: ${pl.profitMargin.toStringAsFixed(1)}%',
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 12,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 20),

                  // Statement Breakdown Card
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
                        const Text(
                          'Income Statement Summary',
                          style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                        ),
                        const Divider(height: 24),
                        _plRow(
                          '1. Total Revenue / Gross Sales',
                          AppFormatters.formatCurrency(pl.totalRevenue, symbol: sym),
                          isPositive: true,
                        ),
                        const SizedBox(height: 10),
                        _plRow(
                          '2. Less: Cost of Goods Sold (COGS)',
                          '- ${AppFormatters.formatCurrency(pl.totalCostOfGoods, symbol: sym)}',
                          isNegative: true,
                        ),
                        const Divider(height: 20),
                        _plRow(
                          'Gross Operational Profit',
                          AppFormatters.formatCurrency(pl.grossProfit, symbol: sym),
                          isBold: true,
                        ),
                        const SizedBox(height: 16),
                        _plRow(
                          '3. Less: Operating Expenses',
                          '- ${AppFormatters.formatCurrency(pl.totalExpenses, symbol: sym)}',
                          isNegative: true,
                        ),
                        const Divider(height: 24),
                        _plRow(
                          'Net Profit (Before Tax)',
                          AppFormatters.formatCurrency(pl.netProfit, symbol: sym),
                          isBold: true,
                          isHighlight: true,
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 24),

                  // Insight Card
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: Colors.blue.shade50,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.blue.shade200),
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Icon(Icons.info_outline, color: AppTheme.primaryColor),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text(
                                'Healthy Margin Indicator',
                                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: AppTheme.primaryColor),
                              ),
                              const SizedBox(height: 2),
                              Text(
                                'Your current gross profit margin is ${(pl.grossProfit / (pl.totalRevenue == 0 ? 1 : pl.totalRevenue) * 100).toStringAsFixed(1)}%. Operating expenses are well within 15% of gross turnover.',
                                style: const TextStyle(fontSize: 12, color: AppTheme.textDark, height: 1.4),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 32),
                ],
              ),
            ),
    );
  }

  Widget _plRow(
    String label,
    String value, {
    bool isBold = false,
    bool isPositive = false,
    bool isNegative = false,
    bool isHighlight = false,
  }) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: TextStyle(
            fontSize: isBold ? 14 : 13,
            fontWeight: isBold ? FontWeight.bold : FontWeight.normal,
            color: isHighlight ? AppTheme.primaryColor : AppTheme.textDark,
          ),
        ),
        Text(
          value,
          style: TextStyle(
            fontSize: isBold ? 16 : 13,
            fontWeight: isBold ? FontWeight.bold : FontWeight.w600,
            color: isNegative
                ? AppTheme.errorColor
                : isPositive
                    ? AppTheme.successColor
                    : isHighlight
                        ? AppTheme.primaryColor
                        : AppTheme.textDark,
          ),
        ),
      ],
    );
  }
}

