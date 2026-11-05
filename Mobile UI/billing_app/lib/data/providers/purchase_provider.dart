import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/purchase_model.dart';
import '../services/api_service.dart';

class PurchaseProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<PurchaseResponse> _purchases = [];
  bool _isLoading = false;
  String _searchQuery = '';

  List<PurchaseResponse> get purchases {
    if (_searchQuery.isEmpty) return _purchases;
    return _purchases.where((p) {
      return p.purchaseNumber.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          (p.supplierName?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false);
    }).toList();
  }

  bool get isLoading => _isLoading;
  String get searchQuery => _searchQuery;

  void setSearchQuery(String query) {
    _searchQuery = query;
    notifyListeners();
  }

  Future<void> fetchPurchases() async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _api.get('/purchases');
      if (response.statusCode == 200 && response.data != null) {
        final parsed = ApiResponse<List<PurchaseResponse>>.fromJson(
          response.data,
          (data) => (data as List<dynamic>)
              .map((e) => PurchaseResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _purchases = parsed.data ?? [];
      }
    } catch (e) {
      if (_purchases.isEmpty) {
        _purchases = _getSamplePurchases();
      }
    } finally {
      if (_purchases.isEmpty) {
        _purchases = _getSamplePurchases();
      }
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> createPurchase({
    required int supplierId,
    required String supplierName,
    required List<PurchaseItemResponse> items,
    required double totalAmount,
  }) async {
    final newPurchase = PurchaseResponse(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      supplierId: supplierId,
      purchaseNumber: 'PO-${DateTime.now().millisecondsSinceEpoch.toString().substring(7)}',
      supplierName: supplierName,
      purchaseDate: DateTime.now(),
      subTotal: totalAmount,
      totalAmount: totalAmount,
      paidAmount: totalAmount,
      status: 'Completed',
      items: items,
    );

    _purchases.insert(0, newPurchase);
    notifyListeners();
    return true;
  }

  List<PurchaseResponse> _getSamplePurchases() {
    final now = DateTime.now();
    return [
      PurchaseResponse(
        id: 1,
        supplierId: 1,
        purchaseNumber: 'PO-2026-081',
        supplierName: 'Aggarwal Wholesale Traders',
        purchaseDate: now.subtract(const Duration(days: 2)),
        subTotal: 45000.0,
        taxAmount: 2250.0,
        totalAmount: 47250.0,
        paidAmount: 47250.0,
        status: 'Completed',
        items: [
          PurchaseItemResponse(
            productId: 1,
            productName: 'Basmati Rice Premium 5kg',
            quantity: 100,
            unitPrice: 380.0,
            totalPrice: 38000.0,
          ),
        ],
      ),
      PurchaseResponse(
        id: 2,
        supplierId: 3,
        purchaseNumber: 'PO-2026-082',
        supplierName: 'Techno Imports India',
        purchaseDate: now.subtract(const Duration(days: 7)),
        subTotal: 84000.0,
        taxAmount: 15120.0,
        totalAmount: 99120.0,
        paidAmount: 50000.0,
        dueAmount: 49120.0,
        status: 'Pending',
        items: [
          PurchaseItemResponse(
            productId: 3,
            productName: 'Bluetooth Earbuds Pro Wireless',
            quantity: 30,
            unitPrice: 2800.0,
            totalPrice: 84000.0,
          ),
        ],
      ),
    ];
  }
}

