class SupplierResponse {
  final int id;
  final String name;
  final String? companyName;
  final String? phone;
  final String? email;
  final String? address;
  final String? city;
  final String? gstNumber;
  final String? supplierGroupName;
  final double creditLimit;
  final double outstandingBalance;
  final int creditDays;
  final bool isActive;

  SupplierResponse({
    required this.id,
    required this.name,
    this.companyName,
    this.phone,
    this.email,
    this.address,
    this.city,
    this.gstNumber,
    this.supplierGroupName,
    this.creditLimit = 0.0,
    this.outstandingBalance = 0.0,
    this.creditDays = 30,
    this.isActive = true,
  });

  factory SupplierResponse.fromJson(Map<String, dynamic> json) {
    return SupplierResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      companyName: json['companyName'] ?? json['CompanyName'],
      phone: json['phone'] ?? json['Phone'],
      email: json['email'] ?? json['Email'],
      address: json['address'] ?? json['Address'],
      city: json['city'] ?? json['City'],
      gstNumber: json['gstNumber'] ?? json['GSTNumber'],
      supplierGroupName: json['supplierGroupName'] ?? json['SupplierGroupName'],
      creditLimit: ((json['creditLimit'] ?? json['CreditLimit'] ?? 0) as num).toDouble(),
      outstandingBalance: ((json['outstandingBalance'] ?? json['OutstandingBalance'] ?? 0) as num).toDouble(),
      creditDays: json['creditDays'] ?? json['CreditDays'] ?? 30,
      isActive: json['isActive'] ?? json['IsActive'] ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'companyName': companyName,
      'phone': phone,
      'email': email,
      'address': address,
      'city': city,
      'gstNumber': gstNumber,
      'supplierGroupName': supplierGroupName,
      'creditLimit': creditLimit,
      'outstandingBalance': outstandingBalance,
      'creditDays': creditDays,
      'isActive': isActive,
    };
  }
}

