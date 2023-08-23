namespace BackSide.Models
{
    public class Simple
    {
        public int id { get; set; } 
        public int categoryId { get; set; }
        
        // MLS 7/27/23 remove navigation
        // public Category? category { get; set; }

        public string name { get; set; }

        public Simple()
        {
            categoryId = 0;
            name = string.Empty;
            id = 0;
        }

    }
}
