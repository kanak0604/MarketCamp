using System.ComponentModel.DataAnnotations;
namespace MarketCampaignProject.Models
{
    public class Campaign
    {
        [Key]
        public int CampaignId { get; set; }

        [Required]
        public string CampaignName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalLeads { get; set; }
        public double OpenRate { get; set; }
        public double ConversionRate { get; set; }
        public double ClickThroughRate { get; set; }

        public string Status { get; set; } = "Active";
        public string? Agency { get; set; }
        public string? Buyer { get; set; }
        public string? Brand { get; set; }


    }
}
