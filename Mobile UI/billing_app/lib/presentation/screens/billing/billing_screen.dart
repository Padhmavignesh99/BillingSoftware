import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/constants/app_constants.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/customer_model.dart';
import '../../models/product_model.dart';
import '../../providers/billing_provider.dart';
import '../../providers/customer_provider.dart';
import '../../providers/product_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/app_button.dart';

class BillingScreen extends StatefulWidget {
  const BillingScreen({super.key});

  @override
  State<BillingScreen> createState() => _BillingScreenState();
}

class _BillingScreenState extends State<BillingScreen> {
  final TextEditingController _searchCtrl = TextEditingController();
  final TextEditingController _discountCtrl = TextEditingController(text: '0');

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<ProductProvider>().fetchProducts();
      context.read<CustomerProvider>().fetchCustomers();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    _discountCtrl.dispose();
    super.dispose();
  }

  void _showCheckoutDialog() {
    final billing = context.read<BillingProvider>();
    final settings = context.read<SettingsProvider>();
    final sym = settings.currencySymbol;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) {
        return StatefulBuilder(
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
                      const Text(
                        'Confirm Payment',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                      ),
                      IconButton(
                        icon: const Icon(Icons.close),
                        onPressed: () => Navigator.pop(ctx),
                      ),
                    ],
                  ),
                  const Divider(),
                  const SizedBox(height: 12),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text('Total Payable', style: TextStyle(fontSize: 15, color: AppTheme.textMuted)),
                      Text(
                        AppFormatters.formatCurrency(billing.totalAmount, symbol: sym),
                        style: const TextStyle(
                          fontSize: 24,
                          fontWeight: FontWeight.bold,
                          color: AppTheme.primaryColor,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 20),
                  const Text('Select Payment Method', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  const SizedBox(height: 10),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: AppConstants.paymentMethods.map((method) {
                      final isSel = billing.paymentMethod == method;
                      return ChoiceChip(
                        label: Text(method),
                        selected: isSel,
                        selectedColor: AppTheme.primaryColor.withOpacity(0.15),
                        labelStyle: TextStyle(
                          color: isSel ? AppTheme.primaryColor : AppTheme.textDark,
                          fontWeight: isSel ? FontWeight.bold : FontWeight.normal,
                        ),
                        onSelected: (val) {
                          if (val) {
                            billing.setPaymentMethod(method);
                            setModalState(() {});
                          }
                        },
                      );
                    }).toList(),
                  ),
                  const SizedBox(height: 24),
                  AppButton(
                    label: 'Complete Sale & Print Bill',
                    isLoading: billing.isProcessing,
                    onPressed: () async {
                      final invoice = await billing.checkout();
                      if (ctx.mounted) Navigator.pop(ctx);
                      if (invoice != null && mounted) {
                        _showSuccessDialog(invoice.invoiceNumber, invoice.totalAmount, sym);
                      }
                    },
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  void _showSuccessDialog(String invNumber, double amount, String sym) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: AppTheme.successColor.withOpacity(0.12),
                shape: BoxShape.circle,
              ),
              child: const Icon(Icons.check_circle_rounded, color: AppTheme.successColor, size: 54),
            ),
            const SizedBox(height: 16),
            const Text(
              'Bill Generated Successfully!',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 8),
            Text(
              'Invoice: $invNumber\nAmount: ${AppFormatters.formatCurrency(amount, symbol: sym)}',
              textAlign: TextAlign.center,
              style: const TextStyle(color: AppTheme.textMuted, fontSize: 14),
            ),
            const SizedBox(height: 20),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    icon: const Icon(Icons.print, size: 16),
                    label: const Text('Print'),
                    onPressed: () {
                      Navigator.pop(ctx);
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Printing bill...')),
                      );
                    },
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: ElevatedButton(
                    child: const Text('Done'),
                    onPressed: () => Navigator.pop(ctx),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  void _showCustomerSelectionSheet() {
    final custProvider = context.read<CustomerProvider>();
    final billing = context.read<BillingProvider>();

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (ctx) {
        return DraggableScrollableSheet(
          expand: false,
          initialChildSize: 0.6,
          maxChildSize: 0.9,
          builder: (_, scrollController) {
            return Column(
              children: [
                const Padding(
                  padding: EdgeInsets.all(16.0),
                  child: Text(
                    'Select Customer',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                ),
                ListTile(
                  leading: const CircleAvatar(
                    backgroundColor: AppTheme.scaffoldBgLight,
                    child: Icon(Icons.person_outline, color: AppTheme.textDark),
                  ),
                  title: const Text('Walk-in Customer'),
                  subtitle: const Text('Standard cash billing'),
                  onTap: () {
                    billing.setCustomer(null);
                    Navigator.pop(ctx);
                  },
                ),
                const Divider(),
                Expanded(
                  child: ListView.builder(
                    controller: scrollController,
                    itemCount: custProvider.customers.length,
                    itemBuilder: (_, i) {
                      final c = custProvider.customers[i];
                      return ListTile(
                        leading: CircleAvatar(
                          backgroundColor: AppTheme.primaryColor.withOpacity(0.1),
                          child: Text(c.name[0].toUpperCase(), style: const TextStyle(color: AppTheme.primaryColor)),
                        ),
                        title: Text(c.name),
                        subtitle: Text(c.phone ?? c.city ?? ''),
                        trailing: c.outstandingBalance > 0
                            ? Text(
                                'Due: ₹${c.outstandingBalance.toInt()}',
                                style: const TextStyle(color: AppTheme.errorColor, fontSize: 12, fontWeight: FontWeight.bold),
                              )
                            : null,
                        onTap: () {
                          billing.setCustomer(c);
                          Navigator.pop(ctx);
                        },
                      );
                    },
                  ),
                ),
              ],
            );
          },
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final productProvider = context.watch<ProductProvider>();
    final billing = context.watch<BillingProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;

    return Scaffold(
      body: LayoutBuilder(
        builder: (context, constraints) {
          final isWide = constraints.maxWidth > 800;

          if (isWide) {
            // Responsive macOS / Desktop Split Screen
            return Row(
              children: [
                Expanded(
                  flex: 3,
                  child: _buildProductCatalog(productProvider, billing, sym),
                ),
                const VerticalDivider(width: 1),
                Expanded(
                  flex: 2,
                  child: _buildCartPanel(billing, sym),
                ),
              ],
            );
          } else {
            // Mobile Stacked View with persistent bottom summary
            return Column(
              children: [
                Expanded(child: _buildProductCatalog(productProvider, billing, sym)),
                if (billing.cart.isNotEmpty) _buildMobileCartSummary(billing, sym),
              ],
            );
          }
        },
      ),
    );
  }

  Widget _buildProductCatalog(ProductProvider pProvider, BillingProvider billing, String sym) {
    return Column(
      children: [
        // Top Search Bar & Customer Selector
        Padding(
          padding: const EdgeInsets.all(12.0),
          child: Row(
            children: [
              Expanded(
                child: TextField(
                  controller: _searchCtrl,
                  onChanged: (v) => pProvider.setSearchQuery(v),
                  decoration: InputDecoration(
                    hintText: 'Search product by name or barcode...',
                    prefixIcon: const Icon(Icons.search, size: 20),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                    suffixIcon: _searchCtrl.text.isNotEmpty
                        ? IconButton(
                            icon: const Icon(Icons.clear, size: 18),
                            onPressed: () {
                              _searchCtrl.clear();
                              pProvider.setSearchQuery('');
                            },
                          )
                        : null,
                  ),
                ),
              ),
              const SizedBox(width: 8),
              OutlinedButton.icon(
                icon: const Icon(Icons.person, size: 18),
                label: Text(
                  billing.customerName.length > 12
                      ? '${billing.customerName.substring(0, 10)}...'
                      : billing.customerName,
                  style: const TextStyle(fontSize: 12),
                ),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 12),
                ),
                onPressed: _showCustomerSelectionSheet,
              ),
            ],
          ),
        ),

        // Category Chips
        SizedBox(
          height: 38,
          child: ListView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 12),
            children: [
              _categoryChip('All', pProvider.selectedCategory == null || pProvider.selectedCategory == 'All', () {
                pProvider.setSelectedCategory('All');
              }),
              ...pProvider.categories.map((c) {
                return _categoryChip(c.name, pProvider.selectedCategory == c.name, () {
                  pProvider.setSelectedCategory(c.name);
                });
              }),
            ],
          ),
        ),
        const SizedBox(height: 8),

        // Product Grid
        Expanded(
          child: pProvider.products.isEmpty
              ? const Center(child: Text('No products found matching search.'))
              : GridView.builder(
                  padding: const EdgeInsets.all(12),
                  gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(
                    maxCrossAxisExtent: 180,
                    childAspectRatio: 0.85,
                    crossAxisSpacing: 10,
                    mainAxisSpacing: 10,
                  ),
                  itemCount: pProvider.products.length,
                  itemBuilder: (_, i) {
                    final product = pProvider.products[i];
                    return _productItemCard(product, billing, sym);
                  },
                ),
        ),
      ],
    );
  }

  Widget _categoryChip(String label, bool isSelected, VoidCallback onTap) {
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: FilterChip(
        label: Text(label),
        selected: isSelected,
        selectedColor: AppTheme.primaryColor.withOpacity(0.12),
        labelStyle: TextStyle(
          fontSize: 12,
          fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
          color: isSelected ? AppTheme.primaryColor : AppTheme.textDark,
        ),
        onSelected: (_) => onTap(),
      ),
    );
  }

  Widget _productItemCard(ProductResponse p, BillingProvider billing, String sym) {
    final inCart = billing.cart.any((item) => item.product.id == p.id);

    return InkWell(
      onTap: () => billing.addToCart(p),
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.all(10),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: inCart ? AppTheme.primaryColor : AppTheme.borderLight,
            width: inCart ? 1.5 : 1,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.02),
              blurRadius: 4,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Center(
                child: Container(
                  width: double.infinity,
                  decoration: BoxDecoration(
                    color: AppTheme.scaffoldBgLight,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: const Icon(Icons.shopping_bag_outlined, color: AppTheme.primaryLight, size: 32),
                ),
              ),
            ),
            const SizedBox(height: 8),
            Text(
              p.name,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 12),
            ),
            const SizedBox(height: 4),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  AppFormatters.formatCurrency(p.sellingPrice, symbol: sym),
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 13,
                    color: AppTheme.primaryColor,
                  ),
                ),
                Container(
                  padding: const EdgeInsets.all(4),
                  decoration: const BoxDecoration(
                    color: AppTheme.primaryColor,
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(Icons.add, color: Colors.white, size: 14),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCartPanel(BillingProvider billing, String sym) {
    return Container(
      color: Colors.white,
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Current Bill (${billing.itemCount} items)',
                style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
              ),
              if (billing.cart.isNotEmpty)
                TextButton(
                  onPressed: () => billing.clearCart(),
                  child: const Text('Clear', style: TextStyle(color: AppTheme.errorColor)),
                ),
            ],
          ),
          const Divider(),
          Expanded(
            child: billing.cart.isEmpty
                ? const Center(
                    child: Text(
                      'Cart is empty.\nTap items on the left to add.',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: AppTheme.textMuted),
                    ),
                  )
                : ListView.separated(
                    itemCount: billing.cart.length,
                    separatorBuilder: (_, __) => const Divider(height: 12),
                    itemBuilder: (_, i) {
                      final item = billing.cart[i];
                      return Row(
                        children: [
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  item.product.name,
                                  style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13),
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                ),
                                Text(
                                  '${AppFormatters.formatCurrency(item.unitPrice, symbol: sym)} × ${item.quantity.toInt()}',
                                  style: const TextStyle(color: AppTheme.textMuted, fontSize: 11),
                                ),
                              ],
                            ),
                          ),
                          Row(
                            children: [
                              IconButton(
                                icon: const Icon(Icons.remove_circle_outline, size: 20),
                                onPressed: () => billing.updateQuantity(item.product.id, item.quantity - 1),
                              ),
                              Text('${item.quantity.toInt()}', style: const TextStyle(fontWeight: FontWeight.bold)),
                              IconButton(
                                icon: const Icon(Icons.add_circle_outline, size: 20),
                                onPressed: () => billing.updateQuantity(item.product.id, item.quantity + 1),
                              ),
                            ],
                          ),
                          Text(
                            AppFormatters.formatCurrency(item.totalPrice, symbol: sym),
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                          ),
                        ],
                      );
                    },
                  ),
          ),
          const Divider(),
          _billSummaryRow('Subtotal', AppFormatters.formatCurrency(billing.subTotal, symbol: sym)),
          _billSummaryRow('Tax', AppFormatters.formatCurrency(billing.totalTax, symbol: sym)),
          if (billing.totalDiscount > 0)
            _billSummaryRow('Discount', '- ${AppFormatters.formatCurrency(billing.totalDiscount, symbol: sym)}', isNegative: true),
          const SizedBox(height: 8),
          _billSummaryRow(
            'Total Payable',
            AppFormatters.formatCurrency(billing.totalAmount, symbol: sym),
            isBold: true,
          ),
          const SizedBox(height: 16),
          AppButton(
            label: 'Proceed to Pay (${AppFormatters.formatCurrency(billing.totalAmount, symbol: sym)})',
            onPressed: billing.cart.isEmpty ? null : _showCheckoutDialog,
          ),
        ],
      ),
    );
  }

  Widget _buildMobileCartSummary(BillingProvider billing, String sym) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.08),
            blurRadius: 10,
            offset: const Offset(0, -3),
          ),
        ],
      ),
      child: SafeArea(
        child: Row(
          children: [
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  '${billing.itemCount} items',
                  style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                ),
                Text(
                  AppFormatters.formatCurrency(billing.totalAmount, symbol: sym),
                  style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: AppTheme.primaryColor),
                ),
              ],
            ),
            const SizedBox(width: 16),
            Expanded(
              child: AppButton(
                label: 'View Cart & Pay',
                onPressed: _showCheckoutDialog,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _billSummaryRow(String label, String value, {bool isBold = false, bool isNegative = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              fontSize: isBold ? 15 : 13,
              fontWeight: isBold ? FontWeight.bold : FontWeight.normal,
              color: isBold ? AppTheme.textDark : AppTheme.textMuted,
            ),
          ),
          Text(
            value,
            style: TextStyle(
              fontSize: isBold ? 17 : 13,
              fontWeight: isBold ? FontWeight.bold : FontWeight.w600,
              color: isNegative
                  ? AppTheme.errorColor
                  : isBold
                      ? AppTheme.primaryColor
                      : AppTheme.textDark,
            ),
          ),
        ],
      ),
    );
  }
}

