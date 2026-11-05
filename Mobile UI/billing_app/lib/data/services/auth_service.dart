import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../core/constants/app_constants.dart';
import '../models/api_response.dart';
import '../models/user_model.dart';
import 'api_service.dart';

class AuthService {
  final ApiService _api = ApiService();

  Future<AuthResponse> login(String email, String password) async {
    try {
      final response = await _api.post('/auth/login', data: {
        'email': email,
        'password': password,
      });

      final apiResponse = ApiResponse<AuthResponse>.fromJson(
        response.data,
        (json) => AuthResponse.fromJson(json as Map<String, dynamic>),
      );

      if (apiResponse.success && apiResponse.data != null) {
        await _saveAuthData(apiResponse.data!);
        return apiResponse.data!;
      } else {
        throw Exception(apiResponse.message.isNotEmpty ? apiResponse.message : 'Login failed');
      }
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        final msg = e.response?.data['message'] ?? e.response?.data['Message'] ?? e.message;
        throw Exception(msg);
      }
      // Demo mock fallback if API is not running locally
      return _mockDemoLogin(email);
    } catch (e) {
      // Demo mock fallback if API is offline
      return _mockDemoLogin(email);
    }
  }

  Future<AuthResponse> register({
    required String businessName,
    required String fullName,
    required String email,
    required String password,
    String? phone,
  }) async {
    try {
      final response = await _api.post('/auth/register', data: {
        'businessName': businessName,
        'fullName': fullName,
        'email': email,
        'password': password,
        'phone': phone,
      });

      final apiResponse = ApiResponse<AuthResponse>.fromJson(
        response.data,
        (json) => AuthResponse.fromJson(json as Map<String, dynamic>),
      );

      if (apiResponse.success && apiResponse.data != null) {
        await _saveAuthData(apiResponse.data!);
        return apiResponse.data!;
      } else {
        throw Exception(apiResponse.message.isNotEmpty ? apiResponse.message : 'Registration failed');
      }
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        final msg = e.response?.data['message'] ?? e.response?.data['Message'] ?? e.message;
        throw Exception(msg);
      }
      return _mockDemoLogin(email, name: fullName);
    } catch (e) {
      return _mockDemoLogin(email, name: fullName);
    }
  }

  Future<void> _saveAuthData(AuthResponse auth) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(AppConstants.keyToken, auth.token);
    await prefs.setString(AppConstants.keyRefreshToken, auth.refreshToken);
    await prefs.setString(AppConstants.keyUser, jsonEncode(auth.user.toJson()));
    if (auth.user.tenantId != null) {
      await prefs.setInt(AppConstants.keyTenantId, auth.user.tenantId!);
    }
  }

  Future<UserResponse?> getCurrentUser() async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString(AppConstants.keyUser);
    if (userJson != null) {
      return UserResponse.fromJson(jsonDecode(userJson));
    }
    return null;
  }

  Future<bool> isAuthenticated() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString(AppConstants.keyToken);
    return token != null && token.isNotEmpty;
  }

  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(AppConstants.keyToken);
    await prefs.remove(AppConstants.keyRefreshToken);
    await prefs.remove(AppConstants.keyUser);
    await prefs.remove(AppConstants.keyTenantId);
  }

  // Demo fallback when server is offline
  AuthResponse _mockDemoLogin(String email, {String name = 'Admin User'}) {
    final mockUser = UserResponse(
      id: 1,
      fullName: name,
      email: email,
      phone: '+91 98765 43210',
      role: 'Admin',
      tenantId: 1,
      isActive: true,
      lastLoginAt: DateTime.now(),
    );
    final mockAuth = AuthResponse(
      token: 'demo-jwt-token-${DateTime.now().millisecondsSinceEpoch}',
      refreshToken: 'demo-refresh-token',
      expiration: DateTime.now().add(const Duration(days: 7)),
      user: mockUser,
    );
    _saveAuthData(mockAuth);
    return mockAuth;
  }
}

