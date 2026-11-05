import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../theme/app_theme.dart';

class AppFormatters {
  static String formatCurrency(double amount, {String symbol = '₹'}) {
    final formatter = NumberFormat.currency(
      locale: 'en_IN',
      symbol: symbol,
      decimalDigits: 2,
    );
    return formatter.format(amount);
  }

  static String formatDate(DateTime? date, {String pattern = 'dd MMM yyyy'}) {
    if (date == null) return '-';
    return DateFormat(pattern).format(date);
  }

  static String formatDateTime(DateTime? date, {String pattern = 'dd MMM yyyy, hh:mm a'}) {
    if (date == null) return '-';
    return DateFormat(pattern).format(date);
  }

  static String formatNumber(num value, {int decimals = 0}) {
    final formatter = NumberFormat.decimalPattern();
    if (decimals > 0) {
      return value.toStringAsFixed(decimals);
    }
    return formatter.format(value);
  }

  static Color getStatusColor(String? status) {
    if (status == null) return AppTheme.textMuted;
    switch (status.toLowerCase()) {
      case 'paid':
      case 'completed':
      case 'active':
      case 'approved':
      case 'delivered':
        return AppTheme.successColor;
      case 'pending':
      case 'partial':
      case 'in progress':
      case 'open':
        return AppTheme.warningColor;
      case 'overdue':
      case 'cancelled':
      case 'inactive':
      case 'rejected':
      case 'out of stock':
        return AppTheme.errorColor;
      case 'low stock':
        return Colors.orange;
      default:
        return AppTheme.infoColor;
    }
  }

  static Color getStatusBgColor(String? status) {
    return getStatusColor(status).withOpacity(0.12);
  }
}

