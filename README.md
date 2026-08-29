[![](https://img.shields.io/nuget/v/soenneker.csv.sepcsvutil.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.csv.sepcsvutil/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.csv.sepcsvutil/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.csv.sepcsvutil/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.csv.sepcsvutil.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.csv.sepcsvutil/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.csv.sepcsvutil/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.csv.sepcsvutil/actions/workflows/codeql.yml)

# Soenneker.Csv.SepCsvUtil

Using the Sep CSV library, provides methods for reading and writing CSV files using strongly-typed objects with automatic property mapping and basic type conversion.

## Install

```bash
dotnet add package Soenneker.Csv.SepCsvUtil
```

## Quick start

```csharp
using Soenneker.Csv.SepCsvUtil.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddSepCsvUtilAsSingleton();
```

Adds `ISepCsvUtil` as a singleton service.

## What you get

- `ISepCsvUtil` — Using the Sep CSV library, provides methods for reading and writing CSV files using strongly-typed objects with automatic property mapping and basic type conversion.
- `SepCsvUtilRegistrar` — Using the Sep CSV library, provides methods for reading and writing CSV files using strongly-typed objects with automatic property mapping and basic type conversion.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `ISepCsvUtil.Read(path)` | Reads a delimited (CSV) file and deserializes each row into an instance of type `T`. The type `T` must have a parameterless constructor and public settable properties matching the CSV column headers by name. | A list of deserialized objects of type `T`. |
| `ISepCsvUtil.Write(objects, filePath)` | Writes a list of objects to a delimited (CSV) file. The public properties of each object will be serialized as columns. | Returns no value; the requested change is complete when the method returns. |
| `SepCsvUtilRegistrar.AddSepCsvUtilAsSingleton(services)` | Adds `ISepCsvUtil` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `SepCsvUtilRegistrar.AddSepCsvUtilAsScoped(services)` | Adds `ISepCsvUtil` as a scoped service. | The same service collection, so additional registrations can be chained. |
