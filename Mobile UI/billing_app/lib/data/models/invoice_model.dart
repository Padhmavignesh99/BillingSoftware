class InvoiceResponse {
  final int id;
  final int? customerId;
  final String invoiceNumber;
  final String? customerName;
  final String? customerPhone;
  final String? customerEmail;
  final DateTime invoiceDate;
  final DateTime? dueDate;
  final double subTotal;
  final double taxAmount;
  final double discountAmount;
  final double roundOff;
  final double totalAmount;
  final double paidAmount;
  final double dueAmount;
  final String status;
  final String? paymentMethodName;
  final String? notes;
  final String? pdfUrl;
  final List<InvoiceItemResponse> items;
  final List<PaymentResponse> payments;

  InvoiceResponse({
    required this.id,
    this.customerId,
    required this.invoiceNumber,
    this.customerName,
    this.customerPhone,
    this.customerEmail,
    required this.invoiceDate,
    this.dueDate,
    this.subTotal = 0.0,
    this.taxAmount = 0.0,
    this.discountAmount = 0.0,
    this.roundOff = 0.0,
    required this.totalAmount,
    this.paidAmount = 0.0,
    this.dueAmount = 0.0,
    this.status = 'Paid',
    this.paymentMethodName,
    this.notes,
    this.pdfUrl,
    this.items = const [],
    this.payments = const [],
  });

  factory InvoiceResponse.fromJson(Map<String, dynamic> json) {
    return InvoiceResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      customerId: json['customerId'] ?? json['CustomerId'],
      invoiceNumber: json['invoiceNumber'] ?? json['InvoiceNumber'] ?? '',
      customerName: json['customerName'] ?? json['CustomerName'],
      customerPhone: json['customerPhone'] ?? json['CustomerPhone'],
      customerEmail: json['customerEmail'] ?? json['CustomerEmail'],
      invoiceDate: json['invoiceDate'] != null || json['InvoiceDate'] != null
          ? DateTime.tryParse(json['invoiceDate'] ?? json['InvoiceDate'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      dueDate: json['dueDate'] != null || json['DueDate'] != null
          ? DateTime.tryParse(json['dueDate'] ?? json['DueDate'] ?? '')
          : null,
      subTotal: ((json['subTotal'] ?? json['SubTotal'] ?? 0) as num).toDouble(),
      taxAmount: ((json['taxAmount'] ?? json['TaxAmount'] ?? 0) as num).toDouble(),
      discountAmount: ((json['discountAmount'] ?? json['DiscountAmount'] ?? 0) as num).toDouble(),
      roundOff: ((json['roundOff'] ?? json['RoundOff'] ?? 0) as num).toDouble(),
      totalAmount: ((json['totalAmount'] ?? json['TotalAmount'] ?? 0) as num).toDouble(),
      paidAmount: ((json['paidAmount'] ?? json['PaidAmount'] ?? 0) as num).toDouble(),
      dueAmount: ((json['dueAmount'] ?? json['DueAmount'] ?? 0) as num).toDouble(),
      status: json['status'] ?? json['Status'] ?? 'Paid',
      paymentMethodName: json['paymentMethodName'] ?? json['PaymentMethodName'],
      notes: json['notes'] ?? json['Notes'],
      pdfUrl: json['pdfUrl'] ?? json['PdfUrl'],
      items: (json['items'] ?? json['Items'] as List<dynamic>?)
              ?.map((e) => InvoiceItemResponse.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      payments: (json['payments'] ?? json['Payments'] as List<dynamic>?)
              ?.map((e) => PaymentResponse.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class InvoiceItemResponse {
  final int id;
  final int productId;
  final String productName;
  final String? hsnCode;
  final double quantity;
  final double unitPrice;
  final double discountAmount;
  final double taxAmount;
  final double? taxPercentage;
  final double totalPrice;

  InvoiceItemResponse({
    this.id = 0,
    required this.productId,
    required this.productName,
    this.hsnCode,
    required this.quantity,
    required this.unitPrice,
    this.discountAmount = 0.0,
    this.taxAmount = 0.0,
    this.taxPercentage,
    required this.totalPrice,
  });

  factory InvoiceItemResponse.fromJson(Map<String, dynamic> json) {
    return InvoiceItemResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      productId: json['productId'] ?? json['ProductId'] ?? 0,
      productName: json['productName'] ?? json['ProductName'] ?? '',
      hsnCode: json['hsnCode'] ?? json['HSNCode'],
      quantity: ((json['quantity'] ?? json['Quantity'] ?? 1) as num).toDouble(),
      unitPrice: ((json['unitPrice'] ?? json['UnitPrice'] ?? 0) as num).toDouble(),
      discountAmount: ((json['discountAmount'] ?? json['DiscountAmount'] ?? 0) as num).toDouble(),
      taxAmount: ((json['taxAmount'] ?? json['TaxAmount'] ?? 0) as num).toDouble(),
      taxPercentage: json['taxPercentage'] != null || json['TaxPercentage'] != null
          ? ((json['taxPercentage'] ?? json['TaxPercentage']) as num).toDouble()
          : null,
      totalPrice: ((json['totalPrice'] ?? json['TotalPrice'] ?? 0) as num).toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'productId': productId,
      'quantity': quantity,
      'unitPrice': unitPrice,
      'discountAmount': discountAmount,
      'taxAmount': taxAmount,
      'totalPrice': totalPrice,
    };
  }
}

class PaymentResponse {
  final int id;
  final int? invoiceId;
  final String? invoiceNumber;
  final String? customerName;
  final double amount;
  final String paymentMethod;
  final String status;
  final DateTime paymentDate;
  final String? referenceNumber;
  final String? transactionId;

  PaymentResponse({
    required this.id,
    this.invoiceId,
    this.invoiceNumber,
    this.customerName,
    required this.amount,
    required this.paymentMethod,
    this.status = 'Completed',
    required this.paymentDate,
    this.referenceNumber,
    this.transactionId,
  });

  factory PaymentResponse.fromJson(Map<String, dynamic> json) {
    return PaymentResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      invoiceId: json['invoiceId'] ?? json['InvoiceId'],
      invoiceNumber: json['invoiceNumber'] ?? json['InvoiceNumber'],
      customerName: json['customerName'] ?? json['CustomerName'],
      amount: ((json['amount'] ?? json['Amount'] ?? 0) as num).toDouble(),
      paymentMethod: json['paymentMethod'] ?? json['PaymentMethod'] ?? 'Cash',
      status: json['status'] ?? json['Status'] ?? 'Completed',
      paymentDate: json['paymentDate'] != null || json['PaymentDate'] != null
          ? DateTime.tryParse(json['paymentDate'] ?? json['PaymentDate'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      referenceNumber: json['referenceNumber'] ?? json['ReferenceNumber'],
      transactionId: json['transactionId'] ?? json['TransactionId'],
    );
  }
}

class POSBillRequest {
  final int? customerId;
  final String? customerName;
  final String? customerPhone;
  final List<InvoiceItemResponse> items;
  final double discountAmount;
  final String paymentMethod;
  final double paidAmount;
  final String? notes;

  POSBillRequest({
    this.customerId,
    this.customerName,
    this.customerPhone,
    required this.items,
    this.discountAmount = 0.0,
    required this.paymentMethod,
    required this.paidAmount,
    this.notes,
  });

  Map<String, dynamic> toJson() {
    return {
      'customerId': customerId,
      'customerName': customerName,
      'customerPhone': customerPhone,
      'items': items.map((i) => i.toJson()).toList(),
      'discountAmount': discountAmount,
      'paymentMethod': paymentMethod,
      'paidAmount': paidAmount,
      'notes': notes,
    };
  }
}

