import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/formatters.dart';
import '../../models/notification_model.dart';
import '../../providers/notification_provider.dart';
import '../widgets/common/empty_state.dart';
import '../widgets/common/loading_widget.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  String _selectedFilter = 'all'; // all, unread, alerts

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<NotificationProvider>().fetchNotifications();
    });
  }

  @override
  Widget build(BuildContext context) {
    final notifProvider = context.watch<NotificationProvider>();
    final allNotifications = notifProvider.notifications;

    final filteredList = allNotifications.where((n) {
      if (_selectedFilter == 'unread') {
        return !n.isRead;
      }
      if (_selectedFilter == 'alerts') {
        return n.type == 'warning' || n.type == 'error';
      }
      return true;
    }).toList();

    return Scaffold(
      appBar: AppBar(
        title: const Text('Notifications'),
        actions: [
          if (notifProvider.unreadCount > 0)
            TextButton.icon(
              onPressed: () => notifProvider.markAllAsRead(),
              icon: const Icon(Icons.done_all, color: Colors.white, size: 18),
              label: const Text(
                'Mark All Read',
                style: TextStyle(color: Colors.white, fontSize: 13),
              ),
            ),
          const SizedBox(width: 8),
        ],
      ),
      body: Column(
        children: [
          // Filter Chips
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
            color: Theme.of(context).cardColor,
            child: Row(
              children: [
                _filterChip('all', 'All (${allNotifications.length})'),
                const SizedBox(width: 8),
                _filterChip('unread', 'Unread (${notifProvider.unreadCount})'),
                const SizedBox(width: 8),
                _filterChip('alerts', 'Alerts'),
              ],
            ),
          ),
          const Divider(height: 1),

          // Notification List
          Expanded(
            child: notifProvider.isLoading && allNotifications.isEmpty
                ? const LoadingWidget(message: 'Loading notifications...')
                : RefreshIndicator(
                    onRefresh: () => notifProvider.fetchNotifications(),
                    child: filteredList.isEmpty
                        ? const EmptyState(
                            icon: Icons.notifications_off_outlined,
                            title: 'No Notifications',
                            message: 'You have caught up with all alerts and updates!',
                          )
                        : ListView.separated(
                            itemCount: filteredList.length,
                            separatorBuilder: (context, index) => const Divider(height: 1),
                            itemBuilder: (context, index) {
                              final item = filteredList[index];
                              return _buildNotificationTile(item, notifProvider);
                            },
                          ),
                  ),
          ),
        ],
      ),
    );
  }

  Widget _filterChip(String filterKey, String label) {
    final isSelected = _selectedFilter == filterKey;
    return ChoiceChip(
      label: Text(label),
      selected: isSelected,
      selectedColor: AppTheme.primaryColor.withOpacity(0.15),
      labelStyle: TextStyle(
        fontSize: 12,
        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
        color: isSelected ? AppTheme.primaryColor : AppTheme.textMuted,
      ),
      onSelected: (selected) {
        if (selected) {
          setState(() {
            _selectedFilter = filterKey;
          });
        }
      },
    );
  }

  Widget _buildNotificationTile(NotificationResponse item, NotificationProvider provider) {
    final typeData = _getTypeData(item.type);

    return InkWell(
      onTap: () {
        if (!item.isRead) {
          provider.markAsRead(item.id);
        }
      },
      child: Container(
        color: item.isRead
            ? Colors.transparent
            : AppTheme.primaryColor.withOpacity(0.04),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Icon
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: typeData.color.withOpacity(0.12),
                shape: BoxShape.circle,
              ),
              child: Icon(typeData.icon, color: typeData.color, size: 20),
            ),
            const SizedBox(width: 14),

            // Content
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Text(
                          item.title,
                          style: TextStyle(
                            fontSize: 14,
                            fontWeight: item.isRead ? FontWeight.w500 : FontWeight.bold,
                            color: AppTheme.textDark,
                          ),
                        ),
                      ),
                      const SizedBox(width: 8),
                      Text(
                        _formatTimestamp(item.createdAt),
                        style: const TextStyle(
                          fontSize: 11,
                          color: AppTheme.textMuted,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    item.message,
                    style: TextStyle(
                      fontSize: 13,
                      color: item.isRead ? AppTheme.textMuted : AppTheme.textDark,
                    ),
                  ),
                ],
              ),
            ),

            // Unread dot
            if (!item.isRead) ...[
              const SizedBox(width: 10),
              Container(
                margin: const EdgeInsets.only(top: 4),
                width: 8,
                height: 8,
                decoration: const BoxDecoration(
                  color: AppTheme.primaryColor,
                  shape: BoxShape.circle,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  String _formatTimestamp(DateTime? dateTime) {
    if (dateTime == null) return '';
    final difference = DateTime.now().difference(dateTime);

    if (difference.inMinutes < 60) {
      return '${difference.inMinutes.clamp(1, 60)}m ago';
    } else if (difference.inHours < 24) {
      return '${difference.inHours}h ago';
    } else if (difference.inDays < 7) {
      return '${difference.inDays}d ago';
    } else {
      return DateFormat('dd MMM').format(dateTime);
    }
  }

  _TypeData _getTypeData(String? type) {
    switch (type?.toLowerCase()) {
      case 'warning':
        return _TypeData(icon: Icons.warning_amber_rounded, color: AppTheme.warningColor);
      case 'error':
        return _TypeData(icon: Icons.error_outline, color: AppTheme.errorColor);
      case 'success':
        return _TypeData(icon: Icons.check_circle_outline, color: AppTheme.successColor);
      case 'info':
      default:
        return _TypeData(icon: Icons.info_outline, color: AppTheme.primaryColor);
    }
  }
}

class _TypeData {
  final IconData icon;
  final Color color;

  _TypeData({required this.icon, required this.color});
}

