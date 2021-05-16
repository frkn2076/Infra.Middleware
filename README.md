# Infra.AspNetCore.RequestResponseLoggingMiddleware

.NET Middleware to log request and response about calling API.


## Log Properties:
* RequestPath
* RemoteIpAddress
* RequestHeaders
* RequestBody
* ResponseStatusCode
* ResponseBody
* CreatedAt

<br>


<br>
Add middleware in Startup.cs with your action

<br>
<br>

```csharp
public void Configure(IApplicationBuilder app)
{
    ...

    app.UseMiddleware<RequestResponseLoggingMiddleware>();

    ...
}
```
<br>

<br>
Or you can pass your string method as action parameter.
<br>
<br>

```csharp
public void Configure(IApplicationBuilder app)
{
    ...
    
    // Uses Console.WriteLine action with no parameter
    Action<string> action = AppDbContext.Logs.Insert;
    app.UseMiddleware<RequestResponseLoggingMiddleware>(action);

    ...
}
```
<br>

<br>
You can disable log properties. (It uses all log properties as default).
<br>
<br>

```csharp
public void Configure(IApplicationBuilder app)
{
    ...
    
    var logProperties = new LogProperties()
    {
        HasRequestPath = false,
        HasRemoteIpAddress = false,
        HasRequestHeaders = false
    };

    Action<string> action = Console.WriteLine;
    app.UseMiddleware<RequestResponseLoggingMiddleware>(action, logProperties);

    ...
}
```
<br>


See: https://www.nuget.org/packages/Infra.AspNetCore.RequestResponseLoggingMiddleware
