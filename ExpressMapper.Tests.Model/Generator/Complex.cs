using System;
using System.Collections.Generic;
using ExpressMapper.Tests.Model.Models;

namespace ExpressMapper.Tests.Model.Generator
{
    public class Complex
    {
        public List<Country> GetCountries()
        {
            return new List<Country>
            {
                new Country
                {
                    Id = Guid.NewGuid(),
                    Code = "BY",
                    Name = "Belarus"
                },
                new Country
                {
                    Id = Guid.NewGuid(),
                    Code = "BE",
                    Name = "Belgium"
                },
                new Country
                {
                    Id = Guid.NewGuid(),
                    Code = "CZ",
                    Name = "Czech Republic"
                },
                new Country
                {
                    Id = Guid.NewGuid(),
                    Code = "SK",
                    Name = "Slovak Republic"
                },
                new Country
                {
                    Id = Guid.NewGuid(),
                    Code = "ES",
                    Name = "Spain"
                }
            };
        }

        public List<City> GetCities(int count)
        {
            var cities = new List<City>();
            for (var i = 0; i < count; i++)
            {
                cities.Add(new City
                {
                    Id = Guid.NewGuid(),
                    Name = string.Format("City - {0}", i)
                });
            }
            return cities;
        }

        public List<Brand> GetBrands(int count)
        {
            var brands = new List<Brand>();
            for (var i = 0; i < count; i++)
            {
                brands.Add(new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = string.Format("Brand - {0}", i),
                });
            }
            return brands;
        }

        public List<Supplier> GetSuppliers(int count)
        {
            var brands = new List<Supplier>();
            for (var i = 0; i < count; i++)
            {
                brands.Add(new Supplier
                {
                    Id = Guid.NewGuid(),
                    Name = string.Format("Supplier - {0}", i),
                });
            }
            return brands;
        }

        public List<Size> GetSizes(int count)
        {
            var sizes = new List<Size>();
            for (var i = 0; i < count; i++)
            {
                sizes.Add(new Size
                {
                    Id = Guid.NewGuid(),
                    Name = string.Format("Size - {0}", i),
                    Alias = string.Format("SZ{0}", i),
                    SortOrder = i
                });
            }
            return sizes;
        }
    }
}
