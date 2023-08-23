using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackSide.Migrations
{
    /// <inheritdoc />
    public partial class RemovedItemsreferenceornavigationtocategoryanduseitemscategoryIdasaFKtocategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    categoryId = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    estimatedValue = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    imageDirectory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    imageName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_items_categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "simpleItems",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simpleItems", x => x.id);
                    table.ForeignKey(
                        name: "FK_simpleItems_categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    itemId = table.Column<int>(type: "int", nullable: false),
                    imageNameB64 = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_images_items_itemId",
                        column: x => x.itemId,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // MLS 7/27/23 Since I intend to do a select * on images where id = itemId,
            // I want to make itemId an index for the image table. However, I don't want to make it unique
            // since a given item can have many images, meaning that itemId will be duplicated in several rows of the image table.
            // Indexes make queries on a given table much quicker.
            //
            migrationBuilder.CreateIndex(
                name: "IX_images_itemId",
                table: "images",
                column: "itemId",
                unique: false);  // MLS 7/27/23 changed from true to false, since a given item can have several images

            // MLS 7/27/23 I don't want to use categoryId as an index for the items table since I won't be searching on that key
            //migrationBuilder.CreateIndex(
            //    name: "IX_items_categoryId",
            //    table: "items",
            //    column: "categoryId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_simpleItems_categoryId",
            //    table: "simpleItems",
            //    column: "categoryId",
            //    unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropTable(
                name: "simpleItems");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
