using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ExpressMapper.Tests.Model.Enums;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.Models.Structs;
using ExpressMapper.Tests.Model.ViewModels;
using ExpressMapper.Tests.Model.ViewModels.Structs;

namespace ExpressMapper.Tests.Model.Generator
{
    public class Functional
    {
        #region Basics

        public static KeyValuePair<Table, TableViewModel> FieldsTestMap()
        {
            var idTable = Guid.NewGuid();
            var tableName = "Just a table!";
            var countryName = "Cuba";
            var countryId = Guid.NewGuid();

            var table = new Table
            {
                Id = idTable,
                Name = tableName,
                Manufacturer = new Country
                {
                    Id = countryId,
                    Name = countryName
                },
                Sizes = new List<Size>(),
                Brands = new List<Brand>()
            };

            var tableViewModel = new TableViewModel
            {
                Id = idTable,
                Name = tableName,
                Manufacturer = new CountryViewModel
                {
                    Id = countryId,
                    Name = countryName
                },
                Sizes = new List<SizeViewModel>(),
                Brands = new List<BrandViewModel>()
            };

            for (var i = 0; i < 10; i++)
            {
                var brandId = Guid.NewGuid();
                var name = string.Format("Brand {0}", i);
                var brand = new Brand
                {
                    Id = brandId,
                    Name = name,
                };

                var brandViewModel = new BrandViewModel
                {
                    Id = brandId,
                    Name = name,
                };

                table.Brands.Add(brand);
                tableViewModel.Brands.Add(brandViewModel);
            }

            for (var i = 0; i < 5; i++)
            {
                var sizeId = Guid.NewGuid();
                var name = string.Format("Size {0}", i);
                var size = new Size
                {
                    Id = sizeId,
                    Name = name,
                };

                var sizeViewModel = new SizeViewModel
                {
                    Id = sizeId,
                    Name = name,

                };

                table.Sizes.Add(size);
                tableViewModel.Sizes.Add(sizeViewModel);
            }

            return new KeyValuePair<Table, TableViewModel>(table, tableViewModel);
        }

        public static KeyValuePair<Booking, BookingViewModel> RecursiveCompilationAssociationTestMap()
        {
            var compositionLvl1 = Guid.NewGuid();
            var bookingLvl2 = Guid.NewGuid();
            var compositionLvl3 = Guid.NewGuid();
            var booking = new Booking
            {
                Id = Guid.Empty,
                Composition = new Composition
                {
                    Id = compositionLvl1,
                    Name = "Test composition",
                    Booking = new Booking
                    {
                        Id = bookingLvl2,
                        Name = "New booking",
                        Composition = new Composition
                        {
                            Id = compositionLvl3,
                            Name = "Recent Composition"
                        }
                    }
                },
                Name = "Booking!"
            };

            var bookingViewModel = new BookingViewModel
            {
                Id = Guid.Empty,
                Composition = new CompositionViewModel
                {
                    Id = compositionLvl1,
                    Name = "Test composition",
                    Booking = new BookingViewModel
                    {
                        Id = bookingLvl2,
                        Name = "New booking",
                        Composition = new CompositionViewModel
                        {
                            Id = compositionLvl3,
                            Name = "Recent Composition"
                        }
                    }
                },
                Name = "Booking!"
            };
            return new KeyValuePair<Booking, BookingViewModel>(booking, bookingViewModel);
        }

        public static KeyValuePair<Employee, EmployeeViewModel> RecursiveCompilationDirectCollectionTestMap()
        {
            var employeeId = Guid.NewGuid();
            var employee = new Employee
            {
                Id = employeeId,
                LastName = "Just a last one",
                FirstName = "First name",
                Employees = new List<Employee>
                {
                    new Employee
                    {
                        Id = employeeId,
                        FirstName = "1",
                        LastName = "2",
                        Employees = new List<Employee>
                        {
                            new Employee
                            {
                                Id = employeeId,
                                LastName = "2",
                                FirstName = "2"
                            }
                        }
                    },
                    new Employee
                    {
                        Id = employeeId,
                        FirstName = "3",
                        LastName = "3",
                        Employees = new List<Employee>
                        {
                            new Employee
                            {
                                Id = employeeId,
                                LastName = "4",
                                FirstName = "4"
                            }
                        }
                    }
                }
            };

            var employeeViewModel = new EmployeeViewModel
            {
                Id = employeeId,
                LastName = "Just a last one",
                FirstName = "First name",
                Employees = new List<EmployeeViewModel>
                {
                    new EmployeeViewModel
                    {
                        Id = employeeId,
                        FirstName = "1",
                        LastName = "2",
                        Employees = new List<EmployeeViewModel>
                        {
                            new EmployeeViewModel
                            {
                                Id = employeeId,
                                LastName = "2",
                                FirstName = "2"
                            }
                        }
                    },
                    new EmployeeViewModel
                    {
                        Id = employeeId,
                        FirstName = "3",
                        LastName = "3",
                        Employees = new List<EmployeeViewModel>
                        {
                            new EmployeeViewModel
                            {
                                Id = employeeId,
                                LastName = "4",
                                FirstName = "4"
                            }
                        }
                    }
                }
            };

            return new KeyValuePair<Employee, EmployeeViewModel>(employee, employeeViewModel);
        }

        public static KeyValuePair<Person, PersonViewModel> RecursiveCompilationDirectAssociationTestMap()
        {
            var personId = Guid.NewGuid();
            var personName = "Mike Watson";

            var relativeId = Guid.NewGuid();
            var relativeName = "Sherlock Hommes";

            var person = new Person
            {
                Id = personId,
                Name = personName,
                Relative = new Person
                {
                    Id = relativeId,
                    Name = relativeName
                }
            };

            var personVm = new PersonViewModel
            {
                Id = personId,
                Name = personName,
                Relative = new PersonViewModel
                {
                    Id = relativeId,
                    Name = relativeName
                }
            };

            return new KeyValuePair<Person, PersonViewModel>(person, personVm);
        }

        public static KeyValuePair<Engine, EngineViewModel> RecursiveCompilationCollectionTestMap()
        {
            var engineId = Guid.NewGuid();
            var cylinderId = Guid.NewGuid();

            var engine = new Engine
            {
                Id = engineId,
                Capacity = "3.5 V6",
                Cylinders = new List<Cylinder>
                {
                    new Cylinder
                    {
                        Id = cylinderId,
                        Engine = new Engine
                        {
                            Id = engineId,
                            Capacity = "2.4",
                            Cylinders = new List<Cylinder> {new Cylinder { Id = cylinderId, Capacity = 5.2M} }
                        }
                    },
                    new Cylinder
                    {
                        Id = cylinderId,
                        Engine = new Engine {Id = engineId, Capacity = "2.4"}
                    },
                    new Cylinder
                    {
                        Id = cylinderId,
                        Engine = new Engine {Id = engineId, Capacity = "2.4"}
                    },
                    new Cylinder
                    {
                        Id = cylinderId,
                        Engine = new Engine {Id = engineId, Capacity = "2.4"}
                    },
                    new Cylinder
                    {
                        Id = cylinderId,
                        Engine = new Engine {Id = engineId, Capacity = "2.4"}
                    }
                }
            };

            var engineVm = new EngineViewModel
            {
                Id = engineId,
                Capacity = "3.5 V6",
                Cylinders = new List<CylinderViewModel>
                {
                    new CylinderViewModel
                    {
                        Id = cylinderId,
                        Engine = new EngineViewModel
                        {
                            Id = engineId,
                            Capacity = "2.4",
                            Cylinders = new List<CylinderViewModel> {new CylinderViewModel { Id = cylinderId, Capacity = 5.2M} }
                        }
                    },
                    new CylinderViewModel
                    {
                        Id = cylinderId,
                        Engine = new EngineViewModel {Id = engineId, Capacity = "2.4"}
                    },
                    new CylinderViewModel
                    {
                        Id = cylinderId,
                        Engine = new EngineViewModel {Id = engineId, Capacity = "2.4"}
                    },
                    new CylinderViewModel
                    {
                        Id = cylinderId,
                        Engine = new EngineViewModel {Id = engineId, Capacity = "2.4"}
                    },
                    new CylinderViewModel
                    {
                        Id = cylinderId,
                        Engine = new EngineViewModel {Id = engineId, Capacity = "2.4"}
                    }
                }
            };

            return new KeyValuePair<Engine, EngineViewModel>(engine, engineVm);
        }

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

        public static KeyValuePair<ItemModel, ItemModelViewModel> ExistingDestinationComplex()
        {
            var itmId = Guid.NewGuid();
            var name = "Item model";
            var itemModel = new ItemModel
            {
                Id = itmId,
                Name = string.Format("{0} - CHANGED!", name),
                SubItems = new List<SubItem>()
            };
            var itemModelVm = new ItemModelViewModel
            {
                Id = itmId,
                Name = name,
                SubItems = new SubItemViewModel[10]
            };

            for (var i = 0; i < 10; i++)
            {
                var subItemId = Guid.NewGuid();
                var subItmName = string.Format("Sub item - {0}", i);
                var subItem = new SubItem
                {
                    Id = subItemId,
                    Name = string.Format("{0} - CHANGED!", subItmName),
                    Units = new Unit[4]
                };

                var subItemVm = new SubItemViewModel
                {
                    Id = subItemId,
                    Name = subItmName,
                    Units = new Collection<UnitViewModel>()
                };

                itemModel.SubItems.Add(subItem);
                itemModelVm.SubItems[i] = subItemVm;

                for (var j = 0; j < 5; j++)
                {
                    var unitId = Guid.NewGuid();
                    var unitName = string.Format("Unit - {0}", j);

                    var unit = new Unit
                    {
                        Id = unitId,
                        Name = string.Format("{0}, - CHANGED!", unitName),
                        SubUnits = new Collection<SubUnit>()
                    };
                    if (j < 4)
                    {
                        subItem.Units[j] = unit;
                    }

                    var unitVm = new UnitViewModel();
                    
                    unitVm.Id = unitId;
                    unitVm.Name = unitName;
                    unitVm.SubUnits = new List<SubUnitViewModel>();
                    subItemVm.Units.Add(unitVm);
                    

                    for (var k = 0; k < 6; k++)
                    {
                        var subUnitId = Guid.NewGuid();
                        var subUnitName = string.Format("Sub unit - {0}", subUnitId);

                        var subUnit = new SubUnit
                        {
                            Id = subUnitId,
                            Name = String.Format("{0}, - CHANGED!", subUnitName)
                        };
                        unit.SubUnits.Add(subUnit);
                        if (k < 3)
                        {
                            var subUnitVm = new SubUnitViewModel
                            {
                                Id = subUnitId,
                                Name = subUnitName,
                            };
                            unitVm.SubUnits.Add(subUnitVm);
                        }
                    }
                }
            }

            return new KeyValuePair<ItemModel, ItemModelViewModel>(itemModel, itemModelVm);
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
                Weight = 32,
                CaseInsensitive = "abc"
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
                Weight = 32,
                CaSeInSeNsItIvE = "abc"
            };

            return new KeyValuePair<TestModel, TestViewModel>(src, result);
        }

        public static KeyValuePair<List<TestModel>, List<TestViewModel>> AutoMemberMapCollection()
        {
            var models = new List<TestModel>();
            var viewModels = new List<TestViewModel>();
            for (var i = 0; i < 10; i++)
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
                    Weight = 32,
                    CaseInsensitive = "abc"
                };
                models.Add(src);

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
                    Weight = 32,
                    CaSeInSeNsItIvE = "abc"
                };
                viewModels.Add(result);
            }
            

            return new KeyValuePair<List<TestModel>, List<TestViewModel>>(models, viewModels);
        }

        public static Tuple<TestItem, TestItemViewModel, TestItemViewModel> ExistingDestCollEquals()
        {
            var testItem = new TestItem();
            var testItemVm = new TestItemViewModel();
            var testItemVmCheck = new TestItemViewModel();

            var testItems = new List<TestCollection>();
            var testItemsVm = new List<TestCollectionViewModel>();
            var testItemsVmCheck = new List<TestCollectionViewModel>();

            for (var i = 0; i < 5; i++)
            {
                var id = Guid.NewGuid();
                var format = string.Format("Name - {0}", i);
                var name = string.Format("{0} - CHANGED!", format);

                var testCollection = new TestCollection
                {
                    Id = id,
                    Name = name
                };

                var testCollectionVm = new TestCollectionViewModel
                {
                    Id = id,
                    Name = format
                };

                var testCollectionVmCheck = new TestCollectionViewModel
                {
                    Id = id,
                    Name = name
                };

                testItems.Add(testCollection);
                testItemsVm.Add(testCollectionVm);
                testItemsVmCheck.Add(testCollectionVmCheck);
            }
            testItem.Queryable = testItems.AsQueryable();
            testItemVm.Array = testItemsVm.ToArray();
            testItemVmCheck.Array = testItemsVmCheck.ToArray();

            return new Tuple<TestItem, TestItemViewModel, TestItemViewModel>(testItem, testItemVm, testItemVmCheck);
        }

        public static KeyValuePair<TestItem, TestItemViewModel> OtherCollectionMapTest()
        {
            var testItem = new TestItem();
            var testItemVm = new TestItemViewModel();

            var testItems = new List<TestCollection>();
            var testItemsVm = new List<TestCollectionViewModel>();

            for (var i = 0; i < 5; i++)
            {
                var id = Guid.NewGuid();
                var format = string.Format("Name - {0}", i);

                var testCollection = new TestCollection
                {
                    Id = id,
                    Name = format
                };

                var testCollectionVm = new TestCollectionViewModel
                {
                    Id = id,
                    Name = format
                };

                testItems.Add(testCollection);
                testItemsVm.Add(testCollectionVm);
            }
            testItem.Array = testItems.ToArray();
            testItemVm.ObservableCollection = new ObservableCollection<TestCollectionViewModel>(testItemsVm);

            return new KeyValuePair<TestItem, TestItemViewModel>(testItem, testItemVm);
        }

        public static Tuple<TestItem, TestItemViewModel, TestItemViewModel> ExistingDestCollEqualsWithNullElement()
        {
            var testItem = new TestItem();
            var testItemVm = new TestItemViewModel();
            var testItemVmCheck = new TestItemViewModel();

            var testItems = new List<TestCollection>();
            var testItemsVm = new List<TestCollectionViewModel>();
            var testItemsVmCheck = new List<TestCollectionViewModel>();

            for (var i = 0; i < 5; i++)
            {
                var id = Guid.NewGuid();
                var format = string.Format("Name - {0}", i);
                var name = string.Format("{0} - CHANGED!", format);

                var testCollection = new TestCollection
                {
                    Id = id,
                    Name = name
                };

                var testCollectionVm = new TestCollectionViewModel
                {
                    Id = id,
                    Name = format
                };

                var testCollectionVmCheck = new TestCollectionViewModel
                {
                    Id = id,
                    Name = name
                };

                testItems.Add(testCollection);
                testItemsVmCheck.Add(testCollectionVmCheck);
                testItemsVm.Add(i == 3 ? null : testCollectionVm);
            }
            testItem.Queryable = testItems.AsQueryable();
            testItemVm.Array = testItemsVm.ToArray();
            testItemVmCheck.Array = testItemsVmCheck.ToArray();

            return new Tuple<TestItem, TestItemViewModel, TestItemViewModel>(testItem, testItemVm, testItemVmCheck);
        }

        public static Tuple<TestItem, TestItemViewModel, TestItemViewModel> ExistingDestSrcCollGreater()
        {
            var testItem = new TestItem();
            var testItemVm = new TestItemViewModel();
            var testItemVmCheck = new TestItemViewModel();

            var testItems = new List<TestCollection>();
            var testItemsVm = new List<TestCollectionViewModel>();
            var testItemsVmCheck = new List<TestCollectionViewModel>();

            for (var i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                var format = string.Format("Name - {0}", i);
                var name = string.Format("{0} - CHANGED!", format);

                var testCollection = new TestCollection
                {
                    Id = id,
                    Name = name
                };

                var testCollectionVm = new TestCollectionViewModel
                {
                    Id = id,
                    Name = format
                };

                var testCollectionVmCheck = new TestCollectionViewModel
                {
                    Id = id,
                    Name = name
                };

                testItems.Add(testCollection);
                testItemsVmCheck.Add(testCollectionVmCheck);

                if (i < 6)
                {
                    testItemsVm.Add(testCollectionVm);
                }
            }
            testItem.Array = testItems.ToArray();
            testItemVm.Collection = testItemsVm;
            testItemVmCheck.Collection = testItemsVmCheck;

            return new Tuple<TestItem, TestItemViewModel, TestItemViewModel>(testItem, testItemVm, testItemVmCheck);
        }

        public static Tuple<TestItem, TestItemViewModel, TestItemViewModel> ExistingDestCollGreater()
        {
            var testItem = new TestItem();
            var testItemVm = new TestItemViewModel();
            var testItemVmCheck = new TestItemViewModel();

            var testItems = new List<TestCollection>();
            var testItemsVm = new List<TestCollectionViewModel>();
            var testItemsVmCheck = new List<TestCollectionViewModel>();

            for (var i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                var format = string.Format("Name - {0}", i);
                var name = string.Format("{0} - CHANGED!", format);

                var testCollection = new TestCollection
                {
                    Id = id,
                    Name = name
                };

                var testCollectionVm = new TestCollectionViewModel
                {
                    Id = id,
                    Name = format
                };

                var testCollectionVmCheck = new TestCollectionViewModel
                {
                    Id = id,
                    Name = name
                };

                if (i < 6)
                {
                    testItems.Add(testCollection);
                    testItemsVmCheck.Add(testCollectionVmCheck);
                }
                testItemsVm.Add(testCollectionVm);
            }
            testItem.Collection = testItems.ToArray();
            testItemVm.List = testItemsVm;
            testItemVmCheck.List = testItemsVmCheck.ToList();

            return new Tuple<TestItem, TestItemViewModel, TestItemViewModel>(testItem, testItemVm, testItemVmCheck);
        }

        public static Tuple<TestItem, TestItemViewModel, TestItemViewModel> ExistingDestCollNotEqual()
        {
            var testItem = new TestItem();
            var testItemVm = new TestItemViewModel();
            var testItemVmCheck = new TestItemViewModel();

            var testItems = new List<TestCollection>();
            var testItemsVm = new List<TestCollectionViewModel>();
            var testItemsVmCheck = new List<TestCollectionViewModel>();

            for (var i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                var format = string.Format("Name - {0}", i);
                var name = string.Format("{0} - CHANGED!", format);

                var testCollection = new TestCollection
                {
                    Id = id,
                    Name = name
                };

                var testCollectionVm = new TestCollectionViewModel
                {
                    Id = id,
                    Name = format
                };

                var testCollectionVmCheck = new TestCollectionViewModel
                {
                    Id = id,
                    Name = name
                };

                if (i < 6)
                {
                    testItems.Add(testCollection);
                    testItemsVmCheck.Add(testCollectionVmCheck);
                }
                testItemsVm.Add(testCollectionVm);
            }
            testItem.Collection = testItems;
            testItemVm.Array = testItemsVm.ToArray();
            testItemVmCheck.Array = testItemsVmCheck.ToArray();

            return new Tuple<TestItem, TestItemViewModel, TestItemViewModel>(testItem, testItemVm, testItemVmCheck);
        }

        public static KeyValuePair<TestModel, TestViewModel> AccessSourceNestedProperty()
        {
            var testId = Guid.NewGuid();

            var created = DateTime.UtcNow;
            var src = new TestModel
            {
                Id = testId,
                Age = 18,
                Created = created,
                Name = "AutoMemberMap",
                Weight = 32
            };

            var result = new TestViewModel
            {
                Id = testId,
                Age = 18,
                Created = created,
                Weight = 32
            };

            return new KeyValuePair<TestModel, TestViewModel>(src, result);
        }

        public static KeyValuePair<TestModel, TestViewModel> ExistingDestinationSimpleMap()
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
                    Name = "USA - changed",
                    Code = "US - changed"
                },
                Created = created,
                Name = "AutoMemberMap - changed",
                Sizes = new List<Size>
                {
                    new Size
                    {
                        Id = xxlId,
                        Name = "XXL size - changed",
                        Alias = "XXL",
                        SortOrder = 3
                    },
                    new Size
                    {
                        Id = xlId,
                        Name = "XL size - CHANGED",
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


        public static KeyValuePair<Trip, TripViewModel> ExistingDestinationMediumMap()
        {
            var tripId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var catalogId = Guid.NewGuid();
            var typeId = Guid.NewGuid();

            var tripType = new TripType
            {
                Id = typeId,
                Name = "Easy - changed",
            };

            var tripTypeViewModel = new TripTypeViewModel
            {
                Id = typeId,
                Name = "Easy",
            };

            var tripCatalog = new TripCatalog
            {
                Id = catalogId,
                Name = "Adventure - changed",
                TripType = tripType
            };

            var tripCatalogViewModel = new TripCatalogViewModel
            {
                Id = catalogId,
                Name = "Adventure",
                TripType = tripTypeViewModel
            };


            var categoryTrip = new CategoryTrip
            {
                Id = categoryId,
                Name = "Asia - changed",
                Catalog = tripCatalog
            };

            var categoryTripViewModel = new CategoryTripViewModel
            {
                Id = categoryId,
                Name = "Asia",
                Catalog = tripCatalogViewModel
            };

            var trip = new Trip
            {
                Id = tripId,
                Name = "Fascinating family - changed",
                Category = categoryTrip
            };

            var tripViewModel = new TripViewModel
            {
                Id = tripId,
                Name = "Fascinating family",
                Category = categoryTripViewModel
            };
            

            return new KeyValuePair<Trip, TripViewModel>(trip, tripViewModel);
        }

        public static KeyValuePair<Trip, TripViewModel> AccessSourceManyNestedProperties()
        {
            var tripId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var catalogId = Guid.NewGuid();

            var tripCatalog = new TripCatalog
            {
                Id = catalogId,
                Name = "Adventure - changed",
            };

            var categoryTrip = new CategoryTrip
            {
                Id = categoryId,
                Name = "Asia - changed",
                Catalog = tripCatalog
            };

            var trip = new Trip
            {
                Id = tripId,
                Name = "Fascinating family - changed",
                Category = categoryTrip
            };

            var tripViewModel = new TripViewModel
            {
                Id = tripId
            };


            return new KeyValuePair<Trip, TripViewModel>(trip, tripViewModel);
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

        public static KeyValuePair<SpecialGift, SpecialGiftViewModel> HiddenInheritedMemberMap()
        {
            var newGuid = Guid.NewGuid();
            var otherGuid = Guid.NewGuid();
            var keyValuePair = new KeyValuePair<SpecialGift, SpecialGiftViewModel>
                (
                new SpecialGift
                {
                    Id = newGuid,
                    Name = "Vase",
                    Recipient = new SpecialPerson
                    {
                        Name = "Mark",
                        AffectionLevel = 10,
                        Id = otherGuid,
                        IsOrganization = false,
                        IsPerson = true
                    }
                },
                new SpecialGiftViewModel
                {
                    Id = newGuid,
                    Name = "Vase",
                    Recipient = new SpecialPersonViewModel
                    {
                        Name = "Mark",
                        AffectionLevel = 10,
                        Id = otherGuid,
                        IsOrganization = false,
                        IsPerson = true
                    }
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

        public static Tuple<TestModel, TestViewModel, TestViewModel> CustomNestedCollectionMap()
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

            var rstCheck = new TestViewModel
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



            var keyValuePair = new Tuple<TestModel, TestViewModel, TestViewModel>
                (
                src,
                rst,
                rstCheck
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
