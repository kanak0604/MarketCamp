using System.ComponentModel.DataAnnotations;

namespace MarketCampaignProject.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
