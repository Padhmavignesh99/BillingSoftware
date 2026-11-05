import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/customer_model.dart';
import '../../providers/customer_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';

class CustomersScreen extends StatefulWidget {
  const CustomersScreen({super.key});

  @override
  State<CustomersScreen> createState() => _CustomersScreenState();
}

class _CustomersScreenState extends State<CustomersScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<CustomerProvider>().fetchCustomers();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _showAddCustomerDialog() {
    final nameCtrl = TextEditingController();
    final phoneCtrl = TextEditingController();
    final emailCtrl = TextEditingController();
    final cityCtrl = TextEditingController();
    final creditLimitCtrl = TextEditingController(text: '10000');
    final formKey = GlobalKey<FormState>();

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Add New Customer'),
        content: SingleChildScrollView(
          child: Form(
            key: formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                AppTextField(
                  controller: nameCtrl,
                  label: 'Customer Name *',
                  hint: 'e.g. Ramesh Gupta',
                  validator: (v) => v == null || v.isEmpty ? 'Name required' : null,
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
                  controller: emailCtrl,
                  label: 'Email (Optional)',
                  hint: 'customer@example.com',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: cityCtrl,
                  label: 'City',
                  hint: 'e.g. Mumbai',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: creditLimitCtrl,
                  label: 'Credit Limit (₹)',
                  hint: '10000',
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
                await context.read<CustomerProvider>().createCustomer(
                      name: nameCtrl.text.trim(),
                      phone: phoneCtrl.text.trim(),
                      email: emailCtrl.text.trim(),
                      city: cityCtrl.text.trim(),
                      creditLimit: double.tryParse(creditLimitCtrl.text) ?? 10000.0,
                    );
                if (ctx.mounted) Navigator.pop(ctx);
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Customer added successfully')),
                  );
                }
              }
            },
            child: const Text('Save Customer'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final custProvider = context.watch<CustomerProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final customers = custProvider.customers;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Customer Directory'),
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.primaryColor,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.person_add),
        label: const Text('Add Customer'),
        onPressed: _showAddCustomerDialog,
      ),
      body: Column(
        children: [
          // Search Bar
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: TextField(
              controller: _searchCtrl,
              onChanged: (v) => custProvider.setSearchQuery(v),
              decoration: InputDecoration(
                hintText: 'Search by customer name, phone or city...',
                prefixIcon: const Icon(Icons.search, size: 20),
                contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                suffixIcon: _searchCtrl.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear, size: 18),
                        onPressed: () {
                          _searchCtrl.clear();
                          custProvider.setSearchQuery('');
                        },
                      )
                    : null,
              ),
            ),
          ),
          const Divider(height: 1),

          // Customers List
          Expanded(
            child: custProvider.isLoading
                ? const LoadingWidget(message: 'Loading customers...')
                : customers.isEmpty
                    ? EmptyState(
                        icon: Icons.people_outline,
                        title: 'No Customers Found',
                        message: 'Add customers to maintain store ledgers and credit bills.',
                        buttonText: 'Add Customer',
                        onButtonPressed: _showAddCustomerDialog,
                      )
                    : RefreshIndicator(
                        onRefresh: () => custProvider.fetchCustomers(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: customers.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final c = customers[i];
                            final hasDue = c.outstandingBalance > 0;

                            return InkWell(
                              onTap: () => context.push('/customers/${c.id}/ledger'),
                              borderRadius: BorderRadius.circular(12),
                              child: Container(
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
                                      backgroundColor: AppTheme.primaryColor.withOpacity(0.1),
                                      child: Text(
                                        c.name.isNotEmpty ? c.name[0].toUpperCase() : 'C',
                                        style: const TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 18,
                                          color: AppTheme.primaryColor,
                                        ),
                                      ),
                                    ),
                                    const SizedBox(width: 14),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            c.name,
                                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                                          ),
                                          const SizedBox(height: 2),
                                          Text(
                                            '${c.phone ?? "No phone"} • ${c.city ?? "General"}',
                                            style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                                          ),
                                          const SizedBox(height: 4),
                                          Text(
                                            'Purchases: ${c.totalPurchaseCount} orders (${AppFormatters.formatCurrency(c.totalPurchaseAmount, symbol: sym)})',
                                            style: const TextStyle(fontSize: 11, color: AppTheme.textDark),
                                          ),
                                        ],
                                      ),
                                    ),
                                    Column(
                                      crossAxisAlignment: CrossAxisAlignment.end,
                                      children: [
                                        if (hasDue) ...[
                                          Text(
                                            AppFormatters.formatCurrency(c.outstandingBalance, symbol: sym),
                                            style: const TextStyle(
                                              fontWeight: FontWeight.bold,
                                              fontSize: 15,
                                              color: AppTheme.errorColor,
                                            ),
                                          ),
                                          const Text(
                                            'Outstanding Due',
                                            style: TextStyle(fontSize: 11, color: AppTheme.errorColor, fontWeight: FontWeight.w500),
                                          ),
                                        ] else ...[
                                          const Text(
                                            'Settled',
                                            style: TextStyle(
                                              color: AppTheme.successColor,
                                              fontWeight: FontWeight.bold,
                                              fontSize: 13,
                                            ),
                                          ),
                                        ],
                                        const SizedBox(height: 6),
                                        Row(
                                          mainAxisSize: MainAxisSize.min,
                                          children: const [
                                            Text('Ledger', style: TextStyle(fontSize: 12, color: AppTheme.primaryColor, fontWeight: FontWeight.bold)),
                                            Icon(Icons.chevron_right, size: 16, color: AppTheme.primaryColor),
                                          ],
                                        ),
                                      ],
                                    ),
                                  ],
                                ),
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

