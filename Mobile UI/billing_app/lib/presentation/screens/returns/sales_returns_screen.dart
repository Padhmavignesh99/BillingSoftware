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

class SalesReturnsScreen extends StatefulWidget {
  const SalesReturnsScreen({super.key});

  @override
  State<SalesReturnsScreen> createState() => _SalesReturnsScreenState();
}

class _SalesReturnsScreenState extends State<SalesReturnsScreen> {
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

  void _showCreateSalesReturnDialog() {
    final invIdCtrl = TextEditingController(text: '101');
    final custCtrl = TextEditingController(text: 'Rahul Sharma');
    final reasonCtrl = TextEditingController(text: 'Defective item returned');
    final itemCtrl = TextEditingController(text: 'Basmati Rice Premium 5kg');
    final qtyCtrl = TextEditingController(text: '1');
    final refundCtrl = TextEditingController(text: '450');

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
                const Text('New Sales Return (Credit Note)', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                IconButton(icon: const Icon(Icons.close), onPressed: () => Navigator.pop(ctx)),
              ],
            ),
            const Divider(),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: AppTextField(controller: invIdCtrl, label: 'Invoice # / ID', hint: '101'),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: AppTextField(controller: custCtrl, label: 'Customer Name', hint: 'Rahul'),
                ),
              ],
            ),
            const SizedBox(height: 12),
            AppTextField(controller: itemCtrl, label: 'Product Name', hint: 'Item name'),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(child: AppTextField(controller: qtyCtrl, label: 'Qty', keyboardType: TextInputType.number)),
                const SizedBox(width: 12),
                Expanded(child: AppTextField(controller: refundCtrl, label: 'Refund (₹)', keyboardType: TextInputType.number)),
              ],
            ),
            const SizedBox(height: 12),
            AppTextField(controller: reasonCtrl, label: 'Return Reason', hint: 'Reason for return'),
            const SizedBox(height: 20),
            AppButton(
              label: 'Process Return & Restock',
              onPressed: () async {
                final refAmt = double.tryParse(refundCtrl.text) ?? 0.0;
                await context.read<ReturnsProvider>().createSalesReturn(
                      invoiceId: int.tryParse(invIdCtrl.text) ?? 101,
                      customerName: custCtrl.text.trim(),
                      reason: reasonCtrl.text.trim(),
                      items: [
                        ReturnItemResponse(
                          productId: 1,
                          productName: itemCtrl.text.trim(),
                          quantity: double.tryParse(qtyCtrl.text) ?? 1,
                          unitPrice: refAmt,
                          totalPrice: refAmt,
                        ),
                      ],
                      totalAmount: refAmt,
                    );
                if (ctx.mounted) Navigator.pop(ctx);
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Sales return processed and stock restored.')),
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
    final returns = returnsProvider.salesReturns;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Sales Returns'),
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.primaryColor,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.assignment_return),
        label: const Text('New Return'),
        onPressed: _showCreateSalesReturnDialog,
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
                hintText: 'Search return ID, customer or reason...',
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
                ? const LoadingWidget(message: 'Loading returns...')
                : returns.isEmpty
                    ? EmptyState(
                        icon: Icons.assignment_return_outlined,
                        title: 'No Sales Returns',
                        message: 'Items returned by customers will be tracked here.',
                        buttonText: 'Process Return',
                        onButtonPressed: _showCreateSalesReturnDialog,
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
                                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: AppTheme.errorColor),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 6),
                                  Text(
                                    'Customer: ${r.customerName ?? "Walk-in"} • Inv: #${r.invoiceId}',
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

