import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/invoice_model.dart';
import '../../providers/invoice_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/loading_widget.dart';

class InvoiceDetailScreen extends StatefulWidget {
  final int invoiceId;

  const InvoiceDetailScreen({super.key, required this.invoiceId});

  @override
  State<InvoiceDetailScreen> createState() => _InvoiceDetailScreenState();
}

class _InvoiceDetailScreenState extends State<InvoiceDetailScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<InvoiceProvider>().getInvoiceById(widget.invoiceId);
    });
  }

  @override
  Widget build(BuildContext context) {
    final invoiceProvider = context.watch<InvoiceProvider>();
    final settings = context.watch<SettingsProvider>();
    final sym = settings.currencySymbol;
    final inv = invoiceProvider.selectedInvoice;

    if (inv == null) {
      return const Scaffold(
        body: LoadingWidget(message: 'Loading invoice details...'),
      );
    }

    final statusColor = AppFormatters.getStatusColor(inv.status);

    return Scaffold(
      appBar: AppBar(
        title: Text(inv.invoiceNumber),
        actions: [
          IconButton(
            icon: const Icon(Icons.share_outlined),
            tooltip: 'Share Bill',
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(content: Text('Sharing invoice ${inv.invoiceNumber}...')),
              );
            },
          ),
          IconButton(
            icon: const Icon(Icons.print_outlined),
            tooltip: 'Print Invoice',
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(content: Text('Printing invoice ${inv.invoiceNumber}...')),
              );
            },
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Header Card
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
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            inv.invoiceNumber,
                            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: AppTheme.primaryColor),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            AppFormatters.formatDateTime(inv.invoiceDate),
                            style: const TextStyle(fontSize: 12, color: AppTheme.textMuted),
                          ),
                        ],
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                        decoration: BoxDecoration(
                          color: statusColor.withOpacity(0.12),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Text(
                          inv.status,
                          style: TextStyle(color: statusColor, fontWeight: FontWeight.bold, fontSize: 12),
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
                          const Text('Customer', style: TextStyle(fontSize: 11, color: AppTheme.textMuted)),
                          const SizedBox(height: 2),
                          Text(inv.customerName ?? 'Walk-in Customer', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                          if (inv.customerPhone != null && inv.customerPhone!.isNotEmpty)
                            Text(inv.customerPhone!, style: const TextStyle(fontSize: 12, color: AppTheme.textMuted)),
                        ],
                      ),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: [
                          const Text('Payment Mode', style: TextStyle(fontSize: 11, color: AppTheme.textMuted)),
                          const SizedBox(height: 2),
                          Text(inv.paymentMethodName ?? 'Cash', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                        ],
                      ),
                    ],
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),

            // Itemized Table
            Container(
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: AppTheme.borderLight),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Padding(
                    padding: EdgeInsets.all(16.0),
                    child: Text(
                      'Billed Items',
                      style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
                    ),
                  ),
                  const Divider(height: 1),
                  ListView.separated(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: inv.items.length,
                    separatorBuilder: (_, __) => const Divider(height: 1),
                    itemBuilder: (ctx, idx) {
                      final item = inv.items[idx];
                      return Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                        child: Row(
                          children: [
                            Expanded(
                              flex: 3,
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(item.productName, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                                  if (item.hsnCode != null)
                                    Text('HSN: ${item.hsnCode}', style: const TextStyle(fontSize: 11, color: AppTheme.textMuted)),
                                ],
                              ),
                            ),
                            Expanded(
                              flex: 1,
                              child: Text(
                                '${item.quantity.toInt()}x',
                                textAlign: TextAlign.center,
                                style: const TextStyle(fontSize: 12),
                              ),
                            ),
                            Expanded(
                              flex: 2,
                              child: Text(
                                AppFormatters.formatCurrency(item.totalPrice, symbol: sym),
                                textAlign: TextAlign.end,
                                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                              ),
                            ),
                          ],
                        ),
                      );
                    },
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),

            // Totals Summary
            Container(
              padding: const EdgeInsets.all(18),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: AppTheme.borderLight),
              ),
              child: Column(
                children: [
                  _totalRow('Sub Total', AppFormatters.formatCurrency(inv.subTotal, symbol: sym)),
                  _totalRow('Tax Amount', AppFormatters.formatCurrency(inv.taxAmount, symbol: sym)),
                  if (inv.discountAmount > 0)
                    _totalRow('Discount', '- ${AppFormatters.formatCurrency(inv.discountAmount, symbol: sym)}', isNegative: true),
                  const Divider(height: 20),
                  _totalRow(
                    'Grand Total',
                    AppFormatters.formatCurrency(inv.totalAmount, symbol: sym),
                    isBold: true,
                  ),
                  _totalRow('Paid Amount', AppFormatters.formatCurrency(inv.paidAmount, symbol: sym)),
                  if (inv.dueAmount > 0)
                    _totalRow('Balance Due', AppFormatters.formatCurrency(inv.dueAmount, symbol: sym), isDue: true),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Send notification buttons
            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    icon: const Icon(Icons.email_outlined, size: 18),
                    label: const Text('Send Email'),
                    onPressed: () async {
                      await invoiceProvider.sendInvoiceEmail(inv.id);
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Invoice sent via Email successfully!')),
                        );
                      }
                    },
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: ElevatedButton.icon(
                    icon: const Icon(Icons.chat_bubble_outline, size: 18),
                    label: const Text('WhatsApp Bill'),
                    style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFF25D366)),
                    onPressed: () async {
                      await invoiceProvider.sendInvoiceWhatsApp(inv.id);
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Invoice sent via WhatsApp!')),
                        );
                      }
                    },
                  ),
                ),
              ],
            ),
            const SizedBox(height: 32),
          ],
        ),
      ),
    );
  }

  Widget _totalRow(String label, String value, {bool isBold = false, bool isNegative = false, bool isDue = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              fontSize: isBold ? 15 : 13,
              fontWeight: isBold ? FontWeight.bold : FontWeight.normal,
              color: isDue ? AppTheme.errorColor : isBold ? AppTheme.textDark : AppTheme.textMuted,
            ),
          ),
          Text(
            value,
            style: TextStyle(
              fontSize: isBold ? 17 : 13,
              fontWeight: isBold || isDue ? FontWeight.bold : FontWeight.w600,
              color: isDue
                  ? AppTheme.errorColor
                  : isNegative
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

