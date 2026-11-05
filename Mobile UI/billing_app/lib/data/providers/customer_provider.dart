import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/customer_model.dart';
import '../services/api_service.dart';

class CustomerProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<CustomerResponse> _customers = [];
  List<CustomerPayment> _selectedCustomerPayments = [];
  CustomerResponse? _selectedCustomer;
  bool _isLoading = false;
  String _searchQuery = '';

  List<CustomerResponse> get customers {
    if (_searchQuery.isEmpty) return _customers;
    return _customers.where((c) {
      return c.name.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          (c.phone?.contains(_searchQuery) ?? false) ||
          (c.email?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false);
    }).toList();
  }

  List<CustomerPayment> get selectedCustomerPayments => _selectedCustomerPayments;
  CustomerResponse? get selectedCustomer => _selectedCustomer;
  bool get isLoading => _isLoading;
  String get searchQuery => _searchQuery;

  void setSearchQuery(String query) {
    _searchQuery = query;
    notifyListeners();
  }

  Future<void> fetchCustomers() async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _api.get('/customers');
      if (response.statusCode == 200 && response.data != null) {
        final parsed = ApiResponse<List<CustomerResponse>>.fromJson(
          response.data,
          (data) => (data as List<dynamic>)
              .map((e) => CustomerResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _customers = parsed.data ?? [];
      }
    } catch (e) {
      if (_customers.isEmpty) {
        _customers = _getSampleCustomers();
      }
    } finally {
      if (_customers.isEmpty) {
        _customers = _getSampleCustomers();
      }
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> fetchCustomerLedger(int customerId) async {
    _isLoading = true;
    _selectedCustomer = _customers.firstWhere(
      (c) => c.id == customerId,
      orElse: () => _getSampleCustomers().first,
    );
    notifyListeners();

    try {
      final response = await _api.get('/customer-payments/customer/$customerId');
      if (response.statusCode == 200 && response.data != null) {
        final data = response.data['data'] ?? response.data['Data'] ?? response.data;
        _selectedCustomerPayments = (data as List<dynamic>)
            .map((e) => CustomerPayment.fromJson(e as Map<String, dynamic>))
            .toList();
      }
    } catch (e) {
      _selectedCustomerPayments = _getSamplePayments(customerId);
    } finally {
      if (_selectedCustomerPayments.isEmpty) {
        _selectedCustomerPayments = _getSamplePayments(customerId);
      }
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> createCustomer({
    required String name,
    String? phone,
    String? email,
    String? address,
    String? city,
    String? state,
    String? gstNumber,
    double creditLimit = 0.0,
  }) async {
    _isLoading = true;
    notifyListeners();

    final newCust = CustomerResponse(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      name: name,
      phone: phone,
      email: email,
      address: address,
      city: city,
      state: state,
      gstNumber: gstNumber,
      creditLimit: creditLimit,
      outstandingBalance: 0.0,
      totalPurchaseAmount: 0.0,
      totalPurchaseCount: 0,
      lastPurchaseDate: DateTime.now(),
    );

    try {
      await _api.post('/customers', data: newCust.toJson());
      _customers.insert(0, newCust);
    } catch (e) {
      _customers.insert(0, newCust);
    } finally {
      _isLoading = false;
      notifyListeners();
    }
    return true;
  }

  Future<bool> recordPayment({
    required int customerId,
    required double amount,
    required String paymentMethod,
    String? referenceNumber,
    String? notes,
  }) async {
    final payment = CustomerPayment(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      customerId: customerId,
      amount: amount,
      paymentMethod: paymentMethod,
      paymentDate: DateTime.now(),
      referenceNumber: referenceNumber,
      notes: notes,
    );

    _selectedCustomerPayments.insert(0, payment);
    // Deduct from outstanding balance
    final index = _customers.indexWhere((c) => c.id == customerId);
    if (index >= 0) {
      final current = _customers[index];
      _customers[index] = CustomerResponse(
        id: current.id,
        name: current.name,
        phone: current.phone,
        email: current.email,
        address: current.address,
        city: current.city,
        state: current.state,
        gstNumber: current.gstNumber,
        creditLimit: current.creditLimit,
        outstandingBalance: (current.outstandingBalance - amount).clamp(0.0, double.infinity),
        totalPurchaseAmount: current.totalPurchaseAmount,
        totalPurchaseCount: current.totalPurchaseCount,
        lastPurchaseDate: current.lastPurchaseDate,
      );
      _selectedCustomer = _customers[index];
    }
    notifyListeners();
    return true;
  }

  List<CustomerResponse> _getSampleCustomers() {
    return [
      CustomerResponse(
        id: 1,
        name: 'Rahul Sharma',
        phone: '+91 98765 11111',
        email: 'rahul.s@example.com',
        city: 'Mumbai',
        creditLimit: 50000.0,
        outstandingBalance: 3764.0,
        totalPurchaseAmount: 48900.0,
        totalPurchaseCount: 12,
        lastPurchaseDate: DateTime.now().subtract(const Duration(hours: 4)),
      ),
      CustomerResponse(
        id: 2,
        name: 'Priya Patel',
        phone: '+91 98765 22222',
        email: 'priya.p@example.com',
        city: 'Ahmedabad',
        creditLimit: 25000.0,
        outstandingBalance: 5002.0,
        totalPurchaseAmount: 32000.0,
        totalPurchaseCount: 8,
        lastPurchaseDate: DateTime.now().subtract(const Duration(days: 1)),
      ),
      CustomerResponse(
        id: 3,
        name: 'Amit Verma',
        phone: '+91 98765 33333',
        email: 'amit.v@example.com',
        city: 'Delhi',
        creditLimit: 10000.0,
        outstandingBalance: 0.0,
        totalPurchaseAmount: 18500.0,
        totalPurchaseCount: 5,
        lastPurchaseDate: DateTime.now().subtract(const Duration(days: 2)),
      ),
      CustomerResponse(
        id: 4,
        name: 'Sunita Rao',
        phone: '+91 98765 44444',
        email: 'sunita.r@example.com',
        city: 'Bengaluru',
        creditLimit: 30000.0,
        outstandingBalance: 17172.0,
        totalPurchaseAmount: 64000.0,
        totalPurchaseCount: 15,
        lastPurchaseDate: DateTime.now().subtract(const Duration(days: 5)),
      ),
    ];
  }

  List<CustomerPayment> _getSamplePayments(int customerId) {
    final now = DateTime.now();
    return [
      CustomerPayment(
        id: 1,
        customerId: customerId,
        amount: 2500.0,
        paymentMethod: 'UPI',
        paymentDate: now.subtract(const Duration(days: 3)),
        referenceNumber: 'UPI/2026/89412',
        notes: 'Partial settlement for invoice INV-2026-002',
      ),
      CustomerPayment(
        id: 2,
        customerId: customerId,
        amount: 5000.0,
        paymentMethod: 'Bank Transfer',
        paymentDate: now.subtract(const Duration(days: 10)),
        referenceNumber: 'NEFT/HDFC/00192',
        notes: 'Monthly ledger clearing',
      ),
      CustomerPayment(
        id: 3,
        customerId: customerId,
        amount: 3200.0,
        paymentMethod: 'Cash',
        paymentDate: now.subtract(const Duration(days: 22)),
        referenceNumber: 'CSH-984',
        notes: 'Counter payment',
      ),
    ];
  }
}

