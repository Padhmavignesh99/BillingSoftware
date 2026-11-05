import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/purchase_model.dart';
import '../../providers/purchase_provider.dart';
import '../../providers/settings_provider.dart';
import '../../providers/supplier_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';

class PurchasesScreen extends StatefulWidget {
  const PurchasesScreen({super.key});

  @override
  State<PurchasesScreen> createState() => _PurchasesScreenState();
}

class _PurchasesScreenState extends State<PurchasesScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<PurchaseProvider>().fetchPurchases();
      context.read<SupplierProvider>().fetchSuppliers();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _showCreatePurchaseDialog() {
    final supProvider = context.read<SupplierProvider>();
    final pCtrl = TextEditingController();
    final qtyCtrl = TextEditingController(text: '10');
    final priceCtrl = TextEditingController(text: '100');
    int? selectedSupplierId = supProvider.suppliers.isNotEmpty ? supProvider.suppliers.first.id : null;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => StatefulBuilder(
        builder: (context, setModalState) {
          return Padding(
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
                    const Text('New Purchase Order', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                    IconButton(icon: const Icon(Icons.close), onPressed: () => Navigator.pop(ctx)),
                  ],
                ),
                const Divider(),
                const SizedBox(height: 12),
                const Text('Select Supplier', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                const SizedBox(height: 6),
                DropdownButtonFormField<int>(
                  value: selectedSupplierId,
                  items: supProvider.suppliers.map((s) {
                    return DropdownMenuItem(value: s.id, child: Text(s.companyName ?? s.name));
                  }).toList(),
                  onChanged: (v) => setModalState(() => selectedSupplierId = v),
                ),
                const SizedBox(height: 14),
                AppTextField(
                  controller: pCtrl,
                  label: 'Item Description',
                  hint: 'e.g. Wholesale Flour 50kg Bags',
                ),
                const SizedBox(height: 14),
                Row(
                  children: [
                    Expanded(
                      child: AppTextField(
                        controller: qtyCtrl,
                        label: 'Quantity',
                        hint: '10',
                        keyboardType: TextInputType.number,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: AppTextField(
                        controller: priceCtrl,
                        label: 'Unit Cost (₹)',
                        hint: '100',
                        keyboardType: TextInputType.number,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 20),
                AppButton(
                  label: 'Create Purchase Entry',
                  onPressed: () async {
                    final qty = double.tryParse(qtyCtrl.text) ?? 1;
                    final price = double.tryParse(priceCtrl.text) ?? 100;
                    final total = qty * price;
                    final sup = supProvider.suppliers.firstWhere(
                      (s) => s.id == selectedSupplierId,
                      orElse: () => supProvider.suppliers.first,
                    );

                    await context.read<PurchaseProvider>().createPurchase(
                          supplierId: sup.id,
                          supplierName: sup.companyName ?? sup.name,
                          items: [
                            PurchaseItemResponse(
                              productId: 1,
                              productName: pCtrl.text.isEmpty ? 'Purchased Bulk Stock' : pCtrl.text,
                              quantity: qty,
                              unitPrice: price,
                              totalPrice: total,
                            ),
                          ],
                          totalAmount: total,
                        );
                    if (ctx.mounted) Navigator.pop(ctx);
                    if (mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Purchase order created successfully')),
                      );
                    }
                  },
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final purchaseProvider = context.watch<PurchaseProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final purchases = purchaseProvider.purchases;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Purchase Orders'),
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.primaryColor,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.add_shopping_cart),
        label: const Text('New Purchase'),
        onPressed: _showCreatePurchaseDialog,
      ),
      body: Column(
        children: [
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: TextField(
              controller: _searchCtrl,
              onChanged: (v) => purchaseProvider.setSearchQuery(v),
              decoration: InputDecoration(
                hintText: 'Search purchase PO number or vendor...',
                prefixIcon: const Icon(Icons.search, size: 20),
                contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                suffixIcon: _searchCtrl.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear, size: 18),
                        onPressed: () {
                          _searchCtrl.clear();
                          purchaseProvider.setSearchQuery('');
                        },
                      )
                    : null,
              ),
            ),
          ),
          const Divider(height: 1),
          Expanded(
            child: purchaseProvider.isLoading
                ? const LoadingWidget(message: 'Loading purchases...')
                : purchases.isEmpty
                    ? EmptyState(
                        icon: Icons.shopping_cart_checkout,
                        title: 'No Purchases Found',
                        message: 'Log supplier purchase bills to update stock counts automatically.',
                        buttonText: 'Add Purchase Order',
                        onButtonPressed: _showCreatePurchaseDialog,
                      )
                    : RefreshIndicator(
                        onRefresh: () => purchaseProvider.fetchPurchases(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: purchases.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final p = purchases[i];
                            final statusColor = AppFormatters.getStatusColor(p.status);

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
                                        p.purchaseNumber,
                                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: AppTheme.primaryColor),
                                      ),
                                      Container(
                                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                        decoration: BoxDecoration(
                                          color: statusColor.withOpacity(0.12),
                                          borderRadius: BorderRadius.circular(12),
                                        ),
                                        child: Text(
                                          p.status,
                                          style: TextStyle(color: statusColor, fontSize: 11, fontWeight: FontWeight.bold),
                                        ),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 8),
                                  Row(
                                    children: [
                                      const Icon(Icons.local_shipping_outlined, size: 16, color: AppTheme.textMuted),
                                      const SizedBox(width: 6),
                                      Text(
                                        p.supplierName ?? 'Vendor',
                                        style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13),
                                      ),
                                      const Spacer(),
                                      Text(
                                        AppFormatters.formatCurrency(p.totalAmount, symbol: sym),
                                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: AppTheme.textDark),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 6),
                                  Text(
                                    AppFormatters.formatDate(p.purchaseDate),
                                    style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
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

