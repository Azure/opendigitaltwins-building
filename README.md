# [RealEstateCore-based](https://doc.realestatecore.io/3.2/full.html) ontology for smart building

Note: this is a work in progress repo

## Motivation and purpose

[Azure Digital Twins (ADT)](https://azure.microsoft.com/en-us/services/digital-twins/) and its underlying [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl) are at the heart of Smart Building Azure solutions. 

Although DTDL provides the schema by which developers can define the models of the entities they expect to use in their topologies, we are faced with problems such as developers not knowing where to start or creating models so fragmented that make it impossible to integrate with other DTDL-based solutions.

Our goal is to deliver a DTDL-based building ontology(or set of models) which we recommand as “gold standard” for smart building modeling, leveraging industry standards to accelerate our work and prevent reinvention. As part of the delivery, also provide best practices for how to consume and properly extend the ontology. 

This is an open-source ontology definition which learns from, builds on, and uses industry standards, meets the needs of downstream developers, and we hope it will be widely adopted and/or extended by developers.

The evolution of our journey started with evaluation of multiple industry standards, adoption of RealEstateCore (REC) ontology, refinement with partners, validation of models with real world solutions, including Microsoft Campus, and will continue to evangelize with Digital Twins Consortium and partner’s ecosystem to extend the developer tools and management of this ontology (see Ontology Evolution picture)

![Ontology Evolution](images/OntologyEvolution.JPG)

## RealEstateCore-based ontology
This ontology is implemented based on [Real Estate Core (REC)](https://doc.realestatecore.io/3.3/full.html) domain ontology, and taking inspiration from prominent industry standards like [Brick](https://brickschema.org/ontology/), [Haystack](https://project-haystack.org/), [Web Ontology Language(OWL)](https://www.w3.org/OWL), and WillowTwin's models.

REC ontology foundation have deep domain knowledge and expertise with smart buildimg and real estate industry and Semantic Web technology, and have years of experience with model authoring and maintenance. Their real-world applications and customers are actively using REC models and demonstrated cost-efficiency solutions to connect their buildings with new services on a large scale, and not have to worry about building- or technology-specific implementation details and formats. 

## REC structure

![Building Ontology](images/OntologyDiagram.JPG)

RealEstateCore consists of a main set of modules:
  - **Asset** – An object which is placed inside of a building, but is not an integral part of that building's structure, for example architectural, furniture, equipment, systems, etc.
  - **LogicalDevice** – A physical or logical object defined as an electronic equipment or software that communicates and interacts with a digital twin platform. A logical device could be an integrated circuit inside of a smart HVAC unit, or a virtual server running on a Kubernetes cluster. Logical devices can have Capability instances (through hasCapability) that describe their input/output capabilties. If Logical Devices are embedded within Asset entities (through the locatedIn property) such capabilties typically denote the capabilities of the asset.
  - **Capability** - A capability indicates the capacity of a entity, be it a Space, an Asset, or a LogicalDevice, to produce or ingest data. This is roughly equivalent to the established Brick Schema and generic BMS term \"point\". Specific subclasses specialize this behaviour: Sensor entities harvest data from the real world, Actuator entities accept commands from a digital twin platform, and Parameter entities configure some capability or system.
  - **Space** - A contiguous part of the physical world that has a 3D spatial extent and that contains or can contain sub-spaces. For example a Region can contain many pieces of Land, which in turn can contain many Buildings, which in turn can contain Levels and Rooms.

  Auxiliary modules:
  - **Agent** - Any basic types of agents (people, organizations, groups), structurally aligned with [FOAF](http://xmlns.com/foaf/spec/).
  - **Building Component** - A part that constitutes a piece of a building's structural makeup, for example Facade, Wall, Slab, RoofInner, etc.
  - **Collection** - An administrative grouping of entities that are adressed and treated as a unit for some purpose. These entities may have some spatial arrangement (e.g., an Apartment is typically contiguous), however that is not a requirement (see, e.g., a distributed Campus consisting of spatially disjoint plots or buildings).
  - **Document** - A formal piece of written, printed or electronic matter that provides information or evidence or that serves as an official record, for example LeaseContract, Building Specification, Warranty, Drawing, etc. 
  - **Event** - A spatiotemporally indexed entity with participants, something which occurs somewhere, and that has or takes some time, for example a Lease or Rent.
  - **Role** -- A role that is held by some agent, for example a person could hold a Sales Representative role, or an organization could hold a Maintenance Responsibility role

## Using REC ontology

Real simple example of a building, two floors, rooms, one HVAC with VAV ...
show this diagram of the building graph as shown, see https://brickschema.org/ - diagram
show DTDL of models you have used in the graph, more simplified .. show inheritance of classes

## REC full
This ontology was generated using OWL2DTDL converter which generated FullBuildingRecModels.json to be uploaded into ADT.
<*point to FullBuildingRecModels.json*>

## Upload the models
<*explain how to upload the models into ADT*>
What Kevin did ...

## Visualizing the models
<*explain this is coming soon and give a preview screenshot of the models, Willow could contribute here with thier model visualizer*>

## Extending the ontology

Lorem ipsum dolor sit amet, consectetur adipiscing elit.
How to customize for yourself ...

## Validating the models

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Contributing to ontology

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


