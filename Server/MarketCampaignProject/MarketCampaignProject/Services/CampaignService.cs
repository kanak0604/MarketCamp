using Microsoft.EntityFrameworkCore;
using MarketCampaignProject.Data;
using MarketCampaignProject.Models;
using MarketCampaignProject.DTOs;

namespace MarketCampaignProject.Services
{
    public class CampaignService
    {
        private readonly ApplicationDbContext _context;

        public CampaignService(ApplicationDbContext context)
        {
            _context = context;
        }
        //Getting all the campaign data 
        public async Task<List<Campaign>> GetAllCampaignAsync()
        {
            return await _context.Campaigns.ToListAsync();
        }

    }
}