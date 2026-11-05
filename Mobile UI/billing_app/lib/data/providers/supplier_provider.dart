import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/supplier_model.dart';
import '../services/api_service.dart';

class SupplierProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<SupplierResponse> _suppliers = [];
  bool _isLoading = false;
  String _searchQuery = '';

  List<SupplierResponse> get suppliers {
    if (_searchQuery.isEmpty) return _suppliers;
    return _suppliers.where((s) {
      return s.name.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          (s.companyName?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false) ||
          (s.phone?.contains(_searchQuery) ?? false);
    }).toList();
  }

  bool get isLoading => _isLoading;
  String get searchQuery => _searchQuery;

  void setSearchQuery(String query) {
    _searchQuery = query;
    notifyListeners();
  }

  Future<void> fetchSuppliers() async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _api.get('/suppliers');
      if (response.statusCode == 200 && response.data != null) {
        final parsed = ApiResponse<List<SupplierResponse>>.fromJson(
          response.data,
          (data) => (data as List<dynamic>)
              .map((e) => SupplierResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _suppliers = parsed.data ?? [];
      }
    } catch (e) {
      if (_suppliers.isEmpty) {
        _suppliers = _getSampleSuppliers();
      }
    } finally {
      if (_suppliers.isEmpty) {
        _suppliers = _getSampleSuppliers();
      }
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> createSupplier({
    required String name,
    String? companyName,
    String? phone,
    String? email,
    String? address,
    String? city,
    String? gstNumber,
    double creditLimit = 0.0,
    int creditDays = 30,
  }) async {
    _isLoading = true;
    notifyListeners();

    final newSupplier = SupplierResponse(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      name: name,
      companyName: companyName,
      phone: phone,
      email: email,
      address: address,
      city: city,
      gstNumber: gstNumber,
      creditLimit: creditLimit,
      outstandingBalance: 0.0,
      creditDays: creditDays,
    );

    try {
      await _api.post('/suppliers', data: newSupplier.toJson());
      _suppliers.insert(0, newSupplier);
    } catch (e) {
      _suppliers.insert(0, newSupplier);
    } finally {
      _isLoading = false;
      notifyListeners();
    }
    return true;
  }

  List<SupplierResponse> _getSampleSuppliers() {
    return [
      SupplierResponse(
        id: 1,
        name: 'Vikas Aggarwal',
        companyName: 'Aggarwal Wholesale Traders',
        phone: '+91 98111 22334',
        email: 'vikas@aggarwaltraders.com',
        city: 'Delhi',
        creditLimit: 200000.0,
        outstandingBalance: 45000.0,
        creditDays: 30,
      ),
      SupplierResponse(
        id: 2,
        name: 'Suresh Patel',
        companyName: 'Gujarat Agro Impex',
        phone: '+91 98222 33445',
        email: 'sales@gujaratagro.com',
        city: 'Surat',
        creditLimit: 150000.0,
        outstandingBalance: 18400.0,
        creditDays: 45,
      ),
      SupplierResponse(
        id: 3,
        name: 'Karthik Raja',
        companyName: 'Techno Imports India',
        phone: '+91 98333 44556',
        email: 'contact@technoimports.in',
        city: 'Chennai',
        creditLimit: 300000.0,
        outstandingBalance: 78900.0,
        creditDays: 15,
      ),
    ];
  }
}

