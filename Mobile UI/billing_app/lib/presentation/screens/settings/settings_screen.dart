import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/constants/app_constants.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/validators.dart';
import '../../models/business_model.dart';
import '../../providers/auth_provider.dart';
import '../../providers/settings_provider.dart';
import '../../services/api_service.dart';
import '../widgets/common/app_button.dart';
import '../widgets/common/app_text_field.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> with SingleTickerProviderStateMixin {
  late TabController _tabController;

  // Connection settings
  final _apiUrlController = TextEditingController();
  bool _isTestingConnection = false;
  String? _connectionTestResult;
  bool _connectionSuccess = false;

  // Business profile form
  final _businessFormKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _legalNameController = TextEditingController();
  final _businessTypeController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _addressController = TextEditingController();
  final _cityController = TextEditingController();
  final _stateController = TextEditingController();
  final _pinCodeController = TextEditingController();
  final _gstController = TextEditingController();

  bool _isInitialized = false;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (!_isInitialized) {
      final settings = context.read<SettingsProvider>();
      _apiUrlController.text = settings.baseUrl;

      final biz = settings.business;
      _nameController.text = biz.name;
      _legalNameController.text = biz.legalName ?? '';
      _businessTypeController.text = biz.businessTypeName ?? 'Retail & Wholesale';
      _emailController.text = biz.email ?? '';
      _phoneController.text = biz.phone ?? '';
      _addressController.text = biz.address ?? '';
      _cityController.text = biz.city ?? '';
      _stateController.text = biz.state ?? '';
      _pinCodeController.text = biz.pinCode ?? '';
      _gstController.text = biz.gstNumber ?? '';

      _isInitialized = true;
    }
  }

  @override
  void dispose() {
    _tabController.dispose();
    _apiUrlController.dispose();
    _nameController.dispose();
    _legalNameController.dispose();
    _businessTypeController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _addressController.dispose();
    _cityController.dispose();
    _stateController.dispose();
    _pinCodeController.dispose();
    _gstController.dispose();
    super.dispose();
  }

  Future<void> _testConnection() async {
    final url = _apiUrlController.text.trim();
    if (url.isEmpty) return;

    setState(() {
      _isTestingConnection = true;
      _connectionTestResult = null;
    });

    try {
      final settings = context.read<SettingsProvider>();
      await settings.updateBaseUrl(url);

      final api = ApiService();
      final response = await api.get('/auth/me');

      if (mounted) {
        setState(() {
          _isTestingConnection = false;
          _connectionSuccess = response.statusCode == 200 || response.statusCode == 401;
          _connectionTestResult = _connectionSuccess
              ? 'Connected successfully to API server!'
              : 'Server responded with status code: ${response.statusCode}';
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isTestingConnection = false;
          _connectionSuccess = false;
          _connectionTestResult = 'Connection failed: Host unreachable or offline';
        });
      }
    }
  }

  void _saveBusinessProfile() async {
    if (!_businessFormKey.currentState!.validate()) return;

    final settings = context.read<SettingsProvider>();
    final updated = BusinessResponse(
      id: settings.business.id,
      name: _nameController.text.trim(),
      legalName: _legalNameController.text.trim(),
      businessTypeName: _businessTypeController.text.trim(),
      email: _emailController.text.trim(),
      phone: _phoneController.text.trim(),
      address: _addressController.text.trim(),
      city: _cityController.text.trim(),
      state: _stateController.text.trim(),
      pinCode: _pinCodeController.text.trim(),
      gstNumber: _gstController.text.trim().toUpperCase(),
      currencySymbol: settings.currencySymbol,
    );

    await settings.updateBusinessProfile(updated);

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Business profile updated successfully!'),
          backgroundColor: AppTheme.successColor,
        ),
      );
    }
  }

  void _confirmLogout() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Confirm Logout'),
        content: const Text('Are you sure you want to log out of your store account?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppTheme.errorColor),
            onPressed: () async {
              Navigator.pop(ctx);
              final auth = context.read<AuthProvider>();
              await auth.logout();
              if (mounted) {
                context.go('/login');
              }
            },
            child: const Text('Logout', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Settings & Configuration'),
        bottom: TabBar(
          controller: _tabController,
          labelColor: Colors.white,
          unselectedLabelColor: Colors.white70,
          indicatorColor: AppTheme.accentColor,
          tabs: const [
            Tab(icon: Icon(Icons.tune), text: 'General & App'),
            Tab(icon: Icon(Icons.store), text: 'Business Profile'),
            Tab(icon: Icon(Icons.info_outline), text: 'About'),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          _buildGeneralSettingsTab(),
          _buildBusinessProfileTab(),
          _buildAboutTab(),
        ],
      ),
    );
  }

  Widget _buildGeneralSettingsTab() {
    final settings = context.watch<SettingsProvider>();

    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Appearance Section
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.palette_outlined, color: AppTheme.primaryColor),
                      const SizedBox(width: 8),
                      Text(
                        'Appearance & Theme',
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                    ],
                  ),
                  const Divider(height: 24),
                  SwitchListTile(
                    title: const Text('Dark Mode'),
                    subtitle: Text(
                      settings.isDarkMode ? 'Dark theme enabled' : 'Light theme enabled',
                    ),
                    secondary: Icon(
                      settings.isDarkMode ? Icons.dark_mode : Icons.light_mode,
                      color: settings.isDarkMode ? Colors.amber : Colors.orange,
                    ),
                    value: settings.isDarkMode,
                    onChanged: (val) => settings.toggleTheme(val),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),

          // Currency Section
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.currency_exchange, color: AppTheme.primaryColor),
                      const SizedBox(width: 8),
                      Text(
                        'Billing Currency',
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                    ],
                  ),
                  const Divider(height: 24),
                  Wrap(
                    spacing: 10,
                    runSpacing: 10,
                    children: [
                      _currencyChoice(settings, '₹', 'INR - Indian Rupee (₹)'),
                      _currencyChoice(settings, '\$', 'USD - US Dollar (\$)'),
                      _currencyChoice(settings, '€', 'EUR - Euro (€)'),
                      _currencyChoice(settings, '£', 'GBP - British Pound (£)'),
                      _currencyChoice(settings, 'AED', 'AED - UAE Dirham (AED)'),
                    ],
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),

          // API Endpoint Section
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.cloud_sync_outlined, color: AppTheme.primaryColor),
                      const SizedBox(width: 8),
                      Text(
                        'API Server Endpoint',
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                    ],
                  ),
                  const Divider(height: 24),
                  const Text(
                    'Configure backend API address (e.g. for local network or cloud deployment):',
                    style: TextStyle(fontSize: 13, color: AppTheme.textMuted),
                  ),
                  const SizedBox(height: 12),
                  AppTextField(
                    label: 'Base API URL',
                    hint: 'http://localhost:5000/api',
                    controller: _apiUrlController,
                    prefixIcon: const Icon(Icons.link),
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(
                        child: AppButton(
                          text: 'Test Connection',
                          icon: Icons.network_check,
                          variant: AppButtonVariant.secondary,
                          isLoading: _isTestingConnection,
                          onPressed: _testConnection,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: AppButton(
                          text: 'Save Endpoint',
                          icon: Icons.check,
                          onPressed: () async {
                            final url = _apiUrlController.text.trim();
                            if (url.isNotEmpty) {
                              await settings.updateBaseUrl(url);
                              if (context.mounted) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                    content: Text('API URL updated and saved!'),
                                    backgroundColor: AppTheme.successColor,
                                  ),
                                );
                              }
                            }
                          },
                        ),
                      ),
                    ],
                  ),
                  if (_connectionTestResult != null) ...[
                    const SizedBox(height: 12),
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: _connectionSuccess
                            ? AppTheme.successColor.withOpacity(0.1)
                            : AppTheme.errorColor.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(
                          color: _connectionSuccess
                              ? AppTheme.successColor
                              : AppTheme.errorColor,
                        ),
                      ),
                      child: Row(
                        children: [
                          Icon(
                            _connectionSuccess ? Icons.check_circle : Icons.error,
                            color: _connectionSuccess
                                ? AppTheme.successColor
                                : AppTheme.errorColor,
                            size: 20,
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              _connectionTestResult!,
                              style: TextStyle(
                                fontSize: 13,
                                color: _connectionSuccess
                                    ? AppTheme.successColor
                                    : AppTheme.errorColor,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ),
          const SizedBox(height: 24),

          // Logout Action
          AppButton(
            text: 'Logout of Account',
            icon: Icons.logout,
            backgroundColor: AppTheme.errorColor,
            onPressed: _confirmLogout,
          ),
          const SizedBox(height: 32),
        ],
      ),
    );
  }

  Widget _currencyChoice(SettingsProvider settings, String symbol, String label) {
    final isSelected = settings.currencySymbol == symbol;
    return ChoiceChip(
      label: Text(label),
      selected: isSelected,
      selectedColor: AppTheme.primaryColor.withOpacity(0.15),
      labelStyle: TextStyle(
        color: isSelected ? AppTheme.primaryColor : AppTheme.textDark,
        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
      ),
      onSelected: (selected) {
        if (selected) {
          settings.updateCurrency(symbol);
        }
      },
    );
  }

  Widget _buildBusinessProfileTab() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Form(
        key: _businessFormKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.business, color: AppTheme.primaryColor),
                        const SizedBox(width: 8),
                        Text(
                          'Business Identity',
                          style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                        ),
                      ],
                    ),
                    const Divider(height: 24),
                    AppTextField(
                      label: 'Store / Trade Name *',
                      controller: _nameController,
                      validator: (val) => Validators.required(val, fieldName: 'Store Name'),
                    ),
                    const SizedBox(height: 12),
                    AppTextField(
                      label: 'Legal Registered Name',
                      controller: _legalNameController,
                    ),
                    const SizedBox(height: 12),
                    AppTextField(
                      label: 'Business Category / Type',
                      controller: _businessTypeController,
                      hint: 'e.g. Retail, Supermarket, Pharmacy',
                    ),
                    const SizedBox(height: 12),
                    AppTextField(
                      label: 'GSTIN / Tax ID Number',
                      controller: _gstController,
                      hint: '29ABCDE1234F1Z5',
                      validator: Validators.gstNumber,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.contact_mail_outlined, color: AppTheme.primaryColor),
                        const SizedBox(width: 8),
                        Text(
                          'Contact & Location',
                          style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                        ),
                      ],
                    ),
                    const Divider(height: 24),
                    AppTextField(
                      label: 'Official Email',
                      controller: _emailController,
                      keyboardType: TextInputType.emailAddress,
                      validator: Validators.email,
                    ),
                    const SizedBox(height: 12),
                    AppTextField(
                      label: 'Official Phone Number',
                      controller: _phoneController,
                      keyboardType: TextInputType.phone,
                      validator: Validators.phone,
                    ),
                    const SizedBox(height: 12),
                    AppTextField(
                      label: 'Store Address',
                      controller: _addressController,
                      maxLines: 2,
                    ),
                    const SizedBox(height: 12),
                    Row(
                      children: [
                        Expanded(
                          child: AppTextField(
                            label: 'City',
                            controller: _cityController,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: AppTextField(
                            label: 'State',
                            controller: _stateController,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    AppTextField(
                      label: 'PIN Code',
                      controller: _pinCodeController,
                      keyboardType: TextInputType.number,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),
            AppButton(
              text: 'Save Business Profile',
              icon: Icons.save,
              onPressed: _saveBusinessProfile,
            ),
            const SizedBox(height: 32),
          ],
        ),
      ),
    );
  }

  Widget _buildAboutTab() {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Card(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              children: [
                Container(
                  width: 72,
                  height: 72,
                  decoration: BoxDecoration(
                    color: AppTheme.primaryColor.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: const Icon(
                    Icons.point_of_sale,
                    size: 40,
                    color: AppTheme.primaryColor,
                  ),
                ),
                const SizedBox(height: 16),
                const Text(
                  'Master Billing',
                  style: TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                    color: AppTheme.textDark,
                  ),
                ),
                const SizedBox(height: 4),
                const Text(
                  'v1.0.0 (Build 2026.1)',
                  style: TextStyle(
                    fontSize: 14,
                    color: AppTheme.textMuted,
                  ),
                ),
                const SizedBox(height: 16),
                const Divider(),
                const SizedBox(height: 16),
                _aboutRow('Target Platforms', 'Android & macOS Desktop'),
                const SizedBox(height: 12),
                _aboutRow('Architecture', 'Flutter Material 3 + REST API'),
                const SizedBox(height: 12),
                _aboutRow('State Management', 'Provider Pattern'),
                const SizedBox(height: 12),
                _aboutRow('Networking', 'Dio HTTP Client + JWT Interceptors'),
                const SizedBox(height: 12),
                _aboutRow('GST Compliance', 'E-Invoicing & B2B/B2C GST Reporting'),
                const SizedBox(height: 12),
                _aboutRow('Offline Mode', 'Resilient Sample Data Cache Enabled'),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _aboutRow(String title, String value) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          title,
          style: const TextStyle(
            fontSize: 13,
            color: AppTheme.textMuted,
            fontWeight: FontWeight.w500,
          ),
        ),
        Text(
          value,
          style: const TextStyle(
            fontSize: 13,
            color: AppTheme.textDark,
            fontWeight: FontWeight.w600,
          ),
        ),
      ],
    );
  }
}

