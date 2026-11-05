class ExpenseResponse {
  final int id;
  final String? expenseNumber;
  final String? categoryName;
  final double amount;
  final DateTime expenseDate;
  final String? description;
  final String? paymentMethodName;
  final String? paidTo;
  final String status;

  ExpenseResponse({
    required this.id,
    this.expenseNumber,
    this.categoryName,
    required this.amount,
    required this.expenseDate,
    this.description,
    this.paymentMethodName,
    this.paidTo,
    this.status = 'Approved',
  });

  factory ExpenseResponse.fromJson(Map<String, dynamic> json) {
    return ExpenseResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      expenseNumber: json['expenseNumber'] ?? json['ExpenseNumber'],
      categoryName: json['categoryName'] ?? json['CategoryName'],
      amount: ((json['amount'] ?? json['Amount'] ?? 0) as num).toDouble(),
      expenseDate: json['expenseDate'] != null || json['ExpenseDate'] != null
          ? DateTime.tryParse(json['expenseDate'] ?? json['ExpenseDate'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      description: json['description'] ?? json['Description'],
      paymentMethodName: json['paymentMethodName'] ?? json['PaymentMethodName'],
      paidTo: json['paidTo'] ?? json['PaidTo'],
      status: json['status'] ?? json['Status'] ?? 'Approved',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'categoryName': categoryName,
      'amount': amount,
      'expenseDate': expenseDate.toIso8601String(),
      'description': description,
      'paymentMethodName': paymentMethodName,
      'paidTo': paidTo,
      'status': status,
    };
  }
}

