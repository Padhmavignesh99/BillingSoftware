class AppValidators {
  static String? required(String? value, [String message = 'This field is required']) {
    if (value == null || value.trim().isEmpty) {
      return message;
    }
    return null;
  }

  static String? email(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'Email is required';
    }
    final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
    if (!emailRegex.hasMatch(value.trim())) {
      return 'Please enter a valid email address';
    }
    return null;
  }

  static String? password(String? value) {
    if (value == null || value.isEmpty) {
      return 'Password is required';
    }
    if (value.length < 6) {
      return 'Password must be at least 6 characters';
    }
    return null;
  }

  static String? phone(String? value) {
    if (value == null || value.trim().isEmpty) {
      return null; // Phone is optional in some forms unless validated with required
    }
    final phoneRegex = RegExp(r'^\+?[0-9]{10,13}$');
    if (!phoneRegex.hasMatch(value.replaceAll(' ', ''))) {
      return 'Enter a valid 10-digit mobile number';
    }
    return null;
  }

  static String? number(String? value, [String message = 'Please enter a valid number']) {
    if (value == null || value.trim().isEmpty) {
      return null;
    }
    if (double.tryParse(value.trim()) == null) {
      return message;
    }
    return null;
  }

  static String? positiveNumber(String? value, [String message = 'Must be greater than 0']) {
    if (value == null || value.trim().isEmpty) {
      return null;
    }
    final num = double.tryParse(value.trim());
    if (num == null || num <= 0) {
      return message;
    }
    return null;
  }

  static String? gstNumber(String? value) {
    if (value == null || value.trim().isEmpty) {
      return null;
    }
    // Indian GST format: 15 chars (e.g. 22AAAAA0000A1Z5)
    final gstRegex = RegExp(r'^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$');
    if (!gstRegex.hasMatch(value.trim().toUpperCase())) {
      return 'Invalid GST format (e.g. 22AAAAA0000A1Z5)';
    }
    return null;
  }
}

