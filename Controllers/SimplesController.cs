using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackSide.Data;
using BackSide.Models;

namespace BackSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimplesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SimplesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Simples
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Simple>>> Getitems()
        {
          if (_context.simpleItems == null)
          {
              return NotFound();
          }
            return await _context.simpleItems.ToListAsync();
        }

        // GET: api/Simples/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Simple>> GetSimple(int id)
        {
          if (_context.simpleItems == null)
          {
              return NotFound();
          }
            var simple = await _context.simpleItems.FindAsync(id);

            if (simple == null)
            {
                return NotFound();
            }

            return simple;
        }

        // PUT: api/Simples/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSimple(int id, Simple simple)
        {
            if (id != simple.id)
            {
                return BadRequest();
            }

            _context.Entry(simple).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SimpleExists(id))
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

        // POST: api/Simples
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Simple>> PostSimple([FromForm] Simple simple)
        {
          if (_context.simpleItems == null)
          {
              return Problem("Entity set 'ApplicationDbContext.items'  is null.");
          }

            try { 
            _context.simpleItems.Add(simple);
            await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                string msg = e.InnerException + e.Message;
            }

            return CreatedAtAction("GetSimple", new { id = simple.id }, simple);
        }

        // DELETE: api/Simples/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSimple(int id)
        {
            if (_context.simpleItems == null)
            {
                return NotFound();
            }
            var simple = await _context.simpleItems.FindAsync(id);
            if (simple == null)
            {
                return NotFound();
            }

            _context.simpleItems.Remove(simple);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SimpleExists(int id)
        {
            return (_context.simpleItems?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
