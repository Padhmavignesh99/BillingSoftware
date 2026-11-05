class ProductResponse {
  final int id;
  final String name;
  final String? sku;
  final String? barcode;
  final String? description;
  final String? hsnCode;
  final String? categoryName;
  final String? brandName;
  final String? unitName;
  final double purchasePrice;
  final double sellingPrice;
  final double? mrp;
  final String? taxRateName;
  final double? taxPercentage;
  final double currentStock;
  final double? minStockLevel;
  final String? imageUrl;
  final bool isService;
  final bool isActive;
  final DateTime? expiryDate;

  ProductResponse({
    required this.id,
    required this.name,
    this.sku,
    this.barcode,
    this.description,
    this.hsnCode,
    this.categoryName,
    this.brandName,
    this.unitName,
    this.purchasePrice = 0.0,
    required this.sellingPrice,
    this.mrp,
    this.taxRateName,
    this.taxPercentage,
    this.currentStock = 0.0,
    this.minStockLevel,
    this.imageUrl,
    this.isService = false,
    this.isActive = true,
    this.expiryDate,
  });

  bool get isLowStock =>
      minStockLevel != null && currentStock <= minStockLevel! && currentStock > 0;
  bool get isOutOfStock => currentStock <= 0;

  factory ProductResponse.fromJson(Map<String, dynamic> json) {
    return ProductResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      sku: json['sku'] ?? json['SKU'],
      barcode: json['barcode'] ?? json['Barcode'],
      description: json['description'] ?? json['Description'],
      hsnCode: json['hsnCode'] ?? json['HSNCode'],
      categoryName: json['categoryName'] ?? json['CategoryName'],
      brandName: json['brandName'] ?? json['BrandName'],
      unitName: json['unitName'] ?? json['UnitName'],
      purchasePrice: ((json['purchasePrice'] ?? json['PurchasePrice'] ?? 0) as num).toDouble(),
      sellingPrice: ((json['sellingPrice'] ?? json['SellingPrice'] ?? 0) as num).toDouble(),
      mrp: json['mrp'] != null || json['MRP'] != null
          ? ((json['mrp'] ?? json['MRP']) as num).toDouble()
          : null,
      taxRateName: json['taxRateName'] ?? json['TaxRateName'],
      taxPercentage: json['taxPercentage'] != null || json['TaxPercentage'] != null
          ? ((json['taxPercentage'] ?? json['TaxPercentage']) as num).toDouble()
          : null,
      currentStock: ((json['currentStock'] ?? json['CurrentStock'] ?? 0) as num).toDouble(),
      minStockLevel: json['minStockLevel'] != null || json['MinStockLevel'] != null
          ? ((json['minStockLevel'] ?? json['MinStockLevel']) as num).toDouble()
          : null,
      imageUrl: json['imageUrl'] ?? json['ImageUrl'],
      isService: json['isService'] ?? json['IsService'] ?? false,
      isActive: json['isActive'] ?? json['IsActive'] ?? true,
      expiryDate: json['expiryDate'] != null || json['ExpiryDate'] != null
          ? DateTime.tryParse(json['expiryDate'] ?? json['ExpiryDate'] ?? '')
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'sku': sku,
      'barcode': barcode,
      'description': description,
      'hsnCode': hsnCode,
      'categoryName': categoryName,
      'brandName': brandName,
      'unitName': unitName,
      'purchasePrice': purchasePrice,
      'sellingPrice': sellingPrice,
      'mrp': mrp,
      'taxRateName': taxRateName,
      'taxPercentage': taxPercentage,
      'currentStock': currentStock,
      'minStockLevel': minStockLevel,
      'imageUrl': imageUrl,
      'isService': isService,
      'isActive': isActive,
      'expiryDate': expiryDate?.toIso8601String(),
    };
  }
}

class ProductRequest {
  final String name;
  final String? sku;
  final String? barcode;
  final String? description;
  final String? hsnCode;
  final int? categoryId;
  final int? brandId;
  final int? unitId;
  final double purchasePrice;
  final double sellingPrice;
  final double? mrp;
  final int? taxRateId;
  final double openingStock;
  final double? minStockLevel;
  final String? imageUrl;
  final bool isService;

  ProductRequest({
    required this.name,
    this.sku,
    this.barcode,
    this.description,
    this.hsnCode,
    this.categoryId,
    this.brandId,
    this.unitId,
    this.purchasePrice = 0.0,
    required this.sellingPrice,
    this.mrp,
    this.taxRateId,
    this.openingStock = 0.0,
    this.minStockLevel,
    this.imageUrl,
    this.isService = false,
  });

  Map<String, dynamic> toJson() {
    return {
      'name': name,
      'sku': sku,
      'barcode': barcode,
      'description': description,
      'hsnCode': hsnCode,
      'categoryId': categoryId,
      'brandId': brandId,
      'unitId': unitId,
      'purchasePrice': purchasePrice,
      'sellingPrice': sellingPrice,
      'mrp': mrp,
      'taxRateId': taxRateId,
      'openingStock': openingStock,
      'minStockLevel': minStockLevel,
      'imageUrl': imageUrl,
      'isService': isService,
    };
  }
}

