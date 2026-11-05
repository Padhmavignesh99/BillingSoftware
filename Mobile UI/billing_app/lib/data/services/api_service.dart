import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../core/constants/app_constants.dart';

class ApiService {
  static final ApiService _instance = ApiService._internal();
  factory ApiService() => _instance;

  late Dio _dio;
  String _baseUrl = AppConstants.defaultBaseUrl;

  Dio get dio => _dio;
  String get baseUrl => _baseUrl;

  ApiService._internal() {
    _dio = Dio(
      BaseOptions(
        baseUrl: _baseUrl,
        connectTimeout: const Duration(seconds: 15),
        receiveTimeout: const Duration(seconds: 15),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );

    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final prefs = await SharedPreferences.getInstance();
          final token = prefs.getString(AppConstants.keyToken);
          final tenantId = prefs.getInt(AppConstants.keyTenantId);

          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          if (tenantId != null && tenantId > 0) {
            options.headers['X-Tenant-Id'] = tenantId.toString();
          }
          return handler.next(options);
        },
        onError: (DioException error, handler) {
          // Pass the error through for handling in providers
          return handler.next(error);
        },
      ),
    );
  }

  Future<void> updateBaseUrl(String newUrl) async {
    _baseUrl = newUrl;
    _dio.options.baseUrl = newUrl;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(AppConstants.keyBaseUrl, newUrl);
  }

  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    final savedUrl = prefs.getString(AppConstants.keyBaseUrl);
    if (savedUrl != null && savedUrl.isNotEmpty) {
      _baseUrl = savedUrl;
      _dio.options.baseUrl = savedUrl;
    }
  }

  // HTTP Helper Methods
  Future<Response> get(String path, {Map<String, dynamic>? queryParameters}) async {
    return await _dio.get(path, queryParameters: queryParameters);
  }

  Future<Response> post(String path, {dynamic data, Map<String, dynamic>? queryParameters}) async {
    return await _dio.post(path, data: data, queryParameters: queryParameters);
  }

  Future<Response> put(String path, {dynamic data, Map<String, dynamic>? queryParameters}) async {
    return await _dio.put(path, data: data, queryParameters: queryParameters);
  }

  Future<Response> delete(String path, {dynamic data, Map<String, dynamic>? queryParameters}) async {
    return await _dio.delete(path, data: data, queryParameters: queryParameters);
  }
}

