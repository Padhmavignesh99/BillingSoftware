import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'package:badges/badges.dart' as badges;
import '../../../core/theme/app_theme.dart';
import '../../providers/notification_provider.dart';
import '../../providers/settings_provider.dart';
import 'app_drawer.dart';
import 'bottom_nav.dart';

class ScaffoldWithNav extends StatefulWidget {
  final Widget child;

  const ScaffoldWithNav({super.key, required this.child});

  @override
  State<ScaffoldWithNav> createState() => _ScaffoldWithNavState();
}

class _ScaffoldWithNavState extends State<ScaffoldWithNav> {
  final GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>();

  int _calculateSelectedIndex(BuildContext context) {
    final location = GoRouterState.of(context).uri.toString();
    if (location.startsWith('/dashboard')) return 0;
    if (location.startsWith('/billing')) return 1;
    if (location.startsWith('/inventory')) return 2;
    if (location.startsWith('/reports')) return 3;
    return 0;
  }

  void _onItemTapped(int index, BuildContext context) {
    switch (index) {
      case 0:
        context.go('/dashboard');
        break;
      case 1:
        context.go('/billing');
        break;
      case 2:
        context.go('/inventory');
        break;
      case 3:
        context.go('/reports');
        break;
      case 4:
        _scaffoldKey.currentState?.openEndDrawer();
        break;
    }
  }

  @override
  Widget build(BuildContext context) {
    final settings = context.watch<SettingsProvider>();
    final notifs = context.watch<NotificationProvider>();
    final unreadCount = notifs.unreadCount;
    final location = GoRouterState.of(context).uri.toString();
    final bool showTopAppBar = location.startsWith('/dashboard') || location.startsWith('/billing');

    return Scaffold(
      key: _scaffoldKey,
      appBar: showTopAppBar
          ? AppBar(
              title: Text(settings.business.name),
              actions: [
                IconButton(
                  icon: badges.Badge(
                    showBadge: unreadCount > 0,
                    badgeContent: Text(
                      unreadCount.toString(),
                      style: const TextStyle(color: Colors.white, fontSize: 10),
                    ),
                    child: const Icon(Icons.notifications_outlined),
                  ),
                  tooltip: 'Notifications',
                  onPressed: () => context.push('/notifications'),
                ),
                IconButton(
                  icon: const Icon(Icons.settings_outlined),
                  tooltip: 'Settings',
                  onPressed: () => context.push('/settings'),
                ),
                const SizedBox(width: 8),
              ],
            )
          : null,
      drawer: const AppDrawer(),
      endDrawer: const AppDrawer(),
      body: widget.child,
      bottomNavigationBar: AppBottomNav(
        currentIndex: _calculateSelectedIndex(context),
        onTap: (index) => _onItemTapped(index, context),
      ),
    );
  }
}
