namespace BackSide.Models
{
    public class Category
    {
        public int id { get; set; }
        public string name { get; set; } = null!;

        public Category()
        {
            name = "";
            id = 0;
        }
    }
}
