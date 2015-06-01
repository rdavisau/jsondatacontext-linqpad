using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LINQPad.Extensibility.DataContext;

namespace JsonDataContextDriver
{
    public static class LinqPadSampleCode
    {
        // These methods taken from the Static/Universal LINQPad context driver sample 

        public static List<ExplorerItem> GetSchema(Type customType)
        {
            // Return the objects with which to populate the Schema Explorer by reflecting over customType.

            // We'll start by retrieving all the properties of the custom type that implement IEnumerable<T>:
            var topLevelProps =
                (
                    from prop in customType.GetProperties()
                    where prop.PropertyType != typeof(string)

                    // Display all properties of type IEnumerable<T> (except for string!)
                    let ienumerableOfT = prop.PropertyType.GetInterface("System.Collections.Generic.IEnumerable`1")
                    where ienumerableOfT != null

                    orderby prop.Name

                    select new ExplorerItem(prop.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                    {
                        IsEnumerable = true,
                        ToolTipText = prop.PropertyType.Name,

                        // Store the entity type to the Tag property. We'll use it later.
                        Tag = ienumerableOfT.GetGenericArguments()[0]
                    }

                    ).ToList();

            // Create a lookup keying each element type to the properties of that type. This will allow
            // us to build hyperlink targets allowing the user to click between associations:
            var elementTypeLookup = topLevelProps.ToLookup(tp => (Type)tp.Tag);

            // Populate the columns (properties) of each entity:
            foreach (ExplorerItem table in topLevelProps)
                table.Children = ((Type)table.Tag)
                    .GetProperties()
                    .Select(childProp => GetChildItem(elementTypeLookup, childProp))
                    .OrderBy(childItem => childItem.Kind)
                    .ToList();

            return topLevelProps;
        }

        static ExplorerItem GetChildItem(ILookup<Type, ExplorerItem> elementTypeLookup, PropertyInfo childProp)
        {
            // If the property's type is in our list of entities, then it's a Many:1 (or 1:1) reference.
            // We'll assume it's a Many:1 (we can't reliably identify 1:1s purely from reflection).
            if (elementTypeLookup.Contains(childProp.PropertyType))
                return new ExplorerItem(childProp.Name, ExplorerItemKind.ReferenceLink, ExplorerIcon.ManyToOne)
                {
                    HyperlinkTarget = elementTypeLookup[childProp.PropertyType].First(),
                    // FormatTypeName is a helper method that returns a nicely formatted type name.
                    ToolTipText = childProp.PropertyType.Name
                };

            // Is the property's type a collection of entities?
            Type ienumerableOfT = childProp.PropertyType.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (ienumerableOfT != null)
            {
                Type elementType = ienumerableOfT.GetGenericArguments()[0];
                if (elementTypeLookup.Contains(elementType))
                    return new ExplorerItem(childProp.Name, ExplorerItemKind.CollectionLink, ExplorerIcon.OneToMany)
                    {
                        HyperlinkTarget = elementTypeLookup[elementType].First(),
                        ToolTipText = elementType.Name
                    };
            }

            // Ordinary property:
            return new ExplorerItem(childProp.Name + " (" + (childProp.PropertyType.Name) + ")",
                ExplorerItemKind.Property, ExplorerIcon.Column);
        }
    }
}