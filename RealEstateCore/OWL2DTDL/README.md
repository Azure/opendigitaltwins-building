# The OWL2DTDL Converter

**Author:** [Karl Hammar](https://karlhammar.com)

Introduction text here.

## Usage

Typical uses exemplified here in brief.

## Options

```
  -n, --no-imports       Sets program to not follow owl:Imports declarations.

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

Describe what is translated and how. Table?