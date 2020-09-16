using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;

namespace OWL2DTDL
{
    /// <summary>
    /// Various extensions to DotNetRdf, particularly relating to the <c>VDS.RDF.Ontology</c> functionality.
    /// </summary>
    public static class DotNetRdfExtensions
    {

        // Used in string handling etc
        private static readonly CultureInfo invariantCulture = CultureInfo.InvariantCulture;

        #region Shared
        /// <summary>
        /// Custom comparer for OntologyResource objects, that simply
        /// defers to comparison of nested INodes.
        /// </summary>
        class OntologyResourceComparer : IEqualityComparer<OntologyResource>
        {
            public bool Equals(OntologyResource x, OntologyResource y)
            {
                return x.Resource.Equals(y.Resource);
            }

            public int GetHashCode(OntologyResource obj)
            {
                return obj.Resource.GetHashCode();
            }
        }
        #endregion

        #region INode/ILiteralNode/IUriNode extensions
        public static bool IsLiteral(this INode node)
        {
            return node.NodeType.Equals(NodeType.Literal);
        }

        public static bool IsUri(this INode node)
        {
            return node.NodeType.Equals(NodeType.Uri);
        }

        public static IUriNode AsUriNode(this INode node)
        {
            if (!node.IsUri())
            {
                throw new RdfException($"Node {node} is not an URI node.");
            }
            return node as IUriNode;
        }

        public static string GetLocalName(this IUriNode node)
        {
            if (node.Uri.Fragment.Length > 0)
            {
                return node.Uri.Fragment.Trim('#');
            }
            return Path.GetFileName(node.Uri.AbsolutePath);
        }

        // TODO This should probably be fixed to handle URN namespaces properly..
        public static Uri GetNamespace(this IUriNode node)
        {
            if (node.Uri.Fragment.Length > 0)
            {
                return new Uri(node.Uri.GetLeftPart(UriPartial.Path) + "#");
            }
            string nodeUriPath = node.Uri.GetLeftPart(UriPartial.Path);
            if (nodeUriPath.Count(x => x == '/') >= 3)
            {
                string nodeUriBase = nodeUriPath.Substring(0, nodeUriPath.LastIndexOf("/", StringComparison.Ordinal) + 1);
                return new Uri(nodeUriBase);
            }
            throw new UriFormatException($"The Uri {node.Uri} doesn't contain a namespace/local name separator.");
        }

        public static bool IsInteger(this ILiteralNode node)
        {
            Uri dataTypeUri = node.DataType;
            if (dataTypeUri == null)
            {
                return false;
            }
            string datatype = dataTypeUri.AbsoluteUri;
            return datatype.StartsWith(XmlSpecsHelper.NamespaceXmlSchema, StringComparison.Ordinal)
                && (datatype.EndsWith("Integer", StringComparison.Ordinal) || datatype.EndsWith("Int", StringComparison.Ordinal));
        }
        #endregion

        #region OntologyResource extensions
        public static bool IsNamed(this OntologyResource ontResource)
        {
            return ontResource.Resource.IsUri();
        }

        public static bool IsBuiltIn(this OntologyResource ontologyResource)
        {
            if (!ontologyResource.IsNamed())
            {
                return false;
            }

            HashSet<string> builtIns = new HashSet<string>() {
                "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
                "http://www.w3.org/2000/01/rdf-schema#",
                "http://www.w3.org/2002/07/owl#",
                "http://www.w3.org/2001/XMLSchema#"
            };
            return builtIns.Any(builtin => ontologyResource.GetUri().AbsoluteUri.Contains(builtin));
        }

        public static IEnumerable<INode> GetNodesViaPredicate(this OntologyResource resource, INode predicate)
        {
            return resource.Graph.GetTriplesWithSubjectPredicate(resource.Resource, predicate).Select(triple => triple.Object);
        }

        public static IUriNode GetUriNode(this OntologyResource ontResource)
        {
            if (!ontResource.IsNamed())
            {
                throw new RdfException($"Ontology resource {ontResource} does not have an IRI.");
            }
            return ontResource.Resource.AsUriNode();
        }

        public static Uri GetUri(this OntologyResource ontResource)
        {
            return ontResource.GetUriNode().Uri;
        }

        public static Uri GetNamespace(this OntologyResource ontResource)
        {
            return ontResource.GetUriNode().GetNamespace();
        }

        public static string GetLocalName(this OntologyResource ontResource)
        {
            return ontResource.GetUriNode().GetLocalName();
        }

        public static bool IsDeprecated(this OntologyResource resource)
        {
            IUriNode deprecated = resource.Graph.CreateUriNode(VocabularyHelper.OWL.deprecated);
            return resource.GetNodesViaPredicate(deprecated).LiteralNodes().Any(node => node.Value == "true");
        }
        #endregion

        #region OntologyClass extensions
        public static int Depth(this OntologyClass oClass)
        {
            int largestParentDepth = 0;
            foreach (OntologyClass superClass in oClass.DirectSuperClasses)
            {
                int superClassDepth = superClass.Depth();
                if (superClassDepth > largestParentDepth)
                {
                    largestParentDepth = superClassDepth;
                }
            }
            return largestParentDepth + 1;
        }

        public static bool IsRestriction(this OntologyClass oClass)
        {
            return oClass.Types.UriNodes().Any(classType => classType.Uri.ToString().Equals(VocabularyHelper.OWL.Restriction.ToString()));
        }

        public static bool IsDatatype(this OntologyClass oClass)
        {
            return oClass.IsXsdDatatype() || oClass.Types.UriNodes().Any(classType => classType.Uri.AbsoluteUri.Equals(VocabularyHelper.RDFS.Datatype.AbsoluteUri));
        }

        public static bool IsEnumerationDatatype(this OntologyClass oClass)
        {
            INode oneOf = oClass.Graph.CreateUriNode(VocabularyHelper.OWL.oneOf);
            if (oClass.IsDatatype()) {
                if (oClass.EquivalentClasses.Count() == 1) {
                    return oClass.EquivalentClasses.Single().GetNodesViaPredicate(oneOf).Count() == 1;
                }
                else
                {
                    return oClass.GetNodesViaPredicate(oneOf).Count() == 1;
                }
            }

            return false;
        }

        public static bool IsSimpleXsdWrapper(this OntologyClass oClass)
        {
            if (oClass.IsDatatype() && oClass.EquivalentClasses.Count() == 1)
            {
                return oClass.EquivalentClasses.Single().IsXsdDatatype();
            }
            return false;
        }
        
        public static bool IsQudtUnit(this OntologyClass oClass)
        {
            return oClass.Types.UriNodes().Any(t => t.Uri.AbsoluteUri.Equals(VocabularyHelper.QUDT.Unit.AbsoluteUri));
        }

        public static IEnumerable<INode> AsEnumeration(this OntologyClass oClass)
        {
            INode oneOf = oClass.Graph.CreateUriNode(VocabularyHelper.OWL.oneOf);
            INode list = oClass.EquivalentClasses.Append(oClass).SelectMany(equiv => equiv.GetNodesViaPredicate(oneOf)).First();
            return oClass.Graph.GetListItems(list);
        }

        public static bool IsOwlThing(this OntologyClass oClass)
        {
            return oClass.IsNamed() && oClass.GetUri().AbsoluteUri.Equals(VocabularyHelper.OWL.Thing.AbsoluteUri);
        }

        public static bool IsXsdDatatype(this OntologyClass oClass)
        {
            if (oClass.IsNamed())
            {
                return oClass.GetUri().AbsoluteUri.StartsWith(XmlSpecsHelper.NamespaceXmlSchema, StringComparison.Ordinal);
            }
            return false;
        }

        public static IEnumerable<Relationship> GetRelationships(this OntologyClass cls)
        {
            List<Relationship> relationships = new List<Relationship>();

            // Start w/ rdfs:domain declarations. At this time we only consider no-range (i.e.,
            // range is owl:Thing) or named singleton ranges
            IEnumerable<OntologyProperty> rdfsDomainProperties = cls.IsDomainOf.Where(
                property =>
                    property.Ranges.Count() == 0 ||
                    (property.Ranges.Count() == 1 && (property.Ranges.First().IsNamed() || property.Ranges.First().IsDatatype())));
            foreach (OntologyProperty property in rdfsDomainProperties)
            {
                Relationship newRelationship;
                if (property.Ranges.Count() == 0)
                {
                    OntologyGraph oGraph = cls.Graph as OntologyGraph;
                    OntologyClass target;
                    if (property.IsObjectProperty())
                    {
                        target = oGraph.CreateOntologyClass(VocabularyHelper.OWL.Thing);
                    }
                    else
                    {
                        target = oGraph.CreateOntologyClass(VocabularyHelper.RDFS.Literal);
                    }
                    newRelationship = new Relationship(property, target);
                }
                else
                {
                    OntologyClass range = property.Ranges.First();
                    newRelationship = new Relationship(property, range);
                }

                if (property.IsFunctional())
                {
                    newRelationship.ExactCount = 1;
                }

                relationships.Add(newRelationship);
            }

            // Continue w/ OWL restrictions on the class
            IEnumerable<OntologyRestriction> ontologyRestrictions = cls.DirectSuperClasses
                .Where(superClass => superClass.IsRestriction())
                .Select(superClass => new OntologyRestriction(superClass));
            foreach (OntologyRestriction ontologyRestriction in ontologyRestrictions)
            {
                OntologyProperty restrictionProperty = ontologyRestriction.OnProperty;
                OntologyClass restrictionClass = ontologyRestriction.OnClass;
                if (restrictionProperty.IsNamed() && (restrictionClass.IsNamed() || restrictionClass.IsDatatype()))
                {
                    Relationship newRelationship = new Relationship(restrictionProperty, restrictionClass);

                    int min = ontologyRestriction.MinimumCardinality;
                    int exactly = ontologyRestriction.ExactCardinality;
                    int max = ontologyRestriction.MaximumCardinality;

                    if (min != 0)
                        newRelationship.MinimumCount = min;
                    if (exactly != 0)
                        newRelationship.ExactCount = exactly;
                    if (max != 0)
                        newRelationship.MaximumCount = max;

                    relationships.Add(newRelationship);
                }
            }

            // Iterate over the gathered list of Relationships and narrow down to the most specific ones, using a Dictionary for lookup and the MergeWith() method for in-place narrowing
            Dictionary<OntologyProperty, Relationship> relationshipsDict = new Dictionary<OntologyProperty, Relationship>(new OntologyResourceComparer());
            foreach (Relationship relationship in relationships)
            {
                OntologyProperty property = relationship.Property;
                OntologyResource target = relationship.Target;
                // If we already have this property listed in the dictionary, first narrow down the relationship by combining it with the old copy
                if (relationshipsDict.ContainsKey(property))
                {
                    Relationship oldRelationship = relationshipsDict[property];
                    relationship.MergeWith(oldRelationship);
                }
                // Put relationship in the dictionary
                relationshipsDict[property] = relationship;
            }

            // Return the values
            return relationshipsDict.Values;
        }

        public static IEnumerable<OntologyClass> SuperClassesWithOwlThing(this OntologyClass cls)
        {
            IGraph graph = cls.Graph;
            IUriNode owlThing = graph.CreateUriNode(VocabularyHelper.OWL.Thing);
            OntologyClass owlThingClass = new OntologyClass(owlThing, graph);
            return cls.SuperClasses.Append(owlThingClass);
        }
        #endregion

        #region OntologyProperty extensions
        public static bool IsFunctional(this OntologyProperty property)
        {
            // Note the toString-based comparison; because .NET Uri class does not differentiate by Uri fragment!
            return property.Types.UriNodes().Any(propertyType => propertyType.Uri.ToString().Equals(VocabularyHelper.OWL.FunctionalProperty.ToString()));
        }

        public static bool IsObjectProperty(this OntologyProperty property)
        {
            return property.Types.UriNodes().Any(propertyType => propertyType.Uri.ToString().Equals(OntologyHelper.OwlObjectProperty, StringComparison.Ordinal));
        }

        public static bool IsDataProperty(this OntologyProperty property)
        {
            return property.Types.UriNodes().Any(propertyType => propertyType.Uri.ToString().Equals(OntologyHelper.OwlDatatypeProperty, StringComparison.Ordinal));
        }

        public static bool IsAnnotationProperty(this OntologyProperty property)
        {
            return property.Types.UriNodes().Any(propertyType => propertyType.Uri.ToString().Equals(OntologyHelper.OwlAnnotationProperty, StringComparison.Ordinal));
        }
        #endregion

        #region OntologyGraph extensions
        public static Ontology GetOntology(this OntologyGraph graph)
        {
            IUriNode rdfType = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            IUriNode owlOntology = graph.CreateUriNode(new Uri(OntologyHelper.OwlOntology));
            IEnumerable<IUriNode> ontologyNodes = graph.GetTriplesWithPredicateObject(rdfType, owlOntology)
                .Select(triple => triple.Subject)
                .UriNodes();

            switch (ontologyNodes.Count())
            {
                case 0:
                    throw new RdfException($"The graph {graph} doesn't contain any owl:Ontology declarations.");
                case 1:
                    return new Ontology(ontologyNodes.Single(), graph);
                default:
                    IUriNode ontologyNode = ontologyNodes.Where(node => node.Uri.AbsoluteUri.Equals(graph.BaseUri.AbsoluteUri)).DefaultIfEmpty(ontologyNodes.First()).First();
                    return new Ontology(ontologyNode, graph);
            }
        }

        public static IEnumerable<Individual> GetIndividuals(this OntologyGraph graph, OntologyClass ontologyClass)
        {
            IUriNode classNode = ontologyClass.GetUriNode();
            IUriNode rdfType = graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            return graph.GetTriplesWithPredicateObject(rdfType, classNode)
                .Where(triple => triple.Subject.IsUri())
                .Select(triple => new Individual(triple.Subject, graph));
        }

        public static IEnumerable<OntologyClass> GetDatatypes(this OntologyGraph graph)
        {
            INode rdfsDatatype = graph.CreateUriNode(VocabularyHelper.RDFS.Datatype);
            return graph.GetClasses(rdfsDatatype);
        }
        #endregion

        #region Ontology extensions
        public static bool HasVersionUri(this Ontology ontology)
        {
            IUriNode versionIri = ontology.Graph.CreateUriNode(VocabularyHelper.OWL.versionIRI);
            return ontology.GetNodesViaPredicate(versionIri).UriNodes().Any();
        }

        public static Uri GetVersionUri(this Ontology ontology)
        {
            if (!ontology.HasVersionUri())
            {
                throw new RdfException($"Ontology {ontology} does not have an owl:versionIRI annotation");
            }

            IUriNode versionIri = ontology.Graph.CreateUriNode(VocabularyHelper.OWL.versionIRI);
            return ontology.GetNodesViaPredicate(versionIri).UriNodes().First().Uri;
        }

        /// <summary>
        /// Gets a short name representation for an ontology, based on the last segment
        /// of the ontology IRI or (in the case of anonymous ontologies) the ontology hash.
        /// Useful for qname prefixes.
        /// </summary>
        /// <param name="ontology"></param>
        /// <returns></returns>
        public static string GetShortName(this Ontology ontology)
        {
            // Fallback way of getting a persistent short identifier in the
            // (unlikely?) case that we are dealing w/ an anonymous ontology
            if (!ontology.IsNamed())
            {
                return ontology.GetHashCode().ToString(invariantCulture);
            }

            // This is a simple string handling thing
            string ontologyUriString = ontology.GetUri().AbsoluteUri;

            // Trim any occurences of entity separation characters
            if (ontologyUriString.EndsWith("/", StringComparison.Ordinal) || ontologyUriString.EndsWith("#", StringComparison.Ordinal))
            {
                char[] trimChars = { '/', '#' };
                ontologyUriString = ontologyUriString.Trim(trimChars);
            }

            // Get the last bit of the string, after the last slash
            ontologyUriString = ontologyUriString.Substring(ontologyUriString.LastIndexOf('/') + 1);

            // If the string contains dots, treat them as file ending delimiter and get rid of them
            // one at a time
            while (ontologyUriString.Contains('.', StringComparison.Ordinal))
            {
                ontologyUriString = ontologyUriString.Substring(0, ontologyUriString.LastIndexOf('.'));
            }

            return ontologyUriString;
        }
        #endregion
    }
}
