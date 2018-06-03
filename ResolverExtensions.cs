using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace kasthack.vksharp.generator
{
    public static class ResolverExtensions {

        /*
         * pattern to look for in the property description to detect dates serialized as integers
         */
        private static readonly Regex TimeDescriptionPattern = new Regex(
            @"\b(date|time|unixtime)\b",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        /*
         * type names of structures which have to be marked as nullables when not present
         */
        private static readonly HashSet<String> StructNames = new HashSet<string> {
            TypeKind.boolean,
            TypeKind.number,
            TypeKind.integer,
            TypeKind.DateTimeOffset
        };

        /*
         * names of fields which are always present even though may be not marked as such
         */
        private static readonly HashSet<String> AlwaysPresentFields = new HashSet<string> {
            "id"
        };

        //checking for base class existense
        public static bool HasBaseClass(this Definition definition) => definition.AllOf != null && definition.AllOf.Count(a => a.IsRef()) == 1;
        
        //base class name resolution
        public static string BaseClassName(this Definition definition) => definition.AllOf.First(a => a.IsRef()).TypeName();

        //properties in inherited classes are serialized a bit different.
        //this function gets the actual definiton with properties for an inherited class
        public static Definition GetChildPropertySource(this Definition definition) => definition.AllOf.FirstOrDefault(a => !a.IsRef());

        //parameter type  name resolution
        public static string ParameterTypeName(this Parameter parameter) => parameter.TypeName();//todo: required, enums

        //property type resolution
        //does the same as typename and then marks nullable fields
        public static string PropertyTypeName(this Definition definition, string propertyName, Dictionary<string, Definition> definitions)
        {
            var property = definition.Properties[propertyName];

            //basic resolution
            var name = property.TypeName();

            //nullables
            if (!property.IsArray())
            {
                //is a well-known stuct or a defined enum and not guaranteed to be present in the response
                if (
                    StructNames.Contains(name)
                    ||
                    (property.IsRef() && definitions[name].IsEnum())
                )
                {
                    if (!definition.Required.Contains(propertyName) && !AlwaysPresentFields.Contains(propertyName))
                    {
                        name += "?";
                    }
                }
            }

            return name;
        }
        
        //enumname resolution
        public static string EnumName(this string name) => name.Replace(" ", "_");
        
        //basic type checks
        public static bool IsArray(this DefinitionBase definition) => definition.Type == TypeKind.array;
        public static bool IsObject(this DefinitionBase definition) => definition.Type == TypeKind.obj;
        public static bool IsEnum(this DefinitionBase definition) => definition.Enum != null;
        public static bool IsIntegerOrStringEnum(this DefinitionBase definition) => definition.IsEnum() && definition.Type == TypeKind.integer;
        public static bool IsRef(this DefinitionBase definition) => definition.Ref != null;
        
        //plain type -> type
        //array -> type[]
        //ref -> resolved type name
        //time formatted as int -> datetimeoffset
        //custom boolean -> plain boolean
        public static string TypeName(this DefinitionBase property) {
            string name;

            //basic resolution: arrays, references, basic types
            if (property.Type == TypeKind.array)
            {
                name = property.Items.TypeName() + "[]";
            }
            else if (property.IsRef())
            {
                name = property.Ref.Split('/').Last();
            }
            else
            {
                name = property.Type;
            }

            //type post-processing
            {
                //merge bools
                if (name == "base_bool_int")
                {
                    name = TypeKind.boolean;
                }
                
                //unix dates formatted as integers -> actual timestamps
                if (property.Description!= null && TimeDescriptionPattern.IsMatch(property.Description) && name == TypeKind.integer)
                {
                    name = TypeKind.DateTimeOffset;
                }
            }
            
            return name;
        }

        //snakeCase -> pascal case
        public static string ToPascalCase(this string name, bool lowerFirst = false)
        {
            var t = new StringBuilder();
            t.Append(lowerFirst ? char.ToLower(name[0]) : char.ToUpper(name[0]));
            for (var index = 1; index < name.Length; index++)
            {
                var c = name[index];
                //add '_' before numbers and capitals 
                if (c == '.' || c == '_') t.Append(char.ToUpper(name[++index]));
                else t.Append(c);
            }
            return t.ToString();
        }
        
        //shortcut for indexof(... ignorecase) > 0
        private static bool ContainsIngoreCase(this string source, string pattern) =>
            source.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
