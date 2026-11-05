import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/constants/app_constants.dart';
import '../../../core/theme/app_theme.dart';
import '../../models/business_model.dart';
import '../../providers/auth_provider.dart';
import '../../providers/settings_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';

class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key});

  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen> {
  int _currentStep = 0;
  final _addressController = TextEditingController(text: 'Commercial Complex, Sector 4');
  final _cityController = TextEditingController(text: 'Bengaluru');
  final _stateController = TextEditingController(text: 'Karnataka');
  final _gstController = TextEditingController(text: '29ABCDE1234F1Z5');
  String _selectedBusinessType = 'Retail & Wholesale';
  String _selectedCurrency = '₹';
  double _defaultTaxRate = 18.0;

  final List<String> _businessTypes = [
    'Retail & Wholesale',
    'Supermarket / Grocery',
    'Electronics & Appliances',
    'Clothing & Apparel',
    'Pharmacy / Medical',
    'Restaurant / Cafe',
    'Hardware & Sanitary',
  ];

  @override
  void dispose() {
    _addressController.dispose();
    _cityController.dispose();
    _stateController.dispose();
    _gstController.dispose();
    super.dispose();
  }

  Future<void> _completeWizard() async {
    final settings = context.read<SettingsProvider>();
    final auth = context.read<AuthProvider>();

    final updated = BusinessResponse(
      id: 1,
      name: settings.business.name.isNotEmpty ? settings.business.name : 'My Business Store',
      businessTypeName: _selectedBusinessType,
      address: _addressController.text.trim(),
      city: _cityController.text.trim(),
      state: _stateController.text.trim(),
      gstNumber: _gstController.text.trim(),
      currencySymbol: _selectedCurrency,
    );

    await settings.updateBusinessProfile(updated);
    await settings.updateCurrency(_selectedCurrency);
    await auth.completeOnboarding();

    if (mounted) {
      context.go('/dashboard');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        title: const Text('Store Setup Wizard'),
        centerTitle: true,
      ),
      body: SafeArea(
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 540),
            child: Column(
              children: [
                // Step Progress Indicator
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
                  color: AppTheme.scaffoldBgLight,
                  child: Row(
                    children: [
                      _stepIndicator(0, 'Business Info'),
                      _stepConnector(0),
                      _stepIndicator(1, 'Currency & Tax'),
                      _stepConnector(1),
                      _stepIndicator(2, 'Ready!'),
                    ],
                  ),
                ),

                // Form Content
                Expanded(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.all(24),
                    child: _buildCurrentStepContent(),
                  ),
                ),

                // Navigation Buttons
                Container(
                  padding: const EdgeInsets.all(20),
                  decoration: const BoxDecoration(
                    color: Colors.white,
                    border: Border(top: BorderSide(color: AppTheme.borderLight)),
                  ),
                  child: Row(
                    children: [
                      if (_currentStep > 0) ...[
                        Expanded(
                          child: AppButton(
                            label: 'Back',
                            type: ButtonType.outline,
                            onPressed: () {
                              setState(() {
                                _currentStep -= 1;
                              });
                            },
                          ),
                        ),
                        const SizedBox(width: 16),
                      ],
                      Expanded(
                        child: AppButton(
                          label: _currentStep == 2 ? 'Launch App' : 'Continue',
                          onPressed: () {
                            if (_currentStep < 2) {
                              setState(() {
                                _currentStep += 1;
                              });
                            } else {
                              _completeWizard();
                            }
                          },
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _stepIndicator(int stepIndex, String title) {
    final isDone = _currentStep > stepIndex;
    final isActive = _currentStep == stepIndex;

    return Expanded(
      child: Column(
        children: [
          CircleAvatar(
            radius: 14,
            backgroundColor: isDone
                ? AppTheme.successColor
                : isActive
                    ? AppTheme.primaryColor
                    : Colors.grey.shade300,
            child: isDone
                ? const Icon(Icons.check, size: 14, color: Colors.white)
                : Text(
                    '${stepIndex + 1}',
                    style: TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                      color: isActive ? Colors.white : Colors.grey.shade600,
                    ),
                  ),
          ),
          const SizedBox(height: 4),
          Text(
            title,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(
              fontSize: 11,
              fontWeight: isActive ? FontWeight.bold : FontWeight.normal,
              color: isActive ? AppTheme.primaryColor : AppTheme.textMuted,
            ),
          ),
        ],
      ),
    );
  }

  Widget _stepConnector(int stepIndex) {
    final isPassed = _currentStep > stepIndex;
    return Container(
      width: 28,
      height: 2,
      color: isPassed ? AppTheme.successColor : Colors.grey.shade300,
      margin: const EdgeInsets.only(bottom: 16),
    );
  }

  Widget _buildCurrentStepContent() {
    switch (_currentStep) {
      case 0:
        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Tell us about your business',
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 6),
            const Text(
              'This will appear on invoices and reports',
              style: TextStyle(fontSize: 13, color: AppTheme.textMuted),
            ),
            const SizedBox(height: 24),
            const Text('Business Sector', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
            const SizedBox(height: 6),
            DropdownButtonFormField<String>(
              value: _selectedBusinessType,
              decoration: const InputDecoration(),
              items: _businessTypes.map((type) {
                return DropdownMenuItem(value: type, child: Text(type));
              }).toList(),
              onChanged: (val) {
                if (val != null) setState(() => _selectedBusinessType = val);
              },
            ),
            const SizedBox(height: 16),
            AppTextField(
              controller: _addressController,
              label: 'Shop Address',
              hint: 'Building, Street name',
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: AppTextField(
                    controller: _cityController,
                    label: 'City',
                    hint: 'City',
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: AppTextField(
                    controller: _stateController,
                    label: 'State',
                    hint: 'State',
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            AppTextField(
              controller: _gstController,
              label: 'GST / Tax ID (Optional)',
              hint: '29ABCDE1234F1Z5',
            ),
          ],
        );
      case 1:
        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Currency & Taxation',
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 6),
            const Text(
              'Choose your operational currency and default tax slab',
              style: TextStyle(fontSize: 13, color: AppTheme.textMuted),
            ),
            const SizedBox(height: 24),
            const Text('Primary Currency', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
            const SizedBox(height: 8),
            Row(
              children: [
                _currencyChoice('₹ (INR)', '₹'),
                const SizedBox(width: 12),
                _currencyChoice('\$ (USD)', '\$'),
                const SizedBox(width: 12),
                _currencyChoice('€ (EUR)', '€'),
                const SizedBox(width: 12),
                _currencyChoice('£ (GBP)', '£'),
              ],
            ),
            const SizedBox(height: 28),
            const Text('Default GST / Tax Rate', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
            const SizedBox(height: 8),
            Wrap(
              spacing: 10,
              children: AppConstants.taxRates.map((rate) {
                final isSelected = _defaultTaxRate == rate;
                return ChoiceChip(
                  label: Text('$rate%'),
                  selected: isSelected,
                  selectedColor: AppTheme.primaryColor.withOpacity(0.15),
                  labelStyle: TextStyle(
                    color: isSelected ? AppTheme.primaryColor : AppTheme.textDark,
                    fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                  ),
                  onSelected: (val) {
                    if (val) setState(() => _defaultTaxRate = rate);
                  },
                );
              }).toList(),
            ),
          ],
        );
      case 2:
      default:
        return Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const SizedBox(height: 20),
            Container(
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: AppTheme.successColor.withOpacity(0.1),
                shape: BoxShape.circle,
              ),
              child: const Icon(Icons.check_circle_rounded, size: 64, color: AppTheme.successColor),
            ),
            const SizedBox(height: 24),
            const Text(
              "You're All Set!",
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            const Text(
              'Your business workspace has been successfully created. You can now start generating bills, managing products and viewing reports.',
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 14, color: AppTheme.textMuted, height: 1.5),
            ),
            const SizedBox(height: 32),
          ],
        );
    }
  }

  Widget _currencyChoice(String label, String symbol) {
    final isSelected = _selectedCurrency == symbol;
    return Expanded(
      child: InkWell(
        onTap: () => setState(() => _selectedCurrency = symbol),
        borderRadius: BorderRadius.circular(10),
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 12),
          decoration: BoxDecoration(
            color: isSelected ? AppTheme.primaryColor.withOpacity(0.08) : Colors.white,
            borderRadius: BorderRadius.circular(10),
            border: Border.all(
              color: isSelected ? AppTheme.primaryColor : AppTheme.borderLight,
              width: isSelected ? 2 : 1,
            ),
          ),
          alignment: Alignment.center,
          child: Text(
            label,
            style: TextStyle(
              fontSize: 13,
              fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
              color: isSelected ? AppTheme.primaryColor : AppTheme.textDark,
            ),
          ),
        ),
      ),
    );
  }
}

