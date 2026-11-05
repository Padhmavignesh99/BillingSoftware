import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/credit_bill_model.dart';
import '../services/api_service.dart';

class CreditProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<CreditBillResponse> _creditBills = [];
  bool _isLoading = false;
  String _filterStatus = 'All'; // All, Pending, Settled, Overdue
  String _searchQuery = '';

  List<CreditBillResponse> get creditBills {
    return _creditBills.where((b) {
      final matchesSearch = _searchQuery.isEmpty ||
          b.customerName.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          b.creditBillNumber.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          (b.invoiceNumber?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false);

      final matchesFilter = _filterStatus == 'All' ||
          (_filterStatus == 'Pending' && b.status == 'Pending') ||
          (_filterStatus == 'Settled' && b.status == 'Settled') ||
          (_filterStatus == 'Overdue' && b.isOverdue);

      return matchesSearch && matchesFilter;
    }).toList();
  }

  double get totalCreditOutstanding => _creditBills.fold(0.0, (sum, b) => sum + b.balanceAmount);
  bool get isLoading => _isLoading;
  String get filterStatus => _filterStatus;
  String get searchQuery => _searchQuery;

  void setFilterStatus(String status) {
    _filterStatus = status;
    notifyListeners();
  }

  void setSearchQuery(String query) {
    _searchQuery = query;
    notifyListeners();
  }

  Future<void> fetchCreditBills() async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _api.get('/credit-bills');
      if (response.statusCode == 200 && response.data != null) {
        final parsed = ApiResponse<List<CreditBillResponse>>.fromJson(
          response.data,
          (data) => (data as List<dynamic>)
              .map((e) => CreditBillResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _creditBills = parsed.data ?? [];
      }
    } catch (e) {
      if (_creditBills.isEmpty) {
        _creditBills = _getSampleCreditBills();
      }
    } finally {
      if (_creditBills.isEmpty) {
        _creditBills = _getSampleCreditBills();
      }
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> settleCreditBill(int billId, double paymentAmount) async {
    final index = _creditBills.indexWhere((b) => b.id == billId);
    if (index >= 0) {
      final current = _creditBills[index];
      final newSettled = current.settledAmount + paymentAmount;
      final newBalance = (current.amount - newSettled).clamp(0.0, double.infinity);
      final newStatus = newBalance == 0 ? 'Settled' : 'Pending';

      _creditBills[index] = CreditBillResponse(
        id: current.id,
        creditBillNumber: current.creditBillNumber,
        customerId: current.customerId,
        customerName: current.customerName,
        customerPhone: current.customerPhone,
        invoiceId: current.invoiceId,
        invoiceNumber: current.invoiceNumber,
        amount: current.amount,
        settledAmount: newSettled,
        balanceAmount: newBalance,
        dueDate: current.dueDate,
        status: newStatus,
        notes: current.notes,
        createdAt: current.createdAt,
      );
      notifyListeners();
      return true;
    }
    return false;
  }

  List<CreditBillResponse> _getSampleCreditBills() {
    final now = DateTime.now();
    return [
      CreditBillResponse(
        id: 1,
        creditBillNumber: 'CR-101',
        customerId: 2,
        customerName: 'Priya Patel',
        customerPhone: '+91 98765 22222',
        invoiceId: 102,
        invoiceNumber: 'INV-2026-002',
        amount: 10002.0,
        settledAmount: 5000.0,
        balanceAmount: 5002.0,
        dueDate: now.add(const Duration(days: 10)),
        status: 'Pending',
        createdAt: now.subtract(const Duration(days: 1)),
      ),
      CreditBillResponse(
        id: 2,
        creditBillNumber: 'CR-102',
        customerId: 4,
        customerName: 'Sunita Rao',
        customerPhone: '+91 98765 44444',
        invoiceId: 104,
        invoiceNumber: 'INV-2026-004',
        amount: 17172.0,
        settledAmount: 0.0,
        balanceAmount: 17172.0,
        dueDate: now.subtract(const Duration(days: 2)), // Overdue
        status: 'Pending',
        createdAt: now.subtract(const Duration(days: 16)),
      ),
    ];
  }
}

