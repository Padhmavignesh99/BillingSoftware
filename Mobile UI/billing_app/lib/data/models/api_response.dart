class ApiResponse<T> {
  final bool success;
  final String message;
  final T? data;
  final List<String>? errors;
  final int? totalCount;
  final int? page;
  final int? pageSize;

  ApiResponse({
    required this.success,
    this.message = '',
    this.data,
    this.errors,
    this.totalCount,
    this.page,
    this.pageSize,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic json)? fromJsonT,
  ) {
    return ApiResponse<T>(
      success: json['success'] ?? json['Success'] ?? false,
      message: json['message'] ?? json['Message'] ?? '',
      data: json['data'] != null && fromJsonT != null
          ? fromJsonT(json['data'] ?? json['Data'])
          : (json['data'] ?? json['Data']) as T?,
      errors: (json['errors'] ?? json['Errors'] as List<dynamic>?)
          ?.map((e) => e.toString())
          .toList(),
      totalCount: json['totalCount'] ?? json['TotalCount'],
      page: json['page'] ?? json['Page'],
      pageSize: json['pageSize'] ?? json['PageSize'],
    );
  }
}

