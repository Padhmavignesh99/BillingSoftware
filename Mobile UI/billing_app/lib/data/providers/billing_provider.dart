import 'package:flutter/material.dart';
import '../models/customer_model.dart';
import '../models/invoice_model.dart';
import '../models/product_model.dart';
import '../services/api_service.dart';

class CartItem {
  final ProductResponse product;
  double quantity;
  double unitPrice;
  double discount;
  double taxPercent;

  CartItem({
    required this.product,
    this.quantity = 1,
    required this.unitPrice,
    this.discount = 0.0,
    double? taxPercent,
  }) : taxPercent = taxPercent ?? product.taxPercentage ?? 0.0;

  double get subTotal => (unitPrice * quantity) - discount;
  double get taxAmount => subTotal * (taxPercent / 100);
  double get totalPrice => subTotal + taxAmount;

  InvoiceItemResponse toInvoiceItem() {
    return InvoiceItemResponse(
      productId: product.id,
      productName: product.name,
      hsnCode: product.hsnCode,
      quantity: quantity,
      unitPrice: unitPrice,
      discountAmount: discount,
      taxAmount: taxAmount,
      taxPercentage: taxPercent,
      totalPrice: totalPrice,
    );
  }
}

class BillingProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  final List<CartItem> _cart = [];
  CustomerResponse? _selectedCustomer;
  String _customerName = 'Walk-in Customer';
  String _customerPhone = '';
  String _paymentMethod = 'Cash';
  double _overallDiscount = 0.0;
  String _notes = '';
  bool _isProcessing = false;
  String? _lastInvoiceNumber;

  List<CartItem> get cart => List.unmodifiable(_cart);
  CustomerResponse? get selectedCustomer => _selectedCustomer;
  String get customerName => _customerName;
  String get customerPhone => _customerPhone;
  String get paymentMethod => _paymentMethod;
  double get overallDiscount => _overallDiscount;
  String get notes => _notes;
  bool get isProcessing => _isProcessing;
  String? get lastInvoiceNumber => _lastInvoiceNumber;

  int get itemCount => _cart.fold(0, (sum, item) => sum + item.quantity.toInt());

  double get subTotal => _cart.fold(0.0, (sum, item) => sum + (item.unitPrice * item.quantity));
  double get itemDiscountTotal => _cart.fold(0.0, (sum, item) => sum + item.discount);
  double get totalDiscount => itemDiscountTotal + _overallDiscount;
  double get totalTax => _cart.fold(0.0, (sum, item) => sum + item.taxAmount);
  double get totalAmount => (subTotal - totalDiscount + totalTax).clamp(0.0, double.infinity);

  void addToCart(ProductResponse product) {
    final existingIndex = _cart.indexWhere((item) => item.product.id == product.id);
    if (existingIndex >= 0) {
      _cart[existingIndex].quantity += 1;
    } else {
      _cart.add(CartItem(
        product: product,
        quantity: 1,
        unitPrice: product.sellingPrice,
      ));
    }
    notifyListeners();
  }

  void removeFromCart(int productId) {
    _cart.removeWhere((item) => item.product.id == productId);
    notifyListeners();
  }

  void updateQuantity(int productId, double quantity) {
    final index = _cart.indexWhere((item) => item.product.id == productId);
    if (index >= 0) {
      if (quantity <= 0) {
        _cart.removeAt(index);
      } else {
        _cart[index].quantity = quantity;
      }
      notifyListeners();
    }
  }

  void updateItemDiscount(int productId, double discount) {
    final index = _cart.indexWhere((item) => item.product.id == productId);
    if (index >= 0) {
      _cart[index].discount = discount;
      notifyListeners();
    }
  }

  void setCustomer(CustomerResponse? customer) {
    _selectedCustomer = customer;
    if (customer != null) {
      _customerName = customer.name;
      _customerPhone = customer.phone ?? '';
    } else {
      _customerName = 'Walk-in Customer';
      _customerPhone = '';
    }
    notifyListeners();
  }

  void setCustomCustomer(String name, String phone) {
    _selectedCustomer = null;
    _customerName = name.isEmpty ? 'Walk-in Customer' : name;
    _customerPhone = phone;
    notifyListeners();
  }

  void setPaymentMethod(String method) {
    _paymentMethod = method;
    notifyListeners();
  }

  void setOverallDiscount(double discount) {
    _overallDiscount = discount;
    notifyListeners();
  }

  void setNotes(String noteText) {
    _notes = noteText;
    notifyListeners();
  }

  Future<InvoiceResponse?> checkout() async {
    if (_cart.isEmpty) return null;

    _isProcessing = true;
    notifyListeners();

    try {
      final items = _cart.map((c) => c.toInvoiceItem()).toList();
      final billRequest = POSBillRequest(
        customerId: _selectedCustomer?.id,
        customerName: _customerName,
        customerPhone: _customerPhone,
        items: items,
        discountAmount: totalDiscount,
        paymentMethod: _paymentMethod,
        paidAmount: totalAmount,
        notes: _notes,
      );

      final response = await _api.post('/pos/bill', data: billRequest.toJson());
      InvoiceResponse createdInvoice;

      if (response.statusCode == 200 && response.data != null) {
        final data = response.data['data'] ?? response.data['Data'] ?? response.data;
        createdInvoice = InvoiceResponse.fromJson(data);
      } else {
        createdInvoice = _createLocalInvoice();
      }

      _lastInvoiceNumber = createdInvoice.invoiceNumber;
      clearCart();
      _isProcessing = false;
      notifyListeners();
      return createdInvoice;
    } catch (e) {
      // Local fallback for offline mode
      final createdInvoice = _createLocalInvoice();
      _lastInvoiceNumber = createdInvoice.invoiceNumber;
      clearCart();
      _isProcessing = false;
      notifyListeners();
      return createdInvoice;
    }
  }

  InvoiceResponse _createLocalInvoice() {
    final invNum = 'INV-${DateTime.now().millisecondsSinceEpoch.toString().substring(7)}';
    return InvoiceResponse(
      id: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      customerId: _selectedCustomer?.id,
      invoiceNumber: invNum,
      customerName: _customerName,
      customerPhone: _customerPhone,
      invoiceDate: DateTime.now(),
      subTotal: subTotal,
      taxAmount: totalTax,
      discountAmount: totalDiscount,
      totalAmount: totalAmount,
      paidAmount: totalAmount,
      status: 'Paid',
      paymentMethodName: _paymentMethod,
      items: _cart.map((c) => c.toInvoiceItem()).toList(),
    );
  }

  void clearCart() {
    _cart.clear();
    _selectedCustomer = null;
    _customerName = 'Walk-in Customer';
    _customerPhone = '';
    _paymentMethod = 'Cash';
    _overallDiscount = 0.0;
    _notes = '';
    notifyListeners();
  }
}

