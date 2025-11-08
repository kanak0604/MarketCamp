using MarketCampaignProject.Data;
using MarketCampaignProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketCampaignProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CampaignController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1️⃣ GET all campaigns with filters (Agency, Buyer, Brand, Date range)
        [HttpGet]
        public async Task<IActionResult> GetAllCampaigns(
            [FromQuery] string? agency,
            [FromQuery] string? buyer,
            [FromQuery] string? brand,
            [FromQuery] string? campaignName,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status)
        {
            var query = _context.Campaigns.AsQueryable();

            // ✅ Apply string-based filters dynamically
            if (!string.IsNullOrEmpty(agency))
                query = query.Where(c => c.Agency == agency);
            if (!string.IsNullOrEmpty(buyer))
                query = query.Where(c => c.Buyer == buyer);
            if (!string.IsNullOrEmpty(brand))
                query = query.Where(c => c.Brand == brand);
            if (!string.IsNullOrEmpty(campaignName))
                query = query.Where(c => c.CampaignName.Contains(campaignName));
            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            // ✅ Enhanced date filtering logic
            if (startDate.HasValue && !endDate.HasValue)
            {
                // Show campaigns starting ON or AFTER the given start date
                query = query.Where(c => c.StartDate >= startDate.Value);
            }
            else if (!startDate.HasValue && endDate.HasValue)
            {
                // Show campaigns ending ON or BEFORE the given end date
                query = query.Where(c => c.EndDate <= endDate.Value);
            }
            else if (startDate.HasValue && endDate.HasValue)
            {
                // Show campaigns between (inclusive)
                query = query.Where(c => c.StartDate >= startDate.Value && c.EndDate <= endDate.Value);
            }

            var campaigns = await query.ToListAsync();
            if (!campaigns.Any())
                return Ok(new { success = false, message = "No campaigns found" });

            // Compute analytics metrics for each campaign
            var campaignList = new List<object>();
            foreach (var c in campaigns)
            {
                var totalLeads = await _context.Leads.CountAsync(l => l.CampaignAssignment == c.CampaignId);
                var openCount = await _context.Leads.CountAsync(l => l.CampaignAssignment == c.CampaignId && l.HasOpenedEmail);
                var convertedCount = await _context.Leads.CountAsync(l => l.CampaignAssignment == c.CampaignId && l.HasConverted);

                double openRate = totalLeads > 0 ? Math.Round((double)openCount / totalLeads * 100, 2) : 0;
                double conversionRate = totalLeads > 0 ? Math.Round((double)convertedCount / totalLeads * 100, 2) : 0;
                double clickThroughRate = openCount > 0 ? Math.Round((double)convertedCount / openCount * 100, 2) : 0;

                campaignList.Add(new
                {
                    c.CampaignId,
                    c.CampaignName,
                    c.StartDate,
                    c.EndDate,
                    c.Status,
                    c.Agency,
                    c.Buyer,
                    c.Brand,
                    TotalLeads = totalLeads,
                    OpenRate = openRate,
                    ConversionRate = conversionRate,
                    ClickThroughRate = clickThroughRate
                });
            }

            return Ok(new { success = true, data = campaignList });
        }


        // 2️⃣ GET available filter options (Agency, Buyer, Brand)
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var agencies = await _context.Campaigns
                .Where(c => c.Agency != null)
                .Select(c => c.Agency!)
                .Distinct()
                .ToListAsync();

            var buyers = await _context.Campaigns
                .Where(c => c.Buyer != null)
                .Select(c => c.Buyer!)
                .Distinct()
                .ToListAsync();

            var brands = await _context.Campaigns
                .Where(c => c.Brand != null)
                .Select(c => c.Brand!)
                .Distinct()
                .ToListAsync();

            return Ok(new { success = true, data = new { agencies, buyers, brands } });
        }

        // 3️⃣ GET campaign by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCampaignById(int id)
        {
            var c = await _context.Campaigns.FindAsync(id);
            if (c == null)
                return NotFound(new { success = false, message = "Campaign not found" });

            var totalLeads = await _context.Leads.CountAsync(l => l.CampaignAssignment == c.CampaignId);
            var openCount = await _context.Leads.CountAsync(l => l.CampaignAssignment == c.CampaignId && l.HasOpenedEmail);
            var convertedCount = await _context.Leads.CountAsync(l => l.CampaignAssignment == c.CampaignId && l.HasConverted);

            double openRate = totalLeads > 0 ? Math.Round((double)openCount / totalLeads * 100, 2) : 0;
            double conversionRate = totalLeads > 0 ? Math.Round((double)convertedCount / totalLeads * 100, 2) : 0;
            double clickThroughRate = openCount > 0 ? Math.Round((double)convertedCount / openCount * 100, 2) : 0;

            return Ok(new
            {
                success = true,
                data = new
                {
                    c.CampaignId,
                    c.CampaignName,
                    c.StartDate,
                    c.EndDate,
                    c.Status,
                    c.Agency,
                    c.Buyer,
                    c.Brand,
                    TotalLeads = totalLeads,
                    OpenRate = openRate,
                    ConversionRate = conversionRate,
                    ClickThroughRate = clickThroughRate
                }
            });
        }

        // 4️⃣ ADD new campaign
        [HttpPost]
        public async Task<IActionResult> AddCampaign([FromBody] Campaign campaign)
        {
            if (campaign == null || string.IsNullOrWhiteSpace(campaign.CampaignName))
                return BadRequest(new { success = false, message = "Please provide a valid campaign name." });

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Campaign added successfully", data = campaign });
        }

        // 5️⃣ UPDATE campaign
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCampaign(int id, [FromBody] Campaign updated)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound(new { success = false, message = "Campaign not found" });

            campaign.CampaignName = updated.CampaignName;
            campaign.StartDate = updated.StartDate;
            campaign.EndDate = updated.EndDate;
            campaign.Status = updated.Status;
            campaign.Agency = updated.Agency;
            campaign.Buyer = updated.Buyer;
            campaign.Brand = updated.Brand;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Campaign updated successfully" });
        }

        // 6️⃣ DELETE campaign
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound(new { success = false, message = "Campaign not found" });

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Campaign deleted successfully" });
        }

        // 7️⃣ GET average campaign metrics
        [HttpGet("averages")]
        public async Task<IActionResult> GetAverageMetrics()
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            if (!campaigns.Any())
                return Ok(new { success = false, message = "No campaigns found" });

            double avgOpenRate = campaigns.Average(c => c.OpenRate);
            double avgConversionRate = campaigns.Average(c => c.ConversionRate);
            double avgClickThroughRate = campaigns.Average(c => c.ClickThroughRate);
            int totalLeads = campaigns.Sum(c => c.TotalLeads);

            return Ok(new
            {
                success = true,
                data = new
                {
                    AvgOpenRate = Math.Round(avgOpenRate, 2),
                    AvgConversionRate = Math.Round(avgConversionRate, 2),
                    AvgClickThroughRate = Math.Round(avgClickThroughRate, 2),
                    TotalLeads = totalLeads
                }
            });
        }

    }
}
