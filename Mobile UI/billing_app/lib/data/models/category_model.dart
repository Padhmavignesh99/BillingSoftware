class CategoryResponse {
  final int id;
  final String name;
  final String? description;
  final int? parentCategoryId;
  final String? parentCategoryName;
  final String? imageUrl;
  final int sortOrder;
  final int productCount;

  CategoryResponse({
    required this.id,
    required this.name,
    this.description,
    this.parentCategoryId,
    this.parentCategoryName,
    this.imageUrl,
    this.sortOrder = 0,
    this.productCount = 0,
  });

  factory CategoryResponse.fromJson(Map<String, dynamic> json) {
    return CategoryResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      description: json['description'] ?? json['Description'],
      parentCategoryId: json['parentCategoryId'] ?? json['ParentCategoryId'],
      parentCategoryName: json['parentCategoryName'] ?? json['ParentCategoryName'],
      imageUrl: json['imageUrl'] ?? json['ImageUrl'],
      sortOrder: json['sortOrder'] ?? json['SortOrder'] ?? 0,
      productCount: json['productCount'] ?? json['ProductCount'] ?? 0,
    );
  }
}

class TaxRateResponse {
  final int id;
  final String name;
  final double percentage;
  final String? description;
  final double? cgstPercentage;
  final double? sgstPercentage;
  final double? igstPercentage;
  final bool isDefault;

  TaxRateResponse({
    required this.id,
    required this.name,
    required this.percentage,
    this.description,
    this.cgstPercentage,
    this.sgstPercentage,
    this.igstPercentage,
    this.isDefault = false,
  });

  factory TaxRateResponse.fromJson(Map<String, dynamic> json) {
    return TaxRateResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      percentage: ((json['percentage'] ?? json['Percentage'] ?? 0) as num).toDouble(),
      description: json['description'] ?? json['Description'],
      cgstPercentage: json['cgstPercentage'] != null || json['CGSTPercentage'] != null
          ? ((json['cgstPercentage'] ?? json['CGSTPercentage']) as num).toDouble()
          : null,
      sgstPercentage: json['sgstPercentage'] != null || json['SGSTPercentage'] != null
          ? ((json['sgstPercentage'] ?? json['SGSTPercentage']) as num).toDouble()
          : null,
      igstPercentage: json['igstPercentage'] != null || json['IGSTPercentage'] != null
          ? ((json['igstPercentage'] ?? json['IGSTPercentage']) as num).toDouble()
          : null,
      isDefault: json['isDefault'] ?? json['IsDefault'] ?? false,
    );
  }
}

