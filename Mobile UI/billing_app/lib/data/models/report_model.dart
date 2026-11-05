class ChartDataPoint {
  final String label;
  final double value;

  ChartDataPoint({required this.label, required this.value});

  factory ChartDataPoint.fromJson(Map<String, dynamic> json) {
    return ChartDataPoint(
      label: json['label'] ?? json['Label'] ?? '',
      value: ((json['value'] ?? json['Value'] ?? 0) as num).toDouble(),
    );
  }
}

class DashboardSummaryResponse {
  final double todaySales;
  final double todayExpenses;
  final int todayInvoiceCount;
  final double totalRevenue;
  final double totalDueAmount;
  final int totalCustomers;
  final int totalProducts;
  final int lowStockProductCount;
  final double monthlyRevenue;
  final double monthlyExpenses;

  DashboardSummaryResponse({
    this.todaySales = 0.0,
    this.todayExpenses = 0.0,
    this.todayInvoiceCount = 0,
    this.totalRevenue = 0.0,
    this.totalDueAmount = 0.0,
    this.totalCustomers = 0,
    this.totalProducts = 0,
    this.lowStockProductCount = 0,
    this.monthlyRevenue = 0.0,
    this.monthlyExpenses = 0.0,
  });

  factory DashboardSummaryResponse.fromJson(Map<String, dynamic> json) {
    return DashboardSummaryResponse(
      todaySales: ((json['todaySales'] ?? json['TodaySales'] ?? 0) as num).toDouble(),
      todayExpenses: ((json['todayExpenses'] ?? json['TodayExpenses'] ?? 0) as num).toDouble(),
      todayInvoiceCount: json['todayInvoiceCount'] ?? json['TodayInvoiceCount'] ?? 0,
      totalRevenue: ((json['totalRevenue'] ?? json['TotalRevenue'] ?? 0) as num).toDouble(),
      totalDueAmount: ((json['totalDueAmount'] ?? json['TotalDueAmount'] ?? 0) as num).toDouble(),
      totalCustomers: json['totalCustomers'] ?? json['TotalCustomers'] ?? 0,
      totalProducts: json['totalProducts'] ?? json['TotalProducts'] ?? 0,
      lowStockProductCount: json['lowStockProductCount'] ?? json['LowStockProductCount'] ?? 0,
      monthlyRevenue: ((json['monthlyRevenue'] ?? json['MonthlyRevenue'] ?? 0) as num).toDouble(),
      monthlyExpenses: ((json['monthlyExpenses'] ?? json['MonthlyExpenses'] ?? 0) as num).toDouble(),
    );
  }
}

class DashboardChartResponse {
  final List<ChartDataPoint> salesChart;
  final List<ChartDataPoint> expenseChart;

  DashboardChartResponse({
    this.salesChart = const [],
    this.expenseChart = const [],
  });

  factory DashboardChartResponse.fromJson(Map<String, dynamic> json) {
    return DashboardChartResponse(
      salesChart: (json['salesChart'] ?? json['SalesChart'] as List<dynamic>?)
              ?.map((e) => ChartDataPoint.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      expenseChart: (json['expenseChart'] ?? json['ExpenseChart'] as List<dynamic>?)
              ?.map((e) => ChartDataPoint.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class SalesSummaryResponse {
  final double totalSales;
  final double totalTax;
  final double totalDiscount;
  final int totalInvoices;
  final double averageOrderValue;
  final List<SalesDayWise> dayWiseSales;

  SalesSummaryResponse({
    this.totalSales = 0.0,
    this.totalTax = 0.0,
    this.totalDiscount = 0.0,
    this.totalInvoices = 0,
    this.averageOrderValue = 0.0,
    this.dayWiseSales = const [],
  });

  factory SalesSummaryResponse.fromJson(Map<String, dynamic> json) {
    return SalesSummaryResponse(
      totalSales: ((json['totalSales'] ?? json['TotalSales'] ?? 0) as num).toDouble(),
      totalTax: ((json['totalTax'] ?? json['TotalTax'] ?? 0) as num).toDouble(),
      totalDiscount: ((json['totalDiscount'] ?? json['TotalDiscount'] ?? 0) as num).toDouble(),
      totalInvoices: json['totalInvoices'] ?? json['TotalInvoices'] ?? 0,
      averageOrderValue: ((json['averageOrderValue'] ?? json['AverageOrderValue'] ?? 0) as num).toDouble(),
      dayWiseSales: (json['dayWiseSales'] ?? json['DayWiseSales'] as List<dynamic>?)
              ?.map((e) => SalesDayWise.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class SalesDayWise {
  final DateTime date;
  final double amount;
  final int invoiceCount;

  SalesDayWise({
    required this.date,
    required this.amount,
    required this.invoiceCount,
  });

  factory SalesDayWise.fromJson(Map<String, dynamic> json) {
    return SalesDayWise(
      date: json['date'] != null || json['Date'] != null
          ? DateTime.tryParse(json['date'] ?? json['Date'] ?? '') ?? DateTime.now()
          : DateTime.now(),
      amount: ((json['amount'] ?? json['Amount'] ?? 0) as num).toDouble(),
      invoiceCount: json['invoiceCount'] ?? json['InvoiceCount'] ?? 0,
    );
  }
}

class ProfitLossResponse {
  final double totalRevenue;
  final double totalCostOfGoods;
  final double grossProfit;
  final double totalExpenses;
  final double netProfit;
  final double profitMargin;

  ProfitLossResponse({
    this.totalRevenue = 0.0,
    this.totalCostOfGoods = 0.0,
    this.grossProfit = 0.0,
    this.totalExpenses = 0.0,
    this.netProfit = 0.0,
    this.profitMargin = 0.0,
  });

  factory ProfitLossResponse.fromJson(Map<String, dynamic> json) {
    return ProfitLossResponse(
      totalRevenue: ((json['totalRevenue'] ?? json['TotalRevenue'] ?? 0) as num).toDouble(),
      totalCostOfGoods: ((json['totalCostOfGoods'] ?? json['TotalCostOfGoods'] ?? 0) as num).toDouble(),
      grossProfit: ((json['grossProfit'] ?? json['GrossProfit'] ?? 0) as num).toDouble(),
      totalExpenses: ((json['totalExpenses'] ?? json['TotalExpenses'] ?? 0) as num).toDouble(),
      netProfit: ((json['netProfit'] ?? json['NetProfit'] ?? 0) as num).toDouble(),
      profitMargin: ((json['profitMargin'] ?? json['ProfitMargin'] ?? 0) as num).toDouble(),
    );
  }
}

class TaxReportResponse {
  final double totalTaxCollected;
  final double totalCGST;
  final double totalSGST;
  final double totalIGST;
  final double totalInputTax;
  final double netTaxPayable;
  final List<TaxBreakdown> breakdown;

  TaxReportResponse({
    this.totalTaxCollected = 0.0,
    this.totalCGST = 0.0,
    this.totalSGST = 0.0,
    this.totalIGST = 0.0,
    this.totalInputTax = 0.0,
    this.netTaxPayable = 0.0,
    this.breakdown = const [],
  });

  factory TaxReportResponse.fromJson(Map<String, dynamic> json) {
    return TaxReportResponse(
      totalTaxCollected: ((json['totalTaxCollected'] ?? json['TotalTaxCollected'] ?? 0) as num).toDouble(),
      totalCGST: ((json['totalCGST'] ?? json['TotalCGST'] ?? 0) as num).toDouble(),
      totalSGST: ((json['totalSGST'] ?? json['TotalSGST'] ?? 0) as num).toDouble(),
      totalIGST: ((json['totalIGST'] ?? json['TotalIGST'] ?? 0) as num).toDouble(),
      totalInputTax: ((json['totalInputTax'] ?? json['TotalInputTax'] ?? 0) as num).toDouble(),
      netTaxPayable: ((json['netTaxPayable'] ?? json['NetTaxPayable'] ?? 0) as num).toDouble(),
      breakdown: (json['breakdown'] ?? json['Breakdown'] as List<dynamic>?)
              ?.map((e) => TaxBreakdown.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class TaxBreakdown {
  final String taxName;
  final double percentage;
  final double amount;
  final int transactionCount;

  TaxBreakdown({
    required this.taxName,
    required this.percentage,
    required this.amount,
    this.transactionCount = 0,
  });

  factory TaxBreakdown.fromJson(Map<String, dynamic> json) {
    return TaxBreakdown(
      taxName: json['taxName'] ?? json['TaxName'] ?? '',
      percentage: ((json['percentage'] ?? json['Percentage'] ?? 0) as num).toDouble(),
      amount: ((json['amount'] ?? json['Amount'] ?? 0) as num).toDouble(),
      transactionCount: json['transactionCount'] ?? json['TransactionCount'] ?? 0,
    );
  }
}

class StockReportResponse {
  final int totalProducts;
  final int inStockProducts;
  final int lowStockProducts;
  final int outOfStockProducts;
  final double totalStockValue;
  final List<StockItemResponse> items;

  StockReportResponse({
    this.totalProducts = 0,
    this.inStockProducts = 0,
    this.lowStockProducts = 0,
    this.outOfStockProducts = 0,
    this.totalStockValue = 0.0,
    this.items = const [],
  });

  factory StockReportResponse.fromJson(Map<String, dynamic> json) {
    return StockReportResponse(
      totalProducts: json['totalProducts'] ?? json['TotalProducts'] ?? 0,
      inStockProducts: json['inStockProducts'] ?? json['InStockProducts'] ?? 0,
      lowStockProducts: json['lowStockProducts'] ?? json['LowStockProducts'] ?? 0,
      outOfStockProducts: json['outOfStockProducts'] ?? json['OutOfStockProducts'] ?? 0,
      totalStockValue: ((json['totalStockValue'] ?? json['TotalStockValue'] ?? 0) as num).toDouble(),
      items: (json['items'] ?? json['Items'] as List<dynamic>?)
              ?.map((e) => StockItemResponse.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class StockItemResponse {
  final int productId;
  final String productName;
  final String? sku;
  final double currentStock;
  final double? minStockLevel;
  final double purchasePrice;
  final double stockValue;
  final String stockStatus;

  StockItemResponse({
    required this.productId,
    required this.productName,
    this.sku,
    required this.currentStock,
    this.minStockLevel,
    this.purchasePrice = 0.0,
    this.stockValue = 0.0,
    this.stockStatus = 'In Stock',
  });

  factory StockItemResponse.fromJson(Map<String, dynamic> json) {
    return StockItemResponse(
      productId: json['productId'] ?? json['ProductId'] ?? 0,
      productName: json['productName'] ?? json['ProductName'] ?? '',
      sku: json['sku'] ?? json['SKU'],
      currentStock: ((json['currentStock'] ?? json['CurrentStock'] ?? 0) as num).toDouble(),
      minStockLevel: json['minStockLevel'] != null || json['MinStockLevel'] != null
          ? ((json['minStockLevel'] ?? json['MinStockLevel']) as num).toDouble()
          : null,
      purchasePrice: ((json['purchasePrice'] ?? json['PurchasePrice'] ?? 0) as num).toDouble(),
      stockValue: ((json['stockValue'] ?? json['StockValue'] ?? 0) as num).toDouble(),
      stockStatus: json['stockStatus'] ?? json['StockStatus'] ?? 'In Stock',
    );
  }
}

