import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/return_model.dart';
import '../../providers/returns_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';

class PurchaseReturnsScreen extends StatefulWidget {
  const PurchaseReturnsScreen({super.key});

  @override
  State<PurchaseReturnsScreen> createState() => _PurchaseReturnsScreenState();
}

class _PurchaseReturnsScreenState extends State<PurchaseReturnsScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<ReturnsProvider>().fetchReturns();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _showCreatePurchaseReturnDialog() {
    final poCtrl = TextEditingController(text: 'PO-2026-081');
    final supCtrl = TextEditingController(text: 'Aggarwal Wholesale Traders');
    final reasonCtrl = TextEditingController(text: 'Damaged or defective stock');
    final itemCtrl = TextEditingController(text: 'Basmati Rice Premium 5kg');
    final qtyCtrl = TextEditingController(text: '5');
    final amountCtrl = TextEditingController(text: '1900');

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.only(
          left: 20,
          right: 20,
          top: 20,
          bottom: MediaQuery.of(context).viewInsets.bottom + 20,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text('New Purchase Return (Debit Note)', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                IconButton(icon: const Icon(Icons.close), onPressed: () => Navigator.pop(ctx)),
              ],
            ),
            const Divider(),
            const SizedBox(height: 12),
            AppTextField(controller: supCtrl, label: 'Supplier / Vendor', hint: 'Supplier name'),
            const SizedBox(height: 12),
            AppTextField(controller: poCtrl, label: 'Purchase PO Reference #', hint: 'PO-123'),
            const SizedBox(height: 12),
            AppTextField(controller: itemCtrl, label: 'Item Name', hint: 'Item'),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(child: AppTextField(controller: qtyCtrl, label: 'Qty', keyboardType: TextInputType.number)),
                const SizedBox(width: 12),
                Expanded(child: AppTextField(controller: amountCtrl, label: 'Debit Total (₹)', keyboardType: TextInputType.number)),
              ],
            ),
            const SizedBox(height: 12),
            AppTextField(controller: reasonCtrl, label: 'Return Reason', hint: 'Reason'),
            const SizedBox(height: 20),
            AppButton(
              label: 'Issue Debit Note & Return Goods',
              onPressed: () async {
                final amt = double.tryParse(amountCtrl.text) ?? 0.0;
                await context.read<ReturnsProvider>().createPurchaseReturn(
                      purchaseId: 1,
                      supplierId: 1,
                      supplierName: supCtrl.text.trim(),
                      reason: reasonCtrl.text.trim(),
                      items: [
                        ReturnItemResponse(
                          productId: 1,
                          productName: itemCtrl.text.trim(),
                          quantity: double.tryParse(qtyCtrl.text) ?? 1,
                          unitPrice: amt,
                          totalPrice: amt,
                        ),
                      ],
                      totalAmount: amt,
                    );
                if (ctx.mounted) Navigator.pop(ctx);
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Purchase return processed successfully')),
                  );
                }
              },
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final returnsProvider = context.watch<ReturnsProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final returns = returnsProvider.purchaseReturns;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Purchase Returns'),
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.primaryColor,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.keyboard_return),
        label: const Text('New Return'),
        onPressed: _showCreatePurchaseReturnDialog,
      ),
      body: Column(
        children: [
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: TextField(
              controller: _searchCtrl,
              onChanged: (v) => returnsProvider.setSearchQuery(v),
              decoration: InputDecoration(
                hintText: 'Search purchase return, vendor or reason...',
                prefixIcon: const Icon(Icons.search, size: 20),
                contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                suffixIcon: _searchCtrl.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear, size: 18),
                        onPressed: () {
                          _searchCtrl.clear();
                          returnsProvider.setSearchQuery('');
                        },
                      )
                    : null,
              ),
            ),
          ),
          const Divider(height: 1),
          Expanded(
            child: returnsProvider.isLoading
                ? const LoadingWidget(message: 'Loading purchase returns...')
                : returns.isEmpty
                    ? EmptyState(
                        icon: Icons.keyboard_return_outlined,
                        title: 'No Purchase Returns',
                        message: 'Vendor returns and debit notes will appear here.',
                        buttonText: 'Process Return',
                        onButtonPressed: _showCreatePurchaseReturnDialog,
                      )
                    : RefreshIndicator(
                        onRefresh: () => returnsProvider.fetchReturns(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: returns.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final r = returns[i];

                            return Container(
                              padding: const EdgeInsets.all(16),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: AppTheme.borderLight),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      Text(
                                        r.returnNumber,
                                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: AppTheme.primaryColor),
                                      ),
                                      Text(
                                        AppFormatters.formatCurrency(r.totalAmount, symbol: sym),
                                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: AppTheme.successColor),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 6),
                                  Text(
                                    'Vendor: ${r.supplierName ?? "Supplier"}',
                                    style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13),
                                  ),
                                  const SizedBox(height: 2),
                                  Text(
                                    'Reason: ${r.reason}',
                                    style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    AppFormatters.formatDate(r.returnDate),
                                    style: const TextStyle(fontSize: 11, color: AppTheme.textMuted),
                                  ),
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

