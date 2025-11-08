namespace MarketCampaignProject.DTOs
{
    public class CampaignDto
    {
        public string CampaignName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalLeads { get; set; }
        public double OpenRate { get; set; }
        public double ConversionRate { get; set; }
    }
}
