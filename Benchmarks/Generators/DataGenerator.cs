using System;
using System.Collections.Generic;
using System.Globalization;
using Benchmarks.Models;

namespace Benchmarks.Generators
{
    public static class DataGenerator
    {
        public static List<Test> GetTests(int count)
        {

            var colors = new List<string>(Enum.GetNames(typeof(ConsoleColor)));

            var result = new List<Test>();
            var random = new Random();

            var productList = new List<Product>();

            var productOptionList = new List<ProductVariant>();

            for (var j = 0; j < random.Next(1, 15); j++)
            {
                productOptionList.Add(
                    new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        Color = colors[random.Next(0, colors.Count - 1)],
                        Size = string.Format("Universal - {0}", j)
                    }
                    );
            }

            for (var i = 0; i < random.Next(1, 20); i++)
            {
                var productVariantList = new List<ProductVariant>();
                for (var j = 0; j < random.Next(1, 5); j++)
                {
                    productVariantList.Add(
                        new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            Color = colors[random.Next(0, colors.Count - 1)],
                            Size = string.Format("Universal - {0} - {1}", i, j)
                        }
                        );
                }

                productList.Add(new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "PRODUCT in COLLECTION" + i,
                    Description = "PRODUCT in COLLECTION description" + i,
                    Weight = Convert.ToDecimal(Math.Round(random.NextDouble() * 100, 2)),
                    DefaultOption = new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        Color = colors[random.Next(0, colors.Count - 1)],
                        Size = i.ToString(CultureInfo.InvariantCulture)
                    },
                    Options = productVariantList
                });
            }

            for (var i = 0; i < count; i++)
            {
                var test = new Test
                {
                    Id = Guid.NewGuid(),
                    Age = i % 10,
                    Created = DateTime.Now,
                    Name = "Test" + i,
                    Weight = random.Next(5, 99999),
                    Type = random.Next(1, 3),
                    Product = new Product
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "TEST PRODUCT" + i,
                        Description = "TEST PRODUCT description" + i,
                        Weight = Convert.ToDecimal(Math.Round(random.NextDouble() * 100, 2)),
                        Options = productOptionList,
                        DefaultOption = new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            Color = colors[random.Next(0, colors.Count - 1)],
                            Size = "Matt"
                        }
                    },
                    SpareProduct = new Product
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "SPARE TEST PRODUCT" + i,
                        Description = "SPARE TEST PRODUCT description" + i,
                        Weight = Convert.ToDecimal(Math.Round(random.NextDouble() * 100, 2)),
                        Options = new List<ProductVariant>
                                {
                                    new ProductVariant
                                    {
                                        Id = Guid.NewGuid(),
                                        Color = colors[random.Next(0, colors.Count - 1)],
                                        Size = "Universal"
                                    }
                                },
                        DefaultOption = new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            Color = colors[random.Next(0, colors.Count - 1)],
                            Size = "Matt"
                        }
                    },
                    Products = productList
                };
                result.Add(test);
            }
            return result;
        }

        public static List<News> GetNews(int count)
        {
            var result = new List<News>();

            for (var i = 0; i < count; i++)
            {
                result.Add(
                    new News
                    {
                        Id = Guid.NewGuid(),
                        IsXml = i%2 == 0,
                        Provider = string.Format("Provider - {0}", i),
                        StartDate = DateTime.Now,
                        Url = string.Format("URL - {0}", i)
                    });
            }
            return result;
        }

        public static List<User> GetUsers(int count)
        {
            var result = new List<User>();

            for (var i = 0; i < count; i++)
            {
                result.Add(
                    new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = string.Format("Username - {0}", i),
                        Active = i % 2 == 0,
                        Address = string.Format("Address - {0}", i),
                        Age = i,
                        CreatedOn = DateTime.Now,
                        Deleted = i % 5 == 0,
                        Email = string.Format("Email - {0}", i),
                        Role = new Role
                        {
                            Id = Guid.NewGuid(),
                            Active = i % 3 == 0,
                            CreatedOn = DateTime.Now,
                            Name = string.Format("Role - {0}", i),
                            Deleted = i % 4 == 0,
                        },
                    });
            }
            return result;
        }

        public static List<Item> GetItems(int count)
        {
            var result = new List<Item>();
            var random = new Random();
            for (var i = 0; i < count; i++)
            {
                result.Add(
                    new Item
                    {
                        Id = Guid.NewGuid(),
                        Height = Convert.ToDecimal(Math.Round(random.NextDouble() * 100, 2)),
                        Length = Convert.ToDecimal(Math.Round(random.NextDouble() * 100, 2)),
                        Weight = Convert.ToDecimal(Math.Round(random.NextDouble() * 100, 2)),
                        Width = Convert.ToDecimal(Math.Round(random.NextDouble() * 100, 2)),
                        Name = string.Format("Item - {0}", i)
                    });
            }
            return result;
        }

        public static List<Author> GetAuthors(int count)
        {
            var result = new List<Author>();
            var random = new Random();
            for (var i = 0; i < count; i++)
            {
                var res = new List<Article>();
                var artCount = random.Next(1, 40);

                for (var j = 0; j < artCount; j++)
                {
                    res.Add(
                        new Article
                        {
                            Id = Guid.NewGuid(),
                            Title = string.Format("Title - {0}", j),
                            Text = string.Format("Text - {0}", j),
                            CreatedOn = DateTime.Now
                        });
                }

                result.Add(
                    new Author
                    {
                        Id = Guid.NewGuid(),
                        FirstName = string.Format("FirstName - {0}", i),
                        LastName = string.Format("LastName - {0}", i),
                        Articles = res,
                        Age = i,
                    });
            }
            return result;
        }
    }
}
