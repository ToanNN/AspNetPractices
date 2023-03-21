 # Authentication
 you do not have to call UseAuthentication and UseAuthorization to register the middleware because WebApplication does this automatically
 after AddAuthentication or AddAuthorization are called.

 if you want to controlling the order of middleware, then call them explicitly
 Authentication strategies are expected to be under Authentication__Schemes__SchemeName in the config file

 # Authorization
 Authorization is used to validate and verify access to resources in an API and is facilitated by the **IAuthorizationService** registered by the AddAuthorization extension method.

 ## How to enable authorization
 a. Configure authorization policies
 b. Applying individual policies to resources

 ## Use dotnet user-jwts for development testing

 `dotnet user-jwts create`

 For example
 `dotnet user-jwts create --claim "greetings_api=master" --role "admin"`