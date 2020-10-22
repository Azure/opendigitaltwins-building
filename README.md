# Digital Twins Definition Language-based [RealEstateCore](https://doc.realestatecore.io/3.2/full.html) ontology for smart buildings

Note: this is a work in progress repo

## Motivation and purpose

[Azure Digital Twins (ADT)](https://azure.microsoft.com/en-us/services/digital-twins/) and its underlying [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl) are at the heart of Smart Building solutions built on Azure. 

DTDL provides the schema by which developers can define the language of the entities they expect to use in their topologies. Since DTDL is a blank canvas which can model any entity, it is important to accelerate developers' time to results while also providing a unified ontology to enable seamless integration between various DTDL-based solutions.

Our partnership with RealEstateCore seeks to deliver a DTDL-based ontology (or set of models) for the real estate industry which provides common ground for modeling smart buildings, leveraging industry standards to prevent reinvention. As part of the delivery, we also provide best practices for how to consume and properly extend the ontology. 

This is an open-source ontology definition which learns from, builds on, and uses industry standards, meets the needs of downstream developers, and we hope it will be widely adopted and/or extended by developers.

## DTDL-based RealEstateCore ontology

This ontology is implemented based on the [Real Estate Core](https://doc.realestatecore.io/3.3/full.html) domain ontology. RealEstateCore is a common language used to model and control buildings, simplifying the development of new services. The ontology is rich and complete, while providing simplicity and real-world applicability with proven industry solutions and partnerships. RealEstateCore specifically does not aim to be a new standard, but rather provides a common denominator and bridge with other industry standards such as Brick Schema, Project Haystack, W3C Building Topology Ontology (W3C BOT), and more. 

For example:
- The *Asset* interface, covering systems and equipment within buildings is based on an interpretation and extension of the [Brick Schema Ontology](https://brickschema.org/ontology/), carried out in conjunction with [Willow Inc.](https://www.willowinc.com/willowtwin/). 
- Our spatial modeling is in line with the [W3C BOT ontology](https://w3c-lbd-cg.github.io/bot) and clearly differentiates between *Building Components* and *Spaces*; where the former make up the building's structural elements, and the latter make up physical spaces inside (rooms, levels, etc) or outside (regions, land, etc) of a building.
- *Capability* model is based on the BMS notion of Points (as represented in Brick Schema or Haystack) or Affordances, as represented in [Web of Things](https://www.w3.org/WoT/). Subclasses of Capability denote specific sensorsing or actuation capabilities that can be assigned to Spaces, Assets, etc.
- LogicalDevice is inspired from [Azure IoT Hub](https://docs.microsoft.com/en-us/azure/iot-hub/about-iot-hub) (IoT Hub is calling it *Device*) and represents a connected entity that pushes data to the cloud or receives commands from the cloud, which is typically an instance of a piece of software like an IoTEdge module, a HomeAssistant install, or some proprietary BMS system, etc.

The DTDL-based RealEstateCore ontology will not only accelerate developers from the “blank page,” but will also facilitate business-to-business integrations between vendors in a smart building. Since the DTDL-based ontology will be open sourced, developers can easily annotate existing models while contributing their own domain expertise. [Read more about Real Estate Core](#read more about real estate core)

## RealEstateCore structure

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

## Using RealEstateCore ontology

Here is a real example of a subgraph of twins' instances based on this ontology

![Using the models](images/UsingModels.JPG)

We have instantiated the following twins:
- A building instance *Building 121* of type [**dtmi:org:w3id:rec:core:Building;1**](Ontology/Space/Building.json)
- One level instance *Level 1* of type [**dtmi:org:w3id:rec:core:Level;1**](Ontology/Space/Level.json) which is part of the building
- Two zones, *Level 1 Left Wing* and *HVAC Zone 1* of type [**dtmi:org:w3id:rec:core:HVACZone;1**](Ontology/Space/Zone/HVACZone.json), both of them have capability Space Utilization virtual sensor 
- Two room instances *Room 101* and *Room 103* of type [**dtmi:org:w3id:rec:building:ClimateControlRoom;1**](Ontology/Space/Room/UtilitiesRoom/ClimateControlRoom.json) which are part of level and HVAC zone
- A VAV physical device *VAVL1.01* of type [**dtmi:org:w3id:rec:asset:VAVBox;1**](Ontology/Asset/Equipment/HVACEquipment/TerminalUnit/VAVBox.json) with three capabilities *AirTemperatureSensor*, *AirFlowSensor* and *AirFlowSetpoint*, feeds the HVAC zone and located in a room
- An AHU physical device *AHUL1.01* of type [**dtmi:org:w3id:rec:asset:AirHandlingUnit;1**](Ontology/Asset/Equipment/HVACEquipment/AirHandlingUnit.json) feeds the VAV and located in a room
- A Vergesense device *VergeSensorDevice* of type [**dtmi:org:w3id:rec:asset:SensorEquipment;1**](Ontology/Asset/Equipment/ICTEquipment/SensorEquipment.json) with two sensors *PeopleCount* and *SignOfLife*, which serves the wing zone and located in a room

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

## Upload the models
<*explain how to upload the models into ADT*>

You can upload all models in your own instance of ADT by using [Model Uploader](ModelUploader). Follow the instructions on ModelUploader on how to upload all models in your own instance. Here is [an article](https://docs.microsoft.com/en-us/azure/digital-twins/how-to-manage-model) on how to manage models, update, retrieve, update, decommision and delete models.


## Visualizing the models
Once you have uploaded these models into your Azure Digital Twins instance, now it's time to view how models are related to each other. Please use [ADT Model Visualizer](AdtModelVisualizer) to view the models. This tool is a draft version (read-only visualizer, no edits) and we also invite you to contribute to it to make it better.

## Extending the ontology
<*Karl to add more or to update*>

We encourage users to extend existing models via inheritance by using **extends**. Interface inheritance can be used to create specialized interfaces from more general interfaces. If you need to add a new interface or add additional properties, try to find the leaf-level interface to extend from. For example, if you need to add a specialized type of room, like *FocusRoom*, add a new interface *FocusRoom* which inherits from *Room* interface. Through inheritance, the *FocusRoom* has two properties *Room*: the *personCapacity* property (from Room) and the *occupied* property (from FocusRoom).

```
[
  {
    "@id": "dtmi:org:w3id:rec:core:Room;1",
    "@type": "Interface",
    "dtmi:dtdl:property:contents;2": {
      "@type": "Property",
      // ...
      "name": "personCapacity",
      "schema": "integer",
      "writable": true
    },
    // ...
    "extends": "dtmi:org:w3id:rec:core:Space;1",
    "@context": "dtmi:dtdl:context;2"
  },
  {
    "@id": "dtmi:org:w3id:rec:core:FocusRoom;1",
    "@type": "Interface",
    "dtmi:dtdl:property:contents;2": {
      "@type": "Property",
      // ...
      "name": "occupied",
      "schema": "bool",
      "writable": true
    },
    // ...
    "extends": "dtmi:org:w3id:rec:core:Room;1",
    "@context": "dtmi:dtdl:context;2"
  }
]

Now that you have extended your specialized interface/s, ask yourself if your extensions are generic and could benefit other users. If the answer is yes, our recommendation is to fork the existing repository, make your changes and send a pull request. 
```

## Validating the models

These models don't have to be validated with the DTDL parser unless you change them. If you have extended our models or made changes, it's recommanded to validate the models as described by this article [Validate models](https://docs.microsoft.com/en-us/azure/digital-twins/concepts-convert-models#validate-and-upload-dtdl-models)


## Contributing to ontology
<*explain how to contribute to this ontology, probably creating PRs in this repo is the easiest one to go - Karl to add*>
We are working on improving the main interfaces, adding more interfaces, as well as making better tools to integrate and use the models in smart building platforms and its applications.

We encourage you to contribute to make DTDL RealEstateCore-based ontology better. Please point out bugs or peculiarities, add or extend interfaces and vocabularies, suggest improvements in order to evolve this ontology.

- Comment or create a new issue for bug reporting
- For improvements, please fork the rec repository, make your changes and send a pull request

## Read more about Azure Digital Twins

- [Azure Digital Twins product page](https://azure.microsoft.com/en-us/services/digital-twins/)
- [Azure Digital Twins: Powering the next generation of IoT connected solutions](https://channel9.msdn.com/Events/Build/2020/INT177)
- [Digital Twins Definition Language Repository](https://github.com/Azure/opendigitaltwins-dtdl)
- [Azure Digital Twins introduction video](https://azure.microsoft.com/en-us/resources/videos/azure-digital-twins-introduction-video/)
- [Azure Digital Twins IoT Show Public Preview](https://www.youtube.com/watch?v=D6kyhrRVdfc&feature=youtu.be)
- [Azure Digital Twins Tech Deep Dive](https://www.youtube.com/watch?v=5Ku55g1GQG8&feature=youtu.be)
- [ADT Explorer](https://github.com/Azure-Samples/digital-twins-explorer)

## Read more about Real Estate Core
One solution powered by RealEstateCore is Idun ProptechOS, which enables real estate owners to analyze and optimize sustainability, well-being, and productivity of their buildings.  ProptechOS is used by a number of significant customers at scale, including Vasakronan, Sweden’s largest property company comprising 174 properties and 24.7 million square feet of real estate, as well as YIT, the largest Finnish and a significant North European construction company and urban developer.

---
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

Microsoft collects performance and usage information which may be used to provide and improve Microsoft products and services and enhance your experience.
To learn more, review the [privacy statement](https://go.microsoft.com/fwlink/?LinkId=521839&clcid=0x409).


