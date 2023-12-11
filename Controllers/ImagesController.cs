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
using Microsoft.Identity.Web.Resource;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using BackSide.Utilities;

namespace BackSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly FileStorageService _fileStorageService;
        private readonly ILogger _logger;
        public ImagesController(ApplicationDbContext context, FileStorageService fileStorageService, ILogger<ImagesController> logger)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _logger = logger;
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


        [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes:Read")]
        public ActionResult<IEnumerable<Image>> GetItemImages(int id, [FromQuery] string imageDirectory)  // [FromQuery] int itemId)
        {
            try
            {
                IEnumerable<Image> images;
                if (_context.images == null)
                {
                    string msg = "ImagesController: GetItemImages: There is a problem accessing the database. Verify database is running.";
                    _logger.LogCritical(msg);
                    return Problem(msg);
                }
                images = _context.images.Where((image) => (image.itemId == id));
                //await _context.images.FindAsync(id);

                if (images == null)
                {
                    _logger.LogError($"ImagesController: Unable to GET images for item w/ id {id}. The images should be located in {imageDirectory}.");
                    return NotFound();
                }

                foreach (var image in images)
                {
                    _logger.LogInformation($"ImagesController: Getting image {image.imageNameB64} in {imageDirectory}");
                    image.imageNameB64 = _fileStorageService.GetImageAsB64String(imageDirectory, image.imageNameB64);
                }

                return Ok(images);
            }
            catch (Exception ex)
            {
                string msg = "ImagesController: An exception occurred  attempting to GET images for item w/ id" + id + ".  " +
                                    ex.Message + " -  " + ex.InnerException;
                _logger.LogCritical(msg);
                return Problem(msg);
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
        [Authorize]
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
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes:Write")]
        public ActionResult<Item> PostImages([FromForm] List<IFormFile> images, [FromQuery] int itemId) // , int itemId) 
        {
            Item? item;


            if (_context.images == null || _context.items == null)
            {
                string msg = "ImagesController: PostImages: There is a problem accessing the database. Verify database is running.";
                _logger.LogCritical(msg);
                return Problem(msg);
            }

            // 12/11/23 The item entity was defined incorrectly (had a 1-to-1 relationship between item and image).
            // this caused problems with missing records.
            // on 12/8/23, I modified item to have a 1-to-many relationship to images, and all my images saved.
            // the reason I created a transaction and savepoint was to try and control the records being saved to database.
            // it didn't work.  The only thing that prevented missing records was the fix from 1-to-1 to 1-to-many.
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                transaction.CreateSavepoint("BeforeImagesSaved");

                // get the images' item...so we can get the imageDirectory
                item = _context.items.Find(itemId);
                if (item == null)
                {
                    _logger.LogError($"ImagesController: Unable to POST images.  Associated item w/ id {itemId} NotFound.");
                    return NotFound();
                }
                else  // save additional images sent in HttpRequest
                {

                    int numImages = 0;
                    foreach (IFormFile image in images)
                    {
                        _logger.LogInformation($"ImagesController: Beginning to POST imageRecord and image for {image.FileName}.");

                        // create a database record
                        Image imageRecord = new Image(itemId, image.FileName);

                        // saves imageName and itemId to database
                        _logger.LogInformation($"ImagesController: Attempting to POST imageRecord for item w/ id {itemId} to database.");
                        _context.images.Add(imageRecord);
                        numImages++;

                        // save the image to hard drive in the item's imageDirectory
                        _logger.LogInformation($"ImagesController: Attempting to POST image {image.FileName} to storage.");
                        _fileStorageService.SaveImage(image, item.imageDirectory);
                        _logger.LogInformation($"ImagesController: POSTED image {image.FileName} to storage.");

                    }

                    int rc = _context.SaveChanges();
                    if (rc == numImages) _logger.LogInformation($"ImagesController: POSTED {rc} imageRecords for item w/ id {itemId} to database.");
                    else _logger.LogError($"ImagesController: POSTED {rc} out of {numImages} imageRecords for item w/ id {itemId} to database.");

                    transaction.Commit();


                    item.imageName = _fileStorageService.GetImageAsB64String(item.imageDirectory, item.imageName);
                    if (String.IsNullOrEmpty(item.imageName)) _logger.LogWarning("Image is Null or Empty");
 
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
            }
            catch (Exception e)
            {
                transaction.RollbackToSavepoint("BeforeImagesSaved");
                string msg = "ImagesController: An exception occurred while POSTING images for item w/ id " + itemId + ".\n" + e.Message;
                _logger.LogCritical(msg);
                return Problem(msg);
            }

        }



        // MLS ToDo: Delete all images associated with a given itemId, not imageId
        // and the function should be called DeleteImages(int itemId)
        // DELETE: api/Images/5
        [Authorize]
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
