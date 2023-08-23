using BackSide.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace BackSide.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> categories { get; set; }
        public DbSet<Simple> simpleItems { get; set; }

        public DbSet<Item> items { get; set; }

        public DbSet<Image> images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // SimpleItem -> Category relationship
            modelBuilder.Entity<Simple>()
                // MLS 7/27/23 removed navigation to category
                // .HasOne(e => e.category)
                .HasOne<Category>()
                .WithOne()  // Category has no relationship to Simple
                .HasForeignKey<Simple>(e => e.categoryId)
                .IsRequired();

            // Item -> Category relationship
            modelBuilder.Entity<Item>()
                // MLS 7/27/23 removed navigation to category
                // .HasOne(e => e.category)
                .HasOne<Category>()
                .WithOne()  // category has no relationship with item
                .HasForeignKey<Item>(e => e.categoryId)
                .IsRequired();

            // Images -> Item relationship
            modelBuilder.Entity<Image>()
                // MLS 7/27/23 removed navigation to item
                // .HasOne(e => e.item)
                .HasOne<Item>()
                .WithOne()  // Item has no relationship to Image
                .HasForeignKey<Image>(e => e.itemId)
                .IsRequired();


        }


    }
}
