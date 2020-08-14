using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Ontology;

namespace OWL2DTDL
{
    /// <summary>
    /// A representation of the relationships (data properties or object properties) that can be expressed on an OWL class.
    /// TODO Here be gremlins! Thid code is ugly and written in a hurry. It should probably be signficantly refactored and cleaned up,
    /// if not removed entirely. The relationship merge logic is particularly hairy.
    /// </summary>
    public class Relationship
    {
        public OntologyProperty Property
        { get; }

        public OntologyClass Target
        { get; set;  }

        public int? MinimumCount
        { get; set; }

        public int? MaximumCount
        { get; set; }

        public int? ExactCount
        {
            get
            {
                if (MinimumCount.HasValue && MaximumCount.HasValue && MinimumCount == MaximumCount)
                {
                    return MinimumCount;
                }
                return null;
            }
            set
            {
                MinimumCount = value;
                MaximumCount = value;
            }
        }

        public Relationship(OntologyProperty property, OntologyClass target)
        {
            if (!property.IsNamed() || !(target.IsNamed() || target.IsDatatype()))
            {
                throw new ArgumentException("Only named properties and named or datatype targets allowed.");
            }
            Property = property;
            Target = target;
        }

        /// <summary>
        /// Merge another Relationship into this one. 
        /// This only works when the properties of the relationships are identical; else an exception is thrown.
        /// If the two relationships are to different target classes of which one subsumes the other, then the
        /// most specific relationship is kept. If the classes do not subsume one another, OWL thing is set as target.
        /// If the relationship targets are the same, then the narrower cardinality restrictions are kept.
        /// </summary>
        /// <param name="other">Another Relationship</param>
        public void MergeWith(Relationship other)
        {
            if (!other.Property.Resource.Equals(this.Property.Resource))
            {
                throw new Exception("Properties are not identical");
            }

            // If target classes are not the same, then we need to keep the most specific one
            if (!other.Target.Resource.Equals(this.Target.Resource))
            {
                // If both target classes are both datatypes, 
                if (this.Target.IsDatatype() && other.Target.IsDatatype())
                {
                    // If the two targets are of same type but not the same, fall back to rdf:Literal
                    if (
                        (this.Target.IsEnumerationDatatype() && other.Target.IsEnumerationDatatype()) ||
                        (this.Target.IsSimpleXsdWrapper() && other.Target.IsSimpleXsdWrapper()) ||
                        (this.Target.IsXsdDatatype() && other.Target.IsXsdDatatype()))
                    {
                        IGraph targetsGraph = this.Target.Graph;
                        IUriNode rdfLiteral = targetsGraph.CreateUriNode(VocabularyHelper.RDFS.Literal);
                        this.Target = new OntologyClass(rdfLiteral, targetsGraph);
                        this.MinimumCount = null;
                        this.MaximumCount = null;
                        return;
                    }
                    else
                    {
                        // Preference order is enumeration, custom xsd wrapper type, built-in xsd type
                        List<Relationship> relationshipCandidates = new List<Relationship>() { this, other };
                        relationshipCandidates.OrderBy(candidate => !candidate.Target.IsEnumerationDatatype())
                            .ThenBy(candidate => !candidate.Target.IsSimpleXsdWrapper())
                            .ThenBy(candidate => !candidate.Target.IsXsdDatatype());
                        if (relationshipCandidates.First() == this)
                        {
                            return;
                        }
                        else
                        {
                            this.Target = other.Target;
                            this.MinimumCount = other.MinimumCount;
                            this.MaximumCount = other.MaximumCount;
                            return;
                        }
                    }
                }

                // If this relationship has the more specific target class, keep it as-is, i.e., return
                if (this.Target.SuperClassesWithOwlThing().Contains(other.Target))
                {
                    return;
                }
                // If the other relationhip has the more specific target class, keep it instead and return
                else if (other.Target.SuperClassesWithOwlThing().Contains(this.Target))
                {
                    this.Target = other.Target;
                    this.MinimumCount = other.MinimumCount;
                    this.MaximumCount = other.MaximumCount;
                    return;
                }
                // The classes do not subsume one another; fall back to OWL:Thing as target class
                else
                {
                    IGraph targetsGraph = this.Target.Graph;
                    IUriNode owlThing = targetsGraph.CreateUriNode(VocabularyHelper.OWL.Thing);
                    this.Target = new OntologyClass(owlThing, targetsGraph);
                    this.MinimumCount = null;
                    this.MaximumCount = null;
                    return;
                }
            }

            // If either restriction has an exact count, then that is the most specific restriction possible and min/max need not be inspected
            if (ExactCount.HasValue || other.ExactCount.HasValue)
            {
                // If both restrictions have exact values they either diverge (e.g., caused by an inconsistent ontology) or they converge (no change is required)
                if (ExactCount.HasValue && other.ExactCount.HasValue)
                {
                    if (ExactCount.Value != other.ExactCount.Value)
                    {
                        throw new Exception("Conflicting ExactCounts");
                    }
                    // The exact value is identical; simply return
                    return;
                }
                // Assign exact count from other only if our own is null, else keep our old exactcount
                ExactCount ??= other.ExactCount.Value;
                return;
            }

            int? newMinimum = new int?();
            if (MinimumCount.HasValue || other.MinimumCount.HasValue)
            {
                // If both have minimum counts, keep the larger one
                if (MinimumCount.HasValue && other.MinimumCount.HasValue)
                {
                    newMinimum = MinimumCount > other.MinimumCount ? MinimumCount : other.MinimumCount;
                }
                else
                {
                    // Else, keep whichever is non-null
                    newMinimum = MinimumCount.HasValue ? MinimumCount : other.MinimumCount;
                }
            }

            int? newMaximum = new int?();
            if (MaximumCount.HasValue || other.MaximumCount.HasValue)
            {
                // If both have maximum counts, keep the smaller one
                if (MaximumCount.HasValue && other.MaximumCount.HasValue)
                {
                    newMaximum = MaximumCount < other.MaximumCount ? MaximumCount : other.MaximumCount;
                }
                else
                {
                    // Else, keep whichever is non-null
                    newMaximum = MaximumCount.HasValue ? MaximumCount : other.MaximumCount;
                }
            }

            // At this point newMinimum is maximized or null and newMaximum is minimized or null
            // If they are inconsistent, i.e., newMinimum is larger than new Maximum, the model is inconsistent;
            // throw an error; else store
            if (newMinimum.HasValue && newMaximum.HasValue && newMinimum > newMaximum)
            {
                throw new Exception("Resulting min > resulting max");
            }
            MinimumCount = newMinimum;
            MaximumCount = newMaximum;
        }
    }
}
