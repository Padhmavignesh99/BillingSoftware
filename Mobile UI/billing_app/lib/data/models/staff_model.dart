import 'user_model.dart';

class PermissionResponse {
  final int id;
  final String name;
  final String? module;
  final String? description;
  final String? action;
  final String key;
  final bool isGranted;

  PermissionResponse({
    required this.id,
    required this.name,
    this.module,
    this.description,
    this.action,
    this.key = '',
    this.isGranted = true,
  });

  factory PermissionResponse.fromJson(Map<String, dynamic> json) {
    return PermissionResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      module: json['module'] ?? json['Module'],
      description: json['description'] ?? json['Description'],
      action: json['action'] ?? json['Action'],
      key: json['key'] ?? json['Key'] ?? '',
      isGranted: json['isGranted'] ?? json['IsGranted'] ?? true,
    );
  }
}

class RoleResponse {
  final int id;
  final String name;
  final String? description;
  final List<PermissionResponse> permissions;

  RoleResponse({
    required this.id,
    required this.name,
    this.description,
    this.permissions = const [],
  });

  factory RoleResponse.fromJson(Map<String, dynamic> json) {
    return RoleResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      name: json['name'] ?? json['Name'] ?? '',
      description: json['description'] ?? json['Description'],
      permissions: (json['permissions'] ?? json['Permissions'] as List<dynamic>?)
              ?.map((e) => PermissionResponse.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class StaffResponse {
  final int id;
  final UserResponse user;
  final String? department;
  final String? designation;
  final double? salary;
  final DateTime? joiningDate;
  final int? branchId;
  final String? branchName;

  StaffResponse({
    required this.id,
    required this.user,
    this.department,
    this.designation,
    this.salary,
    this.joiningDate,
    this.branchId,
    this.branchName,
  });

  factory StaffResponse.fromJson(Map<String, dynamic> json) {
    return StaffResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      user: UserResponse.fromJson(json['user'] ?? json['User'] ?? {}),
      department: json['department'] ?? json['Department'],
      designation: json['designation'] ?? json['Designation'],
      salary: json['salary'] != null || json['Salary'] != null
          ? ((json['salary'] ?? json['Salary']) as num).toDouble()
          : null,
      joiningDate: json['joiningDate'] != null || json['JoiningDate'] != null
          ? DateTime.tryParse(json['joiningDate'] ?? json['JoiningDate'] ?? '')
          : null,
      branchId: json['branchId'] ?? json['BranchId'],
      branchName: json['branchName'] ?? json['BranchName'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'fullName': user.fullName,
      'email': user.email,
      'phone': user.phone,
      'role': user.role,
      'department': department,
      'designation': designation,
      'salary': salary,
      'joiningDate': joiningDate?.toIso8601String(),
    };
  }
}

