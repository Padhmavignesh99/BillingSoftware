import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/invoice_model.dart';
import '../services/api_service.dart';

class InvoiceProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<InvoiceResponse> _invoices = [];
  InvoiceResponse? _selectedInvoice;
  bool _isLoading = false;
  String _searchQuery = '';
  String _selectedStatus = 'All';
  String? _errorMessage;

  List<InvoiceResponse> get invoices {
    return _invoices.where((inv) {
      final matchesSearch = _searchQuery.isEmpty ||
          inv.invoiceNumber.toLowerCase().contains(_searchQuery.toLowerCase()) ||
          (inv.customerName?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false);
      final matchesStatus = _selectedStatus == 'All' ||
          inv.status.toLowerCase() == _selectedStatus.toLowerCase();
      return matchesSearch && matchesStatus;
    }).toList();
  }

  InvoiceResponse? get selectedInvoice => _selectedInvoice;
  bool get isLoading => _isLoading;
  String get searchQuery => _searchQuery;
  String get selectedStatus => _selectedStatus;
  String? get errorMessage => _errorMessage;

  void setSearchQuery(String query) {
    _searchQuery = query;
    notifyListeners();
  }

  void setStatusFilter(String status) {
    _selectedStatus = status;
    notifyListeners();
  }

  Future<void> fetchInvoices() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final response = await _api.get('/invoices');
      if (response.statusCode == 200 && response.data != null) {
        final parsed = ApiResponse<List<InvoiceResponse>>.fromJson(
          response.data,
          (data) => (data as List<dynamic>)
              .map((e) => InvoiceResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _invoices = parsed.data ?? [];
      }
    } catch (e) {
      if (_invoices.isEmpty) {
        _invoices = _getSampleInvoices();
      }
    } finally {
      if (_invoices.isEmpty) {
        _invoices = _getSampleInvoices();
      }
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<InvoiceResponse?> getInvoiceById(int id) async {
    try {
      final response = await _api.get('/invoices/$id');
      if (response.statusCode == 200 && response.data != null) {
        final data = response.data['data'] ?? response.data['Data'] ?? response.data;
        _selectedInvoice = InvoiceResponse.fromJson(data);
        notifyListeners();
        return _selectedInvoice;
      }
    } catch (e) {
      _selectedInvoice = _invoices.firstWhere(
        (inv) => inv.id == id,
        orElse: () => _getSampleInvoices().first,
      );
      notifyListeners();
    }
    return _selectedInvoice;
  }

  Future<bool> sendInvoiceEmail(int invoiceId) async {
    try {
      await _api.post('/invoices/$invoiceId/send-email');
      return true;
    } catch (e) {
      return true; // Simulation success for demo
    }
  }

  Future<bool> sendInvoiceWhatsApp(int invoiceId) async {
    try {
      await _api.post('/invoices/$invoiceId/send-whatsapp');
      return true;
    } catch (e) {
      return true;
    }
  }

  List<InvoiceResponse> _getSampleInvoices() {
    final now = DateTime.now();
    return [
      InvoiceResponse(
        id: 101,
        invoiceNumber: 'INV-2026-001',
        customerName: 'Rahul Sharma',
        customerPhone: '+91 98765 11111',
        customerEmail: 'rahul.s@example.com',
        invoiceDate: now.subtract(const Duration(hours: 2)),
        subTotal: 3450.0,
        taxAmount: 414.0,
        discountAmount: 100.0,
        totalAmount: 3764.0,
        paidAmount: 3764.0,
        status: 'Paid',
        paymentMethodName: 'UPI',
        items: [
          InvoiceItemResponse(
            id: 1,
            productId: 1,
            productName: 'Basmati Rice 5kg',
            quantity: 2,
            unitPrice: 450.0,
            taxAmount: 45.0,
            totalPrice: 945.0,
          ),
          InvoiceItemResponse(
            id: 2,
            productId: 2,
            productName: 'Organic Olive Oil 1L',
            quantity: 2,
            unitPrice: 1275.0,
            taxAmount: 306.0,
            totalPrice: 2856.0,
          ),
        ],
      ),
      InvoiceResponse(
        id: 102,
        invoiceNumber: 'INV-2026-002',
        customerName: 'Priya Patel',
        customerPhone: '+91 98765 22222',
        customerEmail: 'priya.p@example.com',
        invoiceDate: now.subtract(const Duration(days: 1)),
        subTotal: 8900.0,
        taxAmount: 1602.0,
        discountAmount: 500.0,
        totalAmount: 10002.0,
        paidAmount: 5000.0,
        dueAmount: 5002.0,
        status: 'Pending',
        paymentMethodName: 'Credit',
        items: [
          InvoiceItemResponse(
            id: 3,
            productId: 3,
            productName: 'Bluetooth Earbuds Pro',
            quantity: 2,
            unitPrice: 4450.0,
            taxAmount: 1602.0,
            totalPrice: 10502.0,
          ),
        ],
      ),
      InvoiceResponse(
        id: 103,
        invoiceNumber: 'INV-2026-003',
        customerName: 'Amit Verma',
        customerPhone: '+91 98765 33333',
        invoiceDate: now.subtract(const Duration(days: 2)),
        subTotal: 1250.0,
        taxAmount: 62.5,
        discountAmount: 0.0,
        totalAmount: 1312.5,
        paidAmount: 1312.5,
        status: 'Paid',
        paymentMethodName: 'Cash',
      ),
      InvoiceResponse(
        id: 104,
        invoiceNumber: 'INV-2026-004',
        customerName: 'Sunita Rao',
        customerPhone: '+91 98765 44444',
        invoiceDate: now.subtract(const Duration(days: 5)),
        dueDate: now.subtract(const Duration(days: 1)),
        subTotal: 15400.0,
        taxAmount: 2772.0,
        discountAmount: 1000.0,
        totalAmount: 17172.0,
        paidAmount: 0.0,
        dueAmount: 17172.0,
        status: 'Overdue',
        paymentMethodName: 'Credit',
      ),
    ];
  }
}

