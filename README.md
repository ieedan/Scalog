# Scalog
A logging package built to make logging to a file or database easier than it should be.

### Logging to a file is easy!

```csharp
using Scalog;

var logger = new Logger();

logger.LogInfo("Welcome to Scalog");
```

### Log to a database with just a connection string!
Scalog will handle the rest for you

```csharp
using Scalog;

string connString = "yourConnectionStringHere";

var logger = new Logger(connString);

logger.LogInfo("Welcome to Scalog");
```

### Scalog can also change its behavior based on the environment
By setting the **isDev** in the constructor it will now log to a file in development and to the database in production

```csharp
using Scalog;

string connString = "yourConnectionStringHere";

var logger = new Logger(connString,false);

logger.LogInfo("Welcome to Scalog");
```

### Dependency Injection
Heres an example of how you might use it in you ASP.Net API.

```csharp
using Scalog;

var builder = WebApplication.CreateBuilder(args);

var connectionStrings = new ConnectionStrings();
builder.Configuration.Bind("ConnectionStrings", connectionStrings);
builder.Services.AddSingleton(connectionStrings);

var logger = new Logger(connectionStrings.MyDatabase, builder.Environment.IsDevelopment());

builder.Services.AddSingleton(logger);
```

### Logs are configured to be easy to sort by ERROR or INFO
![image](https://user-images.githubusercontent.com/117548273/236574008-7d223374-4415-4b27-a3f9-16ac835ae6a5.png)

### Methods

- `LogInfo(string message, string type = "INFO")` will log with the default type of "INFO"
- `LogInfoAsync(string message, string type = "INFO")` will log with the default type of "INFO"
- `LogError(string message, string type = "ERROR", [CallerMemberName] string? name = null)` will log with the default type of "ERROR"
- `LogErrorAsync(string message, string type = "ERROR", [CallerMemberName] string? name = null)` will log with the default type of "ERROR"
