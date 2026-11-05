import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../providers/expense_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';

class ExpensesScreen extends StatefulWidget {
  const ExpensesScreen({super.key});

  @override
  State<ExpensesScreen> createState() => _ExpensesScreenState();
}

class _ExpensesScreenState extends State<ExpensesScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<ExpenseProvider>().fetchExpenses();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _showAddExpenseDialog() {
    final expProvider = context.read<ExpenseProvider>();
    final amountCtrl = TextEditingController();
    final descCtrl = TextEditingController();
    final paidToCtrl = TextEditingController();
    String selectedCat = 'Rent & Utilities';
    String selectedMethod = 'Cash';

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
                    const Text('Log Store Expense', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                    IconButton(icon: const Icon(Icons.close), onPressed: () => Navigator.pop(ctx)),
                  ],
                ),
                const Divider(),
                const SizedBox(height: 12),
                AppTextField(
                  controller: amountCtrl,
                  label: 'Expense Amount (₹) *',
                  hint: '0.00',
                  keyboardType: TextInputType.number,
                ),
                const SizedBox(height: 14),
                const Text('Expense Category', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                const SizedBox(height: 6),
                DropdownButtonFormField<String>(
                  value: selectedCat,
                  items: expProvider.categories.where((c) => c != 'All').map((c) {
                    return DropdownMenuItem(value: c, child: Text(c));
                  }).toList(),
                  onChanged: (v) {
                    if (v != null) setModalState(() => selectedCat = v);
                  },
                ),
                const SizedBox(height: 14),
                AppTextField(
                  controller: descCtrl,
                  label: 'Description / Purpose',
                  hint: 'e.g. Monthly internet & electricity bill',
                ),
                const SizedBox(height: 14),
                AppTextField(
                  controller: paidToCtrl,
                  label: 'Paid To',
                  hint: 'e.g. Electricity Board',
                ),
                const SizedBox(height: 20),
                AppButton(
                  label: 'Save Expense',
                  onPressed: () async {
                    final amt = double.tryParse(amountCtrl.text);
                    if (amt != null && amt > 0) {
                      await expProvider.addExpense(
                        categoryName: selectedCat,
                        amount: amt,
                        expenseDate: DateTime.now(),
                        description: descCtrl.text.trim(),
                        paidTo: paidToCtrl.text.trim(),
                        paymentMethodName: selectedMethod,
                      );
                      if (ctx.mounted) Navigator.pop(ctx);
                      if (mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Expense logged successfully')),
                        );
                      }
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
    final expProvider = context.watch<ExpenseProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final expenses = expProvider.expenses;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Expenses & Outflows'),
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.primaryColor,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.add),
        label: const Text('Add Expense'),
        onPressed: _showAddExpenseDialog,
      ),
      body: Column(
        children: [
          // Total Expense Header
          Container(
            color: Colors.white,
            padding: const EdgeInsets.all(16),
            child: Column(
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text('Total Logged Expenses', style: TextStyle(color: AppTheme.textMuted, fontSize: 13)),
                    Text(
                      AppFormatters.formatCurrency(expProvider.totalExpenseAmount, symbol: sym),
                      style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppTheme.errorColor),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _searchCtrl,
                  onChanged: (v) => expProvider.setSearchQuery(v),
                  decoration: InputDecoration(
                    hintText: 'Search expenses by note, vendor or ID...',
                    prefixIcon: const Icon(Icons.search, size: 20),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                    suffixIcon: _searchCtrl.text.isNotEmpty
                        ? IconButton(
                            icon: const Icon(Icons.clear, size: 18),
                            onPressed: () {
                              _searchCtrl.clear();
                              expProvider.setSearchQuery('');
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
                    children: expProvider.categories.map((c) {
                      final isSel = expProvider.selectedCategory == c;
                      return Padding(
                        padding: const EdgeInsets.only(right: 8),
                        child: ChoiceChip(
                          label: Text(c),
                          selected: isSel,
                          selectedColor: AppTheme.primaryColor.withOpacity(0.12),
                          onSelected: (_) => expProvider.setSelectedCategory(c),
                        ),
                      );
                    }).toList(),
                  ),
                ),
              ],
            ),
          ),
          const Divider(height: 1),

          // Expenses List
          Expanded(
            child: expProvider.isLoading
                ? const LoadingWidget(message: 'Loading expenses...')
                : expenses.isEmpty
                    ? EmptyState(
                        icon: Icons.account_balance_wallet_outlined,
                        title: 'No Expenses Recorded',
                        message: 'Log daily business expenditures to calculate accurate profits.',
                        buttonText: 'Add Expense',
                        onButtonPressed: _showAddExpenseDialog,
                      )
                    : RefreshIndicator(
                        onRefresh: () => expProvider.fetchExpenses(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: expenses.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final e = expenses[i];

                            return Container(
                              padding: const EdgeInsets.all(16),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: AppTheme.borderLight),
                              ),
                              child: Row(
                                children: [
                                  Container(
                                    padding: const EdgeInsets.all(12),
                                    decoration: BoxDecoration(
                                      color: AppTheme.errorColor.withOpacity(0.1),
                                      shape: BoxShape.circle,
                                    ),
                                    child: const Icon(Icons.receipt_outlined, color: AppTheme.errorColor, size: 22),
                                  ),
                                  const SizedBox(width: 14),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          e.description ?? e.categoryName ?? 'Expense',
                                          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                                        ),
                                        const SizedBox(height: 2),
                                        Text(
                                          'Category: ${e.categoryName ?? "General"} • Paid to: ${e.paidTo ?? "N/A"}',
                                          style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                                        ),
                                        const SizedBox(height: 4),
                                        Text(
                                          AppFormatters.formatDate(e.expenseDate),
                                          style: const TextStyle(fontSize: 11, color: AppTheme.textMuted),
                                        ),
                                      ],
                                    ),
                                  ),
                                  Text(
                                    AppFormatters.formatCurrency(e.amount, symbol: sym),
                                    style: const TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 16,
                                      color: AppTheme.errorColor,
                                    ),
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

