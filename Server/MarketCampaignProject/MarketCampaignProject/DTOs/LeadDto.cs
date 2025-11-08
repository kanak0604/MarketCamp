namespace MarketCampaignProject.DTOs
{
    public class LeadDto
    {
        public int LeadID { get; set; }                     // Auto ID
        public string Name { get; set; } = string.Empty;    // Required
        public string Email { get; set; } = string.Empty;   // Required
        public string? PhoneNumber { get; set; }            // Optional
        public int? CampaignAssignment { get; set; }        // Nullable FK
        public string? Segment { get; set; }                // Optional
    }
}
