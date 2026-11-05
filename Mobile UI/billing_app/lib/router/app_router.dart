import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../data/providers/auth_provider.dart';
import '../presentation/screens/auth/login_screen.dart';
import '../presentation/screens/auth/onboarding_screen.dart';
import '../presentation/screens/auth/register_screen.dart';
import '../presentation/screens/auth/splash_screen.dart';
import '../presentation/screens/billing/billing_screen.dart';
import '../presentation/screens/credit/credit_bills_screen.dart';
import '../presentation/screens/customers/customer_ledger_screen.dart';
import '../presentation/screens/customers/customers_screen.dart';
import '../presentation/screens/dashboard/dashboard_screen.dart';
import '../presentation/screens/expenses/expenses_screen.dart';
import '../presentation/screens/inventory/inventory_screen.dart';
import '../presentation/screens/invoices/invoice_detail_screen.dart';
import '../presentation/screens/invoices/invoices_screen.dart';
import '../presentation/screens/notifications/notifications_screen.dart';
import '../presentation/screens/products/add_product_screen.dart';
import '../presentation/screens/products/products_screen.dart';
import '../presentation/screens/purchases/purchases_screen.dart';
import '../presentation/screens/reports/gst_reports_screen.dart';
import '../presentation/screens/reports/profit_loss_screen.dart';
import '../presentation/screens/reports/reports_screen.dart';
import '../presentation/screens/returns/purchase_returns_screen.dart';
import '../presentation/screens/returns/sales_returns_screen.dart';
import '../presentation/screens/settings/settings_screen.dart';
import '../presentation/screens/staff/staff_screen.dart';
import '../presentation/screens/suppliers/suppliers_screen.dart';
import '../presentation/widgets/navigation/scaffold_with_nav.dart';

class AppRouter {
  static final GlobalKey<NavigatorState> rootNavigatorKey =
      GlobalKey<NavigatorState>(debugLabel: 'root');
  static final GlobalKey<NavigatorState> shellNavigatorKey =
      GlobalKey<NavigatorState>(debugLabel: 'shell');

  static GoRouter createRouter(AuthProvider authProvider) {
    return GoRouter(
      navigatorKey: rootNavigatorKey,
      initialLocation: '/splash',
      refreshListenable: authProvider,
      routes: [
        // Standalone Auth & Splash routes
        GoRoute(
          path: '/splash',
          builder: (context, state) => const SplashScreen(),
        ),
        GoRoute(
          path: '/login',
          builder: (context, state) => const LoginScreen(),
        ),
        GoRoute(
          path: '/register',
          builder: (context, state) => const RegisterScreen(),
        ),
        GoRoute(
          path: '/onboarding',
          builder: (context, state) => const OnboardingScreen(),
        ),

        // Standalone Detail / Editor / Utility screens
        GoRoute(
          path: '/invoices/:id',
          builder: (context, state) {
            final id = int.tryParse(state.pathParameters['id'] ?? '1') ?? 1;
            return InvoiceDetailScreen(invoiceId: id);
          },
        ),
        GoRoute(
          path: '/products/add',
          builder: (context, state) => const AddProductScreen(),
        ),
        GoRoute(
          path: '/products/edit/:id',
          builder: (context, state) {
            final id = int.tryParse(state.pathParameters['id'] ?? '');
            return AddProductScreen(productId: id);
          },
        ),
        GoRoute(
          path: '/customers/:id/ledger',
          builder: (context, state) {
            final id = int.tryParse(state.pathParameters['id'] ?? '1') ?? 1;
            return CustomerLedgerScreen(customerId: id);
          },
        ),
        GoRoute(
          path: '/settings',
          builder: (context, state) => const SettingsScreen(),
        ),
        GoRoute(
          path: '/notifications',
          builder: (context, state) => const NotificationsScreen(),
        ),

        // ShellRoute for main sections with persistent BottomNav + Drawer
        ShellRoute(
          navigatorKey: shellNavigatorKey,
          builder: (context, state, child) {
            return ScaffoldWithNav(child: child);
          },
          routes: [
            GoRoute(
              path: '/dashboard',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: DashboardScreen(),
              ),
            ),
            GoRoute(
              path: '/billing',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: BillingScreen(),
              ),
            ),
            GoRoute(
              path: '/inventory',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: InventoryScreen(),
              ),
            ),
            GoRoute(
              path: '/reports',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: ReportsScreen(),
              ),
            ),
            GoRoute(
              path: '/invoices',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: InvoicesScreen(),
              ),
            ),
            GoRoute(
              path: '/products',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: ProductsScreen(),
              ),
            ),
            GoRoute(
              path: '/customers',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: CustomersScreen(),
              ),
            ),
            GoRoute(
              path: '/suppliers',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: SuppliersScreen(),
              ),
            ),
            GoRoute(
              path: '/purchases',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: PurchasesScreen(),
              ),
            ),
            GoRoute(
              path: '/expenses',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: ExpensesScreen(),
              ),
            ),
            GoRoute(
              path: '/sales-returns',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: SalesReturnsScreen(),
              ),
            ),
            GoRoute(
              path: '/purchase-returns',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: PurchaseReturnsScreen(),
              ),
            ),
            GoRoute(
              path: '/credit-bills',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: CreditBillsScreen(),
              ),
            ),
            GoRoute(
              path: '/gst-reports',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: GstReportsScreen(),
              ),
            ),
            GoRoute(
              path: '/profit-loss',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: ProfitLossScreen(),
              ),
            ),
            GoRoute(
              path: '/staff',
              pageBuilder: (context, state) => const NoTransitionPage(
                child: StaffScreen(),
              ),
            ),
          ],
        ),
      ],
      redirect: (context, state) {
        final loc = state.matchedLocation;
        final isAuth = authProvider.isAuthenticated;
        final isOnboarded = authProvider.isOnboarded;

        // Allow splash to perform initialization
        if (loc == '/splash') return null;

        // If not authenticated and trying to access protected route
        final isAuthRoute = loc == '/login' || loc == '/register';
        if (!isAuth && !isAuthRoute) {
          return '/login';
        }

        // If authenticated but trying to go to login/register
        if (isAuth && isAuthRoute) {
          return isOnboarded ? '/dashboard' : '/onboarding';
        }

        // If authenticated but not onboarded and trying to access app
        if (isAuth && !isOnboarded && loc != '/onboarding') {
          return '/onboarding';
        }

        return null;
      },
    );
  }
}

