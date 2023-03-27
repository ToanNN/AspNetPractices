Protobuf is a contract-first messaging format. An app's messages, including its fields and types, must be specified in .proto files when the app is built. 

# Versioning
## Backwards compatibility
The gRPC protocol is designed to support services that change over time. Generally, additions to gRPC services and methods are non-breaking. 
Non-breaking changes allow existing clients to continue working without changes. 

### Non-breaking changes
Adding a new service
Adding a new method to a service
Adding a field to a request message - Fields added to a request message are deserialized with the default value on the server when not set. To be a non-breaking change, the service must succeed when the new field isn't set by older clients.
Adding a field to a response message - Fields added to a response message are deserialized into the message's unknown fields collection on the client.
Adding a value to an enum - Enums are serialized as a numeric value. New enum values are deserialized on the client to the enum value without an enum name. To be a non-breaking change, older clients must run correctly when receiving the new enum value.

### Binary breaking changes
The following changes are non-breaking at a gRPC protocol level, but the client needs to be updated if it upgrades to the latest .proto contract or client .NET assembly. Binary compatibility is important if you plan to publish a gRPC library to NuGet.

- Removing a field - Values from a removed field are deserialized to a message's unknown fields. This isn't a gRPC protocol breaking change, but the client needs to be updated if it upgrades to the latest contract. It's important that a removed field number isn't accidentally reused in the future. 
To ensure this doesn't happen, specify deleted field numbers and names on the message using Protobuf's reserved keyword.
- Renaming a message - Message names aren't typically sent on the network, so this isn't a gRPC protocol breaking change. 
The client will need to be updated if it upgrades to the latest contract. One situation where message names are sent on the network is with Any fields, when the message name is used to identify the message type.
- Nesting or unnesting a message - Message types can be nested. Nesting or unnesting a message changes its message name. 
Changing how a message type is nested has the same impact on compatibility as renaming.
- Changing csharp_namespace - Changing csharp_namespace will change the namespace of generated .NET types. This isn't a gRPC protocol breaking change, 
but the client needs to be updated if it upgrades to the latest contract.

### Protocol breaking changes
The following items are protocol and binary breaking changes:
- Renaming a field - With Protobuf content, the field names are only used in generated code. The field number is used to identify fields on the network. Renaming a field isn't a protocol breaking change for Protobuf. However, if a server is using JSON content then renaming a field is a breaking change.
- Changing a field data type - Changing a field's data type to an incompatible type will cause errors when deserializing the message. Even if the new data type is compatible, it's likely the client needs to be updated to support the new type if it upgrades to the latest contract.
- Changing a field number - With Protobuf payloads, the field number is used to identify fields on the network.
- Renaming a package, service or method - gRPC uses the package name, service name, and method name to build the URL. The client gets an UNIMPLEMENTED status from the server.
- Removing a service or method - The client gets an UNIMPLEMENTED status from the server when calling the removed method.

## Version number services
 A way to maintain backwards compatibility while making breaking changes is to publish multiple versions of a service.