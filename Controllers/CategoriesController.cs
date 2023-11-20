using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackSide.Data;
using BackSide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;

namespace BackSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // MLS 10/3/23 Add a Logger so I can see what is happening in Azure
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Categories
        [HttpGet]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes:Read")]
        public async Task<ActionResult<IEnumerable<Category>>> Getcategories()
        {
            try
            {
                _logger.LogWarning("\n\n\n");
                _logger.LogWarning("\n\n\n");
                _logger.LogWarning("Call to GetCategories");
                _logger.LogWarning("\n\n\n");
                _logger.LogWarning("\n\n\n");
                if (_context == null)
                {
                    _logger.LogError("\n\n\n");
                    _logger.LogError("\n\n\n");
                    _logger.LogError("Database Context is NULL");
                    _logger.LogError("\n\n\n");
                    _logger.LogError("\n\n\n");
                }

                if (_context.categories == null)
                {
                    return NotFound();
                }
                return await _context.categories.ToListAsync();
            }
            catch(Exception e)
            {
                _logger.LogError("\n\n\n");
                _logger.LogError("\n\n\n");
                _logger.LogError("Call to GetCategories Shit the bed with exception:");
                _logger.LogError($"{e.Message}\n\n\n");
                _logger.LogError($"{e.InnerException}\n\n\n");
                _logger.LogError("\n\n\n");
                _logger.LogError("\n\n\n");

                return Problem($"{ e.Message}");
            }
            
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes:Read")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
          if (_context.categories == null)
          {
              return NotFound();
          }
            var category = await _context.categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory([FromBody] Category category)
        {
          if (_context.categories == null)
          {
              return Problem("Entity set 'ApplicationDbContext.categories'  is null.");
          }
            _context.categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.id }, category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (_context.categories == null)
            {
                return NotFound();
            }
            var category = await _context.categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return (_context.categories?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
