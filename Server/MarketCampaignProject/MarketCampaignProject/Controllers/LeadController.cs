using MarketCampaignProject.Data;
using MarketCampaignProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace MarketCampaignProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeadsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------
        // 1. GET all leads
        // ------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAllLeads()
        {
            var leads = await _context.Leads
                .Select(l => new
                {
                    l.LeadID,
                    l.Name,
                    l.Email,
                    l.PhoneNumber,
                    l.CampaignAssignment,
                    CampaignName = _context.Campaigns
                        .Where(c => c.CampaignId == l.CampaignAssignment)
                        .Select(c => c.CampaignName)
                        .FirstOrDefault(),
                    l.Segment,
                    l.HasOpenedEmail,
                    l.HasConverted
                })
                .ToListAsync();

            if (leads == null || leads.Count == 0)
                return Ok(new { success = false, message = "No leads found" });

            return Ok(new { success = true, data = leads });
        }


        // ------------------------------------------
        // 2. GET lead by ID
        // ------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeadById(int id)
        {
            var lead = await _context.Leads.FindAsync(id);

            if (lead == null)
                return NotFound(new { success = false, message = "Lead not found" });

            return Ok(new { success = true, data = lead });
        }

       
        [HttpPost]
        public async Task<IActionResult> AddLead([FromBody] Lead lead)
        {
            try
            {
                if (lead == null)
                    return BadRequest(new { success = false, message = "Invalid lead data" });

                if (string.IsNullOrWhiteSpace(lead.Name) || string.IsNullOrWhiteSpace(lead.Email))
                    return Ok(new { success = false, message = "⚠️ Name and Email are required." });

                // 🧩 Check for duplicate Lead ID
                var existingLeadById = await _context.Leads.FindAsync(lead.LeadID);
                if (existingLeadById != null)
                    return Conflict(new { success = false, message = "⚠️ Duplicate Lead ID not allowed. Please use a unique ID." });

                // 🧩 Check for duplicate Email (optional but useful)
                var existingLeadByEmail = await _context.Leads.FirstOrDefaultAsync(l => l.Email == lead.Email);
                if (existingLeadByEmail != null)
                    return Conflict(new { success = false, message = "⚠️ A lead with this email already exists." });

                // 🧠 Apply Segment Mapping Rules
                lead.Segment = MapSegment(lead);

                _context.Leads.Add(lead);
                await _context.SaveChangesAsync();

                // ✅ Update related campaign analytics automatically
                if (lead.CampaignAssignment != 0)
                    await UpdateCampaignAnalytics(lead.CampaignAssignment);

                return Ok(new { success = true, message = "✅ Lead added successfully!", data = lead });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AddLead Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal Server Error", error = ex.Message });
            }
        }


        // 🧩 Helper: Map Segment
        private string MapSegment(Lead lead)
        {
            // Based on Campaign
            var campaign = _context.Campaigns.FirstOrDefault(c => c.CampaignId == lead.CampaignAssignment);
            if (campaign != null)
            {
                if (campaign.CampaignName.Contains("Summer Sale", StringComparison.OrdinalIgnoreCase)) return "Seasonal";
                if (campaign.CampaignName.Contains("Corporate", StringComparison.OrdinalIgnoreCase)) return "Corporate";
                if (campaign.CampaignName.Contains("New Product Launch", StringComparison.OrdinalIgnoreCase)) return "Early Adopters";
            }

            // Based on Email
            if (lead.Email.EndsWith("@company.com")) return "Corporate Leads";
            if (lead.Email.EndsWith("@edu.org")) return "Student/Academic";
            if (lead.Email.EndsWith("@gmail.com") || lead.Email.EndsWith("@yahoo.com")) return "General Public";

            // Based on Phone (Optional)
            if (lead.PhoneNumber.StartsWith("+1")) return "US Leads";
            if (lead.PhoneNumber.StartsWith("+91")) return "India Leads";

            // Default
            return "General";
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkUpload([FromBody] List<Lead> leads)
        {
            if (leads == null || !leads.Any())
                return BadRequest(new { success = false, message = "No leads provided" });

            var processedLeads = new List<string>();
            var updatedLeads = new List<string>();
            var rejectedLeads = new List<string>();

            foreach (var lead in leads)
            {
                if (string.IsNullOrWhiteSpace(lead.Name) || string.IsNullOrWhiteSpace(lead.Email))
                {
                    rejectedLeads.Add(lead.Email ?? "(missing email)");
                    continue;
                }

                lead.Segment = MapSegment(lead);

                var existing = await _context.Leads.FirstOrDefaultAsync(l => l.Email == lead.Email);
                if (existing != null)
                {
                    existing.Name = lead.Name;
                    existing.PhoneNumber = lead.PhoneNumber;
                    existing.CampaignAssignment = lead.CampaignAssignment;
                    existing.Segment = lead.Segment;
                    existing.HasOpenedEmail = lead.HasOpenedEmail;
                    existing.HasConverted = lead.HasConverted;

                    updatedLeads.Add(lead.Email);
                }
                else
                {
                    _context.Leads.Add(lead);
                    processedLeads.Add(lead.Email);
                }
            }

            await _context.SaveChangesAsync();

            // Update analytics for all affected campaigns
            var campaignIds = leads
                .Where(l => l.CampaignAssignment != null)
                .Select(l => l.CampaignAssignment.Value)
                .Distinct();

            foreach (var id in campaignIds)
                await UpdateCampaignAnalytics(id);

            return Ok(new
            {
                success = true,
                message = "Bulk upload completed successfully.",
                summary = new
                {
                    processed = processedLeads.Count,
                    updated = updatedLeads.Count,
                    rejected = rejectedLeads.Count,
                    total = leads.Count
                },
                details = new
                {
                    processed = processedLeads,
                    updated = updatedLeads,
                    rejected = rejectedLeads
                }
            });
        }

        private async Task UpdateCampaignAnalytics(int? campaignId)
        {
            if (campaignId == null) return;

            var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.CampaignId == campaignId);
            if (campaign == null) return;

            var totalLeads = await _context.Leads
                .CountAsync(l => l.CampaignAssignment == campaignId);

            var openCount = await _context.Leads
                .CountAsync(l => l.CampaignAssignment == campaignId && l.HasOpenedEmail);

            var convertedCount = await _context.Leads
                .CountAsync(l => l.CampaignAssignment == campaignId && l.HasConverted);

            // 🧮 Calculate Open Rate and Conversion Rate
            campaign.TotalLeads = totalLeads;
            campaign.OpenRate = totalLeads > 0
                ? Math.Round((double)openCount / totalLeads * 100, 2)
                : 0;

            campaign.ConversionRate = totalLeads > 0
                ? Math.Round((double)convertedCount / totalLeads * 100, 2)
                : 0;

            // 🧮 Calculate Click-Through Rate (CTR)
            campaign.ClickThroughRate = openCount > 0
                ? Math.Round((double)convertedCount / openCount * 100, 2)
                : 0;

            _context.Campaigns.Update(campaign);
            await _context.SaveChangesAsync();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLead(int id, [FromBody] Lead updated)
        {
            if (updated == null)
                return BadRequest(new { success = false, message = "Invalid lead data" });

            var lead = await _context.Leads.FindAsync(id);
            if (lead == null)
                return NotFound(new { success = false, message = "Lead not found" });

            if (string.IsNullOrWhiteSpace(updated.Name) || string.IsNullOrWhiteSpace(updated.Email))
                return BadRequest(new { success = false, message = "Name and Email are required" });

            // ✅ Update all editable fields
            lead.Name = updated.Name;
            lead.Email = updated.Email;
            lead.PhoneNumber = updated.PhoneNumber;
            lead.CampaignAssignment = updated.CampaignAssignment;
            lead.Segment = updated.Segment;
            lead.HasOpenedEmail = updated.HasOpenedEmail;
            lead.HasConverted = updated.HasConverted;

            await _context.SaveChangesAsync();

            // ✅ Automatically update related campaign analytics
            if (lead.CampaignAssignment != 0)
                await UpdateCampaignAnalytics(lead.CampaignAssignment);

            return Ok(new { success = true, message = "Lead updated successfully" });
        }


        [HttpPost("search")]
        public async Task<IActionResult> SearchLeads([FromBody] List<string> searchTerms)
        {
            if (searchTerms == null || searchTerms.Count == 0)
                return BadRequest(new { success = false, message = "No search terms provided." });

            // 🧹 Clean input
            var normalized = searchTerms
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToLower())
                .Distinct()
                .ToList();

            if (normalized.Count > 500)
                return BadRequest(new { success = false, message = "Too many search terms (max 500)." });

            // 🧠 Separate emails and IDs
            var emails = normalized.Where(s => s.Contains("@")).ToList();
            var ids = normalized.Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();

            // ✉️ Simple email regex validation
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            var invalidInputs = emails.Where(e => !emailRegex.IsMatch(e)).ToList();
            emails = emails.Except(invalidInputs).ToList();

            // 📊 Query the DB efficiently
            var foundLeads = await (from lead in _context.Leads
                                    join campaign in _context.Campaigns
                                        on lead.CampaignAssignment equals campaign.CampaignId into leadCampaign
                                    from campaign in leadCampaign.DefaultIfEmpty()
                                    where emails.Contains(lead.Email.ToLower()) || ids.Contains(lead.LeadID)
                                    select new
                                    {
                                        lead.LeadID,
                                        lead.Name,
                                        lead.Email,
                                        lead.PhoneNumber,
                                        CampaignName = campaign != null ? campaign.CampaignName : "—",
                                        lead.Segment,
                                        OpenRate = campaign != null ? campaign.OpenRate : 0,
                                        Clicks = campaign != null ? campaign.ClickThroughRate : 0,
                                        Conversions = campaign != null ? campaign.ConversionRate : 0,
                                        lead.HasOpenedEmail,
                                        lead.HasConverted
                                    }).ToListAsync();

            // 🧾 Found and Not Found
            var foundEmails = foundLeads.Select(l => l.Email.ToLower()).ToList();
            var foundIds = foundLeads.Select(l => l.LeadID.ToString()).ToList();

            var notFound = normalized
                .Where(s => !foundEmails.Contains(s) && !foundIds.Contains(s))
                .ToList();

            return Ok(new
            {
                success = true,
                summary = new
                {
                    totalRequested = normalized.Count,
                    foundCount = foundLeads.Count,
                    notFoundCount = notFound.Count,
                    invalidInputs = invalidInputs.Count
                },
                invalidInputs,
                found = foundLeads,
                notFound
            });
        }


        // ------------------------------------------
        // 7. DELETE lead
        // ------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLead(int id)
        {
            var lead = await _context.Leads.FindAsync(id);
            if (lead == null)
                return NotFound(new { success = false, message = "Lead not found" });

            int? campaignId = lead.CampaignAssignment;

            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync();

            // ✅ Recalculate analytics after deletion
            if (campaignId.HasValue)
                await UpdateCampaignAnalytics(campaignId);

            return Ok(new { success = true, message = "Lead deleted and campaign analytics updated successfully" });
        }

    }
}
  