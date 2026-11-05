class CreditBillResponse {
  final int id;
  final String creditBillNumber;
  final int customerId;
  final String customerName;
  final String? customerPhone;
  final int? invoiceId;
  final String? invoiceNumber;
  final int? userId;
  final double amount;
  final double settledAmount;
  final double balanceAmount;
  final DateTime dueDate;
  final String status;
  final String? notes;
  final DateTime createdAt;

  CreditBillResponse({
    required this.id,
    required this.creditBillNumber,
    required this.customerId,
    required this.customerName,
    this.customerPhone,
    this.invoiceId,
    this.invoiceNumber,
    this.userId,
    required this.amount,
    this.settledAmount = 0.0,
    required this.balanceAmount,
    required this.dueDate,
    this.status = 'Pending',
    this.notes,
    required this.createdAt,
  });

  bool get isOverdue =>
      balanceAmount > 0 && dueDate.isBefore(DateTime.now());

  factory CreditBillResponse.fromJson(Map<String, dynamic> json) {
    return CreditBillResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      creditBillNumber: json['creditBillNumber'] ?? json['CreditBillNumber'] ?? '',
      customerId: json['customerId'] ?? json['CustomerId'] ?? 0,
      customerName: json['customerName'] ?? json['CustomerName'] ?? '',
      customerPhone: json['customerPhone'] ?? json['CustomerPhone'],
      invoiceId: json['invoiceId'] ?? json['InvoiceId'],
      invoiceNumber: json['invoiceNumber'] ?? json['InvoiceNumber'],
      userId: json['userId'] ?? json['UserId'],
      amount: ((json['amount'] ?? json['Amount'] ?? 0) as num).toDouble(),
      settledAmount: ((json['settledAmount'] ?? json['SettledAmount'] ?? 0) as num).toDouble(),
      balanceAmount: ((json['balanceAmount'] ?? json['BalanceAmount'] ?? 0) as num).toDouble(),
      dueDate: json['dueDate'] != null || json['DueDate'] != null
          ? DateTime.tryParse(json['dueDate'] ?? json['DueDate'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      status: json['status'] ?? json['Status'] ?? 'Pending',
      notes: json['notes'] ?? json['Notes'],
      createdAt: json['createdAt'] != null || json['CreatedAt'] != null
          ? DateTime.tryParse(json['createdAt'] ?? json['CreatedAt'] ?? '') ?? DateTime.now()
          : DateTime.now(),
    );
  }
}

