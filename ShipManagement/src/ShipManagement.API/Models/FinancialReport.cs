namespace ShipManagement.API.Models
{
    public class FinancialReportRequest
    {
        public string ShipCode { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class FinancialReportItem
    {
        public string AccountDescription { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal ActualValue { get; set; }
        public decimal BudgetValue { get; set; }
        public decimal VarianceActual { get; set; }
        public decimal ActualValueYTD { get; set; }
        public decimal BudgetValueYTD { get; set; }
        public decimal VarianceYTD { get; set; }
        public string AccountPeriodLabel { get; set; } = string.Empty;
        public string FiscalYearStartLabel { get; set; } = string.Empty;
        public string FiscalYearEndLabel { get; set; } = string.Empty;
    }
}
