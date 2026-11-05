import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../providers/auth_provider.dart';
import '../../providers/settings_provider.dart';

class AppDrawer extends StatelessWidget {
  const AppDrawer({super.key});

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final settings = context.watch<SettingsProvider>();
    final user = auth.currentUser;
    final business = settings.business;

    return Drawer(
      child: SafeArea(
        child: Column(
          children: [
            // Business Header
            Container(
              padding: const EdgeInsets.all(20),
              decoration: const BoxDecoration(
                color: AppTheme.primaryColor,
              ),
              child: Row(
                children: [
                  CircleAvatar(
                    radius: 28,
                    backgroundColor: Colors.white,
                    child: Text(
                      business.name.isNotEmpty ? business.name[0].toUpperCase() : 'B',
                      style: const TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                        color: AppTheme.primaryColor,
                      ),
                    ),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          business.name,
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                            color: Colors.white,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        const SizedBox(height: 2),
                        Text(
                          user?.fullName ?? 'Store Admin',
                          style: TextStyle(
                            fontSize: 13,
                            color: Colors.white.withOpacity(0.85),
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        if (business.gstNumber != null) ...[
                          const SizedBox(height: 2),
                          Text(
                            'GST: ${business.gstNumber}',
                            style: TextStyle(
                              fontSize: 11,
                              color: Colors.white.withOpacity(0.7),
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ],
              ),
            ),

            // Navigation Items
            Expanded(
              child: ListView(
                padding: const EdgeInsets.symmetric(vertical: 8),
                children: [
                  _drawerItem(
                    context,
                    icon: Icons.dashboard_outlined,
                    title: 'Dashboard',
                    route: '/dashboard',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.point_of_sale_outlined,
                    title: 'Billing / POS',
                    route: '/billing',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.receipt_long_outlined,
                    title: 'Invoices',
                    route: '/invoices',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.shopping_bag_outlined,
                    title: 'Products',
                    route: '/products',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.inventory_2_outlined,
                    title: 'Inventory & Stock',
                    route: '/inventory',
                  ),
                  const Divider(height: 16),
                  _drawerSectionHeader('PARTNERS & ACCOUNTS'),
                  _drawerItem(
                    context,
                    icon: Icons.people_outline,
                    title: 'Customers',
                    route: '/customers',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.local_shipping_outlined,
                    title: 'Suppliers',
                    route: '/suppliers',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.shopping_cart_checkout_outlined,
                    title: 'Purchases',
                    route: '/purchases',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.account_balance_wallet_outlined,
                    title: 'Expenses',
                    route: '/expenses',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.credit_card_outlined,
                    title: 'Credit Bills',
                    route: '/credit-bills',
                  ),
                  const Divider(height: 16),
                  _drawerSectionHeader('RETURNS & MANAGEMENT'),
                  _drawerItem(
                    context,
                    icon: Icons.assignment_return_outlined,
                    title: 'Sales Returns',
                    route: '/sales-returns',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.keyboard_return_outlined,
                    title: 'Purchase Returns',
                    route: '/purchase-returns',
                  ),
                  const Divider(height: 16),
                  _drawerSectionHeader('REPORTS & ANALYTICS'),
                  _drawerItem(
                    context,
                    icon: Icons.bar_chart_outlined,
                    title: 'Sales Summary Reports',
                    route: '/reports',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.request_quote_outlined,
                    title: 'GST Reports',
                    route: '/gst-reports',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.trending_up_outlined,
                    title: 'Profit & Loss',
                    route: '/profit-loss',
                  ),
                  const Divider(height: 16),
                  _drawerSectionHeader('SYSTEM & CONFIGURATION'),
                  _drawerItem(
                    context,
                    icon: Icons.badge_outlined,
                    title: 'Staff Management',
                    route: '/staff',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.notifications_outlined,
                    title: 'Notifications',
                    route: '/notifications',
                  ),
                  _drawerItem(
                    context,
                    icon: Icons.settings_outlined,
                    title: 'Settings & Business Profile',
                    route: '/settings',
                  ),
                ],
              ),
            ),

            // Logout Footer
            Container(
              padding: const EdgeInsets.all(12),
              decoration: const BoxDecoration(
                border: Border(top: BorderSide(color: AppTheme.borderLight)),
              ),
              child: ListTile(
                leading: const Icon(Icons.logout, color: AppTheme.errorColor),
                title: const Text(
                  'Logout',
                  style: TextStyle(
                    color: AppTheme.errorColor,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                onTap: () async {
                  Navigator.pop(context);
                  await auth.logout();
                  if (context.mounted) {
                    context.go('/login');
                  }
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _drawerSectionHeader(String title) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
      child: Text(
        title,
        style: const TextStyle(
          fontSize: 11,
          fontWeight: FontWeight.bold,
          color: AppTheme.textMuted,
          letterSpacing: 0.8,
        ),
      ),
    );
  }

  Widget _drawerItem(
    BuildContext context, {
    required IconData icon,
    required String title,
    required String route,
  }) {
    return ListTile(
      dense: true,
      leading: Icon(icon, color: AppTheme.textDark, size: 22),
      title: Text(
        title,
        style: const TextStyle(
          fontSize: 14,
          fontWeight: FontWeight.w500,
          color: AppTheme.textDark,
        ),
      ),
      onTap: () {
        Navigator.pop(context);
        context.go(route);
      },
    );
  }
}

