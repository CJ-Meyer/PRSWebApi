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
            var request = await _context.Requests
                .Include(r => r.User)
                .Include(r => r.LineItems)
                .FirstOrDefaultAsync(r => r.RequestId == id);

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
            request.Total = 0;
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

        [HttpPost("submit-review/{id}")]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            var request = await _context.Requests
                .Include(r => r.LineItems)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
                return NotFound();

            if (request.LineItems == null || request.LineItems.Count == 0)
                return BadRequest(new { message = "Cannot submit a request without line items." });
            string resultMessage;

            if (request.Total <= 50)
            {
                request.Status = "Approved";
                resultMessage = "Request automatically approved.";
            }
            else
            {
                request.Status = "Review";
                resultMessage = "Request submitted for review.";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = resultMessage });
        }

        [HttpPut("approve/{Id}")]
        public async Task<IActionResult> ApproveReview(int Id, ApprovalDto approval)
        {
            var user = await _context.Users.FindAsync(approval.UserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if ((bool)!user.Admin)
            {
                return Unauthorized(new { message = "Only admins can approve requests." });
            }

            var request = await _context.Requests.FindAsync(Id);

            if (request == null)
            {
                return NotFound(new { message = "Request not found." });
            }

            if (request.Status == "Approved")
            {
                return BadRequest(new { message = "Request is already approved." });
            }

            request.Status = "Approved";
            await _context.SaveChangesAsync();

            return Ok(request);
        }



        [HttpPut("reject/{Id}")]
        public async Task<IActionResult> RejectReview(int Id, int userId, RejectionDto rejection)
        {
            var user = await _context.Users.FindAsync(rejection.UserID);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if ((bool)!user.Admin)
            {
                return Unauthorized(new { message = "Only admins can approve requests." });
            }

            var request = await _context.Requests.FindAsync(Id);

            if (request == null)
            {
                return NotFound(new { message = "Request not found." });
            }

            if (request.Status == "Rejected")
            {
                return BadRequest(new { message = "Request is already Rejected." });
            }
            if(rejection.Reason == null) return BadRequest(new { message = "Reason for rejection is required." });
            // request.ReasonForRejection -- is the request class
            // rejection.Reason -- is the RejectionDto class -- Input
            request.Status = "Rejected";
            request.ReasonForRejection = rejection.Reason;
            await _context.SaveChangesAsync();

            return Ok(request);
        }

        [HttpGet("list-review/{userId}")]
        public async Task<IActionResult> ListRequestsForReview(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });
            
            var requests = await _context.Requests
                .Include(r => r.LineItems)
                .Where(r => r.Status == "Review" && r.UserId != user.UserId)
                .ToListAsync();
            foreach (var request in requests)
            {
                request.SubmittedDate = DateTime.Now;
            }
            if (requests == null || requests.Count == 0)
            {
                return NotFound(new { message = "No requests found for review." });
            }
            return Ok(requests);
        }
        private async Task<string> GenerateRequestNumber()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd"); 

            int count = await _context.Requests
                .Where(r => r.RequestNumber.StartsWith(today))
                .CountAsync();

            string sequence = (count + 1).ToString().PadLeft(4, '0'); 

            return $"{today}{sequence}"; 
        }


        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.RequestId == id);
        }
    }
}
