using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpressMapper
{
    internal class FlattenMapper<TSource, TDest>
    {
        private readonly StringComparison _stringComparison;

        private readonly PropertyInfo[] _allDestProps;
        private readonly PropertyInfo[] _allSourceProps;

        private readonly List<PropertyInfo> _filteredDestProps;

        private List<FlattenMemberInfo> _foundFlattens;

        public FlattenMapper(ICollection<string> namesOfPropertiesToIgnore, StringComparison stringComparison)
        {
            _stringComparison = stringComparison;
            _allSourceProps = GetPropertiesRightAccess<TSource>();
            _allDestProps = GetPropertiesRightAccess<TDest>();

            //ExpressMapper with match the top level properties, so we ignore those
            _filteredDestProps = FilterOutExactMatches(_allDestProps, _allSourceProps);

            if (!namesOfPropertiesToIgnore.Any()) return;

            //we also need to remove the destinations that have a .Member or .Ignore applied to them
            if (stringComparison == StringComparison.OrdinalIgnoreCase)
                namesOfPropertiesToIgnore = namesOfPropertiesToIgnore.Select(x => x.ToLowerInvariant()).ToList();
            _filteredDestProps = _filteredDestProps.Where(x => !namesOfPropertiesToIgnore.Contains(
                _stringComparison == StringComparison.OrdinalIgnoreCase ? x.Name.ToLowerInvariant() : x.Name)).ToList();
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

            foreach (var matchedStartSrcProp in sourceProps.Where(x => destProp.Name.StartsWith(prefix+x.Name, _stringComparison)))
            {
                var matchStart = prefix + matchedStartSrcProp.Name;
                if (string.Equals(destProp.Name, matchStart, _stringComparison))
                {
                    //direct match of name

                    var underlyingType = Nullable.GetUnderlyingType(destProp.PropertyType);
                    if (destProp.PropertyType == matchedStartSrcProp.PropertyType ||
                        underlyingType == matchedStartSrcProp.PropertyType ||
                        Mapper.MapExists(matchedStartSrcProp.PropertyType, destProp.PropertyType))
                    {
                        //matched a) same type, or b) dest is a nullable version of source 
                        _foundFlattens.Add( new FlattenMemberInfo(destProp, sourcePropPath, matchedStartSrcProp));
                        _filteredDestProps.Remove(destProp);        //matched, so take it out
                    }

                    return;
                }

                if (matchedStartSrcProp.PropertyType == typeof (string))
                    //string can only be directly matched
                    continue;

                if (matchedStartSrcProp.PropertyType.GetInfo().IsClass)
                {
                    var classProps = GetPropertiesRightAccess(matchedStartSrcProp.PropertyType);
                    var clonedList = sourcePropPath.ToList();
                    clonedList.Add(matchedStartSrcProp);
                    ScanSourceClassRecursively(classProps, destProp, matchStart, clonedList.ToArray());
                }
                else if (matchedStartSrcProp.PropertyType.GetInfo().GetInterfaces().Any(i => i.Name == "IEnumerable"))
                {
                    //its an enumerable class so see if the end relates to a LINQ method
                    var endOfName = destProp.Name.Substring(matchStart.Length);
                    var enumeableMethod = FlattenLinqMethod.EnumerableEndMatchsWithLinqMethod(endOfName, _stringComparison);
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
            return classType.GetInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        private List<PropertyInfo> FilterOutExactMatches(PropertyInfo[] propsToFilter, PropertyInfo[] filterAgainst)
        {
            var filterNames = filterAgainst
                .Select(x => _stringComparison == StringComparison.OrdinalIgnoreCase ? x.Name.ToLowerInvariant() : x.Name).ToArray();
            return propsToFilter.Where(x => !filterNames
                .Contains(_stringComparison == StringComparison.OrdinalIgnoreCase ? x.Name.ToLowerInvariant() : x.Name)).ToList();

        }
    }
}
