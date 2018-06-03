using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * type mappings for vk json schema
 * 
 * this generator uses this fork: https://github.com/kasthack/vk-api-schema
 */

namespace kasthack.vksharp.generator
{
    //------------------------------------------------------------------------------------------------------------Methods
    public partial class MethodsSchema
    {
        public Error[] Errors { get; set; }
        public Method[] Methods { get; set; }
    }

    public partial class Error
    {
        public string Name { get; set; }
        public long? Code { get; set; }
        public string Description { get; set; }
        public override string ToString() => $"{{ {nameof(Name)}: '{Name}', {nameof(Code)}: {Code}, {nameof(Description)}:'{Description}' }}";
    }

    public partial class Method
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonProperty("access_token_type")]
        public AccessTokenType[] AccessTokenType { get; set; }
        public Parameter[] Parameters { get; set; } = Array.Empty<Parameter>(); 
        public Dictionary<string, Response> Responses { get; set; }
        public Error[] Errors { get; set; }

        public override string ToString() => $@"{{
    {nameof(Name)}: '{Name}',
    {nameof(Description)}:'{Description}',
    {nameof(AccessTokenType)}: {
            (AccessTokenType==null?"(null)":$"[{string.Join(",", AccessTokenType.Select(b=>$"'{b}'"))}]")
        },
    {nameof(Errors)}: {
            (Errors == null ? "(null)" : $"[{string.Join(",", Errors.Select(b => b.ToString()))}]")
        },
}}";
    }

    public partial class Parameter : DefinitionBase
    {
        public string Name { get; set; }
        public long? MaxItems { get; set; }
        public long? Maximum { get; set; }
        public bool? ParameterRequired { get; set; }
        public long? MinLength { get; set; }
        public long? MaxLength { get; set; }
        public object Default { get; set; }
        public bool Required { get; set; }

        public override string ToString() => $@"{{
    {nameof(Name)}: '{Name}'
    {nameof(Type)}: {Type},
}}";
    }


    public partial class Response
    {
        [JsonProperty("$ref")] public string Ref { get; set; }
    }

    public enum AccessTokenType { group, open, service, user };

    public partial struct Default
    {
        public long? Integer;
        public string String;

        public bool IsNull => Integer == null && String == null;
    }


    //------------------------------------------------------------------------------------------------------Responses & objects

    public partial class ResponsesSchema
    {
        public string Title { get; set; }
        public Dictionary<string, Definition> Definitions { get; set; }
    }
    public class DefinitionBase
    {
        public string Type { get; set; }
        public Dictionary<string, Definition> Properties { get; set; } = new Dictionary<string, Definition>();

        public object[] Enum { get; set; }
        public string[] EnumNames { get; set; }
        public string Description { get; set; }


        [JsonProperty("$ref")] public string Ref { get; set; }
        public Definition Items { get; set; }

        public long? Minimum { get; set; }
        public int? MaxProperties { get; set; }
        public bool AdditionalProperties { get; set; }

        public bool IsConcrete() => Ref == null && (Type != TypeKind.array || Items.IsConcrete()) && (Properties == null || Properties.All(a => a.Value.IsConcrete()));
    }
    public class Definition : DefinitionBase
    {
        public HashSet<string> Required { get; set; } = new HashSet<string>();

        public Definition[] AllOf { get; set; }

    }
}
