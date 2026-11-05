class PurchaseResponse {
  final int id;
  final int supplierId;
  final String purchaseNumber;
  final String? supplierName;
  final DateTime purchaseDate;
  final DateTime? dueDate;
  final double subTotal;
  final double taxAmount;
  final double discountAmount;
  final double totalAmount;
  final double paidAmount;
  final double dueAmount;
  final String status;
  final List<PurchaseItemResponse> items;

  PurchaseResponse({
    required this.id,
    required this.supplierId,
    required this.purchaseNumber,
    this.supplierName,
    required this.purchaseDate,
    this.dueDate,
    this.subTotal = 0.0,
    this.taxAmount = 0.0,
    this.discountAmount = 0.0,
    required this.totalAmount,
    this.paidAmount = 0.0,
    this.dueAmount = 0.0,
    this.status = 'Completed',
    this.items = const [],
  });

  factory PurchaseResponse.fromJson(Map<String, dynamic> json) {
    return PurchaseResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      supplierId: json['supplierId'] ?? json['SupplierId'] ?? 0,
      purchaseNumber: json['purchaseNumber'] ?? json['PurchaseNumber'] ?? '',
      supplierName: json['supplierName'] ?? json['SupplierName'],
      purchaseDate: json['purchaseDate'] != null || json['PurchaseDate'] != null
          ? DateTime.tryParse(json['purchaseDate'] ?? json['PurchaseDate'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      dueDate: json['dueDate'] != null || json['DueDate'] != null
          ? DateTime.tryParse(json['dueDate'] ?? json['DueDate'] ?? '')
          : null,
      subTotal: ((json['subTotal'] ?? json['SubTotal'] ?? 0) as num).toDouble(),
      taxAmount: ((json['taxAmount'] ?? json['TaxAmount'] ?? 0) as num).toDouble(),
      discountAmount: ((json['discountAmount'] ?? json['DiscountAmount'] ?? 0) as num).toDouble(),
      totalAmount: ((json['totalAmount'] ?? json['TotalAmount'] ?? 0) as num).toDouble(),
      paidAmount: ((json['paidAmount'] ?? json['PaidAmount'] ?? 0) as num).toDouble(),
      dueAmount: ((json['dueAmount'] ?? json['DueAmount'] ?? 0) as num).toDouble(),
      status: json['status'] ?? json['Status'] ?? 'Completed',
      items: (json['items'] ?? json['Items'] as List<dynamic>?)
              ?.map((e) => PurchaseItemResponse.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class PurchaseItemResponse {
  final int productId;
  final String productName;
  final double quantity;
  final double unitPrice;
  final double taxAmount;
  final double totalPrice;

  PurchaseItemResponse({
    required this.productId,
    required this.productName,
    required this.quantity,
    required this.unitPrice,
    this.taxAmount = 0.0,
    required this.totalPrice,
  });

  factory PurchaseItemResponse.fromJson(Map<String, dynamic> json) {
    return PurchaseItemResponse(
      productId: json['productId'] ?? json['ProductId'] ?? 0,
      productName: json['productName'] ?? json['ProductName'] ?? '',
      quantity: ((json['quantity'] ?? json['Quantity'] ?? 1) as num).toDouble(),
      unitPrice: ((json['unitPrice'] ?? json['UnitPrice'] ?? 0) as num).toDouble(),
      taxAmount: ((json['taxAmount'] ?? json['TaxAmount'] ?? 0) as num).toDouble(),
      totalPrice: ((json['totalPrice'] ?? json['TotalPrice'] ?? 0) as num).toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'productId': productId,
      'productName': productName,
      'quantity': quantity,
      'unitPrice': unitPrice,
      'taxAmount': taxAmount,
      'totalPrice': totalPrice,
    };
  }
}

