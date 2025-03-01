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
    public class LineItemsController : ControllerBase
    {
        private readonly PRSDbContext _context;

        public LineItemsController(PRSDbContext context)
        {
            _context = context;
        }

        // GET: api/LineItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LineItem>>> GetLineItems()
        {
            return await _context.LineItems
                .Include(p => p.Product)
                .Include(r => r.Request)
                .ToListAsync();
        }

        // GET: api/LineItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LineItem>> GetLineItem(int id)
        {
            var lineItem = await _context.LineItems
                .Include(p => p.Product)
                .Include(r => r.Request)
                .FirstOrDefaultAsync();

            if (lineItem == null)
            {
                return NotFound();
            }

            return lineItem;
        }

        // PUT: api/LineItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLineItem(int id, LineItem lineItem)
        {
            if (id != lineItem.LineItemId)
            {
                return BadRequest();
            }

            _context.Entry(lineItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await UpdateRequestTotal(lineItem.RequestId ?? 0);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LineItemExists(id))
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

        // POST: api/LineItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LineItem>> PostLineItem(LineItem lineItem)
        {
            _context.LineItems.Add(lineItem);
            await _context.SaveChangesAsync();

            await UpdateRequestTotal(lineItem.RequestId ?? 0);

            return CreatedAtAction("GetLineItem", new { id = lineItem.LineItemId }, lineItem);
        }

        // DELETE: api/LineItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLineItem(int id)
        {
            var lineItem = await _context.LineItems.FindAsync(id);
            if (lineItem == null)
            {
                return NotFound();
            }

            _context.LineItems.Remove(lineItem);
            await _context.SaveChangesAsync();
            await UpdateRequestTotal(lineItem.RequestId ?? 0);
            return NoContent();
        }
        private async Task UpdateRequestTotal(int requestId)
        {
            var request = await _context.Requests.FindAsync(requestId);
            if (request != null)
            {
                request.Total = await _context.LineItems
                    .Where(li => li.RequestId == requestId && li.ProductId != null) // Ensure ProductId is not null
                    .SumAsync(li => li.Quantity * _context.Products
                        .Where(p => p.ProductId == (int)li.ProductId) // Convert nullable `int?` to `int`
                        .Select(p => p.Price)
                        .FirstOrDefault());

                await _context.SaveChangesAsync(); // Save the updated total
            }
        }
        private bool LineItemExists(int id)
        {
            return _context.LineItems.Any(e => e.LineItemId == id);
        }
    }
}
