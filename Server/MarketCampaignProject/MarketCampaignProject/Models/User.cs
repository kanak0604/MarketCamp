using Microsoft.EntityFrameworkCore;

namespace MarketCampaignProject.Models
{
    public class User
    {
        // these all are the column names in the database 
        // Models are what the database need 
        public int Id { get; set; } 
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }=string.Empty;
        public string PasswordHash {  get; set; } = string.Empty;
    }
}
