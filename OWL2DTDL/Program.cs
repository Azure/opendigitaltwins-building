using CommandLine;
using Microsoft.Azure.DigitalTwins.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OWL2DTDL.VocabularyHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.JsonLd;
using VDS.RDF.Nodes;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace OWL2DTDL
{
    class Program
    {
        public class Options
        {
            [Option('n', "no-imports", Required = false, HelpText = "Sets program to not follow owl:Imports declarations.")]
            public bool NoImports { get; set; }
            [Option('f', "file-path", Required = true, HelpText = "The path to the on-disk root ontology file to translate.", SetName = "fileOntology")]
            public string FilePath { get; set; }
            [Option('u', "uri-path", Required = true, HelpText = "The URI of the root ontology file to translate.", SetName = "uriOntology")]
            public string UriPath { get; set; }
            [Option('o', "outputPath", Required = true, HelpText = "The directory in which to create DTDL models.")]
            public string OutputPath { get; set; }
            [Option('m', "merged-output", Required = false, HelpText = "Sets program to output one merged JSON-LD file for batch import into ADT.")]
            public bool MergedOutput { get; set; }
            [Option('i', "ignorefile", Required = false, HelpText = "Path to a CSV file, the first column of which lists (whole or partial) IRI:s that should be ignored by this tool and not translated into DTDL output.")]
            public string IgnoreFile { get; set; }
        }

        /// <summary>
        /// Custom comparer for Ontology objects, based on W3C OWL2 specification for version IRIs.
        /// See https://www.w3.org/TR/owl2-syntax/#Ontology_IRI_and_Version_IRI
        /// </summary>
        class OntologyComparer : IEqualityComparer<Ontology>
        {
            // Note use of .AbsoluteUri, since .NET Uri comparison does not take URI fragment into account
            public bool Equals(Ontology x, Ontology y)
            {
                return
                    !x.HasVersionUri() && !y.HasVersionUri() && (x.GetUri().AbsoluteUri == y.GetUri().AbsoluteUri) ||
                    x.HasVersionUri() && y.HasVersionUri() && (x.GetUri().AbsoluteUri == y.GetUri().AbsoluteUri) && (x.GetVersionUri().AbsoluteUri == y.GetVersionUri().AbsoluteUri);
            }

            // Method borrowed from https://stackoverflow.com/a/263416
            public int GetHashCode(Ontology x)
            {
                // Generate partial hashes from identify-carrying fields, i.e., ontology IRI 
                // and version IRI; if no version IRI exists, default to partial hash of 0.
                int oidHash = x.GetUri().AbsoluteUri.GetHashCode();
                int vidHash = x.HasVersionUri() ? x.GetVersionUri().AbsoluteUri.GetHashCode() : 0;

                // 
                int hash = 23;
                hash = hash * 37 + oidHash;
                hash = hash * 37 + vidHash;
                return hash;
            }
        }

        // Configuration fields
        private static bool _noImports;
        private static bool _localOntology;
        private static string _ontologyPath;
        private static string _outputPath;
        private static bool _mergedOutput;

        /// <summary>
        /// The root ontology being parsed.
        /// </summary>
        private static Ontology rootOntology;

        /// <summary>
        /// The joint ontology graph into which all imported ontologies are merged
        /// and upon which this tool subsequently operates.
        /// </summary>
        private static readonly OntologyGraph _ontologyGraph = new OntologyGraph();

        /// <summary>
        /// Set of mappings of URI namespaces to short names, used, e.g., to mint DTMIs
        /// </summary>
        private static readonly Dictionary<Uri, string> namespacePrefixes = new Dictionary<Uri, string>();

        /// <summary>
        /// Set of imported ontology URIs. Used to avoid revisiting a URI more than once in LoadImport().
        /// </summary>
        private static readonly HashSet<Uri> importedOntologyUris = new HashSet<Uri>();

        /// <summary>
        /// URIs that will be ignored by this tool, parsed from CSV file using -i command line option
        /// </summary>
        private static readonly HashSet<string> ignoredUris = new HashSet<string>();


        /// <summary>
        /// Mapping of QUDT units and quantity kinds to DTDL units and semantic types
        /// </summary>
        private static readonly Dictionary<Uri, Uri> semanticTypesMap = new Dictionary<Uri, Uri>()
        {
            { QUDT.UnitNS.A, DTDL.ampere },
            { QUDT.UnitNS.CentiM, DTDL.centimetre },
            { QUDT.UnitNS.DEG, DTDL.degreeOfArc },
            { QUDT.UnitNS.DEG_C, DTDL.degreeCelsius },
            { QUDT.UnitNS.HP, DTDL.horsepower },
            { QUDT.UnitNS.HR, DTDL.hour },
            { QUDT.UnitNS.KiloGM, DTDL.kilogram },
            { QUDT.UnitNS.KiloGM_PER_HR, DTDL.kilogramPerHour },
            { QUDT.UnitNS.KiloPA, DTDL.kilopascal },
            { QUDT.UnitNS.KiloW, DTDL.kilowatt },
            { QUDT.UnitNS.KiloW_HR, DTDL.kilowattHour },
            { QUDT.UnitNS.L, DTDL.litre },
            { QUDT.UnitNS.L_PER_SEC, DTDL.litrePerSecond },
            { QUDT.UnitNS.LUX, DTDL.lux },
            { QUDT.UnitNS.M, DTDL.metre },
            { QUDT.UnitNS.MilliM, DTDL.millimetre },
            { QUDT.UnitNS.MIN, DTDL.minute },
            { QUDT.UnitNS.M_PER_SEC, DTDL.metrePerSecond },
            { QUDT.UnitNS.PSI, DTDL.poundPerSquareInch },
            { QUDT.UnitNS.REV_PER_MIN, DTDL.revolutionPerMinute },
            { QUDT.UnitNS.V, DTDL.volt },
            { QUDT.UnitNS.W, DTDL.watt },
            { QUDT.QuantityKindNS.AngularVelocity, DTDL.AngularVelocity },
            { QUDT.QuantityKindNS.ElectricCurrent, DTDL.Current },
            { QUDT.QuantityKindNS.Energy, DTDL.Energy },
            { QUDT.QuantityKindNS.Illuminance, DTDL.Illuminance },
            { QUDT.QuantityKindNS.PlaneAngle, DTDL.Angle },
            { QUDT.QuantityKindNS.Voltage, DTDL.Voltage },
            { QUDT.QuantityKindNS.Power, DTDL.Power },
            { QUDT.QuantityKindNS.Mass, DTDL.Mass },
            { QUDT.QuantityKindNS.MassPerTime, DTDL.MassFlowRate },
            { QUDT.QuantityKindNS.Pressure, DTDL.Pressure },
            { QUDT.QuantityKindNS.Length, DTDL.Length },
            { QUDT.QuantityKindNS.Temperature, DTDL.Temperature },
            { QUDT.QuantityKindNS.Time, DTDL.TimeSpan },
            { QUDT.QuantityKindNS.Velocity, DTDL.Velocity },
            { QUDT.QuantityKindNS.Volume, DTDL.Volume },
            { QUDT.QuantityKindNS.VolumeFlowRate, DTDL.VolumeFlowRate }
        };

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       _outputPath = o.OutputPath;
                       _noImports = o.NoImports;
                       _mergedOutput = o.MergedOutput;
                       if (o.FilePath != null)
                       {
                           _localOntology = true;
                           _ontologyPath = o.FilePath;
                       }
                       else
                       {
                           _localOntology = false;
                           _ontologyPath = o.UriPath;
                       }

                       // Parse ignored namespaces from ignorefile
                       if (o.IgnoreFile != null)
                       {
                           using (var reader = new StreamReader(o.IgnoreFile))
                           {
                               while (!reader.EndOfStream)
                               {
                                   var line = reader.ReadLine();
                                   var values = line.Split(';');
                                   ignoredUris.Add(values[0]);
                               }
                           }
                       }
                   })
                   .WithNotParsed((errs) =>
                   {
                       Environment.Exit(1);
                   });

            // Turn off caching
            UriLoader.CacheDuration = TimeSpan.MinValue;

            // Load ontology graph from local or remote path
            Console.WriteLine($"Loading {_ontologyPath}.");
            if (_localOntology)
            {
                FileLoader.Load(_ontologyGraph, _ontologyPath);
            }
            else
            {
                UriLoader.Load(_ontologyGraph, new Uri(_ontologyPath));
            }

            // Get the main ontology defined in the graph and add it to the namespace mapper
            rootOntology = _ontologyGraph.GetOntology();
            namespacePrefixes.Add(rootOntology.GetUri(), rootOntology.GetShortName());

            // If configured for it, parse owl:Imports transitively
            if (!_noImports)
            {
                foreach (Ontology import in rootOntology.Imports)
                {
                    LoadImport(import);
                }
            }

            // Execute the main logic that generates DTDL interfaces.
            GenerateInterfaces();

            // Execute DTDL parser-based validation of generated interfaces.
            // TODO commented out due to incompatible DotNetRdf in Microsoft.Azure.DigitalTwins.Parser
            // ValidateInterfaces();
        }

        /// <summary>
        /// Checks if a given Ontology Resource is in the ignored names list.
        /// </summary>
        /// <param name="resource">Resource to check</param>
        /// <returns>True iff the resource is ignored</returns>
        private static bool IsIgnored(OntologyResource resource)
        {
            foreach (string ignoredUri in ignoredUris)
            {
                string resourceUri = resource.GetUri().AbsoluteUri;
                if (resourceUri.Contains(ignoredUri))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Main method that traverses the sets of classes in the imported ontology graph and generates DTDL representations.
        /// </summary>
        private static void GenerateInterfaces()
        {
            // Working graph
            Graph dtdlModel = new Graph();

            // A whole bunch of language definitions.
            // TODO Extract all of these (often reused) node definitions into statics.

            // RDF/OWL specs
            IUriNode rdfType = dtdlModel.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

            // DTDL classes
            IUriNode dtdl_Interface = dtdlModel.CreateUriNode(DTDL.Interface);
            IUriNode dtdl_Property = dtdlModel.CreateUriNode(DTDL.Property);
            IUriNode dtdl_Relationship = dtdlModel.CreateUriNode(DTDL.Relationship);
            IUriNode dtdl_Telemetry = dtdlModel.CreateUriNode(DTDL.Telemetry);
            IUriNode dtdl_Component = dtdlModel.CreateUriNode(DTDL.Component);
            IUriNode dtdl_Enum = dtdlModel.CreateUriNode(DTDL.Enum);
            IUriNode dtdl_EnumValue = dtdlModel.CreateUriNode(DTDL.EnumValue);

            // DTDL properties
            IUriNode dtdl_contents = dtdlModel.CreateUriNode(DTDL.contents);
            IUriNode dtdl_name = dtdlModel.CreateUriNode(DTDL.name);
            IUriNode dtdl_displayName = dtdlModel.CreateUriNode(DTDL.displayName);
            IUriNode dtdl_properties = dtdlModel.CreateUriNode(DTDL.properties);

            IUriNode dtdl_extends = dtdlModel.CreateUriNode(DTDL.extends);
            IUriNode dtdl_maxMultiplicity = dtdlModel.CreateUriNode(DTDL.maxMultiplicity);
            IUriNode dtdl_minMultiplicity = dtdlModel.CreateUriNode(DTDL.minMultiplicity);
            IUriNode dtdl_target = dtdlModel.CreateUriNode(DTDL.target);
            IUriNode dtdl_schema = dtdlModel.CreateUriNode(DTDL.schema);
            IUriNode dtdl_valueSchema = dtdlModel.CreateUriNode(DTDL.valueSchema);
            IUriNode dtdl_writable = dtdlModel.CreateUriNode(DTDL.writable);

            IUriNode dtdl_enumValue = dtdlModel.CreateUriNode(DTDL.enumValue);
            IUriNode dtdl_enumValues = dtdlModel.CreateUriNode(DTDL.enumValues);

            // Used to sort JObjects for merged array output, if selected as runtime option
            Dictionary<JObject, int> interfaceDepths = new Dictionary<JObject, int>();

            Console.WriteLine();
            Console.WriteLine("Generating DTDL Interface declarations: ");

            // Start looping through named, non-deprecated, non-ignored classes
            foreach (OntologyClass oClass in _ontologyGraph.OwlClasses.Where(oClass => oClass.IsNamed() && !oClass.IsDeprecated() && !IsIgnored(oClass) && !oClass.SuperClasses.Any(parent => parent.IsNamed() && IsIgnored(parent))))
            {
                // Create Interface
                string interfaceDtmi = GetDTMI(oClass);
                Console.WriteLine($"\t* {interfaceDtmi}");
                IUriNode interfaceNode = dtdlModel.CreateUriNode(UriFactory.Create(interfaceDtmi));
                dtdlModel.Assert(new Triple(interfaceNode, rdfType, dtdl_Interface));

                // If there are rdfs:labels, use them for DTDL displayName
                if (oClass.Label.Any()) {
                    dtdlModel.Assert(GetDtdlDisplayNameTriples(oClass, interfaceNode));
                 }

                // If there are rdfs:comments, generate and assert DTDL description triples from them
                if (oClass.Comment.Any())
                {
                    dtdlModel.Assert(GetDtdlDescriptionTriples(oClass, interfaceNode));
                }

                // If the class has direct superclasses, implement DTDL extends (for at most two, see limitation in DTDL spec)
                IEnumerable<OntologyClass> namedSuperClasses = oClass.DirectSuperClasses.Where(superClass => superClass.IsNamed() && !superClass.IsOwlThing() && !superClass.IsDeprecated());
                if (namedSuperClasses.Any())
                {
                    foreach (OntologyClass superClass in namedSuperClasses.Take(2))
                    {
                        // Only include non-deprecated subclass relations
                        IUriNode rdfsSubClassOf = _ontologyGraph.CreateUriNode(RDFS.subClassOf);
                        if (PropertyAssertionIsDeprecated(oClass.GetUriNode(), rdfsSubClassOf, superClass.GetUriNode()))
                        {
                            continue;
                        }
                        string superInterfaceDTMI = GetDTMI(superClass);
                        IUriNode superInterfaceNode = dtdlModel.CreateUriNode(UriFactory.Create(superInterfaceDTMI));
                        dtdlModel.Assert(new Triple(interfaceNode, dtdl_extends, superInterfaceNode));
                    }
                }

                // For any outgoing object properties from the class to other classes, create corresponding DTDL Relationships
                foreach (Relationship relationship in oClass.GetRelationships().Where(relationship => relationship.Property.IsObjectProperty() && 
                !relationship.Property.IsDeprecated() && 
                !relationship.Target.IsDeprecated() &&
                !IsIgnored(relationship.Target) &&
                !IsIgnored(relationship.Property)))
                {
                    OntologyProperty oProperty = relationship.Property;

                    // Only include relationships with valid names
                    string relationshipName = string.Concat(oProperty.GetLocalName().Take(64));
                    if (!IsCompliantDtdlName(relationshipName))
                    {
                        Console.Error.WriteLine($"Unable to generate Relationship '{relationshipName}' on Interface '{interfaceDtmi}'; underlying property name does not adhere to DTDL regex.");
                        continue;
                    }

                    // If we have seen this relationship linked from a subclass, skip it; DTDL does not allow subinterfaces
                    // to specialise properties/relationships
                    if (PropertyIsDefinedOnChildClass(oClass, oProperty))
                    {
                        continue;
                    }

                    // Define the Relationship and its name
                    IBlankNode relationshipNode = dtdlModel.CreateBlankNode();
                    dtdlModel.Assert(new Triple(interfaceNode, dtdl_contents, relationshipNode));
                    dtdlModel.Assert(new Triple(relationshipNode, rdfType, dtdl_Relationship));
                    ILiteralNode relationshipNameNode = dtdlModel.CreateLiteralNode(relationshipName);
                    dtdlModel.Assert(new Triple(relationshipNode, dtdl_name, relationshipNameNode));

                    // If there are rdfs:labels, use them for DTDL displayName
                    if (oProperty.Label.Any())
                    {
                        dtdlModel.Assert(GetDtdlDisplayNameTriples(oProperty, relationshipNode));
                    }

                    // If there are rdfs:comments, generate and assert DTDL description triples from them
                    if (oProperty.Comment.Any())
                    {
                        dtdlModel.Assert(GetDtdlDescriptionTriples(oProperty, relationshipNode));
                    }

                    // Assert min multiplicity. Hardcoded: per DTDL v2 spec: "For this release, minMultiplicity must always be 0"
                    if (relationship.MinimumCount.HasValue)
                    {   
                        ILiteralNode minMultiplicityNode = dtdlModel.CreateLiteralNode("0", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
                        dtdlModel.Assert(new Triple(relationshipNode, dtdl_minMultiplicity, minMultiplicityNode));
                    }

                    // Assert max multiplicity
                    if (relationship.MaximumCount.HasValue)
                    {
                        ILiteralNode maxMultiplicityNode = dtdlModel.CreateLiteralNode(relationship.MaximumCount.Value.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
                        dtdlModel.Assert(new Triple(relationshipNode, dtdl_maxMultiplicity, maxMultiplicityNode));
                    }

                    // Assert target (if defined)
                    if (!relationship.Target.IsOwlThing()) { 
                        string targetDtmi = GetDTMI(relationship.Target);
                        IUriNode targetInterfaceNode = dtdlModel.CreateUriNode(UriFactory.Create(targetDtmi));
                        dtdlModel.Assert(new Triple(relationshipNode, dtdl_target, targetInterfaceNode));
                    }

                    // Extract annotations on object properties -- these become DTDL Relationship Properties
                    // We only support annotations w/ singleton ranges (though those singletons may be enumerations)
                    IEnumerable<OntologyProperty> annotationsOnRelationship = _ontologyGraph.OwlAnnotationProperties
                        .Where(annotationProp => annotationProp.Ranges.Count() == 1)
                        .Where(annotationProp => annotationProp.Domains.Select(annotationDomain => annotationDomain.Resource).Contains(oProperty.Resource));
                    foreach (OntologyProperty annotationProperty in annotationsOnRelationship) {

                        // Define nested Property and its name
                        IBlankNode nestedPropertyNode = dtdlModel.CreateBlankNode();
                        dtdlModel.Assert(new Triple(nestedPropertyNode, rdfType, dtdl_Property));
                        dtdlModel.Assert(new Triple(relationshipNode, dtdl_properties, nestedPropertyNode));
                        string nestedPropertyName = string.Concat(annotationProperty.GetLocalName().Take(64));
                        ILiteralNode nestedPropertyNameNode = dtdlModel.CreateLiteralNode(nestedPropertyName);
                        dtdlModel.Assert(new Triple(nestedPropertyNode, dtdl_name, nestedPropertyNameNode));

                        // Assert that the nested property is writable
                        ILiteralNode trueNode = dtdlModel.CreateLiteralNode("true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                        dtdlModel.Assert(new Triple(nestedPropertyNode, dtdl_writable, trueNode));

                        // If there are rdfs:labels, use them for DTDL displayName
                        if (annotationProperty.Label.Any())
                        {
                            dtdlModel.Assert(GetDtdlDisplayNameTriples(annotationProperty, nestedPropertyNode));
                        }

                        // If there are rdfs:comments, generate and assert DTDL description triples from them
                        if (oProperty.Comment.Any())
                        {
                            dtdlModel.Assert(GetDtdlDescriptionTriples(annotationProperty, nestedPropertyNode));
                        }

                        // Set range
                        OntologyClass annotationPropertyRange = annotationProperty.Ranges.First();
                        HashSet<Triple> schemaTriples = GetDtdlTriplesForRange(annotationPropertyRange, nestedPropertyNode);
                        dtdlModel.Assert(schemaTriples);
                    }
                }

                // For any outgoing data properties from the class to datatypes, create corresponding DTDL Properties
                foreach (Relationship relationship in oClass.GetRelationships().Where(relationship => relationship.Property.IsDataProperty() && !relationship.Property.IsDeprecated()))
                {
                    OntologyProperty oProperty = relationship.Property;

                    // Only include properties with valid names
                    string propertyName = string.Concat(oProperty.GetLocalName().Take(64));
                    if (!IsCompliantDtdlName(propertyName))
                    {
                        Console.Error.WriteLine($"Unable to generate Property '{propertyName}' on Interface '{interfaceDtmi}'; underlying property name does not adhere to DTDL regex.");
                        continue;
                    }

                    // If we have seen this relationship linked from a subclass, skip it; DTDL does not allow subinterfaces
                    // to specialise properties/relationships
                    if (PropertyIsDefinedOnChildClass(oClass, oProperty))
                    {
                        continue;
                    }

                    // Create Property node and name
                    IBlankNode propertyNode = dtdlModel.CreateBlankNode();
                    dtdlModel.Assert(new Triple(interfaceNode, dtdl_contents, propertyNode));
                    dtdlModel.Assert(new Triple(propertyNode, rdfType, dtdl_Property));
                    ILiteralNode propertyNameNode = dtdlModel.CreateLiteralNode(propertyName);
                    dtdlModel.Assert(new Triple(propertyNode, dtdl_name, propertyNameNode));

                    // Assert that the property is writable
                    ILiteralNode trueNode = dtdlModel.CreateLiteralNode("true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                    dtdlModel.Assert(new Triple(propertyNode, dtdl_writable, trueNode));

                    // Extract and populate schema
                    HashSet<Triple> propertySchemaTriples = GetDtdlTriplesForRange(relationship.Target, propertyNode);
                    dtdlModel.Assert(propertySchemaTriples);

                    // If there are rdfs:labels, use them for DTDL displayName
                    if (oProperty.Label.Any())
                    {
                        dtdlModel.Assert(GetDtdlDisplayNameTriples(oProperty, propertyNode));
                    }

                    // If there are rdfs:comments, generate and assert DTDL description triples from them
                    if (oProperty.Comment.Any())
                    {
                        dtdlModel.Assert(GetDtdlDescriptionTriples(oProperty, propertyNode));
                    }
                }

                // Write JSON-LD to target file.
                JObject modelAsJsonLd = ToJsonLd(dtdlModel);
                interfaceDepths.Add(modelAsJsonLd, oClass.Depth());

                if (!_mergedOutput) {
                    // Create model output directory based on output path and shortest parent path
                    List<string> parentDirectories = oClass.ShortestParentPathToOwlThing();
                    if (oClass.DirectSubClasses.Any(cls => cls.IsNamed() && !cls.IsDeprecated() && !IsIgnored(cls))) {
                        parentDirectories.Add(oClass.GetLocalName());
                    }
                    string modelPath = string.Join("/", parentDirectories);
                    string modelOutputPath = $"{_outputPath}/{modelPath}/";
                    Directory.CreateDirectory(modelOutputPath);
                    string outputFileName = modelOutputPath + oClass.GetLocalName() + ".json";
                    using (StreamWriter file = File.CreateText(outputFileName))
                    using (JsonTextWriter writer = new JsonTextWriter(file) { Formatting = Formatting.Indented })
                    {
                        modelAsJsonLd.WriteTo(writer);
                    }
                }

                // Clear the working graph for next iteration
                dtdlModel.Clear();
            }

            // Sort and generate merged output file
            // TODO remember what I actually did here and document it
            if (_mergedOutput)
            {
                List<KeyValuePair<JObject, int>> interfacesAndDepths = interfaceDepths.ToList();
                interfacesAndDepths.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                IEnumerable<JObject> interfaces = interfacesAndDepths.Select(pair => pair.Key);
                JArray interfaceArray = new JArray(interfaces);
                Directory.CreateDirectory(_outputPath);
                string outputFileName = _outputPath + "FullBuildingModels.json";
                using (StreamWriter file = File.CreateText(outputFileName))
                using (JsonTextWriter writer = new JsonTextWriter(file) { Formatting = Formatting.Indented })
                {
                    interfaceArray.WriteTo(writer);
                }
            }
        }

        // TODO: move this into the DotNetRdfExtensions class
        internal static bool PropertyAssertionIsDeprecated(INode subj, IUriNode pred, INode obj)
        {
            IUriNode owlAnnotatedSource = _ontologyGraph.CreateUriNode(OWL.annotatedSource);
            IUriNode owlAnnotatedProperty = _ontologyGraph.CreateUriNode(OWL.annotatedProperty);
            IUriNode owlAnnotatedTarget = _ontologyGraph.CreateUriNode(OWL.annotatedTarget);
            IUriNode owlDeprecated = _ontologyGraph.CreateUriNode(OWL.deprecated);

            IEnumerable<INode> axiomAnnotations = _ontologyGraph.Nodes
                .Where(node => _ontologyGraph.ContainsTriple(new Triple(node, owlAnnotatedSource, subj)))
                .Where(node => _ontologyGraph.ContainsTriple(new Triple(node, owlAnnotatedProperty, pred)))
                .Where(node => _ontologyGraph.ContainsTriple(new Triple(node, owlAnnotatedTarget, obj)));

            foreach (INode axiomAnnotation in axiomAnnotations)
            {
                foreach (Triple deprecationAssertion in _ontologyGraph.GetTriplesWithSubjectPredicate(axiomAnnotation, owlDeprecated).Where(trip => trip.Object.NodeType == NodeType.Literal))
                {
                    IValuedNode deprecationValue = deprecationAssertion.Object.AsValuedNode();
                    try { 
                        if (deprecationValue.AsBoolean())
                        {
                            return true;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void ValidateInterfaces()
        {
            Console.WriteLine();
            Console.WriteLine("Validating DTDL Interface declarations: ");

            DirectoryInfo dinfo = new DirectoryInfo(_outputPath);
            var files = dinfo.EnumerateFiles($"*.jsonld", SearchOption.AllDirectories);

            if (files.Count() == 0)
            {
                Log.Alert("No matching files found. Exiting.");
                return;
            }
            Dictionary<FileInfo, string> modelDict = new Dictionary<FileInfo, string>();
            int count = 0;
            string lastFile = "<none>";
            try
            {
                foreach (FileInfo fi in files)
                {
                    StreamReader r = new StreamReader(fi.FullName);
                    string dtdl = r.ReadToEnd(); r.Close();
                    modelDict.Add(fi, dtdl);
                    lastFile = fi.FullName;
                    count++;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Could not read files. \nLast file read: {lastFile}\nError: \n{e.Message}");
                return;
            }
            Log.Ok($"Read {count} files from specified directory");
            int errJson = 0;
            foreach (FileInfo fi in modelDict.Keys)
            {
                modelDict.TryGetValue(fi, out string dtdl);
                try
                {
                    JsonDocument.Parse(dtdl);
                }
                catch (Exception e)
                {
                    Log.Error($"Invalid json found in file {fi.FullName}.\nJson parser error \n{e.Message}");
                    errJson++;
                }
            }
            if (errJson > 0)
            {
                Log.Error($"\nFound  {errJson} Json parsing errors");
                return;
            }
            Log.Ok($"Validated JSON for all files - now validating DTDL");
            List<string> modelList = modelDict.Values.ToList<string>();
            ModelParser parser = new ModelParser();
            parser.DtmiResolver = delegate (IReadOnlyCollection<Dtmi> dtmis)
            {
                Log.Error($"*** Error parsing models. Missing:");
                foreach (Dtmi d in dtmis)
                {
                    Log.Error($"  {d}");
                }
                return null;
            };
            try
            {
                IReadOnlyDictionary<Dtmi, DTEntityInfo> om = parser.ParseAsync(modelList).GetAwaiter().GetResult();
                Log.Out("");
                Log.Ok($"**********************************************");
                Log.Ok($"** Validated all files - Your DTDL is valid **");
                Log.Ok($"**********************************************");
                Log.Out($"Found a total of {om.Keys.Count()} entities");
            }
            catch (ParsingException pe)
            {
                Log.Error($"*** Error parsing models");
                int derrcount = 1;
                foreach (ParsingError err in pe.Errors)
                {
                    Log.Error($"Error {derrcount}:");
                    Log.Error($"{err.Message}");
                    Log.Error($"Primary ID: {err.PrimaryID}");
                    Log.Error($"Secondary ID: {err.SecondaryID}");
                    Log.Error($"Property: {err.Property}\n");
                    derrcount++;
                }
                return;
            }
            catch (ResolutionException rex)
            {
                Log.Error("Could not resolve required references");
            }

        }

        /// <summary>
        /// Generates triples representing a DTDL schema for an OWL (data) property range.
        /// </summary>
        /// <param name="owlPropertyRange">The range to translate (typically an XSD datatype or custom datatype)</param>
        /// <param name="dtdlPropertyNode">The node onto which the generated triples will be grafted</param>
        /// <returns>Set of triples representing the schema</returns>
        private static HashSet<Triple> GetDtdlTriplesForRange(OntologyClass owlPropertyRange, INode dtdlPropertyNode)
        {

            // TODO: ensure that owlPropertyRange is named!

            IGraph dtdlModel = dtdlPropertyNode.Graph;
            IUriNode dtdl_schema = dtdlModel.CreateUriNode(DTDL.schema);
            IUriNode rdfType = dtdlModel.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            IUriNode dtdl_Enum = dtdlModel.CreateUriNode(DTDL.Enum);
            IUriNode dtdl_valueSchema = dtdlModel.CreateUriNode(DTDL.valueSchema);
            IUriNode dtdl_enumValues = dtdlModel.CreateUriNode(DTDL.enumValues);
            IUriNode dtdl_name = dtdlModel.CreateUriNode(VocabularyHelper.DTDL.name);
            IUriNode dtdl_displayName = dtdlModel.CreateUriNode(VocabularyHelper.DTDL.displayName);
            IUriNode dtdl_enumValue = dtdlModel.CreateUriNode(DTDL.enumValue);
            IUriNode dtdl_comment = dtdlModel.CreateUriNode(DTDL.comment);
            IUriNode dtdl_string = dtdlModel.CreateUriNode(DTDL._string);
            IUriNode dtdl_unit = dtdlModel.CreateUriNode(DTDL.unit);

            HashSet<Triple> returnedTriples = new HashSet<Triple>();

            // First check for the simple XSD datatypes
            if (owlPropertyRange.IsXsdDatatype())
            {
                Uri schemaUri = GetXsDatatypeAsDtdlEquivalent(owlPropertyRange);
                IUriNode schemaNode = dtdlModel.CreateUriNode(schemaUri);
                returnedTriples.Add(new Triple(dtdlPropertyNode, dtdl_schema, schemaNode));
                return returnedTriples;
            }

            // Then check for supported custom-defined datatypes
            if (owlPropertyRange.IsDatatype() && !owlPropertyRange.IsBuiltIn())
            {
                // This is an enumeration of allowed values
                if (owlPropertyRange.IsEnumerationDatatype())
                {
                    IBlankNode enumNode = dtdlModel.CreateBlankNode();
                    returnedTriples.Add(new Triple(enumNode, rdfType, dtdl_Enum));
                    returnedTriples.Add(new Triple(dtdlPropertyNode, dtdl_schema, enumNode));
                    returnedTriples.Add(new Triple(enumNode, dtdl_valueSchema, dtdl_string));
                    IEnumerable<ILiteralNode> enumOptions = owlPropertyRange.AsEnumeration().LiteralNodes();
                    foreach (ILiteralNode option in enumOptions)
                    {
                        IBlankNode enumOption = dtdlModel.CreateBlankNode();
                        returnedTriples.Add(new Triple(enumOption, dtdl_name, dtdlModel.CreateLiteralNode(option.Value)));
                        returnedTriples.Add(new Triple(enumOption, dtdl_enumValue, dtdlModel.CreateLiteralNode(option.Value)));
                        returnedTriples.Add(new Triple(enumNode, dtdl_enumValues, enumOption));
                    }
                    return returnedTriples;
                }

                // This is a wrapper around a XSD standard datatype
                if (owlPropertyRange.IsSimpleXsdWrapper())
                {
                    Uri schemaUri = GetXsDatatypeAsDtdlEquivalent(owlPropertyRange.EquivalentClasses.First());
                    IUriNode schemaNode = dtdlModel.CreateUriNode(schemaUri);
                    returnedTriples.Add(new Triple(dtdlPropertyNode, dtdl_schema, schemaNode));

                    // If the wrapper is a punned QUDT unit individual, assign semantic type and unit
                    if (owlPropertyRange.IsQudtUnit() && semanticTypesMap.ContainsKey(owlPropertyRange.GetUri()))
                    {
                        Uri qudtUnit = owlPropertyRange.GetUri();
                        IUriNode hasQuantityKind = dtdlModel.CreateUriNode(QUDT.hasQuantityKind);
                        IEnumerable<IUriNode> quantityKinds = owlPropertyRange.GetNodesViaPredicate(hasQuantityKind).UriNodes();
                        if (quantityKinds.Count() == 1 && semanticTypesMap.ContainsKey(quantityKinds.First().Uri))
                        {
                            Uri qudtQuantityKind = quantityKinds.First().Uri;
                            IUriNode dtdlSemanticType = dtdlModel.CreateUriNode(semanticTypesMap[qudtQuantityKind]);
                            IUriNode dtdlUnit = dtdlModel.CreateUriNode(semanticTypesMap[qudtUnit]);
                            returnedTriples.Add(new Triple(dtdlPropertyNode, rdfType, dtdlSemanticType));
                            returnedTriples.Add(new Triple(dtdlPropertyNode, dtdl_unit, dtdlUnit));
                        }
                    }

                    // Assert comment from XSD datatype on parent DTDL property (prioritizing non-tagged, then english, then anything else)
                    if (owlPropertyRange.Comment.Any())
                    {
                        IEnumerable<ILiteralNode> englishOrNontaggedComments = owlPropertyRange.Comment
                            .Where(node => node.Language == string.Empty || node.Language.StartsWith("en"))
                            .OrderBy(node => node.Language);
                        ILiteralNode comment;
                        if (englishOrNontaggedComments.Any())
                        {
                            comment = englishOrNontaggedComments.First();
                        }
                        else
                        {
                            comment = owlPropertyRange.Comment.First();
                        }
                        ILiteralNode dtdlCommentNode = dtdlModel.CreateLiteralNode(string.Concat(comment.Value.Take(512)));
                        Triple dtdlCommentTriple = new Triple(dtdlPropertyNode, dtdl_comment, dtdlCommentNode);
                        returnedTriples.Add(dtdlCommentTriple);
                    }
                    return returnedTriples;
                }
            }

            // No supported schemas found; fall back to simple string schema
            IUriNode stringSchemaNode = dtdlModel.CreateUriNode(DTDL._string);
            returnedTriples.Add(new Triple(dtdlPropertyNode, dtdl_schema, stringSchemaNode));
            return returnedTriples;
        }

        /// <summary>
        /// Translate an XSD datatype into a DTDL URI
        /// </summary>
        /// <param name="xsdDatatype">XSD datatype to translate</param>
        /// <returns>DTDL-equivalent URI</returns>
        private static Uri GetXsDatatypeAsDtdlEquivalent(OntologyClass xsdDatatype)
        {
            Dictionary<string, Uri> xsdDtdlPrimitiveTypesMappings = new Dictionary<string, Uri>
                {
                    {"boolean", DTDL._boolean },
                    {"byte", DTDL._integer },
                    {"dateTime", DTDL._dateTime },
                    {"dateTimeStamp", DTDL._dateTime },
                    {"double", DTDL._double },
                    {"float", DTDL._float },
                    {"int", DTDL._integer },
                    {"integer", DTDL._integer },
                    {"long", DTDL._long },
                    {"string",DTDL._string }
                };

            if (xsdDtdlPrimitiveTypesMappings.ContainsKey(xsdDatatype.GetLocalName()))
            {
                return xsdDtdlPrimitiveTypesMappings[xsdDatatype.GetLocalName()];
                
            }

            // Fall-back option
            return DTDL._string;
        }

        /// <summary>
        /// Checks whether a certain property declaration on a superclass is also defined on any of its subclasses.
        /// This is necessary since DTDL does not allow sub-interfaces to extend properties from their super-interfaces.
        /// TODO This is _horribly_ inefficient and is run all the time. Refactor!
        /// </summary>
        /// <param name="oClass">Superclass that holds the property being checked</param>
        /// <param name="checkedForProperty">The property to check for</param>
        /// <returns>True iff this property is not defined on any subclass</returns>
        private static bool PropertyIsDefinedOnChildClass(OntologyClass oClass, OntologyProperty checkedForProperty)
        {
            foreach (OntologyClass subClass in oClass.SubClasses)
            {
                if (subClass.GetRelationships()
                    .Select(relationship => relationship.Property)
                    .Any(property => property.GetUri().AbsoluteUri.Equals(checkedForProperty.GetUri().AbsoluteUri)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Validate that a given name is compliant with the DTDL spec.
        /// </summary>
        /// <param name="inputName"></param>
        /// <returns></returns>
        private static bool IsCompliantDtdlName(string inputName) 
        {
            if (inputName.Length > 64)
            {
                return false;
            }
            Regex regex = new Regex("^[a-zA-Z](?:[a-zA-Z0-9_]*[a-zA-Z0-9])?$");

            Match match = regex.Match(inputName);
            if (match.Success)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Translates rdfs:label definitions in multiple languages to DTDL display names, returning the triples representing these
        /// displayNames that need to be grafted onto the model.
        /// </summary>
        /// <param name="resource">The resource for which to check for labels</param>
        /// <param name="subjectNode">The target DTDL node onto which the displayNames will be added</param>
        /// <returns>Triples representing the display names</returns>
        private static IEnumerable<Triple> GetDtdlDisplayNameTriples(OntologyResource resource, INode subjectNode)
        {
            IGraph dtdlModel = subjectNode.Graph;
            IEnumerable<ILiteralNode> labels = resource.Label;
            IUriNode dtdl_displayName = dtdlModel.CreateUriNode(DTDL.displayName);

            IEnumerable<ILiteralNode> nonLocalizedLabels = labels.Where(node => node.Language == string.Empty);
            if (nonLocalizedLabels.Any())
            {
                ILiteralNode labelNode = dtdlModel.CreateLiteralNode(string.Concat(nonLocalizedLabels.First().Value.Take(64)), "en");
                Triple retVal = new Triple(subjectNode, dtdl_displayName, labelNode);
                return new List<Triple> { retVal };
            }
            else
            {
                List<Triple> triples = new List<Triple>();
                foreach (ILiteralNode label in labels)
                {
                    ILiteralNode labelNode = dtdlModel.CreateLiteralNode(string.Concat(label.Value.Take(64)), label.Language);
                    triples.Add(new Triple(subjectNode, dtdl_displayName, labelNode));
                }
                return triples;
            }
        }

        /// <summary>
        /// Translates rdfs:comment definitions in multiple languages to DTDL descriptions, returning the triples representing these
        /// descriptions that need to be grafted onto the model.
        /// </summary>
        /// <param name="resource">The resource for which to check for comments</param>
        /// <param name="subjectNode">The target DTDL node onto which the descriptions will be added</param>
        /// <returns>Triples representing the descriptions</returns>
        private static IEnumerable<Triple> GetDtdlDescriptionTriples(OntologyResource resource, INode subjectNode)
        {
            IGraph dtdlModel = subjectNode.Graph;
            IEnumerable<ILiteralNode> comments = resource.Comment;
            IUriNode dtdl_description = dtdlModel.CreateUriNode(DTDL.description);

            string inverseDescription = "";
            if (resource is OntologyProperty)
            {
                OntologyProperty resourceAsProperty = (OntologyProperty)resource;
                IEnumerable<OntologyProperty> inverses = resourceAsProperty.InverseProperties;
                if (inverses.Any())
                {
                    inverseDescription += $" Inverse of: {string.Join(", ", inverses.Select(property => property.GetLocalName()))}";
                }
            }

            int lengthOfInverseDescription = inverseDescription.Length;
            int lengthOfCommentToKeep = 512 - lengthOfInverseDescription;

            IEnumerable<ILiteralNode> nonLocalizedComments = comments.Where(node => node.Language == string.Empty);
            if (nonLocalizedComments.Any())
            {
                ILiteralNode commentNode = dtdlModel.CreateLiteralNode(string.Concat(nonLocalizedComments.First().Value.Take(lengthOfCommentToKeep)) + inverseDescription, "en");
                Triple retVal = new Triple(subjectNode, dtdl_description, commentNode);
                return new List<Triple> { retVal };
            }
            else
            {
                List<Triple> triples = new List<Triple>();
                foreach (ILiteralNode comment in comments)
                {
                    ILiteralNode commentNode = dtdlModel.CreateLiteralNode(string.Concat(comment.Value.Take(lengthOfCommentToKeep)) + inverseDescription, comment.Language);
                    triples.Add(new Triple(subjectNode, dtdl_description, commentNode));
                }
                return triples;
            }
        }

        /// <summary>
        /// Generate Digital Twin Model Identifiers; these will be based on reverse dns + path.
        /// </summary>
        /// <param name="resource">Resource to generate DTMI for</param>
        /// <returns>DTMI</returns>
        private static string GetDTMI(OntologyResource resource)
        {
            if (!resource.IsNamed())
            {
                throw new RdfException($"Could not generate DTMI for OntologyResource '{resource}'; resource is anonymous.");
            }

            Uri resourceNamespace = resource.GetNamespace();

            // Ensure that the resource is in the namespace mapper. Why do we do this again?
            if (!namespacePrefixes.ContainsKey(resourceNamespace))
            {
                char[] trimChars = { '#', '/' };
                string namespaceShortName = resourceNamespace.AbsoluteUri.Trim(trimChars).Split('/').Last();
                namespacePrefixes[resourceNamespace] = namespaceShortName;
            }

            string[] hostComponents = resourceNamespace.Host.Split('.');
            Array.Reverse(hostComponents);
            string hostComponentsBlock = string.Join(':', hostComponents);
            string pathComponentBlock = resourceNamespace.AbsolutePath.Replace('/', ':').Trim(':');
            string dtmiPrefix = $"{hostComponentsBlock}:{pathComponentBlock}";
            string dtmi = $"{dtmiPrefix}:{resource.GetLocalName()}";

            // Run the candidate DTMI through validation per the spec, removing non-conforming chars
            string[] pathSegments = dtmi.Split(':');
            for (int i = 0; i < pathSegments.Length; i++)
            {
                string pathSegment = pathSegments[i];
                pathSegment = new string((from c in pathSegment
                                          where char.IsLetterOrDigit(c) || c.Equals('_')
                                          select c
                                          ).ToArray());
                pathSegment = pathSegment.TrimEnd('_');
                pathSegment = pathSegment.TrimStart(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'});
                pathSegments[i] = pathSegment;
            }
            dtmi = string.Join(':', pathSegments);

            return $"dtmi:{dtmi};1";
        }

        /// <summary>
        /// Do JSON-LD framing and compacting of a model (i.e., a DTDL Interface) using the DTDL context file.
        /// </summary>
        /// <param name="dtdlModel">DTDL model to frame/compact, as DotNetRDF graph.</param>
        /// <returns>JSON-LD representation of input Interface</returns>
        private static JObject ToJsonLd(Graph dtdlModel)
        {
            JArray initialJsonLd;
            using (TripleStore entitiesStore = new TripleStore())
            {
                entitiesStore.Add(dtdlModel);
                JsonLdWriterOptions writerOptions = new JsonLdWriterOptions();
                writerOptions.UseNativeTypes = true;
                JsonLdWriter jsonLdWriter = new JsonLdWriter(writerOptions);
                initialJsonLd = jsonLdWriter.SerializeStore(entitiesStore);
            }

            // Configure and run JSON-LD framing and compacting
            JsonLdProcessorOptions options = new JsonLdProcessorOptions();
            options.UseNativeTypes = true;
            options.Base = new Uri("https://example.org"); // Throwaway base, not actually used

            JObject frame = new JObject(
                new JProperty("@type", DTDL.Interface.AbsoluteUri)
                );

            JObject context;
            using (StreamReader file = File.OpenText(@"DTDL.v2.context.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                context = (JObject)JToken.ReadFrom(reader);
            }

            JObject framedJson = JsonLdProcessor.Frame(initialJsonLd, frame, options);
            JObject compactedJson = JsonLdProcessor.Compact(framedJson, context, options);

            compactedJson["@context"] = new JValue(DTDL.dtdlContext);

            return compactedJson;
        }

        /// <summary>
        /// Loads imported ontologies transitively. Each imported ontology is added
        /// to the static set <c>importedOntologies</c>.
        /// </summary>
        /// <param name="importedOntology">The ontology to import.</param>
        private static void LoadImport(Ontology importedOntology)
        {
            // We only deal with named ontologies
            if (importedOntology.IsNamed())
            {

                // Parse and load ontology from the stated import URI
                Uri importUri = importedOntology.GetUri();

                // Only proceed if we have not seen this fetched URI before, otherwise we risk 
                // unecessary fetches and computation, and possibly import loops.
                if (!importedOntologyUris.Contains(importUri))
                {
                    importedOntologyUris.Add(importUri);

                    //Uri importedOntologyUri = ((IUriNode)importedOntology.Resource).Uri;
                    OntologyGraph fetchedOntologyGraph = new OntologyGraph();

                    try
                    {
                        Console.WriteLine($"Loading {importUri}.");
                        UriLoader.Load(fetchedOntologyGraph, importUri);
                    }
                    catch (RdfParseException e)
                    {
                        Console.Write(e.Message);
                        Console.Write(e.StackTrace);
                    }

                    // Set up a new ontology metadata object from the retrieved ontology graph.
                    // This is needed since this ontology's self-defined IRI or version IRI often 
                    // differs from the IRI through which it was imported (i.e., importedOntology in 
                    // this method's signature), due to .htaccess redirects, version URIs, etc.
                    Ontology importedOntologyFromFetchedGraph = fetchedOntologyGraph.GetOntology();

                    // Only proceed if the retrieved ontology has an IRI
                    if (importedOntologyFromFetchedGraph.IsNamed())
                    {
                        // Add the fetched ontology to the namespace prefix index
                        // (tacking on _1, _2, etc. to the shortname if it exists since before, 
                        // since we need all prefix names to be unique).
                        string importedOntologyShortname = importedOntologyFromFetchedGraph.GetShortName();
                        int i = 1;
                        while (namespacePrefixes.ContainsValue(importedOntologyShortname))
                        {
                            importedOntologyShortname = importedOntologyShortname.Split('_')[0] + "_" + i;
                            i++;
                        }
                        namespacePrefixes.Add(importedOntologyFromFetchedGraph.GetUri(), importedOntologyShortname);

                        // Merge the fetch graph with the joint ontology graph the tool operates on
                        _ontologyGraph.Merge(fetchedOntologyGraph);

                        // Traverse the imported ontology's import hierarchy transitively
                        foreach (Ontology subImport in importedOntologyFromFetchedGraph.Imports)
                        {
                            LoadImport(subImport);
                        }
                    }

                    // Dispose graph before returning
                    fetchedOntologyGraph.Dispose();
                }
            }
        }
    }
}
