# [RealEstateCore-based](https://doc.realestatecore.io/3.2/full.html) ontology for smart building

Note: this is a work in progress repo

## Motivation and purpose

[Azure Digital Twins (ADT)](https://azure.microsoft.com/en-us/services/digital-twins/) and its underlying [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl) are at the heart of Smart Building Azure solutions. 

Although DTDL provides the schema by which developers can define the models of the entities they expect to use in their topologies, we are faced with problems such as developers not knowing where to start or creating models so fragmented that make it impossible to integrate with other DTDL-based solutions.

Our goal is to deliver a DTDL-based building ontology(or set of models) which we recommand as “gold standard” for smart building modeling, leveraging industry standards to accelerate our work and prevent reinvention. As part of the delivery, also provide best practices for how to consume and properly extend the ontology. 

This is an open-source ontology definition which learns from, builds on, and uses industry standards, meets the needs of downstream developers, and we hope it will be widely adopted and/or extended by developers.

The evolution of our journey started with evaluation of multiple industry standards, adoption of RealEstateCore (REC) ontology, refinement with partners, validation of models with real world solutions, including Microsoft Campus, and will continue to evangelize with Digital Twins Consortium and partner’s ecosystem to extend the developer tools and management of this ontology (see Ontology Evolution picture)

![Ontology Evolution](images/OntologyEvolution.JPG)

## REC-based ontology
This ontology is implemented based on [Real Estate Core (REC)](https://doc.realestatecore.io/3.3/full.html) domain ontology, and taking inspiration from prominent industry standards like [Brick](https://brickschema.org/ontology/), [Haystack](https://project-haystack.org/), [Web Ontology Language(OWL)](https://www.w3.org/OWL), and WillowTwin's models.

REC ontology foundation have deep domain knowledge and expertise with smart buildimg and real estate industry and Semantic Web technology, and have years of experience with model authoring and maintenance. Their real-world applications and customers are actively using REC models and demonstrated cost-efficiency solutions to connect their buildings with new services on a large scale, and not have to worry about building- or technology-specific implementation details and formats. 

## REC structure

![Building Ontology](images/OntologyDiagram.JPG)

RealEstateCore ontology consists of a main set of interfaces:
  - **Asset** – An object which is placed inside of a building, but is not an integral part of that building's structure, for example architectural, furniture, equipment, systems, etc.
  - **LogicalDevice** – A physical or logical object defined as an electronic equipment or software that communicates and interacts with a digital twin platform. A logical device could be an integrated circuit inside of a smart HVAC unit, or a virtual server running on a Kubernetes cluster. Logical devices can have Capability instances (through hasCapability) that describe their input/output capabilties. If Logical Devices are embedded within Asset entities (through the locatedIn property) such capabilties typically denote the capabilities of the asset.
  - **Capability** - A capability indicates the capacity of a entity, be it a Space, an Asset, or a LogicalDevice, to produce or ingest data. This is roughly equivalent to the established Brick Schema and generic BMS term \"point\". Specific subclasses specialize this behaviour: Sensor entities harvest data from the real world, Actuator entities accept commands from a digital twin platform, and Parameter entities configure some capability or system.
  - **Space** - A contiguous part of the physical world that has a 3D spatial extent and that contains or can contain sub-spaces. For example a Region can contain many pieces of Land, which in turn can contain many Buildings, which in turn can contain Levels and Rooms.

  More base interfaces:
  - **Agent** - Any basic types of agents (people, organizations, groups), structurally aligned with [FOAF](http://xmlns.com/foaf/spec/).
  - **Building Component** - A part that constitutes a piece of a building's structural makeup, for example Facade, Wall, Slab, RoofInner, etc.
  - **Collection** - An administrative grouping of entities that are adressed and treated as a unit for some purpose. These entities may have some spatial arrangement (e.g., an Apartment is typically contiguous), however that is not a requirement (see, e.g., a distributed Campus consisting of spatially disjoint plots or buildings).
  - **Document** - A formal piece of written, printed or electronic matter that provides information or evidence or that serves as an official record, for example LeaseContract, Building Specification, Warranty, Drawing, etc. 
  - **Event** - A spatiotemporally indexed entity with participants, something which occurs somewhere, and that has or takes some time, for example a Lease or Rent.
  - **Role** -- A role that is held by some agent, for example a person could hold a Sales Representative role, or an organization could hold a Maintenance Responsibility role

## Using REC ontology

Here is a real example of a subgraph of twins' instances based on this ontology

![Using the models](images/UsingModels.JPG)

We have instantiated the following twins:
- A building instance *Building 121* of type **Building:Space**
- One level instance *Level 1* of type **Level:Space** which is part of the building
- Two zones, *Level 1 Left Wing* and *HVAC Zone 1* of type **HVACZone:Zone:Space**, both of them have capability Space Utilization virtual sensor 
- Two room instances *Room 101* and *Room 103* of type **ClimateControlRoom:Room:Space** which are part of level and HVAC zone
- A VAV physical device *VAVL1.01* of type **VAVBox:TerminalUnit:HVACEquipment:Equipment:Asset** with three capabilities *AirTemperatureSensor*, *AirFlowSensor* and *AirFlowSetpoint*, feeds the HVAC zone and located in a room
- An AHU physical device *AHUL1.01* of type **AirHandlingUnit:HVACEquipment:Equipment:Asset** feeds the VAV and located in a room
- A Vergesense device *VergeSensorDevice* of type **SensorEquipment:ICTEquipment:Equipment:Asset** with two sensors *PeopleCount* and *SignOfLife*, which serves the wing zone and located in a room

Here are the DTDL interfaces snippets for these twins

**Space**
```
{
  "@id": "dtmi:org:w3id:rec:core:Space;1",
  "@type": "Interface",
  "dtmi:dtdl:property:contents;2": [
    //...
    {
      "@type": "Relationship",
      "description": {
        "en": "Indicates a super-entity of the same base type (i.e., Spaces only have Spaces as parents, Organizations only have Organizations, etc). Inverse of: hasPart"
      },
      "displayName": {
        "en": "is part of"
      },
      "name": "isPartOf",
      "target": "dtmi:org:w3id:rec:core:Space;1"
    },
    {
      "@type": "Relationship",
      "displayName": {
        "en": "has capability"
      },
      "name": "hasCapability",
      "target": "dtmi:org:w3id:rec:core:Capability;1"
    }
    //...
  ],
  //...
  "displayName": {
    "en": "Space"
  },
  "@context": "dtmi:dtdl:context;2"
}
```

**Asset**
```
{
  "@id": "dtmi:org:w3id:rec:core:Asset;1",
  "@type": "Interface",
  "dtmi:dtdl:property:contents;2": [
    {
      "@type": "Relationship",
      "displayName": {
        "en": "located in"
      },
      "minMultiplicity": 0,
      "name": "locatedIn",
      "target": "dtmi:org:w3id:rec:core:Space;1"
    },
    {
      "@type": "Relationship",
      "displayName": {
        "en": "has capability"
      },
      "name": "hasCapability",
      "target": "dtmi:org:w3id:rec:core:Capability;1"
    },
    {
      "@type": "Relationship",
      "description": {
        "en": "Indicates a super-entity of the same base type (i.e., Spaces only have Spaces as parents, Organizations only have Organizations, etc). Inverse of: hasPart"
      },
      "displayName": {
        "en": "is part of"
      },
      "name": "isPartOf",
      "target": "dtmi:org:w3id:rec:core:Asset;1"
    },
    {
      "@type": "Relationship",
      "description": {
        "en": "The coverage or impact area of a given Asset or Sensor/Actuator. For example: an air-treatment unit might serve several Rooms or a full Building. Note that Assets can also service one another, e.g., an air-treatment Asset might serve an air diffuser Asset. Inverse of: servedBy"
      },
      "displayName": {
        "en": "serves"
      },
      "name": "serves"
    },
    //...
  ],
  "description": {
    "en": "Something which is placed inside of a building, but is not an integral part of that building's structure; e.g., furniture, equipment, systems, etc."
  },
  "displayName": {
    "en": "Asset"
  },
  "@context": "dtmi:dtdl:context;2"
}
```

**Sensor**
```
{
  "@id": "dtmi:org:w3id:rec:core:Sensor;1",
  "@type": "Interface",
  //...
  "description": {
    "en": "Capability to detect or measure properties of the physical world."
  },
  "displayName": {
    "en": "Sensor"
  },
  "extends": "dtmi:org:w3id:rec:core:Capability;1",
  "@context": "dtmi:dtdl:context;2"
}

```

## REC full
This ontology was generated using OWL2DTDL converter which generated FullBuildingRecModels.json to be uploaded into ADT.
<*point to FullBuildingRecModels.json*>

## Upload the models
<*explain how to upload the models into ADT*>
What Kevin did ...

## Visualizing the models
<*explain this is coming soon and give a preview screenshot of the models, Willow could contribute here with thier model visualizer*>

## Extending the ontology

<*explain how to upload the models into ADT - Karl to add*>
How to customize for yourself ...

## Validating the models

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Contributing to ontology
<*explain how to contribute to this ontology, probably creating PRs in this repo is the easiest one to go - Karl to add*>
We are working on improving the core ontology, adding more modules, and as well is working on making better tools to integrate and use the ontology in smart building platforms and its applications.

We encourage you to contribute to make RealEstateCore-based ontology better. Please point out bugs or peculiarities, add or extend modules and vocabularies, suggest improvements in order to evolve this ontology

- Comment or create a new issue for bug reporting
- For improvements, please fork the rec repository, make your changes and send a pull request
- Email us if you want to make a case(?)

## Read more

- Azure Digital Twins product page
- Azure Digital Twins: Powering the next generation of IoT connected solutions
- Digital Twins Definition Language Repo
- Azure Digital Twins introduction video
- Azure Digital Twins IoT Show Preview
- Azure Digital Twins Tech Deep Dive

---
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

Microsoft collects performance and usage information which may be used to provide and improve Microsoft products and services and enhance your experience.
To learn more, review the [privacy statement](https://go.microsoft.com/fwlink/?LinkId=521839&clcid=0x409).


