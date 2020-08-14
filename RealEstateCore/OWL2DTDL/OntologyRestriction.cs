using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Ontology;

namespace OWL2DTDL
{
    /// <summary>
    /// (Partial) representation of an owl:Restriction.
    /// </summary>
    public class OntologyRestriction
    {
        public OntologyProperty OnProperty
        {
            get
            {
                IUriNode onProperty = _graph.CreateUriNode(VocabularyHelper.OWL.onProperty);
                IEnumerable<IUriNode> properties = _wrappedClass.GetNodesViaPredicate(onProperty).UriNodes();
                if (properties.Count() != 1)
                {
                    throw new RdfException("A restriction must be on exactly one property.");
                }
                return _graph.CreateOntologyProperty(properties.First());
            }
        }

        public OntologyClass OnClass
        {
            get
            {
                IUriNode onClass = _graph.CreateUriNode(VocabularyHelper.OWL.onClass);
                IUriNode allValuesFrom = _graph.CreateUriNode(VocabularyHelper.OWL.allValuesFrom);
                IUriNode someValuesFrom = _graph.CreateUriNode(VocabularyHelper.OWL.someValuesFrom);
                IEnumerable<INode> classes = _wrappedClass.GetNodesViaPredicate(onClass)
                    .Union(_wrappedClass.GetNodesViaPredicate(someValuesFrom))
                    .Union(_wrappedClass.GetNodesViaPredicate(allValuesFrom));
                if (classes.Count() == 0)
                {
                    if (OnProperty.IsObjectProperty())
                    {
                        return _graph.CreateOntologyClass(VocabularyHelper.OWL.Thing);
                    }
                    else
                    {
                        return _graph.CreateOntologyClass(VocabularyHelper.RDFS.Literal);
                    }
                }
                else if (classes.Count() == 1)
                {
                    return _graph.CreateOntologyClass(classes.First());
                }
                throw new RdfException("A restriction must be on at most one class.");
            }
        }

        public int MinimumCardinality
        {
            get
            {
                IUriNode someValuesFrom = _graph.CreateUriNode(VocabularyHelper.OWL.someValuesFrom);
                IUriNode minCardinality = _graph.CreateUriNode(VocabularyHelper.OWL.minCardinality);
                IUriNode minQualifiedCardinality = _graph.CreateUriNode(VocabularyHelper.OWL.minQualifiedCardinality);

                IEnumerable<INode> minCardinalities = _wrappedClass.GetNodesViaPredicate(minCardinality).Union(_wrappedClass.GetNodesViaPredicate(minQualifiedCardinality));
                if (minCardinalities.LiteralNodes().Count() == 1 &&
                    minCardinalities.LiteralNodes().First().IsInteger())
                {
                    return int.Parse(minCardinalities.LiteralNodes().First().Value, CultureInfo.InvariantCulture);
                }

                if (_wrappedClass.GetNodesViaPredicate(someValuesFrom).Count() == 1)
                {
                    return 1;
                }
                return 0;
            }
        }

        public int ExactCardinality
        {
            get
            {
                IUriNode cardinality = _graph.CreateUriNode(VocabularyHelper.OWL.cardinality);
                IUriNode qualifiedCardinality = _graph.CreateUriNode(VocabularyHelper.OWL.qualifiedCardinality);

                IEnumerable<INode> exactCardinalities = _wrappedClass.GetNodesViaPredicate(cardinality).Union(_wrappedClass.GetNodesViaPredicate(qualifiedCardinality));
                if (exactCardinalities.LiteralNodes().Count() == 1 &&
                    exactCardinalities.LiteralNodes().First().IsInteger())
                {
                    return int.Parse(exactCardinalities.LiteralNodes().First().Value, CultureInfo.InvariantCulture);
                }
                return 0;
            }
        }

        public int MaximumCardinality
        {
            get
            {
                IUriNode maxCardinality = _graph.CreateUriNode(VocabularyHelper.OWL.maxCardinality);
                IUriNode maxQualifiedCardinality = _graph.CreateUriNode(VocabularyHelper.OWL.maxQualifiedCardinality);

                IEnumerable<INode> maxCardinalities = _wrappedClass.GetNodesViaPredicate(maxCardinality).Union(_wrappedClass.GetNodesViaPredicate(maxQualifiedCardinality));
                if (maxCardinalities.LiteralNodes().Count() == 1 &&
                    maxCardinalities.LiteralNodes().First().IsInteger())
                {
                    return int.Parse(maxCardinalities.LiteralNodes().First().Value, CultureInfo.InvariantCulture);
                }
                return 0;
            }
        }

        public readonly OntologyGraph _graph;
        public readonly OntologyClass _wrappedClass;

        /// <summary>
        /// Wrapper around an ontology class that exposes some methods particular to ontology restrictions.
        /// </summary>
        /// <param name="wrappedClass"></param>
        public OntologyRestriction(OntologyClass wrappedClass)
        {
            _wrappedClass = wrappedClass;
            _graph = wrappedClass.Graph as OntologyGraph;
        }


    }
}
