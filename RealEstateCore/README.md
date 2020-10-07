# Building Ontology for smart building solutions built on [Azure Digital Twins](https://azure.microsoft.com/en-us/services/digital-twins/)


## Motivation & Purpose

[Azure Digital Twins (ADT)](https://azure.microsoft.com/en-us/services/digital-twins/) and its underlying [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl) are at the heart of Smart Building Azure solutions. ADT provides the fundamental architectural component for many end solutions, while DTDL provides unifying schemas that can facilitate integration between the services underlying solutions (Time Series Insights, Azure Maps, etc.) and integration between these solutions and the vendors which supply them.

Although DTDL provides the schema by which developers can define the models of the entities they expect to use in their topologies, we are faced with problems such as developers not knowing where to start or creating models so fragmented that make it impossible to integrate with other DTDL-based solutions.

Our goal is to deliver a DTDL-based building ontology which we deem to be the “gold standard” for smart building modeling, leveraging industry standards to accelerate our work and prevent reinvention. As part of the delivery, also provide best practices for how to consume and properly extend the ontology. This is an open-source ontology definition which learns from, builds on, and uses industry standards, meets the needs of downstream developers, and will be widely adopted and/or extended by the industry.

## RealEstateCore-aligned ontology
We are releasing an open source Smart Buildings DTDL repository, aligned to the [Real Estate Core (REC)](https://www.realestatecore.io/) standard ontology, and taking inspiration from prominent industry ontologies like [Brick](https://brickschema.org/ontology/), [Haystack](https://project-haystack.org/), [Web Ontology Language(OWL)](https://www.w3.org/OWL) or FOAF.

RealEstateCore ontology foundation have deep domain knowledge and expertise with industry (smart buildings, real estate) and technology (Semantic Web). They are incorporating other industry concepts and standards (BIM, IFC, Brick, Haystack, etc.) into their ontology and have years of experience with model authoring and maintenance. Their real-world applications and partners actively using their models (Vasakronan, Idun, etc.) are demonstrating that ...

## Ontology Structure

<*work in progress*>

RealEstateCore consists of a set of modules, which in the current version include:
  * **Agent** - Basic types of agents (people, organizations, groups), structurally aligned with FOAF.
  * **Asset** – A real-world object, which is tangible like architectural component, furniture, workstation, all kinds of equipment HVAC, plumbing, electrical, lighting, etc. Equipment is a type of asset could be anything from a tool, device, kit, apparatus, etc. which has a maker, a model, installation date, etc. Example of equipment could be from multiple domains, electrical, lighting, plumbing, but also HVAC, metering, elevator, etc. See also Brick definition for equipment 
  * **Device** – Devices are things which are capable of communication, telemetry, events or commands which are connected to a cloud service like IoT Hub.
  * **Capability** - It provides functionality in support of a device such as a sensor, setpoint, status, or command. Capabilities are connected to device but not assets. 
  * **Space** - thing which support location positioning (indoor or outdoor) or geographical region, like Building, Land, Region, Level, Room, Zone, etc.
  * **Lease** - covers leases, leased, lease contracts, premise types, etc.


## Using REC

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Extending REC

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Contributing to REC

Lorem ipsum dolor sit amet, consectetur adipiscing elit.

## Using Models in a Solution

Lorem ipsum dolor sit amet, consectetur adipiscing elit.
