# HL7 Programming Tutorials

A comprehensive collection of code examples for HL7 V2 and FHIR programming in both .NET and Java.

For detailed tutorials explaining these code examples, visit: **[saravanansubramanian.com](http://www.saravanansubramanian.com)**

---

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [.NET Tutorial Series](#net-tutorial-series)
  - [HL7 V2 with NHAPI](#hl7-v2-with-nhapi)
  - [FHIR with Firely .NET SDK](#fhir-with-firely-net-sdk)
- [Java Tutorial Series](#java-tutorial-series)
  - [HL7 V2 with HAPI](#hl7-v2-with-hapi)
  - [FHIR with HAPI FHIR](#fhir-with-hapi-fhir)
- [Test HL7 Message Files](#test-hl7-message-files)
- [Libraries Used](#libraries-used)

---

## Overview

This repository contains working code examples that accompany the HL7 programming tutorial series. The examples demonstrate:

- **HL7 V2 Messaging**: Creating, parsing, sending, receiving, and validating HL7 V2 messages
- **FHIR R4**: CRUD operations, searching, validation, and SMART on FHIR concepts
- **Both .NET and Java**: Parallel implementations using industry-standard libraries

---

## Prerequisites

### For .NET Projects
- .NET 8.0 SDK (for modern projects)
- .NET Framework 4.8 (for legacy projects)
- Visual Studio 2022 or VS Code

### For Java Projects
- Java 17 or higher
- Maven 3.6+
- Any Java IDE (IntelliJ IDEA, Eclipse, VS Code)

---

## Project Structure

```
hl7/
├── HL7 Programming using DotNet - Tutorial Series/
│   ├── HL7 Programming using .NET - A Short Tutorial/
│   ├── HL7 Programming using NHAPI - Creating HL7 Messages/
│   ├── HL7 Programming using NHAPI - Parsing HL7 Messages/
│   ├── HL7 Programming using NHAPI - Sending HL7 Messages/
│   ├── HL7 Programming using NHAPI - Creating ACK Messages/
│   ├── HL7 Programming using NHAPI - HL7 Server Example/
│   ├── HL7 Programming using NHAPI - Message Validation/
│   ├── HL7 Programming using NHAPI - Using Tersers/
│   ├── HL7 Programming using NHAPI - Handling Binary Data/
│   ├── Advanced Validation using NHAPI Tools/
│   ├── FHIR Programming using DotNet - Setting Up Your Environment/
│   ├── FHIR Programming using DotNet - Creating FHIR Resources/
│   ├── FHIR Programming using DotNet - Reading FHIR Resources/
│   ├── FHIR Programming using DotNet - Updating FHIR Resources/
│   ├── FHIR Programming using DotNet - Deleting FHIR Resources/
│   ├── FHIR Programming using DotNet - Searching FHIR Resources/
│   ├── FHIR Programming using DotNet - Validating FHIR Resources/
│   ├── FHIR Programming using DotNet - Chaining Operations/
│   ├── FHIR Programming using DotNet - Advanced Operations/
│   └── FHIR Programming using DotNet - SMART on FHIR/
├── HL7 Programming using Java - Tutorial Series/
│   ├── HL7 Programming using Java - A Short Tutorial/
│   ├── HL7 Programming using HAPI - Creating HL7 Messages/
│   ├── HL7 Programming using HAPI - Parsing HL7 Messages/
│   ├── HL7 Programming using HAPI - Sending HL7 Messages/
│   ├── HL7 Programming using HAPI - Receiving HL7 Messages/
│   ├── HL7 Programming using HAPI - Creating ACK Messages/
│   ├── HL7 Programming using HAPI - HL7 Server Example/
│   ├── HL7 Programming using HAPI - Message Validation/
│   ├── HL7 Programming using HAPI - Using Tersers/
│   ├── HL7 Programming using HAPI - Handling Binary Data/
│   ├── HL7 Programming using HAPI - Testing Conformance Profiles/
│   ├── FHIR Programming using Java HAPI - Setting Up Your Environment/
│   ├── FHIR Programming using Java HAPI - Creating FHIR Resources/
│   ├── FHIR Programming using Java HAPI - Reading FHIR Resources/
│   ├── FHIR Programming using Java HAPI - Updating FHIR Resources/
│   ├── FHIR Programming using Java HAPI - Deleting FHIR Resources/
│   ├── FHIR Programming using Java HAPI - Searching FHIR Resources/
│   ├── FHIR Programming using Java HAPI - Validating FHIR Resources/
│   ├── FHIR Programming using Java HAPI - Chaining Operations/
│   ├── FHIR Programming using Java HAPI - Advanced Operations/
│   └── FHIR Programming using Java HAPI - SMART on FHIR/
├── Test HL7 Message Files/
└── tests/
```

---

## .NET Tutorial Series

### HL7 V2 with NHAPI

#### 1. A Short Tutorial - TCP/IP and MLLP Basics
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using .NET - A Short Tutorial/`

Foundation examples for understanding network communication in healthcare:

| Project | Description |
|---------|-------------|
| `SimpleTcpEchoServer` | Basic TCP server demonstrating socket programming |
| `SimpleTcpEchoClient` | TCP client for testing server connectivity |
| `MultiThreadedTcpEchoServer` | Multi-threaded server handling concurrent connections |
| `SimpleMllpHl7Client` | HL7 client using MLLP (Minimal Lower Layer Protocol) |
| `SimpleMllpHl7ClientAdvanced` | Enhanced MLLP client with better error handling |
| `SimpleMultiThreadedMllpHl7Server` | Production-ready multi-threaded MLLP server |

#### 2. Creating HL7 Messages
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - Creating HL7 Messages/`

| File | Description |
|------|-------------|
| `Program.cs` | Entry point demonstrating message creation and serialization to pipe-delimited and XML formats |
| `AdtMessageFactory.cs` | Factory pattern for creating different ADT message types |
| `AdtA01MessageBuilder.cs` | Builder for constructing ADT^A01 (Admit/Visit Notification) messages with MSH, EVN, PID, PV1 segments |

#### 3. Parsing HL7 Messages
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - Parsing HL7 Messages/`

| Project | Description |
|---------|-------------|
| `NHapiParserBasicOperations` | Basic parsing using PipeParser, accessing segments and fields |
| `NHapi.Model.CustomZSegments` | Custom Z-segment model (ZPV) for vendor-specific extensions |
| `NHapiParserCustomMessageModelExample` | Parsing messages with custom message models |
| `NHapiToolsGenericMessageWrapperParsingApproach` | Generic parsing using NHapiTools library |

#### 4. Sending HL7 Messages
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - Sending HL7 Messages/`

Demonstrates sending HL7 messages over MLLP to a remote HL7 server and handling acknowledgments.

#### 5. Creating ACK Messages
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - Creating ACK Messages/`

Shows how to generate proper HL7 acknowledgment (ACK) messages in response to received messages.

#### 6. HL7 Server Example
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - HL7 Server Example/`

A complete HL7 server implementation with message routing:

| File | Description |
|------|-------------|
| `Program.cs` | Server startup and configuration |
| `MessageRouter.cs` | Routes incoming messages to appropriate handlers |
| `IHl7MessageHandler.cs` | Interface for message handlers |
| `AckGenerator.cs` | Generates acknowledgment messages |
| `Handlers/AdtMessageHandler.cs` | Handles ADT (Admit/Discharge/Transfer) messages |
| `Handlers/OrmMessageHandler.cs` | Handles ORM (Order) messages |
| `Handlers/OruMessageHandler.cs` | Handles ORU (Observation Result) messages |
| `Handlers/GenericMessageHandler.cs` | Fallback handler for other message types |

#### 7. Message Validation
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - Message Validation/`

| File | Description |
|------|-------------|
| `Program.cs` | Demonstrates built-in and custom validation |
| `OurCustomMessageValidation.cs` | Custom validation rule implementation |
| `Evn4MustBeSuppliedRule.cs` | Example rule requiring EVN-4 field |

#### 8. Using Tersers
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - Using Tersers/`

| Project | Description |
|---------|-------------|
| `HapiTerserBasicOperations` | Basic terser operations for field access using path notation |
| `HapiTerserAdvancedOperations` | Advanced terser usage including repetitions and components |

#### 9. Handling Binary Data
**Location:** `HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - Handling Binary Data/`

| Project | Description |
|---------|-------------|
| `SendingBinaryDataExample` | Embedding Base64-encoded binary data (PDFs, images) in ORU messages |
| `ReceivingBinaryDataExample` | Extracting and decoding binary data from received messages |
| `CommonUtils` | Shared utilities for Base64 encoding/decoding |

#### 10. Advanced Validation using NHAPI Tools
**Location:** `HL7 Programming using DotNet - Tutorial Series/Advanced Validation using NHAPI Tools/`

| Project | Description |
|---------|-------------|
| `NhapiToolsAutomatedContextValidationOfHl7Messages` | Automated context-based validation |
| `NhapiToolsConfigurableContextValidationOfHl7Messages` | Configurable validation rules |

---

### FHIR with Firely .NET SDK

All FHIR projects target **.NET 8.0** and use the **Hl7.Fhir.R4** NuGet package.

| Tutorial | Description |
|----------|-------------|
| `FHIR Programming using DotNet - Setting Up Your Environment` | Initial setup, connecting to FHIR servers, basic operations |
| `FHIR Programming using DotNet - Creating FHIR Resources` | Creating Patient resources with identifiers, names, addresses, telecom |
| `FHIR Programming using DotNet - Reading FHIR Resources` | Reading resources by ID, handling different resource types |
| `FHIR Programming using DotNet - Updating FHIR Resources` | Full updates and conditional updates |
| `FHIR Programming using DotNet - Deleting FHIR Resources` | Deleting resources, handling cascading deletes |
| `FHIR Programming using DotNet - Searching FHIR Resources` | Search parameters, modifiers, chained searches |
| `FHIR Programming using DotNet - Validating FHIR Resources` | Profile validation, operation outcomes |
| `FHIR Programming using DotNet - Chaining Operations` | Chained and reverse-chained searches |
| `FHIR Programming using DotNet - Advanced Operations` | Batch/transaction bundles, conditional operations |
| `FHIR Programming using DotNet - SMART on FHIR` | OAuth 2.0 authorization concepts |

---

## Java Tutorial Series

### HL7 V2 with HAPI

All Java projects use **Maven** for dependency management and target **Java 17+**.

#### 1. A Short Tutorial - MLLP Server
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using Java - A Short Tutorial/`

| File | Description |
|------|-------------|
| `MLLPBasedHL7ThreadedServer.java` | Multi-threaded MLLP server implementation |

#### 2. Creating HL7 Messages
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Creating HL7 Messages/`

| File | Description |
|------|-------------|
| `HapiCreateMessageSimpleExample.java` | Main example demonstrating message creation |
| `AdtMessageFactory.java` | Factory for creating ADT messages |
| `OurAdtA01MessageBuilder.java` | Builder for ADT^A01 messages |

#### 3. Parsing HL7 Messages
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Parsing HL7 Messages/`

| File | Description |
|------|-------------|
| `HapiParserBasicOperations.java` | Basic parsing operations |
| `HapiParserCustomMessageModelExample.java` | Custom message model parsing |
| `HapiParserMessageValidationExample.java` | Parsing with validation |
| `custommodel/v25/message/ZDT_A01.java` | Custom message type with Z-segments |
| `custommodel/v25/segment/ZPV.java` | Custom ZPV segment definition |

#### 4. Sending HL7 Messages
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Sending HL7 Messages/`

| File | Description |
|------|-------------|
| `HapiSendMessageSimpleExample.java` | Basic message sending |
| `HapiSendMultipleMessagesExample.java` | Batch message sending with connection pooling |

#### 5. Receiving HL7 Messages
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Receiving HL7 Messages/`

| File | Description |
|------|-------------|
| `BasicListenerWithoutMessageHandling.java` | Minimal listener setup |
| `BasicListenerWithExceptionHandler.java` | Listener with custom exception handling |
| `BasicListenerShowingDefaultErrorHandling.java` | Default error handling behavior |
| `BasicListenerWithEnhancedMessageHandlingAndRouting.java` | Full-featured listener with routing |
| `BasicListenerWithTwoPortBehavior.java` | Dual-port listener configuration |
| `OurSimpleApplication.java` | Custom application handler |
| `OurExceptionHandler.java` | Custom exception handler |

#### 6. Creating ACK Messages
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Creating ACK Messages/`

| File | Description |
|------|-------------|
| `HapiAckMessageExample.java` | ACK message generation example |
| `AckMessageBuilder.java` | Builder for acknowledgment messages |
| `AcknowledgmentCode.java` | Enum for ACK codes (AA, AE, AR) |

#### 7. HL7 Server Example
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - HL7 Server Example/`

| File | Description |
|------|-------------|
| `Hl7ServerMain.java` | Main server entry point |
| `handlers/AdtMessageHandler.java` | ADT message handler |
| `handlers/OrmMessageHandler.java` | ORM message handler |
| `handlers/OruMessageHandler.java` | ORU message handler |
| `handlers/GenericMessageHandler.java` | Generic fallback handler |

#### 8. Message Validation
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Message Validation/`

| File | Description |
|------|-------------|
| `HapiParserBasicMessageValidationDemo.java` | Built-in validation |
| `HapiParserCustomMessageValidationDemo.java` | Custom validation rules |

#### 9. Using Tersers
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Using Tersers/`

| File | Description |
|------|-------------|
| `HapiTerserBasicOperations.java` | Basic terser field access |
| `HapiTerserAdvancedOperations.java` | Advanced terser operations |
| `OurTerserHelper.java` | Utility class for terser operations |

#### 10. Handling Binary Data
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Handling Binary Data/`

| File | Description |
|------|-------------|
| `HapiSendBinaryData.java` | Sending binary data in messages |
| `HapiExtractBinaryData.java` | Extracting binary data from messages |
| `OruMessageFactory.java` | Factory for ORU messages with binary content |
| `OurOruR01MessageBuilder.java` | Builder for ORU^R01 with OBX segments |
| `OurBase64Helper.java` | Base64 encoding/decoding utilities |
| `BadBase64EncodingException.java` | Custom exception for encoding errors |

#### 11. Testing Conformance Profiles
**Location:** `HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Testing Conformance Profiles/`

| File | Description |
|------|-------------|
| `HapiParserConformanceProfileTestingExample.java` | Testing against conformance profiles |
| `HapiConformanceProfileBasicExampleOfValidationFailure.java` | Demonstrating validation failures |
| `Saravanan Adv Testing Example profile - ADT_A01.xml` | Sample conformance profile |

---

### FHIR with HAPI FHIR

All Java FHIR projects use Maven and the **HAPI FHIR** library targeting **FHIR R4**.

| Tutorial | Description |
|----------|-------------|
| `FHIR Programming using Java HAPI - Setting Up Your Environment` | FhirContext setup, server connectivity |
| `FHIR Programming using Java HAPI - Creating FHIR Resources` | Creating resources using HAPI FHIR client |
| `FHIR Programming using Java HAPI - Reading FHIR Resources` | Reading and parsing FHIR resources |
| `FHIR Programming using Java HAPI - Updating FHIR Resources` | Update operations and ETags |
| `FHIR Programming using Java HAPI - Deleting FHIR Resources` | Delete operations |
| `FHIR Programming using Java HAPI - Searching FHIR Resources` | Search operations and parameters |
| `FHIR Programming using Java HAPI - Validating FHIR Resources` | Resource validation against profiles |
| `FHIR Programming using Java HAPI - Chaining Operations` | Chained search queries |
| `FHIR Programming using Java HAPI - Advanced Operations` | Bundles, transactions, batch operations |
| `FHIR Programming using Java HAPI - SMART on FHIR` | SMART authorization concepts |

---

## Test HL7 Message Files

**Location:** `Test HL7 Message Files/`

Sample files for testing and development:

| File | Description |
|------|-------------|
| `FileWithLotsOfHl7Messages.txt` | Multiple HL7 messages for batch processing tests |
| `FileWithNonConformingAdtA01Message.txt` | Invalid ADT^A01 for validation testing |
| `FileWithObservationResultMessage.txt` | Sample ORU^R01 observation result |
| `SaravananOruR01Message.hl7` | ORU^R01 with embedded binary data |
| `ExtractedPathologyReport.pdf` | Sample PDF extracted from HL7 message |

---

## Libraries Used

### .NET Libraries

| Library | Version | Purpose |
|---------|---------|---------|
| [nHapi](https://github.com/nHapiNET/nHapi) | 3.x | HL7 V2 message processing |
| [NHapiTools](https://github.com/dib0/NHapiTools) | 2.x | Extended NHAPI functionality |
| [Hl7.Fhir.R4](https://github.com/FirelyTeam/firely-net-sdk) | 5.x | FHIR R4 operations (Firely SDK) |

### Java Libraries

| Library | Purpose |
|---------|---------|
| [HAPI](https://hapifhir.github.io/hapi-hl7v2/) | HL7 V2 message processing |
| [HAPI FHIR](https://hapifhir.io/) | FHIR R4 operations |

---

## Building and Running

### .NET Projects

```bash
# Build a specific project
cd "HL7 Programming using DotNet - Tutorial Series/HL7 Programming using NHAPI - Creating HL7 Messages"
dotnet build

# Run
dotnet run --project NHapiCreateMessageSimpleExample
```

### Java Projects

```bash
# Build and run using Maven
cd "HL7 Programming using Java - Tutorial Series/HL7 Programming using HAPI - Creating HL7 Messages"
mvn compile exec:java
```

---

## License

These code examples are provided for educational purposes to accompany the HL7 programming tutorials at [saravanansubramanian.com](http://www.saravanansubramanian.com).

---

## Contributing

Contributions and corrections are welcome. Please submit issues or pull requests on GitHub.
