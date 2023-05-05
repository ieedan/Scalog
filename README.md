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
By setting the alwaysWriteToDatabase in the constructor it will now log to a file in development and to the database in development

```csharp
using Scalog;

string connString = "yourConnectionStringHere";

var logger = new Logger(connString, "Logs",false);

logger.LogInfo("Welcome to Scalog");
```
