# Cache aggresively
# Understand hot code paths
# Avoid blocking calls

## DO
- Do make hot code paths asynchronous.
- Do call data access, I/O, and long-running operations APIs asynchronously if an asynchronous API is available.
- Do make controller/Razor Page actions asynchronous. The entire call stack is asynchronous in order to benefit from async/await patterns.
- Return large collections across multiple smaller pages.
- Do add pagination to mitigate the preceding scenarios. Using page size and page index parameters, developers should favor the design of returning a partial result

## DON"T
- Do not block asynchronous execution by calling Task.Wait or Task<TResult>.Result. 
- Do not acquire locks in common code paths. ASP.NET Core apps perform best when architected to run code in parallel. 
- Do not call Task.Run and immediately await it. ASP.NET Core already runs app code on normal Thread Pool threads, so calling Task.Run only results in extra unnecessary Thread Pool scheduling
- Do not use Task.Run to make a synchronous API asynchronous.

# Return IEnumerable<T> or IAsyncEnumerable<T>

Returning IEnumerable<T> from an action results in synchronous collection iteration by the serializer. The result is the blocking of calls and a potential for thread pool starvation
To avoid synchronous enumeration, use ToListAsync before returning the enumerable.

# Minimize large object allocations

Garbage collection is especially expensive on large objects (> 85 K bytes). 
Large objects are stored on the large object heap and require a full (generation 2) garbage collection to clean up. 
Unlike generation 0 and generation 1 collections, a generation 2 collection requires a temporary suspension of app execution. 
Frequent allocation and de-allocation of large objects can cause inconsistent performance.

- Do consider caching large objects that are frequently used. Caching large objects prevents expensive allocations.
- Do pool buffers by using an ArrayPool<T> to store large arrays.
- Do not allocate many, short-lived large objects on hot code paths.

Use PerfView to examine:
- Garbage collection pause time.
- What percentage of the processor time is spent in garbage collection.
- How many garbage collections are generation 0, 1, and 2.

# Optimize data access and I/O

- Do call all data access APIs asynchronously.
- Do not retrieve more data than is necessary. Write queries to return just the data that's necessary for the current HTTP request.
- Do consider caching frequently accessed data retrieved from a database or remote service if slightly out-of-date data is acceptable. 
- Do minimize network round trips. The goal is to retrieve the required data in a single call rather than several calls.
- Do use no-tracking queries in Entity Framework Core when accessing data for read-only purposes.
- Do filter and aggregate LINQ queries (with .Where, .Select, or .Sum statements, for example) so that the filtering is performed by the database.
- Do consider that EF Core resolves some query operators on the client, which may lead to inefficient query execution.

# Pool HTTP connections with HttpClientFactory

- Closed HttpClient instances leave sockets open in the TIME_WAIT state for a short period of time. 
- If a code path that creates and disposes of HttpClient objects is frequently used, the app may exhaust available sockets.

Do not create and dispose of HttpClient instances directly.
Do use HttpClientFactory to retrieve HttpClient instances. For more information, see Use HttpClientFactory to implement resilient HTTP requests.

# Complete long-running Tasks outside of HTTP requests

Do not wait for long-running tasks to complete as part of ordinary HTTP request processing.
Do consider handling long-running requests with background services or out of process with an Azure Function.

# Minify client assets
Do use the bundling and minification guidelines,
Use Webpack

# Compress responses
# Minimize exceptions
- Do not use throwing or catching exceptions as a means of normal program flow, especially in hot code paths.
- Do include logic in the app to detect and handle conditions that would cause an exception.
- Do throw or catch exceptions for unusual or unexpected conditions.

# Avoid synchronous read or write on HttpRequest/HttpResponse body

**Do not do this:**
 var json = new StreamReader(Request.Body).ReadToEnd();

 Do this: The following example uses ReadToEndAsync and does not block the thread while reading.

 ```
	[HttpGet("/contoso")]
    public async Task<ActionResult<ContosoData>> Get()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        return JsonSerializer.Deserialize<ContosoData>(json);
    }
 ```
 If the request is large, reading the entire HTTP request body into memory could lead to an out of memory (OOM) condition

 Do this: The following example is fully asynchronous using a non-buffered request body:

 ```
 [HttpGet("/contoso")]
    public async Task<ActionResult<ContosoData>> Get()
    {
        return await JsonSerializer.DeserializeAsync<ContosoData>(Request.Body);
    }
 ```

 ## Prefer ReadFormAsync over Request.Form

 ```

 [HttpPost("/form-body")]
    public async Task<IActionResult> Post()
    {
       var form = await HttpContext.Request.ReadFormAsync();

        Process(form["id"], form["name"]);

        return Accepted();
    }
```

# Avoid reading large request bodies or response bodies into memory
In .NET, every object allocation greater than 85 KB ends up in the large object heap (LOH).
Storing a large request or response body into a single byte[] or string:
- May result in quickly running out of space in the LOH.
- May cause performance issues for the app because of full GCs running.

# Working with a synchronous data processing API
When using a serializer/de-serializer that only supports synchronous reads and writes (for example, Json.NET):

Buffer the data into memory asynchronously before passing it into the serializer/de-serializer.
ASP.NET Core 3.0 uses System.Text.Json by default for JSON serialization. System.Text.Json:

- Reads and writes JSON asynchronously.
- Is optimized for UTF-8 text.
- Typically is higher performance than Newtonsoft.Json.

# Do not store IHttpContextAccessor.HttpContext in a field
The IHttpContextAccessor.HttpContext returns the HttpContext of the active request when accessed from the request thread. The IHttpContextAccessor.HttpContext should not be stored in a field or variable.

**Do not do this: The following example stores the HttpContext in a field and then attempts to use it later.**

```
public class MyBadType
{
    private readonly HttpContext _context;
    public MyBadType(IHttpContextAccessor accessor)
    {
        _context = accessor.HttpContext;
    }

    public void CheckAdmin()
    {
        if (!_context.User.IsInRole("admin"))
        {
            throw new UnauthorizedAccessException("The current user isn't an admin");
        }
    }
}
```
The preceding code frequently captures a null or incorrect HttpContext in the constructor.'

Instead stor `IHttpContextAccessor` in a field and then access `IHttpContextAccessor.HttpContext` later when needed through the property


```
public class MyGoodType
{
    private readonly IHttpContextAccessor _accessor;
    public MyGoodType(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public void CheckAdmin()
    {
        **var context = _accessor.HttpContext;**
        if (context != null && !context.User.IsInRole("admin"))
        {
            throw new UnauthorizedAccessException("The current user isn't an admin");
        }
    }
}
```

# Do not access HttpContext from multiple threads

HttpContext is not thread-safe.  Accessing HttpContext from multiple threads in parallel can result in unexpected behavior such as hangs, crashes, and data corruption.

**DO NOT ACCESS HttpContext from multiple threads**

For instance ` HttpContext.Request.Path`
Instead extract information from HttpContext such as path and then use the information in multiple threads


# Do not capture the HttpContext in background threads
Do not do this: The following example shows a closure is capturing the HttpContext from the Controller property. This is a bad practice because the work item could:
- Run outside of the request scope.
- Attempt to read the wrong HttpContext.

```
[HttpGet("/fire-and-forget-1")]
public IActionResult BadFireAndForget()
{
    _ = Task.Run(async () =>
    {
        await Task.Delay(1000);

        var path = HttpContext.Request.Path;
        Log(path);
    });

    return Accepted();
}
```

Do this

- Copies the data required in the background task during the request.
- Doesn't reference anything from the controller.

```
[HttpGet("/fire-and-forget-3")]
public IActionResult GoodFireAndForget()
{
    string path = HttpContext.Request.Path;
    _ = Task.Run(async () =>
    {
        await Task.Delay(1000);

        Log(path);
    });

    return Accepted();
}
```
# Do not capture services injected into the controllers on background threads
instead inject IServiceScopeFactory and create a new dependency scope

Do not do this: The following example shows a closure that is capturing the DbContext from the Controller action parameter. This is a bad practice. 
The work item could run outside of the request scope. The ContosoDbContext is scoped to the request, resulting in an ObjectDisposedException.

```
[HttpGet("/fire-and-forget-1")]
public IActionResult FireAndForget1([FromServices]ContosoDbContext context)
{
    _ = Task.Run(async () =>
    {
        await Task.Delay(1000);

        context.Contoso.Add(new Contoso());
        await context.SaveChangesAsync();
    });

    return Accepted();
}
```

## Do this: The following example

- Injects an IServiceScopeFactory in order to create a scope in the background work item. IServiceScopeFactory is a singleton.
- Creates a new dependency injection scope in the background thread.
- Doesn't reference anything from the controller.
- Doesn't capture the ContosoDbContext from the incoming request.
```
[HttpGet("/fire-and-forget-3")]
public IActionResult FireAndForget3([FromServices]IServiceScopeFactory 
                                    serviceScopeFactory)
{
    _ = Task.Run(async () =>
    {
        await Task.Delay(1000);

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ContosoDbContext>();

            context.Contoso.Add(new Contoso());

            await context.SaveChangesAsync();                                        
        }
    });

    return Accepted();
}

```

# Don't assume that HttpRequest.ContentLength is not null

`HttpRequest.ContentLength` is null if the Content-Length header is not received. 
Null in that case means the length of the request body is not known; it doesn't mean the length is zero. 
Because all comparisons with null (except ==) return false, the comparison `Request.ContentLength > 1024`, for example, `might return false when the request body size is more than 1024`. 
Not knowing this can lead to security holes in apps. You might think you're guarding against too-large requests when you aren't.