using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpressMapper
{
    internal class FlattenMapper<TSource, TDest>
    {

        private readonly PropertyInfo[] _allDestProps;
        private readonly PropertyInfo[] _allSourceProps;

        private readonly List<PropertyInfo> _filteredDestProps;

        private List<FlattenMemberInfo> _foundFlattens;

        public FlattenMapper(ICollection<string> namesOfPropertiesToIgnore)
        {
            _allDestProps = GetPropertiesRightAccess<TDest>().Where(x => !namesOfPropertiesToIgnore.Contains(x.Name)).ToArray();
            _allSourceProps = GetPropertiesRightAccess<TSource>();
            //ExpressMapper with match the top level properties, so we ignore those
            _filteredDestProps = FilterOutExactMatches(_allDestProps, _allSourceProps);
        }

        public List<FlattenMemberInfo> BuildMemberMapping()
        {
            _foundFlattens = new List<FlattenMemberInfo>();
            var filteredSourceProps = FilterOutExactMatches(_allSourceProps, _allDestProps);
            ScanSourceProps(filteredSourceProps);
            return _foundFlattens;
        }

        private void ScanSourceProps(List<PropertyInfo> sourcePropsToScan, 
            string prefix = "", PropertyInfo[] sourcePropPath = null)
        {
            foreach (var destProp in _filteredDestProps.ToList())
                //scan source property name against dest that has no direct match with any of the source property names
                if (_filteredDestProps.Contains(destProp))
                    //This allows for entries to be removed from the list
                    ScanSourceClassRecursively(sourcePropsToScan, destProp, prefix, sourcePropPath ?? new PropertyInfo [] {});
        }

        private void ScanSourceClassRecursively(IEnumerable<PropertyInfo> sourceProps, PropertyInfo destProp, 
            string prefix, PropertyInfo [] sourcePropPath)
        {

            foreach (var matchedStartSrcProp in sourceProps.Where(x => destProp.Name.StartsWith(prefix+x.Name)))
            {
                var matchStart = prefix + matchedStartSrcProp.Name;
                if (destProp.Name == matchStart)
                {
                    //direct match of name

                    var underlyingType = Nullable.GetUnderlyingType(destProp.PropertyType);
                    if (destProp.PropertyType == matchedStartSrcProp.PropertyType ||
                        underlyingType == matchedStartSrcProp.PropertyType ||
                        Mapper.MapExists(matchedStartSrcProp.PropertyType, destProp.PropertyType))
                    {
                        //matched a) same type, b) dest is a nullable version of source or c) there is an Express mapping between source and dest
                        _foundFlattens.Add( new FlattenMemberInfo(destProp, sourcePropPath, matchedStartSrcProp));
                        _filteredDestProps.Remove(destProp);        //matched, so take it out
                    }

                    return;
                }

                if (matchedStartSrcProp.PropertyType == typeof (string))
                    //string can only be directly matched
                    continue;

                if (matchedStartSrcProp.PropertyType.IsClass)
                {
                    var classProps = GetPropertiesRightAccess(matchedStartSrcProp.PropertyType);
                    var clonedList = sourcePropPath.ToList();
                    clonedList.Add(matchedStartSrcProp);
                    ScanSourceClassRecursively(classProps, destProp, matchStart, clonedList.ToArray());
                }
                else if (matchedStartSrcProp.PropertyType.GetInterface("IEnumerable") != null)
                {
                    //its an enumerable class so see if the end relates to a LINQ method
                    var endOfName = destProp.Name.Substring(matchStart.Length);
                    var enumeableMethod = FlattenLinqMethod.EnumerableEndMatchsWithLinqMethod(endOfName);
                    if (enumeableMethod != null)
                    {
                        _foundFlattens.Add(new FlattenMemberInfo(destProp, sourcePropPath, matchedStartSrcProp, enumeableMethod));
                        _filteredDestProps.Remove(destProp);        //matched, so take it out
                    }
                }
            }
        }

        private static PropertyInfo[] GetPropertiesRightAccess<T>()
        {
            return GetPropertiesRightAccess(typeof (T));
        }

        private static PropertyInfo[] GetPropertiesRightAccess(Type classType)
        {
            return classType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        private static List<PropertyInfo> FilterOutExactMatches(PropertyInfo[] propsToFilter, PropertyInfo[] filterAgainst)
        {
            var filterNames = filterAgainst.Select(x => x.Name).ToArray();
            return propsToFilter.Where(x => !filterNames.Contains(x.Name)).ToList();

        }
    }
}
