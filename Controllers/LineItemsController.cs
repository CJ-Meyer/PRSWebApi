﻿using System;
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
                .FirstOrDefaultAsync(l => l.LineItemId == id);

            if (lineItem == null)
            {
                return NotFound();
            }

            return lineItem;
        }

        // PUT: api/LineItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutLineItem(LineItem lineItem)
        {
            if (!LineItemExists(lineItem.LineItemId))
            {
                return BadRequest();
            }

            _context.Entry(lineItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await UpdateRequestTotal(lineItem.RequestId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LineItemExists(lineItem.LineItemId))
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

            await UpdateRequestTotal(lineItem.RequestId);

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
            await UpdateRequestTotal(lineItem.RequestId);
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

        [HttpGet("lines-for-req/{reqId}")]
        public async Task<IActionResult> GetLineItemsForRequestUnderReview(int reqId)
        {
            var request = await _context.Requests
                .Include(r => r.LineItems)
                .FirstOrDefaultAsync(r => r.RequestId == reqId);

            if (request == null)
            {
                return NotFound(new { message = "Request not found." });
            }

            var lineItems = request.LineItems;

            if (lineItems == null || lineItems.Count == 0)
            {
                return NotFound(new { message = "No line items found for this request." });
            }

            return Ok(lineItems);
        }

        private bool LineItemExists(int id)
        {
            return _context.LineItems.Any(e => e.LineItemId == id);
        }
    }
}
