class CustomerResponse {
  final int id;
  final String name;
  final String? phone;
  final String? email;
  final String? address;
  final String? city;
  final String? state;
  final String? gstNumber;
  final String? customerGroupName;
  final double creditLimit;
  final double outstandingBalance;
  final double totalPurchaseAmount;
  final int totalPurchaseCount;
  final DateTime? lastPurchaseDate;
  final bool isActive;

  CustomerResponse({
    required this.id,
    required this.name,
    this.phone,
    this.email,
    this.address,
    this.city,
    this.state,
    this.gstNumber,
    this.customerGroupName,
    this.creditLimit = 0.0,
    this.outstandingBalance = 0.0,
    this.totalPurchaseAmount = 0.0,
    this.totalPurchaseCount = 0,
    this.lastPurchaseDate,
    this.isActive = true,
  });

  factory CustomerResponse.fromJson(Map<String, dynamic> json) {
    return CustomerResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      phone: json['phone'] ?? json['Phone'],
      email: json['email'] ?? json['Email'],
      address: json['address'] ?? json['Address'],
      city: json['city'] ?? json['City'],
      state: json['state'] ?? json['State'],
      gstNumber: json['gstNumber'] ?? json['GSTNumber'],
      customerGroupName: json['customerGroupName'] ?? json['CustomerGroupName'],
      creditLimit: ((json['creditLimit'] ?? json['CreditLimit'] ?? 0) as num).toDouble(),
      outstandingBalance: ((json['outstandingBalance'] ?? json['OutstandingBalance'] ?? 0) as num).toDouble(),
      totalPurchaseAmount: ((json['totalPurchaseAmount'] ?? json['TotalPurchaseAmount'] ?? 0) as num).toDouble(),
      totalPurchaseCount: json['totalPurchaseCount'] ?? json['TotalPurchaseCount'] ?? 0,
      lastPurchaseDate: json['lastPurchaseDate'] != null || json['LastPurchaseDate'] != null
          ? DateTime.tryParse(json['lastPurchaseDate'] ?? json['LastPurchaseDate'] ?? '')
          : null,
      isActive: json['isActive'] ?? json['IsActive'] ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'phone': phone,
      'email': email,
      'address': address,
      'city': city,
      'state': state,
      'gstNumber': gstNumber,
      'customerGroupName': customerGroupName,
      'creditLimit': creditLimit,
      'outstandingBalance': outstandingBalance,
      'isActive': isActive,
    };
  }
}

class CustomerPayment {
  final int id;
  final int customerId;
  final String? customerName;
  final double amount;
  final String paymentMethod;
  final DateTime paymentDate;
  final String? referenceNumber;
  final String? notes;

  CustomerPayment({
    required this.id,
    required this.customerId,
    this.customerName,
    required this.amount,
    required this.paymentMethod,
    required this.paymentDate,
    this.referenceNumber,
    this.notes,
  });

  factory CustomerPayment.fromJson(Map<String, dynamic> json) {
    return CustomerPayment(
      id: json['id'] ?? json['Id'] ?? 0,
      customerId: json['customerId'] ?? json['CustomerId'] ?? 0,
      customerName: json['customerName'] ?? json['CustomerName'],
      amount: ((json['amount'] ?? json['Amount'] ?? 0) as num).toDouble(),
      paymentMethod: json['paymentMethod'] ?? json['PaymentMethod'] ?? 'Cash',
      paymentDate: json['paymentDate'] != null || json['PaymentDate'] != null
          ? DateTime.tryParse(json['paymentDate'] ?? json['PaymentDate'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      referenceNumber: json['referenceNumber'] ?? json['ReferenceNumber'],
      notes: json['notes'] ?? json['Notes'],
    );
  }
}

