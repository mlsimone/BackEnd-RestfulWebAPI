using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackSide.Models
{
    public class Image
    {
        [Key]
        public int id { get; set; }
        
        // required FK to item table
        public int itemId { get; set; }

        // MLS 7/27/23 Removed navigation to item - don't need this to // supports FK relationship to item table
        // since we explicityly declare OnModelCreating that 
        // itemId is the FK relationship between image -> item
        // public Item? item { get; set; }
        public String imageNameB64 { get; set; } = null!;

        // We are not storing imageFiles in database,
        // so make this optional field --
        // even better than that - use NotMapped
        [NotMapped]  // this means it will not be a field in the table
        public IFormFile? imageFile { get; set; } = null;

        public Image()
        {
            id = 0;
            this.itemId = -1;
            this.imageNameB64 = "";
        }
        public Image(int itemId, String imageName)
        {
            id = 0;
            this.itemId = itemId;
            this.imageNameB64 = imageName;
        }

    }
}
