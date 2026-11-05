class AppConstants {
  static const String appName = 'Billing & POS';
  static const String defaultBaseUrl = 'http://localhost:5000/api';
  
  // Storage Keys
  static const String keyToken = 'auth_token';
  static const String keyRefreshToken = 'refresh_token';
  static const String keyUser = 'user_data';
  static const String keyBaseUrl = 'base_api_url';
  static const String keyThemeMode = 'app_theme_mode';
  static const String keyCurrency = 'app_currency_symbol';
  static const String keyTenantId = 'tenant_id';
  static const String keyOnboarded = 'has_completed_onboarding';

  // Payment Methods
  static const List<String> paymentMethods = ['Cash', 'Card', 'UPI', 'Bank Transfer', 'Credit', 'Split'];
  
  // Invoice Statuses
  static const List<String> invoiceStatuses = ['All', 'Paid', 'Pending', 'Overdue', 'Cancelled'];
  
  // Default Tax Rates
  static const List<double> taxRates = [0.0, 5.0, 12.0, 18.0, 28.0];

  // Default Categories
  static const List<String> defaultCategories = [
    'General',
    'Electronics',
    'Groceries',
    'Clothing',
    'Hardware',
    'Food & Beverage',
    'Services'
  ];

  // Units
  static const List<String> defaultUnits = ['Pcs', 'Kg', 'Gm', 'Ltr', 'Ml', 'Mtr', 'Box', 'Pack'];
}

