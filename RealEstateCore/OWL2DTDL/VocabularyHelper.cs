using System;

/// <summary>
/// A set of often-used URIs, for easy reference.
/// </summary>
namespace OWL2DTDL.VocabularyHelper
{
    public static class O2O
    {
        public static readonly Uri included = new Uri("https://karlhammar.com/owl2oas/o2o.owl#included");
        public static readonly Uri endpoint = new Uri("https://karlhammar.com/owl2oas/o2o.owl#endpoint");
    }

    public static class DC
    {
        public static readonly Uri title = new Uri("http://purl.org/dc/elements/1.1/title");
        public static readonly Uri description = new Uri("http://purl.org/dc/elements/1.1/description");
    }

    public static class CC
    {
        public static readonly Uri license = new Uri("http://creativecommons.org/ns#license");
    }

    public static class RDFS
    {
        public static readonly Uri label = new Uri("http://www.w3.org/2000/01/rdf-schema#label");
        public static readonly Uri Datatype = new Uri("http://www.w3.org/2000/01/rdf-schema#Datatype");
        public static readonly Uri Literal = new Uri("http://www.w3.org/2000/01/rdf-schema#Literal");
    }

    public static class OWL
    {
        public static readonly Uri Thing = new Uri("http://www.w3.org/2002/07/owl#Thing");
        public static readonly Uri Restriction = new Uri("http://www.w3.org/2002/07/owl#Restriction");
        public static readonly Uri FunctionalProperty = new Uri("http://www.w3.org/2002/07/owl#FunctionalProperty");
        public static readonly Uri versionIRI = new Uri("http://www.w3.org/2002/07/owl#versionIRI");
        public static readonly Uri deprecated = new Uri("http://www.w3.org/2002/07/owl#deprecated");
        public static readonly Uri oneOf = new Uri("http://www.w3.org/2002/07/owl#oneOf");

        #region Restrictions
        public static readonly Uri onProperty = new Uri("http://www.w3.org/2002/07/owl#onProperty");
        public static readonly Uri onClass = new Uri("http://www.w3.org/2002/07/owl#onClass");
        public static readonly Uri cardinality = new Uri("http://www.w3.org/2002/07/owl#cardinality");
        public static readonly Uri qualifiedCardinality = new Uri("http://www.w3.org/2002/07/owl#qualifiedCardinality");
        public static readonly Uri allValuesFrom = new Uri("http://www.w3.org/2002/07/owl#allValuesFrom");
        public static readonly Uri someValuesFrom = new Uri("http://www.w3.org/2002/07/owl#someValuesFrom");
        public static readonly Uri minCardinality = new Uri("http://www.w3.org/2002/07/owl#minCardinality");
        public static readonly Uri minQualifiedCardinality = new Uri("http://www.w3.org/2002/07/owl#minQualifiedCardinality");
        public static readonly Uri maxCardinality = new Uri("http://www.w3.org/2002/07/owl#maxCardinality");
        public static readonly Uri maxQualifiedCardinality = new Uri("http://www.w3.org/2002/07/owl#maxQualifiedCardinality");
        #endregion
    }

    public static class DTDL
    {
        public static readonly string dtdlContext = "dtmi:dtdl:context;2";
        public static readonly Uri Interface = new Uri("dtmi:dtdl:class:Interface;2");
        public static readonly Uri Property = new Uri("dtmi:dtdl:class:Property;2");
        public static readonly Uri Relationship = new Uri("dtmi:dtdl:class:Relationship;2");
        public static readonly Uri Telemetry = new Uri("dtmi:dtdl:class:Telemetry;2");
        public static readonly Uri Component = new Uri("dtmi:dtdl:class:Component;2");
        public static readonly Uri name = new Uri("dtmi:dtdl:property:name;2");
        public static readonly Uri contents = new Uri("dtmi:dtdl:property:contents;2");
        public static readonly Uri displayName = new Uri("dtmi:dtdl:property:displayName;2");
        public static readonly Uri description = new Uri("dtmi:dtdl:property:description;2");
        public static readonly Uri extends = new Uri("dtmi:dtdl:property:extends;2");
        public static readonly Uri maxMultiplicity = new Uri("dtmi:dtdl:property:maxMultiplicity;2");
        public static readonly Uri minMultiplicity = new Uri("dtmi:dtdl:property:minMultiplicity;2");
        public static readonly Uri target = new Uri("dtmi:dtdl:property:target;2");
        public static readonly Uri schema = new Uri("dtmi:dtdl:property:schema;2");
        public static readonly Uri Enum = new Uri("dtmi:dtdl:class:Enum;2");
        public static readonly Uri EnumValue = new Uri("dtmi:dtdl:class:EnumValue;2");
        public static readonly Uri valueSchema = new Uri("dtmi:dtdl:property:valueSchema;2");
        public static readonly Uri enumValue = new Uri("dtmi:dtdl:property:enumValue;2");
        public static readonly Uri enumValues = new Uri("dtmi:dtdl:property:enumValues;2");
        public static readonly Uri comment = new Uri("dtmi:dtdl:property:comment;2");


        public static readonly Uri properties = new Uri("dtmi:dtdl:property:properties;2");

        public static readonly Uri _string = new Uri("dtmi:dtdl:instance:Schema:string;2");
        public static readonly Uri _boolean = new Uri("dtmi:dtdl:instance:Schema:boolean;2");
        public static readonly Uri _integer = new Uri("dtmi:dtdl:instance:Schema:integer;2");
        public static readonly Uri _dateTime = new Uri("dtmi:dtdl:instance:Schema:dateTime;2");
        public static readonly Uri _double = new Uri("dtmi:dtdl:instance:Schema:double;2");
        public static readonly Uri _float = new Uri("dtmi:dtdl:instance:Schema:float;2");
        public static readonly Uri _long = new Uri("dtmi:dtdl:instance:Schema:long;2");
    }
}
