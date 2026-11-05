import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/staff_model.dart';
import '../../providers/settings_provider.dart';
import '../../providers/staff_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/empty_state.dart';
import '../../widgets/common/loading_widget.dart';

class StaffScreen extends StatefulWidget {
  const StaffScreen({super.key});

  @override
  State<StaffScreen> createState() => _StaffScreenState();
}

class _StaffScreenState extends State<StaffScreen> {
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<StaffProvider>().fetchStaff();
    });
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _showAddStaffDialog() {
    final nameCtrl = TextEditingController();
    final emailCtrl = TextEditingController();
    final phoneCtrl = TextEditingController();
    final deptCtrl = TextEditingController(text: 'Billing & POS');
    final desigCtrl = TextEditingController(text: 'Billing Operator');
    final salaryCtrl = TextEditingController(text: '22000');
    String selectedRole = 'Cashier';

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
                    const Text('Add Staff Member', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                    IconButton(icon: const Icon(Icons.close), onPressed: () => Navigator.pop(ctx)),
                  ],
                ),
                const Divider(),
                const SizedBox(height: 12),
                AppTextField(controller: nameCtrl, label: 'Full Name *', hint: 'e.g. Kiran Kumar'),
                const SizedBox(height: 12),
                AppTextField(controller: emailCtrl, label: 'Work Email *', hint: 'kiran@store.com', keyboardType: TextInputType.emailAddress),
                const SizedBox(height: 12),
                AppTextField(controller: phoneCtrl, label: 'Phone Number', hint: '9845012345', keyboardType: TextInputType.phone),
                const SizedBox(height: 12),
                const Text('Role & Permissions', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                const SizedBox(height: 6),
                DropdownButtonFormField<String>(
                  value: selectedRole,
                  items: ['Admin', 'Manager', 'Cashier', 'Inventory Manager', 'Accountant'].map((r) {
                    return DropdownMenuItem(value: r, child: Text(r));
                  }).toList(),
                  onChanged: (v) {
                    if (v != null) setModalState(() => selectedRole = v);
                  },
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(child: AppTextField(controller: deptCtrl, label: 'Department', hint: 'POS')),
                    const SizedBox(width: 12),
                    Expanded(child: AppTextField(controller: desigCtrl, label: 'Designation', hint: 'Cashier')),
                  ],
                ),
                const SizedBox(height: 12),
                AppTextField(controller: salaryCtrl, label: 'Monthly Salary (₹)', hint: '25000', keyboardType: TextInputType.number),
                const SizedBox(height: 20),
                AppButton(
                  label: 'Save Staff Member',
                  onPressed: () async {
                    if (nameCtrl.text.isNotEmpty && emailCtrl.text.isNotEmpty) {
                      await context.read<StaffProvider>().addStaff(
                            fullName: nameCtrl.text.trim(),
                            email: emailCtrl.text.trim(),
                            phone: phoneCtrl.text.trim(),
                            role: selectedRole,
                            department: deptCtrl.text.trim(),
                            designation: desigCtrl.text.trim(),
                            salary: double.tryParse(salaryCtrl.text),
                          );
                      if (ctx.mounted) Navigator.pop(ctx);
                      if (mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Staff member added successfully')),
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
    final staffProvider = context.watch<StaffProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final staffList = staffProvider.staffList;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Staff & Employees'),
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.primaryColor,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.person_add),
        label: const Text('Add Staff'),
        onPressed: _showAddStaffDialog,
      ),
      body: Column(
        children: [
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: TextField(
              controller: _searchCtrl,
              onChanged: (v) => staffProvider.setSearchQuery(v),
              decoration: InputDecoration(
                hintText: 'Search staff by name, role or department...',
                prefixIcon: const Icon(Icons.search, size: 20),
                contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                suffixIcon: _searchCtrl.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear, size: 18),
                        onPressed: () {
                          _searchCtrl.clear();
                          staffProvider.setSearchQuery('');
                        },
                      )
                    : null,
              ),
            ),
          ),
          const Divider(height: 1),
          Expanded(
            child: staffProvider.isLoading
                ? const LoadingWidget(message: 'Loading staff members...')
                : staffList.isEmpty
                    ? EmptyState(
                        icon: Icons.badge_outlined,
                        title: 'No Staff Members',
                        message: 'Add store cashiers, operators and managers.',
                        buttonText: 'Add Staff Member',
                        onButtonPressed: _showAddStaffDialog,
                      )
                    : RefreshIndicator(
                        onRefresh: () => staffProvider.fetchStaff(),
                        child: ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: staffList.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 10),
                          itemBuilder: (ctx, i) {
                            final s = staffList[i];

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
                                    backgroundColor: AppTheme.primaryColor.withOpacity(0.1),
                                    child: Text(
                                      s.user.fullName.isNotEmpty ? s.user.fullName[0].toUpperCase() : 'S',
                                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: AppTheme.primaryColor),
                                    ),
                                  ),
                                  const SizedBox(width: 14),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          s.user.fullName,
                                          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                                        ),
                                        const SizedBox(height: 2),
                                        Text(
                                          '${s.designation ?? "Staff"} • ${s.department ?? "General"}',
                                          style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                                        ),
                                        const SizedBox(height: 4),
                                        Text(
                                          '${s.user.email} • ${s.user.phone ?? ""}',
                                          style: const TextStyle(fontSize: 11, color: AppTheme.textDark),
                                        ),
                                      ],
                                    ),
                                  ),
                                  Column(
                                    crossAxisAlignment: CrossAxisAlignment.end,
                                    children: [
                                      Container(
                                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                        decoration: BoxDecoration(
                                          color: AppTheme.primaryColor.withOpacity(0.1),
                                          borderRadius: BorderRadius.circular(12),
                                        ),
                                        child: Text(
                                          s.user.role,
                                          style: const TextStyle(
                                            color: AppTheme.primaryColor,
                                            fontWeight: FontWeight.bold,
                                            fontSize: 11,
                                          ),
                                        ),
                                      ),
                                      if (s.salary != null) ...[
                                        const SizedBox(height: 4),
                                        Text(
                                          AppFormatters.formatCurrency(s.salary!, symbol: sym),
                                          style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600),
                                        ),
                                      ],
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

