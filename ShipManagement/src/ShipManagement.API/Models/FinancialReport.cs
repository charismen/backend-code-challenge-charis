namespace ShipManagement.API.Models
{
    public class FinancialReportRequest
    {
        public int ShipId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class FinancialReportItem
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public int? ParentAccountId { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal YTDActual { get; set; }
        public decimal YTDBudget { get; set; }
        public decimal YTDVariance { get; set; }
    }
}