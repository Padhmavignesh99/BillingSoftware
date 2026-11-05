import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/staff_model.dart';
import '../models/user_model.dart';
import '../services/api_service.dart';

class StaffProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<StaffResponse> _staffList = [];
  List<RoleResponse> _roles = [];
  bool _isLoading = false;
  String _searchQuery = '';

  List<StaffResponse> get staffList {
    if (_searchQuery.isEmpty) return _staffList;
    return _staffList.where((s) {
      return s.user.fullName.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          (s.designation?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false) ||
          (s.department?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false);
    }).toList();
  }

  List<RoleResponse> get roles => _roles;
  bool get isLoading => _isLoading;
  String get searchQuery => _searchQuery;

  void setSearchQuery(String query) {
    _searchQuery = query;
    notifyListeners();
  }

  Future<void> fetchStaff() async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _api.get('/staff');
      if (response.statusCode == 200 && response.data != null) {
        final parsed = ApiResponse<List<StaffResponse>>.fromJson(
          response.data,
          (data) => (data as List<dynamic>)
              .map((e) => StaffResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _staffList = parsed.data ?? [];
      }
    } catch (e) {
      if (_staffList.isEmpty) {
        _staffList = _getSampleStaff();
      }
    } finally {
      if (_staffList.isEmpty) {
        _staffList = _getSampleStaff();
      }
      _roles = [
        RoleResponse(id: 1, name: 'Admin', description: 'Full access to all modules'),
        RoleResponse(id: 2, name: 'Cashier / Billing', description: 'Access to Billing and Invoices'),
        RoleResponse(id: 3, name: 'Inventory Manager', description: 'Access to Products and Inventory'),
        RoleResponse(id: 4, name: 'Accountant', description: 'Access to Reports and Expenses'),
      ];
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> addStaff({
    required String fullName,
    required String email,
    required String phone,
    required String role,
    String? department,
    String? designation,
    double? salary,
  }) async {
    final newStaff = StaffResponse(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      user: UserResponse(
        id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
        fullName: fullName,
        email: email,
        phone: phone,
        role: role,
        isActive: true,
      ),
      department: department ?? 'Billing',
      designation: designation ?? 'Billing Operator',
      salary: salary ?? 25000.0,
      joiningDate: DateTime.now(),
    );

    _staffList.insert(0, newStaff);
    notifyListeners();
    return true;
  }

  List<StaffResponse> _getSampleStaff() {
    return [
      StaffResponse(
        id: 1,
        user: UserResponse(
          id: 1,
          fullName: 'Ananya Deshmukh',
          email: 'ananya@business.com',
          phone: '+91 98450 12345',
          role: 'Manager',
          isActive: true,
        ),
        department: 'Operations',
        designation: 'Store Manager',
        salary: 45000.0,
        joiningDate: DateTime(2024, 1, 15),
      ),
      StaffResponse(
        id: 2,
        user: UserResponse(
          id: 2,
          fullName: 'Kiran Kumar',
          email: 'kiran.k@business.com',
          phone: '+91 98450 67890',
          role: 'Cashier',
          isActive: true,
        ),
        department: 'POS / Sales',
        designation: 'Senior Cashier',
        salary: 22000.0,
        joiningDate: DateTime(2024, 6, 1),
      ),
      StaffResponse(
        id: 3,
        user: UserResponse(
          id: 3,
          fullName: 'Manoj Singh',
          email: 'manoj.s@business.com',
          phone: '+91 98450 11223',
          role: 'Inventory Manager',
          isActive: true,
        ),
        department: 'Warehouse',
        designation: 'Stock Supervisor',
        salary: 28000.0,
        joiningDate: DateTime(2024, 3, 10),
      ),
    ];
  }
}

