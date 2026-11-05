import 'package:flutter/material.dart';
import '../models/api_response.dart';
import '../models/notification_model.dart';
import '../services/api_service.dart';

class NotificationProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  List<NotificationResponse> _notifications = [];
  bool _isLoading = false;

  List<NotificationResponse> get notifications => _notifications;
  int get unreadCount => _notifications.where((n) => !n.isRead).length;
  bool get isLoading => _isLoading;

  Future<void> fetchNotifications() async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await _api.get('/notifications');
      if (response.statusCode == 200 && response.data != null) {
        final parsed = ApiResponse<List<NotificationResponse>>.fromJson(
          response.data,
          (data) => (data as List<dynamic>)
              .map((e) => NotificationResponse.fromJson(e as Map<String, dynamic>))
              .toList(),
        );
        _notifications = parsed.data ?? [];
      }
    } catch (e) {
      if (_notifications.isEmpty) {
        _notifications = _getSampleNotifications();
      }
    } finally {
      if (_notifications.isEmpty) {
        _notifications = _getSampleNotifications();
      }
      _isLoading = false;
      notifyListeners();
    }
  }

  void markAsRead(int notificationId) {
    final index = _notifications.indexWhere((n) => n.id == notificationId);
    if (index >= 0) {
      _notifications[index].isRead = true;
      notifyListeners();
    }
  }

  void markAllAsRead() {
    for (var n in _notifications) {
      n.isRead = true;
    }
    notifyListeners();
  }

  List<NotificationResponse> _getSampleNotifications() {
    final now = DateTime.now();
    return [
      NotificationResponse(
        id: 1,
        title: 'Low Stock Alert',
        message: 'Instant Arabica Coffee Beans 250g has reached low stock level (4 units left).',
        type: 'warning',
        isRead: false,
        createdAt: now.subtract(const Duration(minutes: 25)),
      ),
      NotificationResponse(
        id: 2,
        title: 'Out of Stock Alert',
        message: 'Stainless Steel Water Bottle 1L is completely out of stock!',
        type: 'error',
        isRead: false,
        createdAt: now.subtract(const Duration(hours: 2)),
      ),
      NotificationResponse(
        id: 3,
        title: 'Payment Received',
        message: 'Rahul Sharma paid ₹3,764 for invoice INV-2026-001 via UPI.',
        type: 'success',
        isRead: true,
        createdAt: now.subtract(const Duration(hours: 4)),
      ),
      NotificationResponse(
        id: 4,
        title: 'Credit Overdue Reminder',
        message: 'Sunita Rao credit bill CR-102 of ₹17,172 is overdue by 2 days.',
        type: 'warning',
        isRead: true,
        createdAt: now.subtract(const Duration(days: 1)),
      ),
    ];
  }
}

