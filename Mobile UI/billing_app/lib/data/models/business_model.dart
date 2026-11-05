class BusinessResponse {
  final int id;
  final String name;
  final String? legalName;
  final String? businessTypeName;
  final String? email;
  final String? phone;
  final String? website;
  final String? address;
  final String? city;
  final String? state;
  final String? country;
  final String? pinCode;
  final String? gstNumber;
  final String? panNumber;
  final String? logoUrl;
  final String? currencyCode;
  final String? currencySymbol;
  final String status;

  BusinessResponse({
    required this.id,
    required this.name,
    this.legalName,
    this.businessTypeName,
    this.email,
    this.phone,
    this.website,
    this.address,
    this.city,
    this.state,
    this.country,
    this.pinCode,
    this.gstNumber,
    this.panNumber,
    this.logoUrl,
    this.currencyCode = 'INR',
    this.currencySymbol = '₹',
    this.status = 'Active',
  });

  factory BusinessResponse.fromJson(Map<String, dynamic> json) {
    return BusinessResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      legalName: json['legalName'] ?? json['LegalName'],
      businessTypeName: json['businessTypeName'] ?? json['BusinessTypeName'],
      email: json['email'] ?? json['Email'],
      phone: json['phone'] ?? json['Phone'],
      website: json['website'] ?? json['Website'],
      address: json['address'] ?? json['Address'],
      city: json['city'] ?? json['City'],
      state: json['state'] ?? json['State'],
      country: json['country'] ?? json['Country'],
      pinCode: json['pinCode'] ?? json['PinCode'],
      gstNumber: json['gstNumber'] ?? json['GSTNumber'],
      panNumber: json['panNumber'] ?? json['PANNumber'],
      logoUrl: json['logoUrl'] ?? json['LogoUrl'],
      currencyCode: json['currencyCode'] ?? json['CurrencyCode'] ?? 'INR',
      currencySymbol: json['currencySymbol'] ?? json['CurrencySymbol'] ?? '₹',
      status: json['status'] ?? json['Status'] ?? 'Active',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'legalName': legalName,
      'businessTypeName': businessTypeName,
      'email': email,
      'phone': phone,
      'website': website,
      'address': address,
      'city': city,
      'state': state,
      'country': country,
      'pinCode': pinCode,
      'gstNumber': gstNumber,
      'panNumber': panNumber,
      'logoUrl': logoUrl,
      'currencyCode': currencyCode,
      'currencySymbol': currencySymbol,
      'status': status,
    };
  }
}

class BranchResponse {
  final int id;
  final String name;
  final String? code;
  final String? address;
  final String? city;
  final String? state;
  final String? phone;
  final String? email;
  final bool isMainBranch;

  BranchResponse({
    required this.id,
    required this.name,
    this.code,
    this.address,
    this.city,
    this.state,
    this.phone,
    this.email,
    this.isMainBranch = false,
  });

  factory BranchResponse.fromJson(Map<String, dynamic> json) {
    return BranchResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      code: json['code'] ?? json['Code'],
      address: json['address'] ?? json['Address'],
      city: json['city'] ?? json['City'],
      state: json['state'] ?? json['State'],
      phone: json['phone'] ?? json['Phone'],
      email: json['email'] ?? json['Email'],
      isMainBranch: json['isMainBranch'] ?? json['IsMainBranch'] ?? false,
    );
  }
}

