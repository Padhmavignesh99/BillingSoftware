class ReturnItemResponse {
  final int productId;
  final String productName;
  final double quantity;
  final double unitPrice;
  final double totalPrice;

  ReturnItemResponse({
    required this.productId,
    this.productName = '',
    required this.quantity,
    required this.unitPrice,
    required this.totalPrice,
  });

  factory ReturnItemResponse.fromJson(Map<String, dynamic> json) {
    return ReturnItemResponse(
      productId: json['productId'] ?? json['ProductId'] ?? 0,
      productName: json['productName'] ?? json['ProductName'] ?? '',
      quantity: ((json['quantity'] ?? json['Quantity'] ?? 1) as num).toDouble(),
      unitPrice: ((json['unitPrice'] ?? json['UnitPrice'] ?? 0) as num).toDouble(),
      totalPrice: ((json['totalPrice'] ?? json['TotalPrice'] ?? 0) as num).toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'productId': productId,
      'productName': productName,
      'quantity': quantity,
      'unitPrice': unitPrice,
      'totalPrice': totalPrice,
    };
  }
}

class SalesReturnResponse {
  final int id;
  final String returnNumber;
  final int invoiceId;
  final int? customerId;
  final String? customerName;
  final String reason;
  final DateTime returnDate;
  final double totalAmount;
  final String status;
  final List<ReturnItemResponse> items;

  SalesReturnResponse({
    required this.id,
    required this.returnNumber,
    required this.invoiceId,
    this.customerId,
    this.customerName,
    required this.reason,
    required this.returnDate,
    required this.totalAmount,
    this.status = 'Completed',
    this.items = const [],
  });

  factory SalesReturnResponse.fromJson(Map<String, dynamic> json) {
    return SalesReturnResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      returnNumber: json['returnNumber'] ?? json['ReturnNumber'] ?? '',
      invoiceId: json['invoiceId'] ?? json['InvoiceId'] ?? 0,
      customerId: json['customerId'] ?? json['CustomerId'],
      customerName: json['customerName'] ?? json['CustomerName'],
      reason: json['reason'] ?? json['Reason'] ?? '',
      returnDate: json['returnDate'] != null || json['ReturnDate'] != null
          ? DateTime.tryParse(json['returnDate'] ?? json['ReturnDate'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      totalAmount: ((json['totalAmount'] ?? json['TotalAmount'] ?? 0) as num).toDouble(),
      status: json['status'] ?? json['Status'] ?? 'Completed',
      items: (json['items'] ?? json['Items'] as List<dynamic>?)
              ?.map((e) => ReturnItemResponse.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class PurchaseReturnResponse {
  final int id;
  final String returnNumber;
  final int purchaseId;
  final int supplierId;
  final String? supplierName;
  final String reason;
  final DateTime returnDate;
  final double totalAmount;
  final String status;
  final List<ReturnItemResponse> items;

  PurchaseReturnResponse({
    required this.id,
    required this.returnNumber,
    required this.purchaseId,
    required this.supplierId,
    this.supplierName,
    required this.reason,
    required this.returnDate,
    required this.totalAmount,
    this.status = 'Completed',
    this.items = const [],
  });

  factory PurchaseReturnResponse.fromJson(Map<String, dynamic> json) {
    return PurchaseReturnResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      returnNumber: json['returnNumber'] ?? json['ReturnNumber'] ?? '',
      purchaseId: json['purchaseId'] ?? json['PurchaseId'] ?? 0,
      supplierId: json['supplierId'] ?? json['SupplierId'] ?? 0,
      supplierName: json['supplierName'] ?? json['SupplierName'],
      reason: json['reason'] ?? json['Reason'] ?? '',
      returnDate: json['returnDate'] != null || json['ReturnDate'] != null
          ? DateTime.tryParse(json['returnDate'] ?? json['ReturnDate'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      totalAmount: ((json['totalAmount'] ?? json['TotalAmount'] ?? 0) as num).toDouble(),
      status: json['status'] ?? json['Status'] ?? 'Completed',
      items: (json['items'] ?? json['Items'] as List<dynamic>?)
              ?.map((e) => ReturnItemResponse.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

