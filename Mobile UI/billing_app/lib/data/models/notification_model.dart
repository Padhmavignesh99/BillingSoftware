class NotificationResponse {
  final int id;
  final String title;
  final String message;
  final String type;
  bool isRead;
  final DateTime createdAt;
  final String? actionUrl;

  NotificationResponse({
    required this.id,
    required this.title,
    required this.message,
    this.type = 'info',
    this.isRead = false,
    required this.createdAt,
    this.actionUrl,
  });

  factory NotificationResponse.fromJson(Map<String, dynamic> json) {
    return NotificationResponse(
      id: json['id'] ?? json['Id'] ?? 0,
      title: json['title'] ?? json['Title'] ?? '',
      message: json['message'] ?? json['Message'] ?? '',
      type: json['type'] ?? json['Type'] ?? 'info',
      isRead: json['isRead'] ?? json['IsRead'] ?? false,
      createdAt: json['createdAt'] != null || json['CreatedAt'] != null
          ? DateTime.tryParse(json['createdAt'] ?? json['CreatedAt'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      actionUrl: json['actionUrl'] ?? json['ActionUrl'],
    );
  }
}

