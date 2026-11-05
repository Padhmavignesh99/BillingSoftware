import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/expense_model.dart';
import '../services/api_service.dart';

class ExpenseProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<ExpenseResponse> _expenses = [];
  bool _isLoading = false;
  String _searchQuery = '';
  String _selectedCategory = 'All';

  final List<String> categories = [
    'All',
    'Rent & Utilities',
    'Salaries & Wages',
    'Packaging & Supplies',
    'Logistics & Courier',
    'Marketing & Advertising',
    'Maintenance',
    'Office & Stationery',
    'Miscellaneous',
  ];

  List<ExpenseResponse> get expenses {
    return _expenses.where((e) {
      final matchesSearch = _searchQuery.isEmpty ||
          (e.description?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false) ||
          (e.paidTo?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false) ||
          (e.expenseNumber?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false);
      final matchesCategory = _selectedCategory == 'All' || e.categoryName == _selectedCategory;
      return matchesSearch && matchesCategory;
    }).toList();
  }

  double get totalExpenseAmount => _expenses.fold(0.0, (sum, e) => sum + e.amount);
  bool get isLoading => _isLoading;
  String get searchQuery => _searchQuery;
  String get selectedCategory => _selectedCategory;

  void setSearchQuery(String query) {
    _searchQuery = query;
    notifyListeners();
  }

  void setSelectedCategory(String category) {
    _selectedCategory = category;
    notifyListeners();
  }

  Future<void> fetchExpenses() async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _api.get('/expenses');
      if (response.statusCode == 200 && response.data != null) {
        final parsed = ApiResponse<List<ExpenseResponse>>.fromJson(
          response.data,
          (data) => (data as List<dynamic>)
              .map((e) => ExpenseResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _expenses = parsed.data ?? [];
      }
    } catch (e) {
      if (_expenses.isEmpty) {
        _expenses = _getSampleExpenses();
      }
    } finally {
      if (_expenses.isEmpty) {
        _expenses = _getSampleExpenses();
      }
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> addExpense({
    required String categoryName,
    required double amount,
    required DateTime expenseDate,
    String? description,
    String? paymentMethodName,
    String? paidTo,
  }) async {
    final newExp = ExpenseResponse(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      expenseNumber: 'EXP-${DateTime.now().millisecondsSinceEpoch.toString().substring(7)}',
      categoryName: categoryName,
      amount: amount,
      expenseDate: expenseDate,
      description: description,
      paymentMethodName: paymentMethodName ?? 'Cash',
      paidTo: paidTo,
      status: 'Approved',
    );

    try {
      await _api.post('/expenses', data: newExp.toJson());
      _expenses.insert(0, newExp);
    } catch (e) {
      _expenses.insert(0, newExp);
    }
    notifyListeners();
    return true;
  }

  List<ExpenseResponse> _getSampleExpenses() {
    final now = DateTime.now();
    return [
      ExpenseResponse(
        id: 1,
        expenseNumber: 'EXP-2026-012',
        categoryName: 'Rent & Utilities',
        amount: 35000.0,
        expenseDate: now.subtract(const Duration(days: 1)),
        description: 'Monthly store shop rent',
        paymentMethodName: 'Bank Transfer',
        paidTo: 'Shree Real Estate',
        status: 'Approved',
      ),
      ExpenseResponse(
        id: 2,
        expenseNumber: 'EXP-2026-013',
        categoryName: 'Packaging & Supplies',
        amount: 4500.0,
        expenseDate: now.subtract(const Duration(days: 3)),
        description: 'Eco friendly paper carry bags & boxes',
        paymentMethodName: 'UPI',
        paidTo: 'Apex Packaging',
        status: 'Approved',
      ),
      ExpenseResponse(
        id: 3,
        expenseNumber: 'EXP-2026-014',
        categoryName: 'Logistics & Courier',
        amount: 2800.0,
        expenseDate: now.subtract(const Duration(days: 4)),
        description: 'Same day customer deliveries',
        paymentMethodName: 'Cash',
        paidTo: 'Dunzo Delivery',
        status: 'Approved',
      ),
    ];
  }
}

