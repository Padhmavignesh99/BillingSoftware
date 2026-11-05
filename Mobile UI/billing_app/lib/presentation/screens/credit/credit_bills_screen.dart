import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/credit_bill_model.dart';
import '../../providers/credit_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';

class CreditBillsScreen extends StatefulWidget {
  const CreditBillsScreen({super.key});

  @override
  State<CreditBillsScreen> createState() => _CreditBillsScreenState();
}

class _CreditBillsScreenState extends State<CreditBillsScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<CreditProvider>().fetchCreditBills();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _showSettleCreditDialog(CreditBillResponse bill) {
    final amountCtrl = TextEditingController(text: bill.balanceAmount.toString());

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Text('Settle Credit: ${bill.creditBillNumber}'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Customer: ${bill.customerName}', style: const TextStyle(fontWeight: FontWeight.bold)),
            Text('Balance Due: ₹${bill.balanceAmount.toStringAsFixed(2)}', style: const TextStyle(color: AppTheme.errorColor)),
            const SizedBox(height: 16),
            AppTextField(
              controller: amountCtrl,
              label: 'Settlement Payment Amount (₹)',
              keyboardType: TextInputType.number,
            ),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Cancel')),
          ElevatedButton(
            onPressed: () async {
              final amt = double.tryParse(amountCtrl.text);
              if (amt != null && amt > 0) {
                await context.read<CreditProvider>().settleCreditBill(bill.id, amt);
                if (ctx.mounted) Navigator.pop(ctx);
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Credit bill settled successfully')),
                  );
                }
              }
            },
            child: const Text('Confirm Settlement'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final creditProvider = context.watch<CreditProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final bills = creditProvider.creditBills;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Credit Bills & Udhaar'),
      ),
      body: Column(
        children: [
          // Header summary card
          Container(
            color: Colors.white,
            padding: const EdgeInsets.all(16),
            child: Column(
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text('Total Credit Outstanding', style: TextStyle(color: AppTheme.textMuted, fontSize: 13)),
                    Text(
                      AppFormatters.formatCurrency(creditProvider.totalCreditOutstanding, symbol: sym),
                      style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppTheme.errorColor),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _searchCtrl,
                  onChanged: (v) => creditProvider.setSearchQuery(v),
                  decoration: InputDecoration(
                    hintText: 'Search customer or credit bill ID...',
                    prefixIcon: const Icon(Icons.search, size: 20),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                    suffixIcon: _searchCtrl.text.isNotEmpty
                        ? IconButton(
                            icon: const Icon(Icons.clear, size: 18),
                            onPressed: () {
                              _searchCtrl.clear();
                              creditProvider.setSearchQuery('');
                            },
                          )
                        : null,
                  ),
                ),
                const SizedBox(height: 10),
                SizedBox(
                  height: 32,
                  child: ListView(
                    scrollDirection: Axis.horizontal,
                    children: ['All', 'Pending', 'Overdue', 'Settled'].map((st) {
                      final isSel = creditProvider.filterStatus == st;
                      return Padding(
                        padding: const EdgeInsets.only(right: 8),
                        child: ChoiceChip(
                          label: Text(st),
                          selected: isSel,
                          selectedColor: AppTheme.primaryColor.withOpacity(0.12),
                          onSelected: (_) => creditProvider.setFilterStatus(st),
                        ),
                      );
                    }).toList(),
                  ),
                ),
              ],
            ),
          ),
          const Divider(height: 1),

          // Credit Bills List
          Expanded(
            child: creditProvider.isLoading
                ? const LoadingWidget(message: 'Loading credit bills...')
                : bills.isEmpty
                    ? const EmptyState(
                        icon: Icons.credit_card_outlined,
                        title: 'No Credit Bills',
                        message: 'All customer credit bills have been settled.',
                      )
                    : RefreshIndicator(
                        onRefresh: () => creditProvider.fetchCreditBills(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: bills.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final b = bills[i];
                            final isOverdue = b.isOverdue;

                            return Container(
                              padding: const EdgeInsets.all(16),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(
                                  color: isOverdue ? AppTheme.errorColor.withOpacity(0.5) : AppTheme.borderLight,
                                ),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      Text(
                                        b.creditBillNumber,
                                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: AppTheme.primaryColor),
                                      ),
                                      if (isOverdue)
                                        Container(
                                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                                          decoration: BoxDecoration(
                                            color: AppTheme.errorColor.withOpacity(0.12),
                                            borderRadius: BorderRadius.circular(4),
                                          ),
                                          child: const Text(
                                            'OVERDUE',
                                            style: TextStyle(color: AppTheme.errorColor, fontSize: 10, fontWeight: FontWeight.bold),
                                          ),
                                        )
                                      else
                                        Text(
                                          b.status,
                                          style: TextStyle(
                                            color: b.balanceAmount == 0 ? AppTheme.successColor : AppTheme.warningColor,
                                            fontWeight: FontWeight.bold,
                                            fontSize: 12,
                                          ),
                                        ),
                                    ],
                                  ),
                                  const SizedBox(height: 8),
                                  Row(
                                    children: [
                                      Expanded(
                                        child: Column(
                                          crossAxisAlignment: CrossAxisAlignment.start,
                                          children: [
                                            Text(
                                              b.customerName,
                                              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                                            ),
                                            const SizedBox(height: 2),
                                            Text(
                                              'Due Date: ${AppFormatters.formatDate(b.dueDate)}',
                                              style: TextStyle(fontSize: 12, color: isOverdue ? AppTheme.errorColor : AppTheme.textMuted),
                                            ),
                                          ],
                                        ),
                                      ),
                                      Column(
                                        crossAxisAlignment: CrossAxisAlignment.end,
                                        children: [
                                          Text(
                                            AppFormatters.formatCurrency(b.balanceAmount, symbol: sym),
                                            style: const TextStyle(
                                              fontWeight: FontWeight.bold,
                                              fontSize: 16,
                                              color: AppTheme.errorColor,
                                            ),
                                          ),
                                          Text(
                                            'Total: ${AppFormatters.formatCurrency(b.amount, symbol: sym)}',
                                            style: const TextStyle(fontSize: 11, color: AppTheme.textMuted),
                                          ),
                                        ],
                                      ),
                                    ],
                                  ),
                                  if (b.balanceAmount > 0) ...[
                                    const SizedBox(height: 12),
                                    Row(
                                      mainAxisAlignment: MainAxisAlignment.end,
                                      children: [
                                        OutlinedButton(
                                          style: OutlinedButton.styleFrom(
                                            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                                            minimumSize: Size.zero,
                                          ),
                                          onPressed: () => _showSettleCreditDialog(b),
                                          child: const Text('Record Settlement', style: TextStyle(fontSize: 12)),
                                        ),
                                      ],
                                    ),
                                  ],
                                ],
                              ),
                            );
                          },
                        ),
                      ),
          ),
        ],
      ),
    );
  }
}

