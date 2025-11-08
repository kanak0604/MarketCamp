using System.Runtime.CompilerServices;

namespace MarketCampaignProject.DTOs
{
    //  DTOs are what the API and frontend needs 
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "BrandPartner";

    }
}
