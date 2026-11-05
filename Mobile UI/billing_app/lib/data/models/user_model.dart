class UserResponse {
  final int id;
  final String fullName;
  final String email;
  final String? phone;
  final String role;
  final int? tenantId;
  final String? profileImageUrl;
  final DateTime? lastLoginAt;
  final bool isActive;

  UserResponse({
    required this.id,
    required this.fullName,
    required this.email,
    this.phone,
    required this.role,
    this.tenantId,
    this.profileImageUrl,
    this.lastLoginAt,
    this.isActive = true,
  });

  factory UserResponse.fromJson(Map<String, dynamic> json) {
    return UserResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      fullName: json['fullName'] ?? json['FullName'] ?? '',
      email: json['email'] ?? json['Email'] ?? '',
      phone: json['phone'] ?? json['Phone'],
      role: json['role'] ?? json['Role'] ?? 'User',
      tenantId: json['tenantId'] ?? json['TenantId'],
      profileImageUrl: json['profileImageUrl'] ?? json['ProfileImageUrl'],
      lastLoginAt: json['lastLoginAt'] != null || json['LastLoginAt'] != null
          ? DateTime.tryParse(json['lastLoginAt'] ?? json['LastLoginAt'] ?? '')
          : null,
      isActive: json['isActive'] ?? json['IsActive'] ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'fullName': fullName,
      'email': email,
      'phone': phone,
      'role': role,
      'tenantId': tenantId,
      'profileImageUrl': profileImageUrl,
      'lastLoginAt': lastLoginAt?.toIso8601String(),
      'isActive': isActive,
    };
  }
}

class AuthResponse {
  final String token;
  final String refreshToken;
  final DateTime? expiration;
  final UserResponse user;

  AuthResponse({
    required this.token,
    required this.refreshToken,
    this.expiration,
    required this.user,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      token: json['token'] ?? json['Token'] ?? '',
      refreshToken: json['refreshToken'] ?? json['RefreshToken'] ?? '',
      expiration: json['expiration'] != null || json['Expiration'] != null
          ? DateTime.tryParse(json['expiration'] ?? json['Expiration'] ?? '')
          : null,
      user: UserResponse.fromJson(json['user'] ?? json['User'] ?? {}),
    );
  }
}

