using System.Collections.Generic;

namespace Soenneker.Csv.SepCsvUtil.Abstract;

/// <summary>
/// Reads and writes comma-separated files by mapping headers to public readable and settable properties.
/// </summary>
public interface ISepCsvUtil
{
    /// <summary>
    /// Reads a delimited (CSV) file and deserializes each row into an instance of type <typeparamref name="T"/>.
    /// The type <typeparamref name="T"/> must have a parameterless constructor and public settable properties
    /// matching the CSV column headers by name. Nonblank values that cannot be converted cause the read to fail.
    /// </summary>
    /// <typeparam name="T">The type to deserialize each row into.</typeparam>
    /// <param name="path">The full file path of the CSV file to read.</param>
    /// <returns>A list of deserialized objects of type <typeparamref name="T"/>.</returns>
    List<T> Read<T>(string path);

    /// <summary>
    /// Writes objects to a comma-separated file using invariant formatting. Public readable and settable properties become columns.
    /// </summary>
    /// <typeparam name="T">The type of objects to serialize.</typeparam>
    /// <param name="objects">The list of objects to write to the file.</param>
    /// <param name="filePath">The full file path to write the CSV output to.</param>
    void Write<T>(List<T> objects, string filePath);
}
