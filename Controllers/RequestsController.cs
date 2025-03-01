using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRSWebApi.Models;

namespace PRSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly PRSDbContext _context;

        public RequestsController(PRSDbContext context)
        {
            _context = context;
        }

        // GET: api/Requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequests()
        {
            return await _context.Requests
                .Include(r => r.User)
                .Include(l => l.LineItems)
                .ToListAsync();
        }

        // GET: api/Requests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Request>> GetRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            return request;
        }

        // PUT: api/Requests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequest(int id, Request request)
        {
            if (id != request.RequestId)
            {
                return BadRequest();
            }

            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Requests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Request>> PostRequest(Request request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            request.RequestNumber = await GenerateRequestNumber();
            request.Status = "New";
            request.SubmittedDate = DateTime.UtcNow;
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRequest", new { id = request.RequestId }, request);
        }

        // DELETE: api/Requests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize()]
        [HttpPut("{requestId}/status")]

        public async Task<IActionResult> UpdateRequestStatus(int requestId, RequestStatusUpdateDto statusUpdate)
        {
            var request = await _context.Requests
                .Include(r => r.LineItems) // Ensure LineItems are loaded
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                return NotFound();
            }

            if (request.LineItems == null || request.LineItems.Count == 0)
            {
                return BadRequest(new { message = "Cannot approve/reject a request without line items." });
            }

            // Ensure status is valid
            if (statusUpdate.Status != "Approved" && statusUpdate.Status != "Rejected")
            {
                return BadRequest(new { message = "Invalid status. Must be 'Approved' or 'Rejected'." });
            }

            // Update status and rejection reason if applicable
            request.Status = statusUpdate.Status;
            request.ReasonForRejection = statusUpdate.Status == "Rejected" ? statusUpdate.ReasonForRejection : null;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Request {statusUpdate.Status} successfully." });
        }
        private async Task<string> GenerateRequestNumber()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd"); // Example: "20250228"

            int count = await _context.Requests
                .Where(r => r.RequestNumber.StartsWith(today))
                .CountAsync();

            string sequence = (count + 1).ToString().PadLeft(4, '0'); // Ensures "0001", "0010", etc.

            return $"{today}{sequence}"; // Example: "202502280001"
        }

        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.RequestId == id);
        }
    }
}
