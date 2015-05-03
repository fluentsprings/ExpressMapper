using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressMapper.Tests.Enums;
using ExpressMapper.Tests.Models;
using ExpressMapper.Tests.Models.Structs;
using ExpressMapper.Tests.ViewModels;
using ExpressMapper.Tests.ViewModels.Structs;

namespace ExpressMapper.Tests.Generator
{
    public class Functional
    {
        #region Basics

        public static KeyValuePair<Item, ItemViewModel> StructWithCollectionMap()
        {
            var id = Guid.NewGuid();
            const string name = "Test item!";
            var date = DateTime.Now;

            var f1 = Guid.NewGuid();
            const string f1Name = "Feature 1";
            const string f1Description = "Description of Feature 1";
            
            const string f2Name = "Feature 2";
            const string f2Description = "Description of Feature 2";
            var f2 = Guid.NewGuid();
            var src = new Item
            {
                Id = id,
                Name = name,
                Created = date,
                Features = new List<Feature>
                {
                    new Feature
                    {
                        Id = f1,
                        Name = f1Name,
                        Description = f1Description,
                        Rank = 3
                    },
                    new Feature
                    {
                        Id = f2,
                        Name = f2Name,
                        Description = f2Description,
                        Rank = 3
                    }
                }
            };
            var result = new ItemViewModel
            {
                Id = id,
                Name = name,
                Created = date,
                FeatureList = new List<FeatureViewModel>
                {
                     new FeatureViewModel
                    {
                        Id = f1,
                        Name = f1Name,
                        Description = f1Description,
                        Rank = 3
                    },
                    new FeatureViewModel
                    {
                        Id = f2,
                        Name = f2Name,
                        Description = f2Description,
                        Rank = 3
                    }
                }
            };
            return new KeyValuePair<Item, ItemViewModel>(src, result);
        } 

        public static KeyValuePair<Item, ItemViewModel> AutoMemberStructMap()
        {
            var id = Guid.NewGuid();
            const string name = "Test item!";
            var date = DateTime.Now;

            return new KeyValuePair<Item, ItemViewModel>(new Item
            {
                Id = id,
                Name = name,
                Created = date
            }, new ItemViewModel
            {
                Id = id,
                Name = name,
                Created = date
            });
        } 

        public static KeyValuePair<TestModel, TestViewModel> AutoMemberMap()
        {
            var testId = Guid.NewGuid();
            var countryId = Guid.NewGuid();

            var created = DateTime.UtcNow;
            var xxlId = Guid.NewGuid();
            var xlId = Guid.NewGuid();
            var src = new TestModel
            {
                Id = testId,
                Age = 18,
                Country = new Country
                {
                    Id = countryId,
                    Name = "USA",
                    Code = "US"
                },
                Created = created,
                Name = "AutoMemberMap",
                Sizes = new List<Size>
                {
                    new Size
                    {
                        Id = xxlId,
                        Name = "XXL size",
                        Alias = "XXL",
                        SortOrder = 3
                    },
                    new Size
                    {
                        Id = xlId,
                        Name = "XL size",
                        Alias = "XL",
                        SortOrder = 2
                    }
                },
                Weight = 32
            };

            var result = new TestViewModel
            {
                Id = testId,
                Age = 18,
                Country = new CountryViewModel
                {
                    Id = countryId,
                    Name = "USA",
                    Code = "US"
                },
                Created = created,
                Name = "AutoMemberMap",
                Sizes = new List<SizeViewModel>
                {
                    new SizeViewModel
                    {
                        Id = xxlId,
                        Name = "XXL size",
                        Alias = "XXL",
                        SortOrder = 3
                    },
                    new SizeViewModel
                    {
                        Id = xlId,
                        Name = "XL size",
                        Alias = "XL",
                        SortOrder = 2
                    }
                },
                Weight = 32
            };

            return new KeyValuePair<TestModel, TestViewModel>(src, result);
        }

        public static KeyValuePair<Size, SizeViewModel> ManualPrimitiveMemberMap()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            var keyValuePair = new KeyValuePair<Size, SizeViewModel>
                (
                new Size
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = "XXXL Size",
                    SortOrder = 2
                },
                new SizeViewModel
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = string.Format("Full - {0} - Size", xxxl),
                    SortOrder = newGuid.GetHashCode()
                }
                );
            return keyValuePair;
        }

        public static KeyValuePair<Size, SizeViewModel> InstantiateMap()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            const string xxxlSize = "XXXL Size";
            const int sortOrder = 2;
            var keyValuePair = new KeyValuePair<Size, SizeViewModel>
                (
                new Size
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                },
                new SizeViewModel(s => string.Format("{0} - Full name - {1}", newGuid, s))
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                }
                );
            return keyValuePair;
        }

        public static KeyValuePair<Size, SizeViewModel> BeforeMap()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            const string xxxlSize = "XXXL Size";
            const int sortOrder = 2;
            var keyValuePair = new KeyValuePair<Size, SizeViewModel>
                (
                new Size
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                },
                new SizeViewModel
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                }
                );
            return keyValuePair;
        }


        public static KeyValuePair<TestItem, TestItemViewModel> CollectionTypeMap()
        {
            var random = new Random();
            var next = random.Next(5, 10);
            var testCollections = new List<TestCollection>();
            var testCollectionViewModels = new List<TestCollectionViewModel>();


            for (var i = 0; i < next; i++)
            {
                var newGuid = Guid.NewGuid();
                testCollections.Add(new TestCollection
                {
                    Id = newGuid,
                    Name = string.Format("Name - {0}", i)
                });

                testCollectionViewModels.Add(new TestCollectionViewModel
                {
                    Id = newGuid,
                    Name = string.Format("Name - {0}", i)
                });
            }

            var testItem = new TestItem
            {
                Array = testCollections.ToArray(),
                Collection = testCollections,
                Enumerable = testCollections,
                List = testCollections,
                Queryable = testCollections.AsQueryable()
            };

            var testItemViewModel = new TestItemViewModel
            {
                Array = testCollectionViewModels.ToArray(),
                Collection = testCollectionViewModels,
                Enumerable = testCollectionViewModels,
                List = testCollectionViewModels,
                Queryable = testCollectionViewModels.AsQueryable()
            };


            var keyValuePair = new KeyValuePair<TestItem, TestItemViewModel>(testItem, testItemViewModel);
            return keyValuePair;
        }

        public static KeyValuePair<FashionProduct, FashionProductViewModel> ComplexMap()
        {
            var sizes = new List<Size>();
            var sizesViewModels = new List<SizeViewModel>();

            var random = new Random();
            var sizeCount = random.Next(3, 10);
            var cityCount = random.Next(4, 10);

            for (var i = 0; i < sizeCount; i++)
            {
                var newGuid = Guid.NewGuid();
                var name = string.Format("Size {0}", i);
                var alias = string.Format("Alias Size {0}", i);
                sizes.Add(
                    new Size
                    {
                        Id = newGuid,
                        Name = name,
                        Alias = alias,
                        SortOrder = i
                    });
                sizesViewModels.Add(new SizeViewModel
                {
                    Id = newGuid,
                    Name = name,
                    Alias = alias,
                    SortOrder = i
                });
            }

            var cities = new List<City>();
            var cityViewModels = new List<CityViewModel>();

            var ftRandom = new Random();
            for (var i = 0; i < cityCount; i++)
            {
                var newGuid = Guid.NewGuid();
                var name = string.Format("City {0}", i);

                var featureCount = ftRandom.Next(7 , 50);
                var features = new Feature[featureCount];
                var featureViewModels = new List<FeatureViewModel>();

                for (var j = 0; j < featureCount; j++)
                {
                    var fId = Guid.NewGuid();
                    var fName = string.Format("Feature - {0}", j);
                    var fDescription = string.Format("Description Feature - {0}", j);
                    features[j] =
                        new Feature
                        {
                            Id = fId,
                            Name = fName,
                            Description = fDescription,
                            Rank = 8
                        };
                    featureViewModels.Add(new FeatureViewModel
                    {
                        Id = fId,
                        Name = fName,
                        Description = fDescription,
                        Rank = 8
                    });
                }

                cities.Add(new City
                {
                    Id = newGuid,
                    Name = name,
                    Features = features
                });
                cityViewModels.Add(new CityViewModel
                {
                    Id = newGuid,
                    Name = name,
                    FeaturesList = featureViewModels
                });
            }

            var brandId = Guid.NewGuid();
            var brandName = "Brand name";
            var brand = new Brand
            {
                Id = brandId,
                Name = brandName
            };
            var brandViewModel = new BrandViewModel
            {
                Id = brandId,
                Name = brandName
            };

            var supId = Guid.NewGuid();
            var supplierName = "Supplier name";
            var agreementDate = DateTime.Now;
            var supplier = new Supplier
            {
                Id = supId,
                Name = supplierName,
                AgreementDate = agreementDate,
                Rank = 6,
                Sizes = sizes,
            };

            var supplierViewModel = new SupplierViewModel
            {
                Id = supId,
                Name = supplierName,
                AgreementDate = agreementDate,
                Sizes = sizesViewModels,
            };

            var sizeId = Guid.NewGuid();
            var lonelySize = "Lonely size";
            var sizeSAlias = "Size's alias";
            var size = new Size
            {
                Id = sizeId,
                Name = lonelySize,
                Alias = sizeSAlias,
                SortOrder = 5
            };
            var sizeViewModel = new SizeViewModel
            {
                Id = sizeId,
                Name = lonelySize,
                Alias = sizeSAlias,
                SortOrder = 5
            };

            var optionsCount = random.Next(10, 50);

            var options = new List<ProductOption>();
            var optionViewModels = new List<ProductOptionViewModel>();

            for (var i = 0; i < optionsCount; i++)
            {
                var optionId = Guid.NewGuid();
                var color = "Random";
                var discount = 54M;
                var price = 34M;
                var stock = 102;
                var weight = 23;
                options.Add(
                    new ProductOption
                    {
                        Id = optionId,
                        Cities = cities,
                        Color = color,
                        Discount = discount,
                        Price = price,
                        Stock = stock,
                        Weight = weight,
                        Size = size
                    });
                optionViewModels.Add(
                    new ProductOptionViewModel
                    {
                        Id = optionId,
                        Cities = cityViewModels,
                        Color = color,
                        Discount = discount,
                        Price = price,
                        Stock = stock,
                        Weight = weight,
                        Size = sizeViewModel,
                        DiscountedPrice = Math.Floor(price * discount / 100)
                    });
            }

            var fpId = Guid.NewGuid();
            var fashionProductDescription = "Fashion product description";
            var ean = "6876876-5345345-345345tgreg-435345df-adskf34";
            var topFashionProductName = "Top Fashion Product";
            var createdOn = DateTime.Now;
            var end = DateTime.Now.AddDays(30);
            var start = DateTime.Now;
            var warehouseOn = DateTime.Now.AddDays(-3);
            var fashionProduct = new FashionProduct
            {
                Id = fpId,
                Brand = brand,
                CreatedOn = createdOn,
                Description = fashionProductDescription,
                Ean = ean,
                End = end,
                Gender = GenderTypes.Unisex,
                Name = topFashionProductName,
                Options = options,
                Start = start,
                Supplier = supplier,
                WarehouseOn = warehouseOn
            };

            var fashionProductViewModel = new FashionProductViewModel
            {
                Id = fpId,
                Brand = brandViewModel,
                CreatedOn = createdOn,
                Description = fashionProductDescription,
                Ean = ean,
                End = end,
                OptionalGender = null,
                Name = topFashionProductName,
                Options = optionViewModels,
                Start = start,
                Supplier = supplierViewModel,
                WarehouseOn = warehouseOn
            };

            var result = new KeyValuePair<FashionProduct, FashionProductViewModel>(fashionProduct, fashionProductViewModel);
            return result;
        }

        public static KeyValuePair<Size, SizeViewModel> IgnoreMap()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            const string xxxlSize = "XXXL Size";
            const int sortOrder = 2;
            var keyValuePair = new KeyValuePair<Size, SizeViewModel>
                (
                new Size
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                },
                new SizeViewModel
                {
                    Id = newGuid,
                    Alias = xxxl,
                    SortOrder = sortOrder
                }
                );
            return keyValuePair;
        }

        public static KeyValuePair<Size, SizeViewModel> AfterMap()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            const string xxxlSize = "XXXL Size";
            const int sortOrder = 2;
            var keyValuePair = new KeyValuePair<Size, SizeViewModel>
                (
                new Size
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                },
                new SizeViewModel
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = "OVERRIDE BY AFTER MAP",
                    SortOrder = sortOrder
                }
                );
            return keyValuePair;
        }

        public static KeyValuePair<TestModel, TestViewModel> CustomNestedCollectionMap()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            const string xxxlSize = "XXXL Size";
            const int sortOrder = 2;


            var testId = Guid.NewGuid();
            var countryId = Guid.NewGuid();

            var created = DateTime.UtcNow;
            var name = string.Format("USA-1");
            var code = "US";
            var automembermap = "AutoMemberMap";
            var age = 43;
            var src = new TestModel
            {
                Id = testId,
                Age = age,
                Country = new Country
                {
                    Id = countryId,
                    Name = name,
                    Code = code
                },
                Created = created,
                Name = automembermap,
                Weight = 23.6M,
                Sizes = new List<Size>
                {
                    new Size
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                }
                }
            };

            var rst = new TestViewModel
            {
                Id = testId,
                Age = age,
                Country = new CountryViewModel
                {
                    Id = countryId,
                    Name = name,
                    Code = code
                },
                Created = created,
                Name = automembermap,
                Weight = 23.6M,
                Sizes = new List<SizeViewModel>
                {
                    new SizeViewModel
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                }
                }
            };



            var keyValuePair = new KeyValuePair<TestModel, TestViewModel>
                (
                src,
                rst
                );
            return keyValuePair;
        }

        public static KeyValuePair<TestModel, TestViewModel> NullPropertyAndNullCollectionMap()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            const string xxxlSize = "XXXL Size";
            const int sortOrder = 2;


            var testId = Guid.NewGuid();
            var countryId = Guid.NewGuid();

            var created = DateTime.UtcNow;
            var name = string.Format("USA-1");
            var code = "US";
            var automembermap = "AutoMemberMap";
            var age = 43;
            var src = new TestModel
            {
                Id = testId,
                Age = age,
                Created = created,
                Name = automembermap,
                Weight = 23.6M,
            };

            var rst = new TestViewModel
            {
                Id = testId,
                Age = age,
                Created = created,
                Name = automembermap,
                Weight = 23.6M,
            };



            var keyValuePair = new KeyValuePair<TestModel, TestViewModel>
                (
                src,
                rst
                );
            return keyValuePair;
        }

        public static KeyValuePair<Supplier, SupplierViewModel> GetPropertyMaps()
        {
            var id = Guid.NewGuid();
            var samsung = "Samsung";
            var agreementDate = DateTime.Now.AddDays(-2);
            var src = new Supplier
            {
                Id = id,
                Name = samsung,
                AgreementDate = agreementDate,
                Rank = 7
            };

            var dest = new SupplierViewModel
            {
                Id = id,
                Name = samsung,
                AgreementDate = agreementDate,
            };


            var keyValuePair = new KeyValuePair<Supplier, SupplierViewModel>
                (
                src,
                dest
                );
            return keyValuePair;
        }

        public static KeyValuePair<Size, SizeViewModel> CustomMap()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            const string xxxlSize = "XXXL Size";
            const int sortOrder = 2;
            var keyValuePair = new KeyValuePair<Size, SizeViewModel>
                (
                new Size
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                },
                new SizeViewModel
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                }
                );
            return keyValuePair;
        }

        #endregion

        #region Collections

        public static KeyValuePair<List<TestModel>, List<TestViewModel>> CollectionAutoMemberMap()
        {
            var source = new List<TestModel>();
            var result = new List<TestViewModel>();
            Func<string, int, string> iterateFormatFunc = (str, ind) => string.Format("{0}-{1}", str, ind);

            for (var i = 0; i < 10; i++)
            {
                var testId = Guid.NewGuid();
                var countryId = Guid.NewGuid();

                var created = DateTime.UtcNow;
                var name = string.Format("USA-{0}", i);
                var code = iterateFormatFunc("US", i);
                var automembermap = iterateFormatFunc("AutoMemberMap", i);
                var age = i;
                var src = new TestModel
                {
                    Id = testId,
                    Age = age,
                    Country = new Country
                    {
                        Id = countryId,
                        Name = name,
                        Code = code
                    },
                    Created = created,
                    Name = automembermap,
                    Weight = i % 2
                };

                var rst = new TestViewModel
                {
                    Id = testId,
                    Age = age,
                    Country = new CountryViewModel
                    {
                        Id = countryId,
                        Name = name,
                        Code = code
                    },
                    Created = created,
                    Name = automembermap,
                    Weight = i % 2
                };

                source.Add(src);
                result.Add(rst);
            }
            
            return new KeyValuePair<List<TestModel>, List<TestViewModel>>(source, result);
        }

        public static KeyValuePair<List<Size>, List<SizeViewModel>> CollectionCustomMap()
        {
            var source = new List<Size>();
            var result = new List<SizeViewModel>();
            Func<string, int, string> iterateFormatFunc = (str, ind) => string.Format("{0}-{1}", str, ind);

            for (var i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                var name = string.Format("size-{0}", i);
                var alias = string.Format("alias-{0}", i);
                
                var src = new Size
                {
                    Id = id,
                    Name = name,
                    Alias = alias,
                    SortOrder = i 
                };

                var rst = new SizeViewModel
                {
                    Id = id,
                    Name = name,
                    Alias = alias,
                    SortOrder = i
                };

                source.Add(src);
                result.Add(rst);
            }

            return new KeyValuePair<List<Size>, List<SizeViewModel>>(source, result);
        }

        public static KeyValuePair<List<TestCollection>, List<TestCollectionViewModel>> EnumerableToListTypeMap()
        {
            var random = new Random();
            var next = random.Next(5, 10);
            var testCollections = new List<TestCollection>();
            var testCollectionViewModels = new List<TestCollectionViewModel>();


            for (var i = 0; i < next; i++)
            {
                var newGuid = Guid.NewGuid();
                testCollections.Add(new TestCollection
                {
                    Id = newGuid,
                    Name = string.Format("Name - {0}", i)
                });

                testCollectionViewModels.Add(new TestCollectionViewModel
                {
                    Id = newGuid,
                    Name = string.Format("Name - {0}", i)
                });
            }

            return new KeyValuePair<List<TestCollection>, List<TestCollectionViewModel>>(testCollections, testCollectionViewModels);
        }

        public static KeyValuePair<List<TestCollection>, TestCollectionViewModel[]> EnumerableToArrayTypeMap()
        {
            var random = new Random();
            var next = random.Next(5, 10);
            var testCollections = new List<TestCollection>();
            var testCollectionViewModels = new List<TestCollectionViewModel>();


            for (var i = 0; i < next; i++)
            {
                var newGuid = Guid.NewGuid();
                testCollections.Add(new TestCollection
                {
                    Id = newGuid,
                    Name = string.Format("Name - {0}", i)
                });

                testCollectionViewModels.Add(new TestCollectionViewModel
                {
                    Id = newGuid,
                    Name = string.Format("Name - {0}", i)
                });
            }

            return new KeyValuePair<List<TestCollection>, TestCollectionViewModel[]>(testCollections, testCollectionViewModels.ToArray());
        }

        #endregion

        #region Exceptions

        public static KeyValuePair<Size, SizeViewModel> NoMapping()
        {
            var newGuid = Guid.NewGuid();
            const string xxxl = "XXXL";
            const string xxxlSize = "XXXL Size";
            const int sortOrder = 2;
            var keyValuePair = new KeyValuePair<Size, SizeViewModel>
                (
                new Size
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = xxxlSize,
                    SortOrder = sortOrder
                },
                new SizeViewModel
                {
                    Id = newGuid,
                    Alias = xxxl,
                    Name = "OVERRIDE BY AFTER MAP",
                    SortOrder = sortOrder
                }
                );
            return keyValuePair;
        }

        public static KeyValuePair<TestModel, TestViewModel> NoMappingForProperty()
        {
            return AutoMemberMap();
        }

        #endregion


    }
}
