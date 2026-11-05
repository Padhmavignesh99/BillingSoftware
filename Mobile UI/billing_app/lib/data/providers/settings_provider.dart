import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../core/constants/app_constants.dart';
import '../models/business_model.dart';
import '../services/api_service.dart';

class SettingsProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  ThemeMode _themeMode = ThemeMode.light;
  String _baseUrl = AppConstants.defaultBaseUrl;
  String _currencySymbol = '₹';
  BusinessResponse _business = BusinessResponse(
    id: 1,
    name: 'SuperMart Retailers',
    legalName: 'SuperMart Private Limited',
    businessTypeName: 'Retail & Wholesale',
    email: 'contact@supermart.com',
    phone: '+91 98765 00000',
    address: 'Plot No. 42, Commercial Complex, MG Road',
    city: 'Bengaluru',
    state: 'Karnataka',
    pinCode: '560001',
    gstNumber: '29ABCDE1234F1Z5',
    currencySymbol: '₹',
  );

  ThemeMode get themeMode => _themeMode;
  bool get isDarkMode => _themeMode == ThemeMode.dark;
  String get baseUrl => _baseUrl;
  String get currencySymbol => _currencySymbol;
  BusinessResponse get business => _business;

  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    final isDark = prefs.getBool(AppConstants.keyThemeMode) ?? false;
    _themeMode = isDark ? ThemeMode.dark : ThemeMode.light;

    final savedUrl = prefs.getString(AppConstants.keyBaseUrl);
    if (savedUrl != null && savedUrl.isNotEmpty) {
      _baseUrl = savedUrl;
    }

    final savedCurrency = prefs.getString(AppConstants.keyCurrency);
    if (savedCurrency != null && savedCurrency.isNotEmpty) {
      _currencySymbol = savedCurrency;
    }

    final savedBiz = prefs.getString('saved_business_profile');
    if (savedBiz != null) {
      try {
        _business = BusinessResponse.fromJson(jsonDecode(savedBiz));
      } catch (_) {}
    }

    notifyListeners();
  }

  void toggleTheme(bool isDark) async {
    _themeMode = isDark ? ThemeMode.dark : ThemeMode.light;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(AppConstants.keyThemeMode, isDark);
    notifyListeners();
  }

  Future<void> updateBaseUrl(String newUrl) async {
    _baseUrl = newUrl;
    await _api.updateBaseUrl(newUrl);
    notifyListeners();
  }

  Future<void> updateCurrency(String symbol) async {
    _currencySymbol = symbol;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(AppConstants.keyCurrency, symbol);
    notifyListeners();
  }

  Future<void> updateBusinessProfile(BusinessResponse updated) async {
    _business = updated;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('saved_business_profile', jsonEncode(updated.toJson()));
    notifyListeners();
  }
}

