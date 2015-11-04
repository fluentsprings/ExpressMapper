namespace ExpressMapper.Tests.Projections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Size",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Code = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductVariant",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Color = c.String(),
                        SizeId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Size", t => t.SizeId)
                .Index(t => t.SizeId);
            
            CreateTable(
                "dbo.Product",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Dimensions = c.String(),
                        VariantId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProductVariant", t => t.VariantId)
                .Index(t => t.VariantId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CategoryProducts",
                c => new
                    {
                        Category_Id = c.Guid(nullable: false),
                        Product_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Category_Id, t.Product_Id })
                .ForeignKey("dbo.Categories", t => t.Category_Id, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.Product_Id, cascadeDelete: true)
                .Index(t => t.Category_Id)
                .Index(t => t.Product_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductVariant", "SizeId", "dbo.Size");
            DropForeignKey("dbo.Product", "VariantId", "dbo.ProductVariant");
            DropForeignKey("dbo.CategoryProducts", "Product_Id", "dbo.Product");
            DropForeignKey("dbo.CategoryProducts", "Category_Id", "dbo.Categories");
            DropIndex("dbo.CategoryProducts", new[] { "Product_Id" });
            DropIndex("dbo.CategoryProducts", new[] { "Category_Id" });
            DropIndex("dbo.Product", new[] { "VariantId" });
            DropIndex("dbo.ProductVariant", new[] { "SizeId" });
            DropTable("dbo.CategoryProducts");
            DropTable("dbo.Categories");
            DropTable("dbo.Product");
            DropTable("dbo.ProductVariant");
            DropTable("dbo.Size");
        }
    }
}
