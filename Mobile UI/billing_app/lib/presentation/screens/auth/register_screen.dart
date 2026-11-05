import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/validators.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _businessNameController = TextEditingController();
  final _fullNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _passwordController = TextEditingController();

  @override
  void dispose() {
    _businessNameController.dispose();
    _fullNameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _handleRegister() async {
    if (!_formKey.currentState!.validate()) return;

    final auth = context.read<AuthProvider>();
    final success = await auth.register(
      businessName: _businessNameController.text.trim(),
      fullName: _fullNameController.text.trim(),
      email: _emailController.text.trim(),
      password: _passwordController.text,
      phone: _phoneController.text.trim(),
    );

    if (!mounted) return;

    if (success) {
      context.go('/onboarding');
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(auth.errorMessage ?? 'Registration failed. Try again.'),
          backgroundColor: AppTheme.errorColor,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();

    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        title: const Text('Create Account'),
      ),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 440),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    const Text(
                      'Register Your Business',
                      style: TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                        color: AppTheme.textDark,
                      ),
                    ),
                    const SizedBox(height: 6),
                    const Text(
                      'Get started with cloud billing & inventory management',
                      style: TextStyle(fontSize: 14, color: AppTheme.textMuted),
                    ),
                    const SizedBox(height: 28),

                    // Business Name
                    AppTextField(
                      controller: _businessNameController,
                      label: 'Business / Store Name',
                      hint: 'e.g. Metro Supermarket',
                      prefixIcon: const Icon(Icons.storefront_outlined, size: 20),
                      validator: (v) => AppValidators.required(v, 'Please enter your business name'),
                    ),
                    const SizedBox(height: 16),

                    // Full Name
                    AppTextField(
                      controller: _fullNameController,
                      label: 'Owner / Manager Full Name',
                      hint: 'e.g. John Doe',
                      prefixIcon: const Icon(Icons.person_outline, size: 20),
                      validator: (v) => AppValidators.required(v, 'Please enter your name'),
                    ),
                    const SizedBox(height: 16),

                    // Email
                    AppTextField(
                      controller: _emailController,
                      label: 'Email Address',
                      hint: 'owner@store.com',
                      keyboardType: TextInputType.emailAddress,
                      prefixIcon: const Icon(Icons.email_outlined, size: 20),
                      validator: AppValidators.email,
                    ),
                    const SizedBox(height: 16),

                    // Phone
                    AppTextField(
                      controller: _phoneController,
                      label: 'Mobile Number',
                      hint: '9876543210',
                      keyboardType: TextInputType.phone,
                      prefixIcon: const Icon(Icons.phone_outlined, size: 20),
                      validator: AppValidators.phone,
                    ),
                    const SizedBox(height: 16),

                    // Password
                    AppTextField(
                      controller: _passwordController,
                      label: 'Password',
                      hint: 'At least 6 characters',
                      isPassword: true,
                      prefixIcon: const Icon(Icons.lock_outline, size: 20),
                      validator: AppValidators.password,
                    ),
                    const SizedBox(height: 28),

                    // Register Button
                    AppButton(
                      label: 'Register & Setup Store',
                      isLoading: auth.isLoading,
                      onPressed: _handleRegister,
                    ),
                    const SizedBox(height: 24),

                    // Login Link
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Text(
                          'Already have an account? ',
                          style: TextStyle(fontSize: 13, color: AppTheme.textMuted),
                        ),
                        GestureDetector(
                          onTap: () => context.pop(),
                          child: const Text(
                            'Sign In',
                            style: TextStyle(
                              fontSize: 13,
                              fontWeight: FontWeight.bold,
                              color: AppTheme.primaryColor,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

