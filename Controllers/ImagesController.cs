using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackSide.Data;
using BackSide.Models;
using System.IO;

namespace BackSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly string BaseImageDirectory; // = System.Configuration.ConfigurationManager.AppSettings["ImageDirectory"];
        private readonly IConfiguration Configuration;

        public ImagesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            BaseImageDirectory = Configuration["ImageDirectory"]; // var defaultLogLevel = Configuration["Logging:LogLevel:Default"];
            // MLS 7/26/23 below gave null result -- didn't work
            // System.Configuration.ConfigurationManager.AppSettings["ImageDirectory"];
        }

        //// GET: api/Images
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Image>>> Getimages()
        //{
        //    if (_context.images == null)
        //    {
        //        return NotFound();
        //    }

        //    return await _context.images.ToListAsync();
        //}

        // GET: api/Images/5
        [HttpGet("{id}")] // MLS 7/25/26 note: getting the id from URL, not the querystring as indicated below.
                          // Swagger didn't work when I tried to get id from QueryString.
                          // You should be able to do either/or?
                          // public async Task<ActionResult<Image>> GetImages(int id)
        public async Task<ActionResult<IEnumerable<Image>>> GetItemImages(int id, [FromQuery] string imageDirectory)  // [FromQuery] int itemId)
        {
            try
            {
                IEnumerable<Image> images;
                if (_context.images == null)
                {
                    return Problem("$There is a problem accessing the database. Verify database is running.");
                }
                images = _context.images.Where((image) => (image.itemId == id));
                //await _context.images.FindAsync(id);

                if (images == null)
                {
                    return NotFound();
                }

                // get the images from the hard drive and save as Base64 strings
                // get the imageDirectory from the item associated with the images...
                string filePath = Path.Combine(BaseImageDirectory, imageDirectory);
                foreach (var image in images)
                    image.imageNameB64 = BackSide.Utilities.Files.GetImageFromHDandReturnB64String(filePath, image.imageNameB64);

                return Ok(images);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        // PUT: api/Images/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutImage(int id, Image image)
        //{
        //    if (id != image.id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(image).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ImageExists(id))
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

        // POST: api/Images
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        // GET: api/Images/5
        // [HttpPost("{itemId}")]  // MLS 7/25/23 - Swagger doesn't work when I attempt to use this, so have to send the itemId in the QueryString as indicated below
        // public async Task<ActionResult<Image>> PostImage(Image image)
        // MLS 7/24/23 
        // This function is called when an item is created
        // and there are additional images to store besides the item's
        // primary image. 

        // Because this function is called only when adding an
        // item, I decided to instead return the item itself
        // that was created, rather than the parameter --
        // a list of the item's additional images which need to be saved

        // public async Task<ActionResult<IEnumerable<Image>>> PostImages([FromForm] List<Image> images)
        // MLS 7/24/23
        // MLS 7/31/23 No matter what I tried, I was not able to receive an array of Objects
        // public async Task<ActionResult<Item>> PostImages([FromForm] List<Image> images, [FromQuery] int itemId) // , int itemId) 
        // I am able to receive an array of IFormFile, therefore decided to do that...
        public async Task<ActionResult<Item>> PostImages([FromForm] List<IFormFile> images, [FromQuery] int itemId) // , int itemId) 
        {
            Item item;
            string imageDirectory;

            try
            {

                if (_context.images == null || _context.items == null)
                {
                    return Problem("$There is a problem accessing the database. Verify database is running.");
                }

                // get the images' item...so we can get the imageDirectory
                item = await _context.items.FindAsync(itemId);
                if (item == null)
                {
                    return NotFound();
                }
                else  // save additional images sent in HttpRequest
                {
                    try
                    {
                        // get the imageDirectory from the item associated with the images...
                        imageDirectory = Path.Combine(BaseImageDirectory, item.imageDirectory);
                        foreach (IFormFile image in images)
                        {
                            // create a database record
                            Image imageRecord = new Image(itemId, image.FileName);

                            // saves imageName and itemId to database

                            _context.images.Add(imageRecord);
                            await _context.SaveChangesAsync();

                            // save the image to hard drive
                            BackSide.Utilities.Files.SaveImageToHardDrive(image, imageDirectory);
                        }

                    }
                    catch (Exception e)
                    {
                        string msg = "Problem saving additional images for" + item.name + "   -   " + e.Message;
                        return Problem(msg);
                    }


                    try
                    {
                        item.imageName = Utilities.Files.GetImageFromHDandReturnB64String(imageDirectory, item.imageName);
                        // Note: before sending the item back in HttpResponse, we need to convert
                        // the image.name to a base64string representation
                        // of image.

                        // MLS 7/24/23 revisit this at another time.
                        // CreatedAtAction calls are more complex and several variations.
                        // For now just return the item in an OK message returns 200
                        // 
                        // CreatedAtAction returns a 201 which means that something was created
                        // CreatedAtAction("GetItemImages", new {}, imagesB64
                        // CreatedAtAction("GetImage", new { id = image.id }, image);
                        // MLS 7/24/23
                        return Ok(item);
                    }
                    catch (Exception e)
                    {
                        string msg = "Problem sending " + item.name + " back to browser in HttpResponse -   " + e.Message;
                        return Problem(msg);
                    }

                }
            }
            catch (Exception e)
            {
                string msg = "Problem saving additional images for item" + e.Message;
                return Problem(msg);
            }
        }



        // MLS ToDo: Delete all images associated with a given itemId, not imageId
        // and the function should be called DeleteImages(int itemId)
        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            if (_context.images == null)
            {
                return NotFound();
            }
            var image = await _context.images.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            _context.images.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ImageExists(int id)
        {
            return (_context.images?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
