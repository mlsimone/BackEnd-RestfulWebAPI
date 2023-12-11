using BackSide.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace BackSide.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> categories { get; set; }
        // public DbSet<Simple> simpleItems { get; set; }

        public DbSet<Item> items { get; set; }

        public DbSet<Image> images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // MLS 12/8/23 Not needed
            // SimpleItem -> Category relationship
            //modelBuilder.Entity<Simple>()
            //    // MLS 7/27/23 removed navigation to category
            //    // .HasOne(e => e.category)
            //    .HasOne<Category>()
            //    .WithOne()  // Category has no relationship to Simple
            //    .HasForeignKey<Simple>(e => e.categoryId)
            //    .IsRequired();

            // Item -> Category relationship
            modelBuilder.Entity<Item>()
                // MLS 7/27/23 removed navigation to category
                // .HasOne(e => e.category
                .HasOne<Category>()
                .WithOne()  // category has no relationship with item
                .HasForeignKey<Item>(e => e.categoryId)
                .IsRequired();

            // MLS 12/8/23 Wrong - an Image HasOne<Item>(). WithMany<Image>()
            // .WithOne() is incorrect
            // Images -> Item relationship
            modelBuilder.Entity<Image>()
                // MLS 7/27/23 removed navigation to item
                // .HasOne(e => e.item)
                .HasOne<Item>()
                // MLS 12/8/23 this should say .WithMany() to indicate that many images are associated with an item.
                //.WithOne()  // Item has no relationship to Image
                .WithMany()
                .HasForeignKey(e => e.itemId)
                .IsRequired();

            // MLS 12/8/23 This is another way to state the relationship starting with the principal and going to dependent
            // Item -> Image relationship
            //modelBuilder.Entity<Item>()
            //    .HasMany<Image>()
            //    .WithOne()
            //    .HasForeignKey("itemId")
            //    .IsRequired();


        }


    }
}
