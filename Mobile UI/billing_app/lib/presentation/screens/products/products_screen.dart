import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/product_model.dart';
import '../../providers/product_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';

class ProductsScreen extends StatefulWidget {
  const ProductsScreen({super.key});

  @override
  State<ProductsScreen> createState() => _ProductsScreenState();
}

class _ProductsScreenState extends State<ProductsScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<ProductProvider>().fetchProducts();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final productProvider = context.watch<ProductProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final products = productProvider.products;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Products & Catalog'),
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.primaryColor,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.add),
        label: const Text('Add Product'),
        onPressed: () => context.push('/products/add'),
      ),
      body: Column(
        children: [
          // Search & Category Filters
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
            child: Column(
              children: [
                TextField(
                  controller: _searchCtrl,
                  onChanged: (v) => productProvider.setSearchQuery(v),
                  decoration: InputDecoration(
                    hintText: 'Search products by name, SKU or barcode...',
                    prefixIcon: const Icon(Icons.search, size: 20),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                    suffixIcon: _searchCtrl.text.isNotEmpty
                        ? IconButton(
                            icon: const Icon(Icons.clear, size: 18),
                            onPressed: () {
                              _searchCtrl.clear();
                              productProvider.setSearchQuery('');
                            },
                          )
                        : null,
                  ),
                ),
                const SizedBox(height: 10),
                SizedBox(
                  height: 34,
                  child: ListView(
                    scrollDirection: Axis.horizontal,
                    children: [
                      Padding(
                        padding: const EdgeInsets.only(right: 8),
                        child: ChoiceChip(
                          label: const Text('All Categories'),
                          selected: productProvider.selectedCategory == null ||
                              productProvider.selectedCategory == 'All',
                          selectedColor: AppTheme.primaryColor.withOpacity(0.12),
                          onSelected: (_) => productProvider.setSelectedCategory('All'),
                        ),
                      ),
                      ...productProvider.categories.map((c) {
                        final isSel = productProvider.selectedCategory == c.name;
                        return Padding(
                          padding: const EdgeInsets.only(right: 8),
                          child: ChoiceChip(
                            label: Text(c.name),
                            selected: isSel,
                            selectedColor: AppTheme.primaryColor.withOpacity(0.12),
                            onSelected: (_) => productProvider.setSelectedCategory(c.name),
                          ),
                        );
                      }),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const Divider(height: 1),

          // Products List
          Expanded(
            child: productProvider.isLoading
                ? const LoadingWidget(message: 'Loading products...')
                : products.isEmpty
                    ? EmptyState(
                        icon: Icons.inventory_2_outlined,
                        title: 'No Products Found',
                        message: 'Add items to your inventory catalog to get started.',
                        buttonText: 'Add Product',
                        onButtonPressed: () => context.push('/products/add'),
                      )
                    : RefreshIndicator(
                        onRefresh: () => productProvider.fetchProducts(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: products.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final p = products[i];
                            final isLow = p.isLowStock;
                            final isOut = p.isOutOfStock;

                            return Container(
                              padding: const EdgeInsets.all(14),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: AppTheme.borderLight),
                              ),
                              child: Row(
                                children: [
                                  // Product Icon
                                  Container(
                                    width: 48,
                                    height: 48,
                                    decoration: BoxDecoration(
                                      color: AppTheme.scaffoldBgLight,
                                      borderRadius: BorderRadius.circular(10),
                                    ),
                                    child: const Icon(Icons.shopping_bag_outlined, color: AppTheme.primaryColor),
                                  ),
                                  const SizedBox(width: 14),

                                  // Product Info
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          p.name,
                                          style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14),
                                        ),
                                        const SizedBox(height: 2),
                                        Text(
                                          'SKU: ${p.sku ?? "-"} • ${p.categoryName ?? "General"}',
                                          style: const TextStyle(color: AppTheme.textMuted, fontSize: 12),
                                        ),
                                        const SizedBox(height: 4),
                                        Row(
                                          children: [
                                            Text(
                                              'Stock: ${p.currentStock.toInt()} ${p.unitName ?? "Pcs"}',
                                              style: TextStyle(
                                                fontSize: 12,
                                                fontWeight: FontWeight.bold,
                                                color: isOut
                                                    ? AppTheme.errorColor
                                                    : isLow
                                                        ? Colors.orange
                                                        : AppTheme.successColor,
                                              ),
                                            ),
                                            if (isLow)
                                              Container(
                                                margin: const EdgeInsets.only(left: 6),
                                                padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                                                decoration: BoxDecoration(
                                                  color: Colors.orange.withOpacity(0.12),
                                                  borderRadius: BorderRadius.circular(4),
                                                ),
                                                child: const Text(
                                                  'LOW',
                                                  style: TextStyle(color: Colors.orange, fontSize: 9, fontWeight: FontWeight.bold),
                                                ),
                                              ),
                                          ],
                                        ),
                                      ],
                                    ),
                                  ),

                                  // Price & Edit Action
                                  Column(
                                    crossAxisAlignment: CrossAxisAlignment.end,
                                    children: [
                                      Text(
                                        AppFormatters.formatCurrency(p.sellingPrice, symbol: sym),
                                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: AppTheme.primaryColor),
                                      ),
                                      IconButton(
                                        icon: const Icon(Icons.edit_outlined, size: 20, color: AppTheme.textMuted),
                                        onPressed: () => context.push('/products/edit/${p.id}'),
                                      ),
                                    ],
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

