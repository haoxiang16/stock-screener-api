namespace StockAPI.Models.DTOs
{
    public class FinancialGrowthQueryDTO
    {
        public int? EpsYears { get; set; }
        public int? OperatingMarginYears { get; set; }
        public int? GrossMarginYears { get; set; }
        public int? NetProfitMarginYears { get; set; }
        public decimal? MinOperatingMargin { get; set; }
        public int? MinOperatingMarginYears { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
} 