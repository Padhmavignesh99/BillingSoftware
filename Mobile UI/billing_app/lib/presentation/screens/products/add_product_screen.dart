import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../../core/constants/app_constants.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/utils/validators.dart';
import '../../models/product_model.dart';
import '../../providers/product_provider.dart';
import '../../widgets/common/app_button.dart';
import '../../widgets/common/app_text_field.dart';

class AddProductScreen extends StatefulWidget {
  final int? productId;

  const AddProductScreen({super.key, this.productId});

  @override
  State<AddProductScreen> createState() => _AddProductScreenState();
}

class _AddProductScreenState extends State<AddProductScreen> {
  final _formKey = GlobalKey<FormState>();

  late TextEditingController _nameCtrl;
  late TextEditingController _skuCtrl;
  late TextEditingController _barcodeCtrl;
  late TextEditingController _hsnCtrl;
  late TextEditingController _costPriceCtrl;
  late TextEditingController _sellingPriceCtrl;
  late TextEditingController _mrpCtrl;
  late TextEditingController _stockCtrl;
  late TextEditingController _minStockCtrl;

  String _selectedCategory = 'Groceries';
  String _selectedUnit = 'Pcs';
  double _taxPercentage = 18.0;
  bool _isService = false;

  bool get isEdit => widget.productId != null;

  @override
  void initState() {
    super.initState();
    _nameCtrl = TextEditingController();
    _skuCtrl = TextEditingController();
    _barcodeCtrl = TextEditingController();
    _hsnCtrl = TextEditingController();
    _costPriceCtrl = TextEditingController();
    _sellingPriceCtrl = TextEditingController();
    _mrpCtrl = TextEditingController();
    _stockCtrl = TextEditingController(text: '0');
    _minStockCtrl = TextEditingController(text: '5');

    if (isEdit) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        final p = context.read<ProductProvider>().products.firstWhere(
              (prod) => prod.id == widget.productId,
              orElse: () => context.read<ProductProvider>().products.first,
            );
        setState(() {
          _nameCtrl.text = p.name;
          _skuCtrl.text = p.sku ?? '';
          _barcodeCtrl.text = p.barcode ?? '';
          _hsnCtrl.text = p.hsnCode ?? '';
          _costPriceCtrl.text = p.purchasePrice.toString();
          _sellingPriceCtrl.text = p.sellingPrice.toString();
          _mrpCtrl.text = p.mrp?.toString() ?? '';
          _stockCtrl.text = p.currentStock.toString();
          _minStockCtrl.text = p.minStockLevel?.toString() ?? '5';
          _selectedCategory = p.categoryName ?? 'Groceries';
          _selectedUnit = p.unitName ?? 'Pcs';
          _taxPercentage = p.taxPercentage ?? 18.0;
          _isService = p.isService;
        });
      });
    }
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _skuCtrl.dispose();
    _barcodeCtrl.dispose();
    _hsnCtrl.dispose();
    _costPriceCtrl.dispose();
    _sellingPriceCtrl.dispose();
    _mrpCtrl.dispose();
    _stockCtrl.dispose();
    _minStockCtrl.dispose();
    super.dispose();
  }

  Future<void> _saveProduct() async {
    if (!_formKey.currentState!.validate()) return;

    final pProvider = context.read<ProductProvider>();
    final request = ProductRequest(
      name: _nameCtrl.text.trim(),
      sku: _skuCtrl.text.trim().isEmpty ? null : _skuCtrl.text.trim(),
      barcode: _barcodeCtrl.text.trim().isEmpty ? null : _barcodeCtrl.text.trim(),
      hsnCode: _hsnCtrl.text.trim().isEmpty ? null : _hsnCtrl.text.trim(),
      purchasePrice: double.tryParse(_costPriceCtrl.text) ?? 0.0,
      sellingPrice: double.tryParse(_sellingPriceCtrl.text) ?? 0.0,
      mrp: double.tryParse(_mrpCtrl.text),
      openingStock: double.tryParse(_stockCtrl.text) ?? 0.0,
      minStockLevel: double.tryParse(_minStockCtrl.text),
      isService: _isService,
    );

    bool success;
    if (isEdit) {
      success = await pProvider.updateProduct(widget.productId!, request);
    } else {
      success = await pProvider.createProduct(request);
    }

    if (!mounted) return;

    if (success) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(isEdit ? 'Product updated successfully' : 'Product created successfully'),
          backgroundColor: AppTheme.successColor,
        ),
      );
      context.pop();
    }
  }

  @override
  Widget build(BuildContext context) {
    final pProvider = context.watch<ProductProvider>();

    return Scaffold(
      appBar: AppBar(
        title: Text(isEdit ? 'Edit Product' : 'Add New Product'),
        actions: [
          if (isEdit)
            IconButton(
              icon: const Icon(Icons.delete_outline, color: AppTheme.errorColor),
              onPressed: () async {
                final confirm = await showDialog<bool>(
                  context: context,
                  builder: (ctx) => AlertDialog(
                    title: const Text('Delete Product'),
                    content: const Text('Are you sure you want to delete this product?'),
                    actions: [
                      TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Cancel')),
                      ElevatedButton(
                        style: ElevatedButton.styleFrom(backgroundColor: AppTheme.errorColor),
                        onPressed: () => Navigator.pop(ctx, true),
                        child: const Text('Delete'),
                      ),
                    ],
                  ),
                );
                if (confirm == true && mounted) {
                  await pProvider.deleteProduct(widget.productId!);
                  if (context.mounted) context.pop();
                }
              },
            ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              AppTextField(
                controller: _nameCtrl,
                label: 'Product Name *',
                hint: 'e.g. Basmati Rice 5kg',
                validator: (v) => AppValidators.required(v, 'Product name is required'),
              ),
              const SizedBox(height: 16),

              Row(
                children: [
                  Expanded(
                    child: AppTextField(
                      controller: _skuCtrl,
                      label: 'SKU Code',
                      hint: 'GRO-001',
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: AppTextField(
                      controller: _barcodeCtrl,
                      label: 'Barcode',
                      hint: '890123456789',
                      suffixIcon: IconButton(
                        icon: const Icon(Icons.qr_code_scanner, size: 20),
                        onPressed: () {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text('Barcode scanner active')),
                          );
                        },
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),

              Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Category', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                        const SizedBox(height: 6),
                        DropdownButtonFormField<String>(
                          value: _selectedCategory,
                          items: AppConstants.defaultCategories.map((c) {
                            return DropdownMenuItem(value: c, child: Text(c));
                          }).toList(),
                          onChanged: (v) {
                            if (v != null) setState(() => _selectedCategory = v);
                          },
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Unit', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                        const SizedBox(height: 6),
                        DropdownButtonFormField<String>(
                          value: _selectedUnit,
                          items: AppConstants.defaultUnits.map((u) {
                            return DropdownMenuItem(value: u, child: Text(u));
                          }).toList(),
                          onChanged: (v) {
                            if (v != null) setState(() => _selectedUnit = v);
                          },
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 20),

              const Divider(),
              const SizedBox(height: 12),
              const Text('Pricing & Taxation', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
              const SizedBox(height: 14),

              Row(
                children: [
                  Expanded(
                    child: AppTextField(
                      controller: _costPriceCtrl,
                      label: 'Purchase Cost (₹)',
                      hint: '0.00',
                      keyboardType: TextInputType.number,
                      validator: AppValidators.number,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: AppTextField(
                      controller: _sellingPriceCtrl,
                      label: 'Selling Price (₹) *',
                      hint: '0.00',
                      keyboardType: TextInputType.number,
                      validator: (v) => AppValidators.positiveNumber(v, 'Required'),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: AppTextField(
                      controller: _mrpCtrl,
                      label: 'MRP (₹)',
                      hint: '0.00',
                      keyboardType: TextInputType.number,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),

              Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('GST Tax Rate', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                        const SizedBox(height: 6),
                        DropdownButtonFormField<double>(
                          value: _taxPercentage,
                          items: AppConstants.taxRates.map((r) {
                            return DropdownMenuItem(value: r, child: Text('$r%'));
                          }).toList(),
                          onChanged: (v) {
                            if (v != null) setState(() => _taxPercentage = v);
                          },
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: AppTextField(
                      controller: _hsnCtrl,
                      label: 'HSN Code',
                      hint: '1006',
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 20),

              const Divider(),
              const SizedBox(height: 12),
              const Text('Stock Control', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
              const SizedBox(height: 14),

              Row(
                children: [
                  Expanded(
                    child: AppTextField(
                      controller: _stockCtrl,
                      label: 'Opening Stock',
                      hint: '0',
                      keyboardType: TextInputType.number,
                      validator: AppValidators.number,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: AppTextField(
                      controller: _minStockCtrl,
                      label: 'Low Stock Alert At',
                      hint: '5',
                      keyboardType: TextInputType.number,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 32),

              AppButton(
                label: isEdit ? 'Update Product' : 'Save & Add Product',
                isLoading: pProvider.isLoading,
                onPressed: _saveProduct,
              ),
              const SizedBox(height: 24),
            ],
          ),
        ),
      ),
    );
  }
}

