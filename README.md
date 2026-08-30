[![](https://img.shields.io/nuget/v/soenneker.csv.sepcsvutil.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.csv.sepcsvutil/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.csv.sepcsvutil/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.csv.sepcsvutil/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.csv.sepcsvutil.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.csv.sepcsvutil/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.csv.sepcsvutil/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.csv.sepcsvutil/actions/workflows/codeql.yml)

# Soenneker.Csv.SepCsvUtil

Strongly typed CSV file reading and writing built on the high-performance Sep library.

## Installation

```bash
dotnet add package Soenneker.Csv.SepCsvUtil
```

## Registration

```csharp
using Soenneker.Csv.SepCsvUtil.Abstract;
using Soenneker.Csv.SepCsvUtil.Registrars;

services.AddSepCsvUtilAsSingleton();

ISepCsvUtil csv = serviceProvider.GetRequiredService<ISepCsvUtil>();
```

`AddSepCsvUtilAsScoped()` is also available. The utility caches reflection metadata and compiled parameterless constructors by model type, so singleton registration is normally appropriate.

## Define a row model

```csharp
public sealed class PersonRow
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal Balance { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Read-only properties are ignored.
    public string DisplayName => $"{Name} ({Age})";
}
```

Mapped properties must be public, readable, settable, and non-indexed. Reference types require a public parameterless constructor; value types are created with their default value. CSV headers are matched to property names.

## Write and read

```csharp
var rows = new List<PersonRow>
{
    new()
    {
        Name = "Alice",
        Age = 30,
        Balance = 12.50m,
        CreatedAt = DateTimeOffset.UtcNow
    }
};

csv.Write(rows, "people.csv");

List<PersonRow> restored = csv.Read<PersonRow>("people.csv");
```

Writing always produces comma-separated output. Sep handles field quoting for commas, quotes, and line breaks. Existing files are replaced by the writer.

Numeric and date-like values are written with invariant culture. `DateTime` and `DateTimeOffset` use round-trip format, and `TimeSpan` uses the constant format. Reading uses invariant conversion for primitives, decimals, booleans, enums, GUIDs, date/time types, and other types supported by `Soenneker.Extensions.Type`.

An empty string cell is preserved for a `string` property. Blank non-string cells leave the property's default value. A nonblank value that cannot be converted throws `InvalidOperationException` instead of silently substituting a default.

## Operational notes

`Read<T>` materializes the entire file into a `List<T>`, and `Write<T>` accepts a complete list. Both APIs perform synchronous file I/O and do not expose cancellation. Use a streaming Sep API directly for very large files or asynchronous pipeline requirements.

CSV escaping is not spreadsheet formula sanitization. If files will be opened in Excel or another spreadsheet, neutralize untrusted values beginning with formula characters such as `=`, `+`, `-`, or `@` according to your application's import policy.

File access, missing headers, malformed CSV, constructor failures, and conversion errors propagate to the caller.
