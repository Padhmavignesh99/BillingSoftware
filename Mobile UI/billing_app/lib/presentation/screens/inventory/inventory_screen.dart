import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/report_model.dart';
import '../../providers/inventory_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';
import '../../widgets/common/stat_card.dart';

class InventoryScreen extends StatefulWidget {
  const InventoryScreen({super.key});

  @override
  State<InventoryScreen> createState() => _InventoryScreenState();
}

class _InventoryScreenState extends State<InventoryScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<InventoryProvider>().fetchInventory();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _showAdjustStockDialog(StockItemResponse item) {
    final qtyCtrl = TextEditingController();
    final reasonCtrl = TextEditingController(text: 'Physical stock reconciliation');
    bool isAddition = true;

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (context, setDlgState) => AlertDialog(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          title: Text('Adjust Stock: ${item.productName}'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Current Stock: ${item.currentStock.toInt()} units', style: const TextStyle(color: AppTheme.textMuted)),
              const SizedBox(height: 16),
              Row(
                children: [
                  Expanded(
                    child: ChoiceChip(
                      label: const Center(child: Text('+ Add Stock')),
                      selected: isAddition,
                      selectedColor: AppTheme.successColor.withOpacity(0.15),
                      onSelected: (v) => setDlgState(() => isAddition = true),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: ChoiceChip(
                      label: const Center(child: Text('- Remove Stock')),
                      selected: !isAddition,
                      selectedColor: AppTheme.errorColor.withOpacity(0.15),
                      onSelected: (v) => setDlgState(() => isAddition = false),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              TextField(
                controller: qtyCtrl,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(labelText: 'Quantity to adjust', hintText: 'e.g. 10'),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: reasonCtrl,
                decoration: const InputDecoration(labelText: 'Reason for adjustment'),
              ),
            ],
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Cancel')),
            ElevatedButton(
              onPressed: () async {
                final qty = double.tryParse(qtyCtrl.text);
                if (qty != null && qty > 0) {
                  final finalAdjustment = isAddition ? qty : -qty;
                  await context.read<InventoryProvider>().adjustStock(item.productId, finalAdjustment, reasonCtrl.text);
                  if (ctx.mounted) Navigator.pop(ctx);
                  if (mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text('Stock level adjusted successfully')),
                    );
                  }
                }
              },
              child: const Text('Save Adjustment'),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final invProvider = context.watch<InventoryProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final report = invProvider.stockReport;
    final items = invProvider.filteredItems;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Inventory & Stock Levels'),
      ),
      body: Column(
        children: [
          // KPI Metric Header
          Container(
            color: Colors.white,
            padding: const EdgeInsets.all(16),
            child: Column(
              children: [
                Row(
                  children: [
                    Expanded(
                      child: StatCard(
                        title: 'Total Stock Value',
                        value: AppFormatters.formatCurrency(report.totalStockValue, symbol: sym),
                        icon: Icons.account_balance_wallet,
                        color: AppTheme.primaryColor,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: StatCard(
                        title: 'Low Stock Alerts',
                        value: '${report.lowStockProducts} items',
                        icon: Icons.warning_amber_rounded,
                        color: AppTheme.errorColor,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 14),
                TextField(
                  controller: _searchCtrl,
                  onChanged: (v) => invProvider.setSearchQuery(v),
                  decoration: InputDecoration(
                    hintText: 'Search items by name or SKU...',
                    prefixIcon: const Icon(Icons.search, size: 20),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                    suffixIcon: _searchCtrl.text.isNotEmpty
                        ? IconButton(
                            icon: const Icon(Icons.clear, size: 18),
                            onPressed: () {
                              _searchCtrl.clear();
                              invProvider.setSearchQuery('');
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
                    children: ['All', 'Low Stock', 'Out of Stock', 'In Stock'].map((st) {
                      final isSel = invProvider.filterStatus == st;
                      return Padding(
                        padding: const EdgeInsets.only(right: 8),
                        child: ChoiceChip(
                          label: Text(st),
                          selected: isSel,
                          selectedColor: AppTheme.primaryColor.withOpacity(0.12),
                          onSelected: (_) => invProvider.setFilterStatus(st),
                        ),
                      );
                    }).toList(),
                  ),
                ),
              ],
            ),
          ),
          const Divider(height: 1),

          // Stock Items List
          Expanded(
            child: invProvider.isLoading
                ? const LoadingWidget(message: 'Updating stock levels...')
                : items.isEmpty
                    ? const EmptyState(
                        icon: Icons.inventory_2_outlined,
                        title: 'No Items Found',
                        message: 'All inventory items are stocked or matched search is empty.',
                      )
                    : RefreshIndicator(
                        onRefresh: () => invProvider.fetchInventory(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: items.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final item = items[i];
                            final statusColor = AppFormatters.getStatusColor(item.stockStatus);

                            return Container(
                              padding: const EdgeInsets.all(14),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: AppTheme.borderLight),
                              ),
                              child: Row(
                                children: [
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          item.productName,
                                          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                                        ),
                                        const SizedBox(height: 2),
                                        Text(
                                          'SKU: ${item.sku ?? "-"} • Purchase: ${AppFormatters.formatCurrency(item.purchasePrice, symbol: sym)}',
                                          style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                                        ),
                                        const SizedBox(height: 6),
                                        Row(
                                          children: [
                                            Container(
                                              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                                              decoration: BoxDecoration(
                                                color: statusColor.withOpacity(0.12),
                                                borderRadius: BorderRadius.circular(4),
                                              ),
                                              child: Text(
                                                item.stockStatus,
                                                style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: statusColor),
                                              ),
                                            ),
                                            const SizedBox(width: 10),
                                            Text(
                                              'Valuation: ${AppFormatters.formatCurrency(item.stockValue, symbol: sym)}',
                                              style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w500),
                                            ),
                                          ],
                                        ),
                                      ],
                                    ),
                                  ),
                                  Column(
                                    crossAxisAlignment: CrossAxisAlignment.end,
                                    children: [
                                      Text(
                                        '${item.currentStock.toInt()} Units',
                                        style: TextStyle(
                                          fontSize: 16,
                                          fontWeight: FontWeight.bold,
                                          color: statusColor,
                                        ),
                                      ),
                                      const SizedBox(height: 4),
                                      OutlinedButton(
                                        style: OutlinedButton.styleFrom(
                                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                          minimumSize: Size.zero,
                                          tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                                        ),
                                        onPressed: () => _showAdjustStockDialog(item),
                                        child: const Text('Adjust', style: TextStyle(fontSize: 11)),
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

