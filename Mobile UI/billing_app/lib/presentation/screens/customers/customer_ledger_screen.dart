import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/constants/app_constants.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../providers/customer_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';
import '../../widgets/common/loading_widget.dart';

class CustomerLedgerScreen extends StatefulWidget {
  final int customerId;

  const CustomerLedgerScreen({super.key, required this.customerId});

  @override
  State<CustomerLedgerScreen> createState() => _CustomerLedgerScreenState();
}

class _CustomerLedgerScreenState extends State<CustomerLedgerScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<CustomerProvider>().fetchCustomerLedger(widget.customerId);
    });
  }

  void _showRecordPaymentDialog() {
    final amountCtrl = TextEditingController();
    final refCtrl = TextEditingController();
    final notesCtrl = TextEditingController();
    String selectedMethod = 'Cash';

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
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
                      const Text('Receive Payment', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                      IconButton(icon: const Icon(Icons.close), onPressed: () => Navigator.pop(ctx)),
                    ],
                  ),
                  const Divider(),
                  const SizedBox(height: 12),
                  AppTextField(
                    controller: amountCtrl,
                    label: 'Payment Amount Received (₹) *',
                    hint: '0.00',
                    keyboardType: TextInputType.number,
                  ),
                  const SizedBox(height: 14),
                  const Text('Payment Mode', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  const SizedBox(height: 8),
                  Wrap(
                    spacing: 8,
                    children: ['Cash', 'UPI', 'Bank Transfer', 'Cheque'].map((m) {
                      final isSel = selectedMethod == m;
                      return ChoiceChip(
                        label: Text(m),
                        selected: isSel,
                        onSelected: (val) {
                          if (val) setModalState(() => selectedMethod = m);
                        },
                      );
                    }).toList(),
                  ),
                  const SizedBox(height: 14),
                  AppTextField(
                    controller: refCtrl,
                    label: 'Reference / Transaction ID',
                    hint: 'UPI-12345 or Cheque #',
                  ),
                  const SizedBox(height: 14),
                  AppTextField(
                    controller: notesCtrl,
                    label: 'Notes',
                    hint: 'e.g. Ledger clearing against past invoices',
                  ),
                  const SizedBox(height: 20),
                  AppButton(
                    label: 'Record Payment & Update Ledger',
                    onPressed: () async {
                      final amt = double.tryParse(amountCtrl.text);
                      if (amt != null && amt > 0) {
                        await context.read<CustomerProvider>().recordPayment(
                              customerId: widget.customerId,
                              amount: amt,
                              paymentMethod: selectedMethod,
                              referenceNumber: refCtrl.text.trim().isEmpty ? null : refCtrl.text.trim(),
                              notes: notesCtrl.text.trim().isEmpty ? null : notesCtrl.text.trim(),
                            );
                        if (ctx.mounted) Navigator.pop(ctx);
                        if (mounted) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text('Payment recorded to ledger successfully!')),
                          );
                        }
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

  @override
  Widget build(BuildContext context) {
    final custProvider = context.watch<CustomerProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final customer = custProvider.selectedCustomer;
    final payments = custProvider.selectedCustomerPayments;

    if (custProvider.isLoading && customer == null) {
      return const Scaffold(body: LoadingWidget(message: 'Loading ledger history...'));
    }

    return Scaffold(
      appBar: AppBar(
        title: Text('${customer?.name ?? "Customer"} Ledger'),
      ),
      bottomNavigationBar: Container(
        padding: const EdgeInsets.all(16),
        decoration: const BoxDecoration(
          color: Colors.white,
          border: Border(top: BorderSide(color: AppTheme.borderLight)),
        ),
        child: SafeArea(
          child: AppButton(
            label: 'Receive Payment / Settle Balance',
            icon: Icons.payments_outlined,
            onPressed: _showRecordPaymentDialog,
          ),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Customer Header Card
            Container(
              padding: const EdgeInsets.all(18),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: AppTheme.borderLight),
              ),
              child: Column(
                children: [
                  Row(
                    children: [
                      CircleAvatar(
                        radius: 28,
                        backgroundColor: AppTheme.primaryColor.withOpacity(0.1),
                        child: Text(
                          customer?.name.isNotEmpty == true ? customer!.name[0].toUpperCase() : 'C',
                          style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: AppTheme.primaryColor),
                        ),
                      ),
                      const SizedBox(width: 14),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              customer?.name ?? 'Customer Name',
                              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                            ),
                            Text(
                              '${customer?.phone ?? ""} • ${customer?.city ?? ""}',
                              style: const TextStyle(fontSize: 13, color: AppTheme.textMuted),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                  const Divider(height: 24),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text('Credit Limit', style: TextStyle(fontSize: 11, color: AppTheme.textMuted)),
                          const SizedBox(height: 2),
                          Text(
                            AppFormatters.formatCurrency(customer?.creditLimit ?? 0, symbol: sym),
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                          ),
                        ],
                      ),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: [
                          const Text('Outstanding Due', style: TextStyle(fontSize: 11, color: AppTheme.errorColor)),
                          const SizedBox(height: 2),
                          Text(
                            AppFormatters.formatCurrency(customer?.outstandingBalance ?? 0, symbol: sym),
                            style: const TextStyle(
                              fontWeight: FontWeight.bold,
                              fontSize: 18,
                              color: AppTheme.errorColor,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),

            // Ledger Transaction Entries
            const Text(
              'Payment & Receipt History',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppTheme.textDark),
            ),
            const SizedBox(height: 12),

            payments.isEmpty
                ? Container(
                    padding: const EdgeInsets.all(32),
                    alignment: Alignment.center,
                    child: const Text('No ledger entries recorded yet.'),
                  )
                : ListView.separated(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: payments.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 10),
                    itemBuilder: (ctx, i) {
                      final p = payments[i];
                      return Container(
                        padding: const EdgeInsets.all(14),
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(12),
                          border: Border.all(color: AppTheme.borderLight),
                        ),
                        child: Row(
                          children: [
                            Container(
                              padding: const EdgeInsets.all(10),
                              decoration: BoxDecoration(
                                color: AppTheme.successColor.withOpacity(0.12),
                                shape: BoxShape.circle,
                              ),
                              child: const Icon(Icons.arrow_downward, color: AppTheme.successColor, size: 20),
                            ),
                            const SizedBox(width: 14),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    'Payment Received (${p.paymentMethod})',
                                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                                  ),
                                  const SizedBox(height: 2),
                                  Text(
                                    AppFormatters.formatDateTime(p.paymentDate),
                                    style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                                  ),
                                  if (p.referenceNumber != null) ...[
                                    const SizedBox(height: 2),
                                    Text(
                                      'Ref: ${p.referenceNumber}',
                                      style: const TextStyle(fontSize: 11, color: AppTheme.textMuted),
                                    ),
                                  ],
                                ],
                              ),
                            ),
                            Text(
                              '+ ${AppFormatters.formatCurrency(p.amount, symbol: sym)}',
                              style: const TextStyle(
                                fontWeight: FontWeight.bold,
                                fontSize: 15,
                                color: AppTheme.successColor,
                              ),
                            ),
                          ],
                        ),
                      );
                    },
                  ),
            const SizedBox(height: 24),
          ],
        ),
      ),
    );
  }
}

