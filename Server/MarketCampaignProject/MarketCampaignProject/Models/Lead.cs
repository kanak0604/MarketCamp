namespace MarketCampaignProject.Models
{
    public class Lead
    {
        public int LeadID { get; set; }             // Primary key
        public string Name { get; set; }            // Required
        public string Email { get; set; }           // Required
        public string PhoneNumber { get; set; }     // Optional
        public int? CampaignAssignment { get; set; } // Nullable FK
        public string? Segment { get; set; }        // Optional
        public bool HasOpenedEmail { get; set; }      
        public bool HasConverted { get; set; }
    }
}
