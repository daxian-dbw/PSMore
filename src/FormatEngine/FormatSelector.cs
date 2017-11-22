
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PSMore.Formatting
{
    public enum FormatStyle
    {
        Default,
        List,
        Table
    }

    public struct FormatSelectionCriteria
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public FormatStyle Style { get; set; }
    }

    class FormatSelector
    {
        private static readonly Dictionary<Type, List<FormatDirective>> FormatDefinitions =
            new Dictionary<Type, List<FormatDirective>>(100);

        public static FormatDirective FindDirective(FormatSelectionCriteria criteria)
        {
            var type = criteria.Type;

            lock (FormatDefinitions)
            {
                while (type != null)
                {
                    if (!FormatDefinitions.TryGetValue(type, out var directives))
                    {
                        if (type.IsGenericType)
                        {
                            // Try again with the unspecialized type
                            var generic = type.GetGenericTypeDefinition();
                            FormatDefinitions.TryGetValue(generic, out directives);
                        }
                    }

                    if (directives != null)
                    {
                        foreach (var directive in directives)
                        {
                            switch (criteria.Style)
                            {
                                case FormatStyle.List:
                                    if (!(directive is ListFormat)) continue;
                                    break;
                                case FormatStyle.Table:
                                    //if (!(directive is TableFormat)) continue;
                                    break;
                            }

                            if (!string.IsNullOrEmpty(criteria.Name) &&
                                !criteria.Name.Equals(directive.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            return directive;
                        }
                    }

                    type = type.BaseType;
                }
            }

            return null;
        }

        public static void AddDirective(Type type, FormatDirective directive)
        {
            lock (FormatDefinitions)
            {
                if (!FormatDefinitions.TryGetValue(type, out var directives))
                {
                    directives = new List<FormatDirective>();
                    FormatDefinitions.Add(type, directives);
                }
                directives.Add(directive);
            }
        }
    }
}
