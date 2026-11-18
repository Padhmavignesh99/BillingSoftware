import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/validators.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController    = TextEditingController(text: 'admin@billing.com');
  final _passwordController = TextEditingController(text: 'Password@123');
  bool _rememberMe = true;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _handleLogin() async {
    if (!_formKey.currentState!.validate()) return;

    final auth = context.read<AuthProvider>();
    final success = await auth.login(
      _emailController.text.trim(),
      _passwordController.text,
    );

    if (!mounted) return;

    if (success) {
      if (!auth.isOnboarded) {
        context.go('/onboarding');
      } else {
        context.go('/dashboard');
      }
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(auth.errorMessage ?? 'Login failed. Please check credentials.'),
          backgroundColor: AppTheme.errorColor,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();

    return Scaffold(
      backgroundColor: const Color(0xFFF0F2F5),
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
                    // ── App Icon ──
                    Center(
                      child: Container(
                        width: 68,
                        height: 68,
                        decoration: BoxDecoration(
                          color: AppTheme.primaryColor,
                          borderRadius: BorderRadius.circular(20),
                          boxShadow: [
                            BoxShadow(
                              color: AppTheme.primaryColor.withOpacity(0.35),
                              blurRadius: 20,
                              offset: const Offset(0, 8),
                            ),
                          ],
                        ),
                        child: const Icon(
                          Icons.account_balance_rounded,
                          size: 36,
                          color: Colors.white,
                        ),
                      ),
                    ),
                    const SizedBox(height: 24),

                    // ── Title ──
                    const Text(
                      'Sign in to Master Billing',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.w900,
                        color: AppTheme.textDark,
                        letterSpacing: -0.5,
                      ),
                    ),
                    const SizedBox(height: 6),
                    const Text(
                      'Access your institutional billing & real-time\ncapital flow portal',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 13, color: AppTheme.textMuted, height: 1.5),
                    ),
                    const SizedBox(height: 28),

                    // ── Google SSO ──
                    _SsoButton(
                      label: 'Sign in with Google',
                      icon: _GoogleIcon(),
                      onTap: () {},
                    ),
                    const SizedBox(height: 12),

                    // ── Apple SSO ──
                    _SsoButton(
                      label: 'Sign in with Apple',
                      icon: const Icon(Icons.apple, size: 22, color: Color(0xFF0F172A)),
                      onTap: () {},
                    ),
                    const SizedBox(height: 20),

                    // ── Divider ──
                    Row(
                      children: const [
                        Expanded(child: Divider()),
                        Padding(
                          padding: EdgeInsets.symmetric(horizontal: 12),
                          child: Text(
                            'OR CONTINUE WITH EMAIL',
                            style: TextStyle(
                              fontSize: 10,
                              fontWeight: FontWeight.w700,
                              letterSpacing: 1.2,
                              color: AppTheme.textMuted,
                            ),
                          ),
                        ),
                        Expanded(child: Divider()),
                      ],
                    ),
                    const SizedBox(height: 20),

                    // ── Email Field ──
                    _FieldLabel(
                      label: 'Email Address',
                      trailing: _Badge(label: 'SSO ENROLLED', color: const Color(0xFF10B981)),
                    ),
                    const SizedBox(height: 6),
                    AppTextField(
                      controller: _emailController,
                      hint: 'you@company.com',
                      keyboardType: TextInputType.emailAddress,
                      prefixIcon: const Icon(Icons.email_outlined, size: 20),
                      validator: AppValidators.email,
                    ),
                    const SizedBox(height: 16),

                    // ── Password Field ──
                    _FieldLabel(
                      label: 'Password',
                      trailing: GestureDetector(
                        onTap: () {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text('Password reset instructions sent to registered email.')),
                          );
                        },
                        child: const Text(
                          'Forgot password?',
                          style: TextStyle(
                            fontSize: 13,
                            fontWeight: FontWeight.w600,
                            color: AppTheme.primaryColor,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 6),
                    AppTextField(
                      controller: _passwordController,
                      hint: '••••••••',
                      isPassword: true,
                      prefixIcon: const Icon(Icons.lock_outline, size: 20),
                      validator: AppValidators.password,
                    ),
                    const SizedBox(height: 14),

                    // ── Remember me ──
                    Row(
                      children: [
                        SizedBox(
                          height: 24,
                          width: 24,
                          child: Checkbox(
                            value: _rememberMe,
                            activeColor: AppTheme.primaryColor,
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(4)),
                            onChanged: (val) => setState(() => _rememberMe = val ?? true),
                          ),
                        ),
                        const SizedBox(width: 8),
                        const Expanded(
                          child: Text(
                            'Remember this verified hardware',
                            style: TextStyle(fontSize: 13, color: AppTheme.textDark),
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                          decoration: BoxDecoration(
                            color: const Color(0xFFEEF2FF),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Row(
                            children: const [
                              Icon(Icons.circle, size: 6, color: AppTheme.primaryColor),
                              SizedBox(width: 4),
                              Text('30d', style: TextStyle(fontSize: 11, fontWeight: FontWeight.w700, color: AppTheme.primaryColor)),
                            ],
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),

                    // ── Sign In Button ──
                    AppButton(
                      label: 'Sign In  →',
                      isLoading: auth.isLoading,
                      onPressed: _handleLogin,
                    ),
                    const SizedBox(height: 12),

                    // ── Face ID / Touch ID ──
                    Container(
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(14),
                        border: Border.all(color: AppTheme.borderLight),
                      ),
                      child: ListTile(
                        leading: Container(
                          width: 40,
                          height: 40,
                          decoration: BoxDecoration(
                            color: const Color(0xFFF1F5F9),
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(Icons.fingerprint, size: 22, color: AppTheme.primaryColor),
                        ),
                        title: const Text(
                          'Face ID / Touch ID',
                          style: TextStyle(fontWeight: FontWeight.w700, fontSize: 14),
                        ),
                        subtitle: const Text(
                          'INSTANT HARDWARE KEY',
                          style: TextStyle(fontSize: 9, letterSpacing: 1.1, fontWeight: FontWeight.w700, color: AppTheme.textMuted),
                        ),
                        onTap: () {},
                        dense: true,
                      ),
                    ),
                    const SizedBox(height: 28),

                    // ── Register Link ──
                    GestureDetector(
                      onTap: () => context.push('/register'),
                      child: RichText(
                        textAlign: TextAlign.center,
                        text: const TextSpan(
                          style: TextStyle(fontSize: 13, color: AppTheme.textMuted),
                          children: [
                            TextSpan(text: 'New to Master Billing? '),
                            TextSpan(
                              text: 'Create an institutional account',
                              style: TextStyle(
                                color: AppTheme.primaryColor,
                                fontWeight: FontWeight.w700,
                                decoration: TextDecoration.underline,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(height: 24),

                    // ── Security footer ──
                    const Divider(),
                    const SizedBox(height: 10),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: const [
                        Icon(Icons.lock_outline, size: 11, color: AppTheme.textMuted),
                        SizedBox(width: 4),
                        Text('256-BIT SSL ENCRYPTION', style: TextStyle(fontSize: 9, letterSpacing: 1.1, fontWeight: FontWeight.w700, color: AppTheme.textMuted)),
                        SizedBox(width: 10),
                        Text('•', style: TextStyle(color: AppTheme.textMuted)),
                        SizedBox(width: 10),
                        Icon(Icons.verified_outlined, size: 11, color: AppTheme.textMuted),
                        SizedBox(width: 4),
                        Text('SOC-2 TYPE II', style: TextStyle(fontSize: 9, letterSpacing: 1.1, fontWeight: FontWeight.w700, color: AppTheme.textMuted)),
                      ],
                    ),
                    const SizedBox(height: 6),
                    const Text(
                      'Zero-knowledge authentication powered by Master Billing\nSecurity Vault',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 10, color: AppTheme.textMuted, height: 1.5),
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

// ── Shared small widgets ──

class _SsoButton extends StatelessWidget {
  final String label;
  final Widget icon;
  final VoidCallback onTap;
  const _SsoButton({required this.label, required this.icon, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 14),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: AppTheme.borderLight),
          boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.04), blurRadius: 6, offset: const Offset(0, 2))],
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [icon, const SizedBox(width: 10), Text(label, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppTheme.textDark))],
        ),
      ),
    );
  }
}

class _FieldLabel extends StatelessWidget {
  final String label;
  final Widget? trailing;
  const _FieldLabel({required this.label, this.trailing});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w700, color: AppTheme.textDark)),
        if (trailing != null) trailing!,
      ],
    );
  }
}

class _Badge extends StatelessWidget {
  final String label;
  final Color color;
  const _Badge({required this.label, required this.color});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Text(
        label,
        style: TextStyle(fontSize: 9, fontWeight: FontWeight.w800, letterSpacing: 1.0, color: color),
      ),
    );
  }
}

class _GoogleIcon extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 20,
      height: 20,
      child: CustomPaint(painter: _GooglePainter()),
    );
  }
}

class _GooglePainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()..style = PaintingStyle.fill;
    // Simple colored G representation via text
    final tp = TextPainter(
      text: const TextSpan(
        text: 'G',
        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF4285F4)),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset((size.width - tp.width) / 2, (size.height - tp.height) / 2));
  }

  @override
  bool shouldRepaint(_) => false;
}
