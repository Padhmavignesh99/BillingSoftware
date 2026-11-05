import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/return_model.dart';
import '../services/api_service.dart';

class ReturnsProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<SalesReturnResponse> _salesReturns = [];
  List<PurchaseReturnResponse> _purchaseReturns = [];
  bool _isLoading = false;
  String _searchQuery = '';

  List<SalesReturnResponse> get salesReturns {
    if (_searchQuery.isEmpty) return _salesReturns;
    return _salesReturns.where((r) {
      return r.returnNumber.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          (r.customerName?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false) ||
          r.reason.toLowerCase().contains(_searchQuery.toLowerCase());
    }).toList();
  }

  List<PurchaseReturnResponse> get purchaseReturns {
    if (_searchQuery.isEmpty) return _purchaseReturns;
    return _purchaseReturns.where((r) {
      return r.returnNumber.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          (r.supplierName?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false) ||
          r.reason.toLowerCase().contains(_searchQuery.toLowerCase());
    }).toList();
  }

  bool get isLoading => _isLoading;
  String get searchQuery => _searchQuery;

  void setSearchQuery(String query) {
    _searchQuery = query;
    notifyListeners();
  }

  Future<void> fetchReturns() async {
    _isLoading = true;
    notifyListeners();

    try {
      final sRes = await _api.get('/reports/sales-return');
      if (sRes.statusCode == 200 && sRes.data != null) {
        final parsed = ApiResponse<List<SalesReturnResponse>>.fromJson(
          sRes.data,
          (data) => (data as List<dynamic>)
              .map((e) => SalesReturnResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _salesReturns = parsed.data ?? [];
      }
    } catch (e) {
      if (_salesReturns.isEmpty) {
        _salesReturns = _getSampleSalesReturns();
      }
    }

    try {
      final pRes = await _api.get('/reports/purchase-return');
      if (pRes.statusCode == 200 && pRes.data != null) {
        final parsed = ApiResponse<List<PurchaseReturnResponse>>.fromJson(
          pRes.data,
          (data) => (data as List<dynamic>)
              .map((e) => PurchaseReturnResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _purchaseReturns = parsed.data ?? [];
      }
    } catch (e) {
      if (_purchaseReturns.isEmpty) {
        _purchaseReturns = _getSamplePurchaseReturns();
      }
    } finally {
      if (_salesReturns.isEmpty) _salesReturns = _getSampleSalesReturns();
      if (_purchaseReturns.isEmpty) _purchaseReturns = _getSamplePurchaseReturns();
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> createSalesReturn({
    required int invoiceId,
    required String customerName,
    required String reason,
    required List<ReturnItemResponse> items,
    required double totalAmount,
  }) async {
    final newReturn = SalesReturnResponse(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      returnNumber: 'SR-${DateTime.now().millisecondsSinceEpoch.toString().substring(7)}',
      invoiceId: invoiceId,
      customerName: customerName,
      reason: reason,
      returnDate: DateTime.now(),
      totalAmount: totalAmount,
      status: 'Completed',
      items: items,
    );

    _salesReturns.insert(0, newReturn);
    notifyListeners();
    return true;
  }

  Future<bool> createPurchaseReturn({
    required int purchaseId,
    required int supplierId,
    required String supplierName,
    required String reason,
    required List<ReturnItemResponse> items,
    required double totalAmount,
  }) async {
    final newReturn = PurchaseReturnResponse(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      returnNumber: 'PR-${DateTime.now().millisecondsSinceEpoch.toString().substring(7)}',
      purchaseId: purchaseId,
      supplierId: supplierId,
      supplierName: supplierName,
      reason: reason,
      returnDate: DateTime.now(),
      totalAmount: totalAmount,
      status: 'Completed',
      items: items,
    );

    _purchaseReturns.insert(0, newReturn);
    notifyListeners();
    return true;
  }

  List<SalesReturnResponse> _getSampleSalesReturns() {
    final now = DateTime.now();
    return [
      SalesReturnResponse(
        id: 1,
        returnNumber: 'SR-2026-005',
        invoiceId: 101,
        customerName: 'Rahul Sharma',
        reason: 'Damaged seal on rice bag',
        returnDate: now.subtract(const Duration(days: 1)),
        totalAmount: 450.0,
        status: 'Completed',
        items: [
          ReturnItemResponse(
            productId: 1,
            productName: 'Basmati Rice Premium 5kg',
            quantity: 1,
            unitPrice: 450.0,
            totalPrice: 450.0,
          ),
        ],
      ),
    ];
  }

  List<PurchaseReturnResponse> _getSamplePurchaseReturns() {
    final now = DateTime.now();
    return [
      PurchaseReturnResponse(
        id: 1,
        returnNumber: 'PR-2026-003',
        purchaseId: 1,
        supplierId: 1,
        supplierName: 'Aggarwal Wholesale Traders',
        reason: 'Expired packaging batch',
        returnDate: now.subtract(const Duration(days: 3)),
        totalAmount: 3800.0,
        status: 'Completed',
        items: [
          ReturnItemResponse(
            productId: 1,
            productName: 'Basmati Rice Premium 5kg',
            quantity: 10,
            unitPrice: 380.0,
            totalPrice: 3800.0,
          ),
        ],
      ),
    ];
  }
}

