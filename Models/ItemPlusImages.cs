namespace BackSide.Models
{
    public class ItemPlusImages: Item
    {
        public List<IFormFile> images { get; set; }
        public List<String> imagesAsBase64 { get; set; }
        public ItemPlusImages():base()
        {
            images = new List<IFormFile>();
            imagesAsBase64 = new List<String>();

        }
    }
}
