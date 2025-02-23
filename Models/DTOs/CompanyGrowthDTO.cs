namespace StockAPI.Models.DTOs
{
    public class CompanyGrowthDTO
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public List<YearlyEpsDTO> YearlyEps { get; set; } = new List<YearlyEpsDTO>();
    }

    public class YearlyEpsDTO
    {
        public int? Year { get; set; }
        public decimal? Eps { get; set; }
    }
} 