# Hub
## Hubs are transient
- Don't store state in a property of the hub class. Each hub method call is executed on a new hub instance.
- Don't instantiate a hub directly via dependency injection. Use IHubContext instead
- Use await when calling asynchronous methods that depend on the hub staying alive