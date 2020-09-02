# The OWL2DTDL Converter

**Author:** [Karl Hammar](https://karlhammar.com)

The OWL2DTDL converter is a tool that translates an OWL ontology or an ontology network (one root ontology reusing other ontologies through `owl:imports` declarations) into a set of DTDL Interface declarations for use, e.g., with the Azure Digital Twins service. This converter is a work-in-progress; if you find bugs, do not hesitate to contact the author.

## Example usage

`./OWL2DTDL -u https://w3id.org/rec/full/3.3/ -i ./RecIgnoredNames.csv -o /Users/karl/Desktop/DTDL/`

## Options

```
  -n, --no-imports       Sets program to not follow owl:imports declarations.

  -f, --file-path        Required. The path to the on-disk root ontology file to
                         translate.

  -u, --uri-path         Required. The URI of the root ontology file to
                         translate.

  -o, --outputPath       Required. The directory in which to create DTDL models.

  -m, --merged-output    Sets program to output one merged JSON-LD file for
                         batch import into ADT.

  -i, --ignorefile       Path to a CSV file, the first column of which lists
                         (whole or partial) IRI:s that should be ignored by this
                         tool and not translated into DTDL output.
```

## Supported OWL features

### owl:Class

* Named OWL classes are translated into DTDL Interfaces.
* Anonymous classes are ignored.

### owl:ObjectProperty

* Object properties that are defined for a class (i.e., that are used in an OWL restriction on that class itself, or that have that class as defined rdfs:domain) are become DTDL Relationships on that Interface. 
* As DTDL only allows a single Target definition per Relationship, we only map such properties that have named singleton ranges (targets). More complex properties are ignored (for now).

### owl:DataProperty

* Data Properties that are defined for a class (i.e., that are either used in a restriction on that class itself, or have that class as defined rdfs:domain) become DTDL Properties on that Interface. 
* Data properties that have ranges that are XSD base types are translated into corresponding DTDL primitive schemas per the table below. 
* Ranges that are custom data types that derive from exactly one built-in XSD base type gets translated the same way, with a comment added to the parent Property indicating the label of the derived type (e.g., "LitersPerMinute" for an int).

### owl:AnnotationProperty on owl:ObjectProperty

OWL annotation properties that have an object property defined as their rdfs:domains are translated into DTDL Properties on the corresponding DTDL Relationships (see above). Per this approach, the property-graph behaviour of DTDL can be loosely emulated in OWL.

### Documentation and Deprecation

* rdfs:labels become DTDL displayNames; language tags are maintained.
* rdfs:comments become DTDL comments; language tags are maintained.
* Entities that are marked as deprecated (i.e., are annotated with the property owl:deprecated and the boolean value true) are ignored in translation.

### DTMI Minting

DTMI:s for named classes are minted based on the classes' URIs by concatenating five components: 

* The `"dtmi:"` prefix
* The hostname component of the URI, reversed, with periods converted to colons
* The path component of the URI, with slashes converted to colons
* The local name of the class
* The DTMI version identifier; for now hardcoded as `";1"`

E.g., `https://w3id.org/rec/device/Actuator` becomes `dtmi:org:w3id:rec:device:Actuator;1`.

## Translation examples

TBD