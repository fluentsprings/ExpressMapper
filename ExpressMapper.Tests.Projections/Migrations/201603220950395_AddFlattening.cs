namespace ExpressMapper.Tests.Projections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFlattening : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Father",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MyInt = c.Int(nullable: false),
                        MyString = c.String(),
                        Son_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Son", t => t.Son_Id, cascadeDelete: true)
                .Index(t => t.Son_Id);
            
            CreateTable(
                "dbo.Son",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MyInt = c.Int(nullable: false),
                        MyString = c.String(),
                        Grandson_Id = c.Int(),
                        FatherSons_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grandson", t => t.Grandson_Id)
                .ForeignKey("dbo.FatherSons", t => t.FatherSons_Id)
                .Index(t => t.Grandson_Id)
                .Index(t => t.FatherSons_Id);
            
            CreateTable(
                "dbo.Grandson",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MyInt = c.Int(nullable: false),
                        MyString = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FatherSons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MyInt = c.Int(nullable: false),
                        MyString = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Son", "FatherSons_Id", "dbo.FatherSons");
            DropForeignKey("dbo.Father", "Son_Id", "dbo.Son");
            DropForeignKey("dbo.Son", "Grandson_Id", "dbo.Grandson");
            DropIndex("dbo.Son", new[] { "FatherSons_Id" });
            DropIndex("dbo.Son", new[] { "Grandson_Id" });
            DropIndex("dbo.Father", new[] { "Son_Id" });
            DropTable("dbo.FatherSons");
            DropTable("dbo.Grandson");
            DropTable("dbo.Son");
            DropTable("dbo.Father");
        }
    }
}
