import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/validators.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';

const _kDarkBg    = Color(0xFF0F1629);
const _kCardBg    = Color(0xFF1A2235);
const _kBorderCol = Color(0xFF2D3A52);
const _kTextLight = Color(0xFFE2E8F0);
const _kTextMuted = Color(0xFF64748B);
const _kIndigo    = Color(0xFF4F46E5);

final _businessTypes = [
  'SaaS & Software',
  'E-commerce & Retail',
  'Financial Services',
  'Healthcare & Pharma',
  'Manufacturing & Supply Chain',
  'Professional Services',
  'Hospitality & Food',
  'Education & Non-Profit',
  'Other',
];

/// Password entropy score 0-4
int _entropy(String pwd) {
  if (pwd.isEmpty) return 0;
  int s = 0;
  if (pwd.length >= 8)         s++;
  if (RegExp(r'[A-Z]').hasMatch(pwd)) s++;
  if (RegExp(r'[0-9]').hasMatch(pwd)) s++;
  if (RegExp(r'[^A-Za-z0-9]').hasMatch(pwd)) s++;
  return s;
}

Color _entropyColor(int s) {
  switch (s) {
    case 0: return const Color(0xFF334155);
    case 1: return const Color(0xFFEF4444);
    case 2: return const Color(0xFFF59E0B);
    case 3: return const Color(0xFF3B82F6);
    default: return const Color(0xFF10B981);
  }
}

String _entropyLabel(int s) {
  switch (s) {
    case 0: return 'Minimal';
    case 1: return 'Weak';
    case 2: return 'Fair';
    case 3: return 'Good';
    default: return 'Strong';
  }
}

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey               = GlobalKey<FormState>();
  final _fullNameController    = TextEditingController();
  final _emailController       = TextEditingController();
  final _passwordController    = TextEditingController();
  final _phoneController       = TextEditingController();
  final _businessNameController= TextEditingController();
  String? _businessType;
  bool   _agreed       = false;
  bool   _showPassword = false;

  int get _pwdScore => _entropy(_passwordController.text);

  @override
  void dispose() {
    _fullNameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _phoneController.dispose();
    _businessNameController.dispose();
    super.dispose();
  }

  Future<void> _handleRegister() async {
    if (!_formKey.currentState!.validate()) return;
    if (!_agreed) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please accept the Master Service Agreement.')),
      );
      return;
    }

    final auth = context.read<AuthProvider>();
    final success = await auth.register(
      businessName: _businessNameController.text.trim(),
      fullName:     _fullNameController.text.trim(),
      email:        _emailController.text.trim(),
      password:     _passwordController.text,
      phone:        _phoneController.text.trim(),
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
      backgroundColor: _kDarkBg,
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 440),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    // ── Shield Icon ──
                    Center(
                      child: Container(
                        width: 60,
                        height: 60,
                        decoration: BoxDecoration(
                          color: _kIndigo,
                          borderRadius: BorderRadius.circular(16),
                          boxShadow: [BoxShadow(color: _kIndigo.withOpacity(0.4), blurRadius: 20, offset: const Offset(0, 8))],
                        ),
                        child: const Icon(Icons.security_rounded, size: 32, color: Colors.white),
                      ),
                    ),
                    const SizedBox(height: 16),

                    // ── Subtitle ──
                    const Text(
                      'Enterprise billing infrastructure',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 12, color: _kTextMuted, letterSpacing: 0.5),
                    ),
                    const SizedBox(height: 28),

                    // ── Full Name ──
                    _DarkLabel(label: 'FULL NAME'),
                    const SizedBox(height: 6),
                    _DarkField(
                      controller: _fullNameController,
                      hint: 'Alex Morgan',
                      icon: Icons.person_outline,
                      validator: (v) => AppValidators.required(v, 'Please enter your full name'),
                    ),
                    const SizedBox(height: 14),

                    // ── Email ──
                    _DarkLabel(label: 'EMAIL (WORK EMAIL)'),
                    const SizedBox(height: 6),
                    _DarkField(
                      controller: _emailController,
                      hint: 'alex@company.com',
                      icon: Icons.email_outlined,
                      keyboardType: TextInputType.emailAddress,
                      validator: AppValidators.email,
                    ),
                    const SizedBox(height: 14),

                    // ── Password ──
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const _DarkLabel(label: 'PASSWORD'),
                        Text(
                          'Entropy: ${_entropyLabel(_pwdScore)}',
                          style: TextStyle(fontSize: 10, fontWeight: FontWeight.w700, color: _entropyColor(_pwdScore)),
                        ),
                      ],
                    ),
                    const SizedBox(height: 6),
                    _DarkPasswordField(
                      controller: _passwordController,
                      showPassword: _showPassword,
                      onToggle: () => setState(() => _showPassword = !_showPassword),
                      onChanged: (_) => setState(() {}),
                    ),
                    const SizedBox(height: 6),
                    // Strength bar
                    Row(
                      children: List.generate(4, (i) => Expanded(
                        child: Container(
                          margin: EdgeInsets.only(right: i < 3 ? 4 : 0),
                          height: 3,
                          decoration: BoxDecoration(
                            color: i < _pwdScore ? _entropyColor(_pwdScore) : _kBorderCol,
                            borderRadius: BorderRadius.circular(2),
                          ),
                        ),
                      )),
                    ),
                    const SizedBox(height: 14),

                    // ── Phone ──
                    _DarkLabel(label: 'PHONE (SMS AUTHENTICATION)'),
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        // Country code
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
                          decoration: BoxDecoration(
                            color: _kCardBg,
                            border: Border.all(color: _kBorderCol),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: const Text('🇮🇳  +91', style: TextStyle(fontSize: 14, color: _kTextLight, fontWeight: FontWeight.w600)),
                        ),
                        const SizedBox(width: 10),
                        Expanded(
                          child: _DarkField(
                            controller: _phoneController,
                            hint: '(555) 000-8392',
                            icon: Icons.phone_outlined,
                            keyboardType: TextInputType.phone,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 14),

                    // ── Business Name ──
                    _DarkLabel(label: 'BUSINESS NAME (LEGAL ENTITY NAME)'),
                    const SizedBox(height: 6),
                    _DarkField(
                      controller: _businessNameController,
                      hint: 'Acme Financial Corp',
                      icon: Icons.corporate_fare_rounded,
                      validator: (v) => AppValidators.required(v, 'Please enter your business name'),
                    ),
                    const SizedBox(height: 14),

                    // ── Business Type ──
                    _DarkLabel(label: 'BUSINESS TYPE (ARCHITECTURE SEGMENT)'),
                    const SizedBox(height: 6),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 14),
                      decoration: BoxDecoration(
                        color: _kCardBg,
                        border: Border.all(color: _kBorderCol),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: DropdownButtonHideUnderline(
                        child: DropdownButton<String>(
                          value: _businessType,
                          hint: const Text('Select billing model...', style: TextStyle(color: _kTextMuted, fontSize: 14)),
                          dropdownColor: _kCardBg,
                          icon: const Icon(Icons.keyboard_arrow_down_rounded, color: _kTextMuted),
                          isExpanded: true,
                          style: const TextStyle(color: _kTextLight, fontSize: 14),
                          items: _businessTypes.map((t) => DropdownMenuItem(value: t, child: Text(t))).toList(),
                          onChanged: (v) => setState(() => _businessType = v),
                        ),
                      ),
                    ),
                    const SizedBox(height: 20),

                    // ── MSA checkbox ──
                    Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        SizedBox(
                          width: 20,
                          height: 20,
                          child: Checkbox(
                            value: _agreed,
                            activeColor: _kIndigo,
                            side: const BorderSide(color: _kBorderCol, width: 1.5),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(4)),
                            onChanged: (v) => setState(() => _agreed = v ?? false),
                          ),
                        ),
                        const SizedBox(width: 10),
                        Expanded(
                          child: RichText(
                            text: const TextSpan(
                              style: TextStyle(fontSize: 13, color: _kTextMuted, height: 1.4),
                              children: [
                                TextSpan(text: 'I agree to the '),
                                TextSpan(text: 'Master Service Agreement', style: TextStyle(color: _kIndigo, fontWeight: FontWeight.w600)),
                                TextSpan(text: ' and '),
                                TextSpan(text: 'Privacy Policy', style: TextStyle(color: _kIndigo, fontWeight: FontWeight.w600)),
                                TextSpan(text: '.'),
                              ],
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),

                    // ── CTA Button ──
                    SizedBox(
                      height: 52,
                      child: ElevatedButton(
                        onPressed: auth.isLoading ? null : _handleRegister,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: _kIndigo,
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                          elevation: 0,
                        ),
                        child: auth.isLoading
                            ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                            : const Text('Create Business Account  →', style: TextStyle(fontSize: 15, fontWeight: FontWeight.w700, color: Colors.white)),
                      ),
                    ),
                    const SizedBox(height: 16),

                    // ── Sign In Link ──
                    GestureDetector(
                      onTap: () => context.pop(),
                      child: const Text(
                        'Already registered? Sign In ›',
                        textAlign: TextAlign.center,
                        style: TextStyle(fontSize: 13, color: _kIndigo, fontWeight: FontWeight.w600),
                      ),
                    ),
                    const SizedBox(height: 28),

                    // ── Footer Badges ──
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        _FooterBadge(label: 'PCI DSS LEVEL 1', icon: Icons.shield_outlined, color: _kIndigo),
                        const SizedBox(width: 10),
                        _FooterBadge(label: 'ISO 27001 CERTIFIED', icon: Icons.verified_outlined, color: const Color(0xFF10B981)),
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

// ── Dark-themed field helpers ──

class _DarkLabel extends StatelessWidget {
  final String label;
  const _DarkLabel({required this.label});

  @override
  Widget build(BuildContext context) {
    return Text(
      label,
      style: const TextStyle(
        fontSize: 10,
        fontWeight: FontWeight.w800,
        letterSpacing: 1.3,
        color: _kTextMuted,
      ),
    );
  }
}

class _DarkField extends StatelessWidget {
  final TextEditingController controller;
  final String hint;
  final IconData icon;
  final TextInputType? keyboardType;
  final String? Function(String?)? validator;

  const _DarkField({
    required this.controller,
    required this.hint,
    required this.icon,
    this.keyboardType,
    this.validator,
  });

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      controller: controller,
      keyboardType: keyboardType,
      validator: validator,
      style: const TextStyle(color: _kTextLight, fontSize: 14),
      decoration: InputDecoration(
        hintText: hint,
        hintStyle: const TextStyle(color: _kTextMuted, fontSize: 14),
        prefixIcon: Icon(icon, size: 18, color: _kTextMuted),
        filled: true,
        fillColor: _kCardBg,
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: _kBorderCol),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: _kBorderCol),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: _kIndigo, width: 1.5),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: AppTheme.errorColor),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: AppTheme.errorColor, width: 1.5),
        ),
      ),
    );
  }
}

class _DarkPasswordField extends StatelessWidget {
  final TextEditingController controller;
  final bool showPassword;
  final VoidCallback onToggle;
  final ValueChanged<String> onChanged;

  const _DarkPasswordField({
    required this.controller,
    required this.showPassword,
    required this.onToggle,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      controller: controller,
      obscureText: !showPassword,
      onChanged: onChanged,
      validator: AppValidators.password,
      style: const TextStyle(color: _kTextLight, fontSize: 14),
      decoration: InputDecoration(
        hintText: 'Create strong password',
        hintStyle: const TextStyle(color: _kTextMuted, fontSize: 14),
        prefixIcon: const Icon(Icons.lock_outline, size: 18, color: _kTextMuted),
        suffixIcon: IconButton(
          icon: Icon(showPassword ? Icons.visibility_off_outlined : Icons.visibility_outlined, size: 18, color: _kTextMuted),
          onPressed: onToggle,
        ),
        filled: true,
        fillColor: _kCardBg,
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: _kBorderCol)),
        enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: _kBorderCol)),
        focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: _kIndigo, width: 1.5)),
        errorBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: AppTheme.errorColor)),
        focusedErrorBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: AppTheme.errorColor, width: 1.5)),
      ),
    );
  }
}

class _FooterBadge extends StatelessWidget {
  final String label;
  final IconData icon;
  final Color color;
  const _FooterBadge({required this.label, required this.icon, required this.color});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Row(
        children: [
          Icon(icon, size: 12, color: color),
          const SizedBox(width: 5),
          Text(label, style: TextStyle(fontSize: 9, fontWeight: FontWeight.w800, letterSpacing: 0.8, color: color)),
        ],
      ),
    );
  }
}
