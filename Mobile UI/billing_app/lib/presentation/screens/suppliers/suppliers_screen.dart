import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/supplier_model.dart';
import '../../providers/settings_provider.dart';
import '../../providers/supplier_provider.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';

class SuppliersScreen extends StatefulWidget {
  const SuppliersScreen({super.key});

  @override
  State<SuppliersScreen> createState() => _SuppliersScreenState();
}

class _SuppliersScreenState extends State<SuppliersScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<SupplierProvider>().fetchSuppliers();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _showAddSupplierDialog() {
    final nameCtrl = TextEditingController();
    final companyCtrl = TextEditingController();
    final phoneCtrl = TextEditingController();
    final emailCtrl = TextEditingController();
    final cityCtrl = TextEditingController();
    final gstCtrl = TextEditingController();
    final daysCtrl = TextEditingController(text: '30');
    final formKey = GlobalKey<FormState>();

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Add New Supplier'),
        content: SingleChildScrollView(
          child: Form(
            key: formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                AppTextField(
                  controller: companyCtrl,
                  label: 'Company / Business Name *',
                  hint: 'e.g. Acme Wholesale Ltd',
                  validator: (v) => v == null || v.isEmpty ? 'Required' : null,
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: nameCtrl,
                  label: 'Contact Person Name',
                  hint: 'e.g. Vikas Sharma',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: phoneCtrl,
                  label: 'Phone Number',
                  hint: '9876543210',
                  keyboardType: TextInputType.phone,
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: cityCtrl,
                  label: 'City',
                  hint: 'e.g. Delhi',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: gstCtrl,
                  label: 'GST Number',
                  hint: '07AAAAA0000A1Z5',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: daysCtrl,
                  label: 'Credit Terms (Days)',
                  hint: '30',
                  keyboardType: TextInputType.number,
                ),
              ],
            ),
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Cancel')),
          ElevatedButton(
            onPressed: () async {
              if (formKey.currentState!.validate()) {
                await context.read<SupplierProvider>().createSupplier(
                      name: nameCtrl.text.trim().isEmpty ? companyCtrl.text.trim() : nameCtrl.text.trim(),
                      companyName: companyCtrl.text.trim(),
                      phone: phoneCtrl.text.trim(),
                      email: emailCtrl.text.trim(),
                      city: cityCtrl.text.trim(),
                      gstNumber: gstCtrl.text.trim(),
                      creditDays: int.tryParse(daysCtrl.text) ?? 30,
                    );
                if (ctx.mounted) Navigator.pop(ctx);
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Supplier added successfully')),
                  );
                }
              }
            },
            child: const Text('Save Supplier'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final supProvider = context.watch<SupplierProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final suppliers = supProvider.suppliers;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Suppliers & Vendors'),
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.primaryColor,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.add_business),
        label: const Text('Add Supplier'),
        onPressed: _showAddSupplierDialog,
      ),
      body: Column(
        children: [
          // Search Bar
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: TextField(
              controller: _searchCtrl,
              onChanged: (v) => supProvider.setSearchQuery(v),
              decoration: InputDecoration(
                hintText: 'Search by vendor company or contact person...',
                prefixIcon: const Icon(Icons.search, size: 20),
                contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                suffixIcon: _searchCtrl.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear, size: 18),
                        onPressed: () {
                          _searchCtrl.clear();
                          supProvider.setSearchQuery('');
                        },
                      )
                    : null,
              ),
            ),
          ),
          const Divider(height: 1),

          // Suppliers List
          Expanded(
            child: supProvider.isLoading
                ? const LoadingWidget(message: 'Loading suppliers...')
                : suppliers.isEmpty
                    ? EmptyState(
                        icon: Icons.local_shipping_outlined,
                        title: 'No Suppliers Found',
                        message: 'Add suppliers to track purchases and payables.',
                        buttonText: 'Add Supplier',
                        onButtonPressed: _showAddSupplierDialog,
                      )
                    : RefreshIndicator(
                        onRefresh: () => supProvider.fetchSuppliers(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: suppliers.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final s = suppliers[i];

                            return Container(
                              padding: const EdgeInsets.all(16),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: AppTheme.borderLight),
                              ),
                              child: Row(
                                children: [
                                  CircleAvatar(
                                    radius: 24,
                                    backgroundColor: AppTheme.secondaryColor.withOpacity(0.1),
                                    child: const Icon(Icons.local_shipping, color: AppTheme.secondaryColor),
                                  ),
                                  const SizedBox(width: 14),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          s.companyName ?? s.name,
                                          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                                        ),
                                        const SizedBox(height: 2),
                                        Text(
                                          'Contact: ${s.name} • ${s.phone ?? ""}',
                                          style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                                        ),
                                        const SizedBox(height: 4),
                                        Text(
                                          'Credit Terms: ${s.creditDays} Days',
                                          style: const TextStyle(fontSize: 11, color: AppTheme.textDark),
                                        ),
                                      ],
                                    ),
                                  ),
                                  Column(
                                    crossAxisAlignment: CrossAxisAlignment.end,
                                    children: [
                                      Text(
                                        AppFormatters.formatCurrency(s.outstandingBalance, symbol: sym),
                                        style: TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 15,
                                          color: s.outstandingBalance > 0 ? AppTheme.warningColor : AppTheme.successColor,
                                        ),
                                      ),
                                      Text(
                                        s.outstandingBalance > 0 ? 'Payable Balance' : 'Clear',
                                        style: TextStyle(
                                          fontSize: 11,
                                          color: s.outstandingBalance > 0 ? AppTheme.warningColor : AppTheme.successColor,
                                        ),
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

