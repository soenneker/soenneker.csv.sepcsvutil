using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Csv.SepCsvUtil.Abstract;

namespace Soenneker.Csv.SepCsvUtil.Registrars;

/// <summary>
/// Using the Sep CSV library, provides methods for reading and writing CSV files using strongly-typed objects with automatic property mapping and basic type conversion
/// </summary>
public static class SepCsvUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ISepCsvUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddSepCsvUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<ISepCsvUtil, SepCsvUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="ISepCsvUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddSepCsvUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<ISepCsvUtil, SepCsvUtil>();

        return services;
    }
}
