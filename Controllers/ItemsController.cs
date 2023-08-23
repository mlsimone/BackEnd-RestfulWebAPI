using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Text;
using BackSide.Data;
using BackSide.Models;
using BackSide.Utilities;

namespace BackSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly string BaseImageDirectory;
        private readonly IConfiguration Configuration;

        public ItemsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            BaseImageDirectory = Configuration["ImageDirectory"]; // var defaultLogLevel = Configuration["Logging:LogLevel:Default"];
            // MLS 7/26/23 below gave null result -- didn't work
            // System.Configuration.ConfigurationManager.AppSettings["ImageDirectory"];
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetAllItems([FromQuery] string? searchFor)
        {
            try
            {
                IEnumerable<Item> items;
                if (_context.items == null)
                {
                    return Problem("$There is a problem accessing the database. Verify database is running.");
                }

                if (!string.IsNullOrEmpty(searchFor))  // get all items from database
                    items = await _context.items.Where(i => i.name.Contains(searchFor)).ToListAsync();
                else
                    items = await _context.items.ToListAsync();


                // get the image from the hard drive and store it as base64 string which is <img src=base64
                foreach (var item in items)
                    item.imageName = BackSide.Utilities.Files.GetImageFromHDandReturnB64String(Path.Combine(BaseImageDirectory, item.imageDirectory), item.imageName);

                return Ok(items);
            }
            catch (Exception e)
            {
                return Problem("$There is a problem retrieving items (database) and their images.");
            }

        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            if (_context.items == null)
            {
                return Problem("There is a problem accessing the database. Verify that it is running.");
            }

            try
            {
                var item = await _context.items.FindAsync(id);

                if (item == null)
                {
                    return NotFound();
                }

                // get the image from the hard drive and store it as base64 string so it can be displayed in image tag
                item.imageName = BackSide.Utilities.Files.GetImageFromHDandReturnB64String(Path.Combine(BaseImageDirectory, item.imageDirectory), item.imageName);

                return Ok(item);
            }
            catch (Exception e)
            {
                String msg = e.Message + e.InnerException;
                return Problem($"There was a problem getting the item (database) or image (hard drive): id = {id}.");
            }

        }

        // PUT: api/Items/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutItem(int id, Item item)
        //{
        //    if (id != item.id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(item).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ItemExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Items
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Item>> PostItem([FromForm] Item item)
        {
            // Item itemNoFormFile = item.CloneNoImage();
            if (_context.items == null)
            {
                return Problem($"{item.name} cannot be saved to the database. There is a problem accessing the database.");
            }


            try
            {
                // MLS 7/25/23 I think there is a problem with the Add and Save when item.image is Non-null.
                // Will clone the item and strip off the item.image
                // _context.items.Add(itemNoFormFile);
                _context.items.Add(item);
                await _context.SaveChangesAsync();

                // MLS ToDo: save the image to hard drive
                if (item.image != null)
                {
                    BackSide.Utilities.Files.SaveImageToHardDrive(item.image, Path.Combine(BaseImageDirectory, item.imageDirectory));

                    // MLS 7/25/23 When I return the item, the image has to be in it's base64 representation 
                    // To implement, it means that item.image = null, and item.imageName = base64 string representation

                    // Now that the itemNoFormFile has been saved to DB,
                    // Tack on the base64 string representation into item.imageName...
                    item.imageName = BackSide.Utilities.Files.GetImageFromHDandReturnB64String(
                                            Path.Combine(BaseImageDirectory, item.imageDirectory), item.imageName);
                }

            }
            catch (Exception e)
            {
                string msg = e.Message + e.InnerException;
                return Problem($"{item.name} was not saved to the database. {msg}");
            }

            return CreatedAtAction("GetItem", new { id = item.id }, item);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            if (_context.items == null)
            {
                return NotFound();
            }
            var item = await _context.items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemExists(int id)
        {
            return (_context.items?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
