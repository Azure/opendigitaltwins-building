# Capabilities

## Overview

A capability indicates the ability of a entity such as a Space, an Asset, or a System, to produce or ingest data. This is equivalent to the term `point` in Project Haystack, Brick Schema, or a Building Management System or the terms `trend`, `metric`, or `variable` in a time series data store. Specific subclasses specialize this behavior: `Sensor` entities harvest data from the real world, `Actuator` entities accept commands from a digital twin platform, `Parameter` entities configure some capability or system, and `State` entities maintain the status of the target twins.

When the digital twin connects to a system or source of data, a capability twin maintains the current value (or latest value received) and a time series record of historical values.

It's important to understand the difference between a `capability` and other entities such as an `asset`. A `capability` is a virtual concept while an `asset` is a physical object. For example, a physical asset of a `sensor equipment` may have one or more capabilities such as `occupancy sensor`, `temperature sensor`, and `illuminance sensor`.

This guide will provide an understanding of how to become familiar with the capabiltiy data model, define capabilty twins and recommendations for how to best use the ontology.

## Capabiltiy Models
The `Capability` model class has been sub-classified in several ways that enable the implementer to give semantics to the type of data that the capability is referencing. The concept of using classification and model inheritance provides a few benefits over an alternative approach such as tagging:
* Model classification promotes structure and consistancy compared to ad-hoc tags
* Model classification promotes understanding over flexibility. A tag by itself only has context in terms of how it gets used with other tags.

In order to provide the highest fidelity digital twin, the implementer should strive to create twins that have the most specific classification, or deepest inheritance. For example, an `Active Electrical Energy Sensor` should be used over `Electrical Energy Sensor` when it is known that the sensor is measuring active or real power.

### Capability Functions
The first sub-classification of `Capability` is by **function**. The function describes the purpose or behavior of that capability in the context of its parent twin(s). The **function** sub-classes are the following:

| Function | Description |
| -------- | ----------- |
| Sensor | Measures, detects, or harvests data from the real world. An `input` to a controller which is considered read-only. |
| Actuator | Executes some real-world action based on an input command. Often an `output` from a controller. |
| Parameter | Any configuration setting used by a controller or system to guide its operation. |
| Setpoint | A sub-classification to `Parameter` which is a configuration `input` to a controller defining the desired quantity or state of its parent entity. |
| State | Defines the status of an entity such as mode, position, or level which is not directly defined by a real-world sensor. An `input` to a controller which is considered read-only. |

### Capability Kinds
The second sub-classification of `Capability` is by **kind**. The kind describes the primitive schema of the values of the capability with the following sub-classes:

| Kind | Description |
| -------- | ----------- |
| Quantity | A kind of quantity values defined by measurement units. Inspired by QUDT. |
| State | A kind of unitless values such as boolean, percent, and strings. |

If the type of values and units of a Capability are known, the implementer can indentify which **kind** a capability should be classified as.

#### Quantity Kind
Within the Quantity Kind class, there are many sub-classes which are inspired by both the [DTDL Semantic types](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#semantic-types) and [QUDT](http://www.qudt.org/doc/DOC_VOCAB-QUANTITY-KINDS.html). This buildings ontology focuses on applicable Quantity Kinds to the Building and Real Estate industry and has omitted many of the classes found in QUDT that are applicable to other scientific and physics-centric industries.

The Quantity Kind subclass names (i.e. Temperature, Power, and Flow) are common across each of the top-level Capability sub-classes of `Sensor`, `Actuator`, and `Parameter\Setpoint`. However, these top-level sub-classes do not use multiple inheritance of a common QuantityKind class. As such, clients can query across these top-level classes (i.e. TemperatureSensor and TemperatureSetpoint) by using a string match function such as `STARTSWITH`.

#### State Kind
Within the State Kind class, there are three sub-classes which are defined by their unitless value schema:

| State Kind | Description |
| ---------- | ----------- |
| Binary | Boolean values `true` or `false` |
| Percent | Number values between `0...1` |
| Multi-State | `String` values often defined by an enumeration |

Each of the above State Kind classes are further sub-classed to give context to the type of State that is being sensed, actuated, configured, or maintained. The State Kind subclass names (i.e. OnOff, Mode, and Position) are common across each of the top-level Capability sub-classes of `Sensor`, `Actuator`, and `Parameter\Setpoint`. However, these top-level sub-classes do not use multiple inheritance of a common StateKind class. As such, clients can query across these top-level classes (i.e. TemperatureSensor and TemperatureSetpoint) by using a string match function such as `STARTSWITH`.

## Capability Properties
The Capability models maintain several **properties** which allow additional metadata on the twin to provide context to 1) classificaiton 2) values and 3) communication configuration. The properties also maintain the latest value the twin received from the connected entity (`lastValue`) and the time at which it was sampled or reported by the connected entity (`lastValueTime`).

### Tags Properties
The Capabiltiy models described above provide a great means of classifying based on **function** and **kind** which brings consistency and inheritence to these two common means by which a Capability is defined and would be queried. However, most impelementations of a digital twin desire additional metadata context to query, analyze, and filter amongs twins of the same model. Because there are many ways in which a capability could be classified in the real world, there are too many combinations to pre-define models for every possible real-world scneario. The use of properties within the **tags** component allows the flexibility to add context to the base set of models which have been defined above.

The following [**tags**](https://github.com/Azure/opendigitaltwins-building/blob/master/Ontology/Information/TagSet/CapabilityTagSet.json) allow the implementer to define that additional context upon creating a capability twin. These provide similar functionality of tags in that they add meaning to the type of capability; however, they differ in implementation. Because these are defined as Properties in the DTDL Capability model, the twin maintains a *key-value pair* which provides the context in the *key* as to why the *value* of the property has been set. Additionally, the properties have been defined as disjoint enumerations which provides additional structure to the ontology over a taging dictionary.

#### Phenomenon
A capability's `phenomenon` defines the aspect of scientific interest that it is measuring, actuating, or configuring. This is inspired by Project Haystack. It is the most common classification property that an implementer would define.

The Quantity Kind defines the measurable property of a phenomenon. As such, every twin within a Quantity Kind sub-class should define the phenomenon property. Common phenomenon - quantity kind pairs in the building domain include the following:

| **Phenomenon** | Quantity Kinds |
| --- | ---|
| Air | Temperature, Volume Flow, Static Pressure, Air Quality, Humidity, Humidity Ratio, Enthalpy |
| Water | Temperature, Volume Flow, Mass Flow, Pressure |
| Chilled Water | Temperature, Volume Flow, Mass Flow, Mass, Pressure |
| Natural Gas | Mass Flow, Mass |
| AC Electricity | Voltage, Current, Electrical Power, Electrical Energy, Frequency, Power Factor |
| Drive Electricity | Frequency, Voltage, Current, Electrical Power |
| Precipitation | Volume Flow |
| Wind | Wind Direction, Linear Velocity |
| Solar | Irradiance, Luminance |
| Light | Illuminance |
| Data | Data Rate, Data Size |

```
Note:
Because phenomemon is defined as a string enumeration, there is no inheritance as you would have from models that extend one another. For example, `Chilled Water`, `Hot Water`, and `Domestic Cold Water` are all considered types of `Water`. `Water` and `Air` are considered types of **fluids**. Similarly, `Precipitation` and `Wind` are considered types of **Weather**. At this time, it is recommended to query using `ENDSWITH` for the phenomenon which have a common inheritance such as `Hot Water` and `Domestic Cold Water`.
```

#### Position
A capability's `position` defines the location or placement relative to its parent entity such as an asset, system, or space. This context is often necessary to define whether the capability is related to an input, output, or net differential to the parent entity.

Different types of entities use established industry terminology to define the same semantic meaning of placement. For example, piping systems use the terms `entering` (input), `leaving` (output), and `delta` (net) whereas electrical energy uses the terms `import` (input), `export` (output), and `net` (net).

In order to encourage consistency across implementations, its extremely important to align on when to use which placement term. At this time, the ontology doesn't restrict usage to specific scenarios. Additionally, DTDL doesn't support a "sameAs" definition as found in OWL which would enable linking different terminology that have the same semantic meaning. This makes it even more critical to have a common understanding of the terminology.

Here is a reference on how to select the proper `position` value:

| Scenario | **Position** |
| --- | ---|
| Duct (Air) | Exhaust, Outside, Return, Discharge, Zone, Mixed, Underfloor, Economizer |
| Pipe (Water) | Entering, Leaving, Header, Bypass, Circulating, Delta |
| Electricity | Input, Output |
| Energy | Import, Export, Net |
| Solar | Azimuth, Zenith |
| Data | Download, Upload |
| People Counting | Entering, Leaving |


#### Other Tags Properties
The following table includes the other capability classification properties which can be defined in addition to `phenomenon` and `position`:

| Property | Description | Example |
| --- | --- | --- |
| Asset Component | Defines which component of a larger asset the capability is associated with. This is required when the larger asset twin hasn't been created with separate twins for these components using the `isPartOf` relationship. | **Air Handling Unit** has several **fan** components for Discharge, Return, and Exhaust. |
| HVAC Mode | Defines which HVAC mode the capability is associated with such as **heating**, **cooling**, or **economizing**. | HVAC **Terminal Unit** has a **Heating Temperature Setpoint** and a **Cooling Temperature Setpoint** to define a temperature band to control to. |
| Occupancy Mode | Defines which occupancy state the capability is associated with such as **occupied**, **unoccupied**, or **standby**. | HVAC **Terminal Unit** has an **Occupied Temperature Setpoint** and an **Unoccupied Temperature Setpoint** to adjust behavior based on occupancy. |
| Electrical Phase | Defines which phase(s) of a three-phase circuit the capability is associated with. This is required when measuring three-phase power because individual phases do not get their own twin identity. | **Electrical Circuit 3-Pole** has a **Voltage Sensor** measurement for each phase **A**, **B**, **C**, **AB**, **BC**, and **CA**. |
| Limit | Parameter that places and upper (maximum) or lower (minimum) bound on permitted values of another capability. | HVAC **Terminal Unit** has a **Maximum Airflow Setpoint** and **Minimum Airflow Setpoint** to define a band the unit can operate within. |
| Demand | Rate required for a process. For a setpoint, this sets the required rate for a process such as cooling, heating, air flow, or water flow. For a sensor, this measures the rate of a process over a given interval such as electrical power demand or cooling energy demand. | HVAC **Terminal Unit** has a **Cooling Demand Setpoint** which can be adjusted by a reset strategy. |
| Effective | Current control setpoint in effect taking into account other factors (Project Haystack). | HVAC **Air Handling Unit** has an **Effective Discharge Air Temperature Setpoint** based on the current weather conditions, season, and building occupancy. |
| Stage | Stage number of a control loop for an equipment, equipment group, or system that is defined by the process controller. The first stage is 1, second stage 2, etc. (Project Haystack). | Set of **Plumbing Pumps** have a **Stage 1 Start Actuator** and **Stage 2 Start Actuator**. |

### Value Properties
The most important aspect of capabilities are the values themselves that are being reported and configured by the physical devices and the digital twin. The following properties provide the latest value and context of how to interpret the meaning of that value either by iteself or in a time series.

#### Last Value and Last Value Time
The `lastValue` property maintains the most recent value reported by a device or gateway to the digital twin. This property is also historized in a time series.

Because `lastValue` can have many different data schemas (i.e. boolean, number, string) depending on the capability model, this property is not defined on the root capability DTDL model but rather many of the models which extend from capability.

The `lastValueTime` property accompanies `lastValue` to record the timestamp in which the value is attributed. Because connected systems vary in how they present the timestamp, the meaning of this property can change slightly. For example, this time may represent when a sensor sampled the environment or it may represent when the gateway which the sensor sends data through requested the sampled value.

#### Unit (Proposed)
The `unit` property should be defined for all of the `Quantity Kind` capabilities. At this time, it is a **string** data schema which allows the user to input any text value. However, it is recommended to align on a common dictionary of units such that unit conversations can be performed by client applications, analytics, and reporting.

In the future, the `unit` property may be deprecated to take advantage of [DTDL Semantic Types](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#semantic-types). Because DTDL semantic types require units to be defined in the DTDL models, this requires units to be normalized prior to twin creation and all time series data ingested to be transformed prior to being stored. Therefore, we have chosen not to adopt these in the DTDL ontology at this time in favor of storing the ingested data in the same units as its being produced by the connected system.

#### Valid Values (Proposed)
The `validValues` property is an object which contains `minimum` and `maximum` properties. These define a valid range for the `lastValue` to be within. The `validValues` property is only applicable for numeric value schemas such as a **double** and not **boolean** or **string**.

These properties are commonly set within the controller of a connected system as dedicated capabilities such as a `Minimum/Maximum Temperature Setpoint`. These properties may also be defined by the user directly in the digital twin.

When defined on a `sensor` or `state`, this range represents the valid values reported in normal operation. If values are outside of the range, the sensor is considered in a fault state. When defined on an `actuator` or `setpoint`, this range represents valid values that a user or application can update or command the capability.

#### Interpolation (Proposed)
The `interpolation` property defines how the time series data should be filled in between samples. This is required to understand how to interpret time series data that comes in at different intervals and aggregate it for analytics and reporting. Interpolation is a common process for signal reconstruction, time bucketing, and filling in gaps. This property is an enum which can be set to **linear**, **stepForward**, or **stepBackward** described as follows:

| Interpolation | Description | Use Case |
| --- | --- | --- |
| **linear** (Most common) | Performs a linear interpolation between the previous and next values found in a time series | Data sampled on a regular interval that can be assumed as continuous. |
| **stepForward** | Performs a forward fill of the previous value found in a time series | Data known to be reported as it changes value. |
| **stepBackward** (Least common) | Performs a backward fill of the next value found in a time series | Data sampled that cannot be assumed continuous. |

#### Totalized (Proposed)
The `totalized` property is a boolean which defines a capability that is continuously counting upward. This is common in **metering**. In order to determine consumption within a desired time interval, the delta between two values must be calculated as an aggregate. This property is only applicable for numeric value schemas such as **double**.

## Capability Relationships
The capability models define several **relationships** which capability twins can have with other twins.

Just as the capability models and properties are important to align on their usage in the ontology, relationships should have consistent use by all users which implement and interpret the digital twin. Refer to the [Samples](https://github.com/WillowInc/opendigitaltwins-building/tree/main/Samples) for different scenarios of how to use these relationships.

| Relationship | Description |
| ------------ | ----------- |
| isCapabilityOf | Indicates that a Space, Asset or LogicalDevice has the ability to produce or ingest data. |
| hostedBy | Indicates that a capability is hosted by another entity such as an asset. |
| isControlledBy | Indicates that a capability's value or output is controlled by another capability. |
