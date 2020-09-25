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

        public static readonly Uri annotatedSource = new Uri("http://www.w3.org/2002/07/owl#annotatedSource");
        public static readonly Uri annotatedProperty = new Uri("http://www.w3.org/2002/07/owl#annotatedProperty");
        public static readonly Uri annotatedTarget = new Uri("http://www.w3.org/2002/07/owl#annotatedTarget");

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

    public static class QUDT
    {
        public static readonly Uri Unit = new Uri("http://qudt.org/schema/qudt/Unit");
        public static readonly Uri hasQuantityKind = new Uri("http://qudt.org/schema/qudt/hasQuantityKind");

        public static class UnitNS
        {
            public static readonly Uri A = new Uri("http://qudt.org/vocab/unit/A");
            public static readonly Uri CentiM = new Uri("http://qudt.org/vocab/unit/CentiM");
            public static readonly Uri DEG = new Uri("http://qudt.org/vocab/unit/DEG");
            public static readonly Uri HP = new Uri("http://qudt.org/vocab/unit/HP");
            public static readonly Uri HR = new Uri("http://qudt.org/vocab/unit/HR");
            public static readonly Uri KiloGM = new Uri("http://qudt.org/vocab/unit/KiloGM");
            public static readonly Uri L = new Uri("http://qudt.org/vocab/unit/L");
            public static readonly Uri L_PER_SEC = new Uri("http://qudt.org/vocab/unit/L-PER-SEC");
            public static readonly Uri M = new Uri("http://qudt.org/vocab/unit/M");
            public static readonly Uri MilliM = new Uri("http://qudt.org/vocab/unit/MilliM");
            public static readonly Uri MIN = new Uri("http://qudt.org/vocab/unit/MIN");
            public static readonly Uri M_PER_SEC = new Uri("http://qudt.org/vocab/unit/M-PER-SEC");
            public static readonly Uri PSI = new Uri("http://qudt.org/vocab/unit/PSI");
            public static readonly Uri REV_PER_MIN = new Uri("http://qudt.org/vocab/unit/REV-PER-MIN");
            public static readonly Uri V = new Uri("http://qudt.org/vocab/unit/V");
            public static readonly Uri W = new Uri("http://qudt.org/vocab/unit/W");
        }

        public static class QuantityKindNS
        {
            public static readonly Uri AngularVelocity = new Uri("http://qudt.org/vocab/quantitykind/AngularVelocity");
            public static readonly Uri ElectricCurrent = new Uri("http://qudt.org/vocab/quantitykind/ElectricCurrent");
            public static readonly Uri PlaneAngle = new Uri("http://qudt.org/vocab/quantitykind/PlaneAngle");
            public static readonly Uri Power = new Uri("http://qudt.org/vocab/quantitykind/Power");
            public static readonly Uri Length = new Uri("http://qudt.org/vocab/quantitykind/Length");
            public static readonly Uri Mass = new Uri("http://qudt.org/vocab/quantitykind/Mass");
            public static readonly Uri Pressure = new Uri("http://qudt.org/vocab/quantitykind/Pressure");
            public static readonly Uri Time = new Uri("http://qudt.org/vocab/quantitykind/Time");
            public static readonly Uri Velocity = new Uri("http://qudt.org/vocab/quantitykind/Velocity");
            public static readonly Uri Voltage = new Uri("http://qudt.org/vocab/quantitykind/Voltage");
            public static readonly Uri Volume = new Uri("http://qudt.org/vocab/quantitykind/Volume");
            public static readonly Uri VolumeFlowRate = new Uri("http://qudt.org/vocab/quantitykind/VolumeFlowRate");
        }

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
        public static readonly Uri writable = new Uri("dtmi:dtdl:property:writable;2");
        public static readonly Uri unit = new Uri("dtmi:dtdl:property:unit;2");
        public static readonly Uri properties = new Uri("dtmi:dtdl:property:properties;2");

        public static readonly Uri _string = new Uri("dtmi:dtdl:instance:Schema:string;2");
        public static readonly Uri _boolean = new Uri("dtmi:dtdl:instance:Schema:boolean;2");
        public static readonly Uri _integer = new Uri("dtmi:dtdl:instance:Schema:integer;2");
        public static readonly Uri _dateTime = new Uri("dtmi:dtdl:instance:Schema:dateTime;2");
        public static readonly Uri _double = new Uri("dtmi:dtdl:instance:Schema:double;2");
        public static readonly Uri _float = new Uri("dtmi:dtdl:instance:Schema:float;2");
        public static readonly Uri _long = new Uri("dtmi:dtdl:instance:Schema:long;2");

        
        public static readonly Uri ampere = new Uri("dtmi:standard:unit:ampere;2");
        public static readonly Uri volt = new Uri("dtmi:standard:unit:volt;2");
        public static readonly Uri centimetre = new Uri("dtmi:standard:unit:centimetre;2");
        public static readonly Uri degreeOfArc = new Uri("dtmi:standard:unit:degreeOfArc;2");
        public static readonly Uri horsepower = new Uri("dtmi:standard:unit:horsepower;2");
        public static readonly Uri hour = new Uri("dtmi:standard:unit:hour;2");
        public static readonly Uri kilogram = new Uri("dtmi:standard:unit:kilogram;2");
        public static readonly Uri litre = new Uri("dtmi:standard:unit:litre;2");
        public static readonly Uri litrePerSecond = new Uri("dtmi:standard:unit:litrePerSecond;2");
        public static readonly Uri metre = new Uri("dtmi:standard:unit:metre;2");
        public static readonly Uri metrePerSecond = new Uri("dtmi:standard:unit:metrePerSecond;2");
        public static readonly Uri millimetre = new Uri("dtmi:standard:unit:millimetre;2");
        public static readonly Uri minute = new Uri("dtmi:standard:unit:minute;2");
        public static readonly Uri poundPerSquareInch = new Uri("dtmi:standard:unit:poundPerSquareInch;2");
        public static readonly Uri revolutionPerMinute = new Uri("dtmi:standard:unit:revolutionPerMinute;2");
        public static readonly Uri watt = new Uri("dtmi:standard:unit:watt;2");
        
        public static readonly Uri Angle = new Uri("dtmi:standard:class:Angle;2");
        public static readonly Uri AngularVelocity = new Uri("dtmi:standard:class:AngularVelocity;2");
        public static readonly Uri Current = new Uri("dtmi:standard:class:Current;2");
        public static readonly Uri Voltage = new Uri("dtmi:standard:class:Voltage;2");
        public static readonly Uri Power = new Uri("dtmi:standard:class:Power;2");
        public static readonly Uri Pressure = new Uri("dtmi:standard:class:Pressure;2");
        public static readonly Uri Length = new Uri("dtmi:standard:class:Length;2");
        public static readonly Uri Mass = new Uri("dtmi:standard:class:Mass;2");
        public static readonly Uri TimeSpan = new Uri("dtmi:standard:class:TimeSpan;2");
        public static readonly Uri Volume = new Uri("dtmi:standard:class:Volume;2");
        public static readonly Uri Velocity = new Uri("dtmi:standard:class:Velocity;2");
        public static readonly Uri VolumeFlowRate = new Uri("dtmi:standard:class:VolumeFlowRate;2");

    }
}
