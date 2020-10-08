# Building Ontology for smart building solutions built on [Azure Digital Twins](https://azure.microsoft.com/en-us/services/digital-twins/)


## Motivation and purpose

[Azure Digital Twins (ADT)](https://azure.microsoft.com/en-us/services/digital-twins/) and its underlying [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl) are at the heart of Smart Building Azure solutions. ADT provides the fundamental architectural component for many end solutions, while DTDL provides unifying schemas that can facilitate integration between the services underlying solutions (Time Series Insights, Azure Maps, etc.) and integration between these solutions and the vendors which supply them.

Although DTDL provides the schema by which developers can define the models of the entities they expect to use in their topologies, we are faced with problems such as developers not knowing where to start or creating models so fragmented that make it impossible to integrate with other DTDL-based solutions.

Our goal is to deliver a DTDL-based building ontology which we deem to be the “gold standard” for smart building modeling, leveraging industry standards to accelerate our work and prevent reinvention. As part of the delivery, also provide best practices for how to consume and properly extend the ontology. This is an open-source ontology definition which learns from, builds on, and uses industry standards, meets the needs of downstream developers, and will be widely adopted and/or extended by the industry.

<*show ontology evolution picture here*>

## RealEstateCore-based ontology
We are releasing an open source Smart Buildings DTDL repository, aligned to the [Real Estate Core (REC)](https://www.realestatecore.io/) standard ontology, and taking inspiration from prominent industry ontologies like [Brick](https://brickschema.org/ontology/), [Haystack](https://project-haystack.org/), [Web Ontology Language(OWL)](https://www.w3.org/OWL) or FOAF.

<*talk about Willow's expertise and contribution*>

RealEstateCore ontology foundation have deep domain knowledge and expertise with industry (smart buildings, real estate) and technology (Semantic Web). They are incorporating other industry concepts and standards (BIM, IFC, Brick, Haystack, etc.) into their ontology and have years of experience with model authoring and maintenance. Their real-world applications and partners actively using their models (Vasakronan, Idun, etc.) are demonstrating that ...

<*show link to REC ontology*>

## Ontology structure

<*change the diagram to reflext the latest*>
![Building Ontology](images/OntologyDiagram.jpg)

<*work in progress*>
RealEstateCore consists of a set of modules, which in the current version include:
  * **Agent** - Basic types of agents (people, organizations, groups), structurally aligned with [FOAF](http://xmlns.com/foaf/spec/).
  * **Asset** – An object which is placed inside of a building, but is not an integral part of that building's structure; e.g., architectural, furniture, equipment, systems, etc.
  * **LogicalDevice** – Physical or logical object which are capable of communication, telemetry, events or commands which are connected to a cloud service like IoT Hub. A logical device is also defined as of electronic equipment or software that communicates and interacts with a digital twin platform. Could, e.g., be an integrated circuit inside of a smart HVAC unit, or a virtual server running on a Kubernetes cluster. Logical devices can have Capability instances (through hasCapability) that describe their input/output capabilties. If Logical Devices are embedded within Asset entities (through the locatedIn property) such capabilties typically denote the capabilities of the asset.
  * **Capability** - A Capability indicates the capacity of a rec entity, be it a Space, an Asset, or a Device, to produce or ingest data. This is roughly equivalent to the established Brick Schema and generic BMS term \"point\". Specific subclasses specialize this behaviour: Sensor entities harvest data from the real world, Actuator entities accept commands from a digital twin platform, and Parameter entities configure some capability or system.
  * **Space** - A contiguous part of the physical world that has a 3D spatial extent and that contains or can contain sub-spaces. E.g., a Region can contain many pieces of Land, which in turn can contain many Buildings, which in turn can contain Levels and Rooms.
  * **Collection** - An administrative grouping of entities that are adressed and treated as a unit for some purpose. These entities may have some spatial arrangement (e.g., an apartment is typically contiguous) but that is not a requirement (see, e.g., a distributed campus consisting of spatially disjoint plots or buildings).
  * **RealEstate** - The legal/administrative representation of some lands and/or buildings.
  * **Lease** - A contract by which one party conveys land, property, services, etc. to another for a specified time, usually in return for a periodic payment. It covers leases, leased, lease contracts, premise types, etc.
  * **LeaseContract** - Formal document that identifies the Tenant and the leased asset or property; states lease term and fee (rent), and detailed terms and conditions of the lease agreement.
  * **Role** -- A role that is held by some agent, e.g., a Person could hold a Sales Representative role, or an organization could hold a Maintenance Responsibility role

## Using the ontology

<*explain how to use classes, how to use it with Hub, similar to how [REC getting started](https://www.realestatecore.io/getting-started)*>

## Validating the models

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Upload the models
<*explain how to upload the models into ADT*>

## Ontology full
<*talk about FullBuildingOntology.json or RecModels.jsonld*>

## Using models in a solution

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## REC/OWL to DTDL convertor
<*explain the scope of this convertor and point to the OWL2DTDL*>

<*Point to other converters, e.g. [How to Convert industry-standard models to DTDL for Azure Digital Twins](https://review.docs.microsoft.com/en-us/azure/digital-twins/concepts-convert-models?branch=pr-en-us-131556#industry-models)*>


## Visualizing the models
<*explain this is coming soon and give a preview screenshot of the models, Willow could contribute here with thier model visualizer*>

## Extending the ontology

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Contributing to ontology

Lorem ipsum dolor sit amet, consectetur adipiscing elit.


