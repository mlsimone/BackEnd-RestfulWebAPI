using System.ComponentModel.DataAnnotations.Schema;

namespace BackSide.Models
{
    public class Item
    {
        // A complex type must have a public default constructor and public writable properties to bind.
        // When model binding occurs, the class is instantiated using the public default constructor.
        public Item()
        {
            id = 0;
            name = "";
            categoryId = 0;
            description = "";
            estimatedValue = 0;
            imageDirectory = "";
            imageName = "";
        }
        public int id { get; set; }
        public string name { get; set; }
        public int categoryId { get; set; }

        // MLS 7/27/23 remove navigation
        // public Category? category { get; set; }  // home or fashion for now.
        public string description { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public double? estimatedValue { get; set; }
        public string imageDirectory { get; set; }  // this is a directory which contains pictures of images

        // When browser requests an item...
        // imageName will be populated with the Base64 representation
        // of an image.  
        public String imageName { get; set; }

        // This is only populated when capturing item details
        // Images are not stored in the database --
        // they are stored on hard drive of web server
        // Therefore, image is optional to indicate that relationship
        // even better than that - use NotMapped
        [NotMapped]  // this means it will not be a field in the table
        public IFormFile? image { get; set; }

    }
}
