# Building Ontology for smart building solutions built on [Azure Digital Twins](https://azure.microsoft.com/en-us/services/digital-twins/)


## Motivation and purpose

[Azure Digital Twins (ADT)](https://azure.microsoft.com/en-us/services/digital-twins/) and its underlying [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl) are at the heart of Smart Building Azure solutions. ADT provides the fundamental architectural component for many end solutions, while DTDL provides unifying modeling schemas that can facilitate integration between the services underlying solutions (Time Series Insights, Azure Maps, etc.) and integration between these solutions and the vendors which supply them.

Although DTDL provides the schema by which developers can define the models of the entities they expect to use in their topologies, we are faced with problems such as developers not knowing where to start or creating models so fragmented that make it impossible to integrate with other DTDL-based solutions.

Our goal is to deliver a DTDL-based building ontology(or set of models) which we recommand as “gold standard” for smart building modeling, leveraging industry standards to accelerate our work and prevent reinvention. As part of the delivery, also provide best practices for how to consume and properly extend the ontology. 

This is an open-source ontology definition which learns from, builds on, and uses industry standards, meets the needs of downstream developers, and we hope it will be widely adopted and/or extended by developers.

The evolution of our journey started with evaluation of multiple industry standards, adoption of RealEstateCore (REC) ontology, refinement with partners, validation of models with real world solutions, including Microsoft Campus, and will continue to evangelize with Digital Twins Consortium and partner’s ecosystem to extend the developer tools and management of this ontology (see Ontology Evolution picture)

![Ontology Evolution](images/OntologyEvolution.JPG)

## RealEstateCore-based ontology
This ontology is implemented based on [Real Estate Core (REC)](https://www.realestatecore.io/) domain ontology, and taking inspiration from prominent industry standards like [Brick](https://brickschema.org/ontology/), [Haystack](https://project-haystack.org/), [Web Ontology Language(OWL)](https://www.w3.org/OWL), and WillowTwin's models.

REC ontology foundation have deep domain knowledge and expertise with smart buildimg and real estate industry and Semantic Web technology, and have years of experience with model authoring and maintenance. Their real-world applications and customers are actively using REC models and demonstrated cost-efficiency solutions to connect their buildings with new services on a large scale, and not have to worry about building- or technology-specific implementation details and formats. 

## Ontology structure

<*change the diagram to reflext the latest*>
![Building Ontology](images/OntologyDiagram.JPG)

<*work in progress*>
RealEstateCore consists of a set of modules, which in the current version include these main modules:
  * **Asset** – An object which is placed inside of a building, but is not an integral part of that building's structure; e.g., architectural, furniture, equipment, systems, etc.
  * **LogicalDevice** – Physical or logical object which are capable of communication, telemetry, events or commands which are connected to a cloud service like IoT Hub. A logical device is also defined as of electronic equipment or software that communicates and interacts with a digital twin platform. Could, e.g., be an integrated circuit inside of a smart HVAC unit, or a virtual server running on a Kubernetes cluster. Logical devices can have Capability instances (through hasCapability) that describe their input/output capabilties. If Logical Devices are embedded within Asset entities (through the locatedIn property) such capabilties typically denote the capabilities of the asset.
  * **Capability** - A Capability indicates the capacity of a rec entity, be it a Space, an Asset, or a Device, to produce or ingest data. This is roughly equivalent to the established Brick Schema and generic BMS term \"point\". Specific subclasses specialize this behaviour: Sensor entities harvest data from the real world, Actuator entities accept commands from a digital twin platform, and Parameter entities configure some capability or system.
  * **Space** - A contiguous part of the physical world that has a 3D spatial extent and that contains or can contain sub-spaces. E.g., a Region can contain many pieces of Land, which in turn can contain many Buildings, which in turn can contain Levels and Rooms.

  More modules:
  * **Agent** - Basic types of agents (people, organizations, groups), structurally aligned with [FOAF](http://xmlns.com/foaf/spec/).
  * **Collection** - An administrative grouping of entities that are adressed and treated as a unit for some purpose. These entities may have some spatial arrangement (e.g., an apartment is typically contiguous) but that is not a requirement (see, e.g., a distributed campus consisting of spatially disjoint plots or buildings).
  * **RealEstate** - The legal/administrative representation of some lands and/or buildings.
  * **Lease** - A contract by which one party conveys land, property, services, etc. to another for a specified time, usually in return for a periodic payment. It covers leases, leased, lease contracts, premise types, etc.
  * **Document** - A formal piece of written, printed or electronic matter that provides information or evidence or that serves as an official record. 
  * **Role** -- A role that is held by some agent, e.g., a Person could hold a Sales Representative role, or an organization could hold a Maintenance Responsibility role

## Using the ontology

<*explain how to use classes, how to use it with Hub, similar to how [REC getting started](https://www.realestatecore.io/getting-started)*>

## Upload the models
<*explain how to upload the models into ADT*>

## Ontology full
This ontology was generated using OWL2DTDL converter which generated FullBuildingRecModels.json to be uploaded into ADT.
<*point to FullBuildingRecModels.json*>

## Visualizing the models
<*explain this is coming soon and give a preview screenshot of the models, Willow could contribute here with thier model visualizer*>

## Using models in a solution

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Extending the ontology

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Validating the models

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Contributing to ontology

Lorem ipsum dolor sit amet, consectetur adipiscing elit.


