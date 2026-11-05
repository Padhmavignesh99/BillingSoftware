import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../core/constants/app_constants.dart';
import '../models/user_model.dart';
import '../services/auth_service.dart';

class AuthProvider with ChangeNotifier {
  final AuthService _authService = AuthService();

  UserResponse? _currentUser;
  bool _isAuthenticated = false;
  bool _isLoading = true;
  bool _isOnboarded = true;
  String? _errorMessage;

  UserResponse? get currentUser => _currentUser;
  bool get isAuthenticated => _isAuthenticated;
  bool get isLoading => _isLoading;
  bool get isOnboarded => _isOnboarded;
  String? get errorMessage => _errorMessage;

  Future<void> checkAuthStatus() async {
    _isLoading = true;
    notifyListeners();

    try {
      final prefs = await SharedPreferences.getInstance();
      _isAuthenticated = await _authService.isAuthenticated();
      _isOnboarded = prefs.getBool(AppConstants.keyOnboarded) ?? true;
      if (_isAuthenticated) {
        _currentUser = await _authService.getCurrentUser();
      }
    } catch (e) {
      _isAuthenticated = false;
      _currentUser = null;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> login(String email, String password) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final authResponse = await _authService.login(email, password);
      _currentUser = authResponse.user;
      _isAuthenticated = true;
      _isLoading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<bool> register({
    required String businessName,
    required String fullName,
    required String email,
    required String password,
    String? phone,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final authResponse = await _authService.register(
        businessName: businessName,
        fullName: fullName,
        email: email,
        password: password,
        phone: phone,
      );
      _currentUser = authResponse.user;
      _isAuthenticated = true;
      _isOnboarded = false; // Trigger onboarding setup
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool(AppConstants.keyOnboarded, false);
      _isLoading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<void> completeOnboarding() async {
    _isOnboarded = true;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(AppConstants.keyOnboarded, true);
    notifyListeners();
  }

  Future<void> logout() async {
    await _authService.logout();
    _currentUser = null;
    _isAuthenticated = false;
    notifyListeners();
  }
}

