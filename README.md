# Infra.Middleware

.NET Middleware to log request and response about calling API.

* #### PM> Install-Package Infra.AspNetCore.RequestResponseLoggingMiddleware -Version 1.0.0
<br>
Add middleware in Startup.cs with your action

<br>
<br>

```csharp
public void Configure(IApplicationBuilder app)
{
    ...

    Action<string> action = Console.WriteLine;
    app.UseMiddleware<RequestResponseLoggingMiddleware>(action);

    ...
}
```
<br>
See: https://www.nuget.org/packages/Infra.AspNetCore.RequestResponseLoggingMiddleware
