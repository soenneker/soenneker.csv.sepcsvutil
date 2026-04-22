using Soenneker.Csv.SepCsvUtil.Abstract;
using Soenneker.Tests.HostedUnit;
using System;
using System.Collections.Generic;
using System.IO;
using Soenneker.Csv.SepCsvUtil.Tests.Dtos;
using AwesomeAssertions;

namespace Soenneker.Csv.SepCsvUtil.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class SepCsvUtilTests : HostedUnitTest
{
    private readonly ISepCsvUtil _csvUtil;

    public SepCsvUtilTests(Host host) : base(host)
    {
        _csvUtil = Resolve<ISepCsvUtil>(true);
    }

    [Test]
    public void Default()
    {
        // This test is intentionally left blank.
    }

    [Test]
    public void Write_And_Read_Should_Preserve_Data()
    {
        // Arrange
        string tempPath = Path.Combine(Path.GetTempPath(), $"people_{Guid.NewGuid()}.csv");

        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30, IsActive = true, BirthDate = new DateTime(1994, 4, 1) },
            new Person { Name = "Bob", Age = 45, IsActive = false, BirthDate = new DateTime(1979, 7, 15) },
        };

        // Act
        _csvUtil.Write(people, tempPath);
        List<Person> result = _csvUtil.Read<Person>(tempPath);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result[0].Name.Should().Be("Alice");
        result[0].Age.Should().Be(30);
        result[0].IsActive.Should().BeTrue();
        result[0].BirthDate.Should().Be(new DateTime(1994, 4, 1));

        result[1].Name.Should().Be("Bob");
        result[1].Age.Should().Be(45);
        result[1].IsActive.Should().BeFalse();
        result[1].BirthDate.Should().Be(new DateTime(1979, 7, 15));

        // Cleanup
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Test]
    public void Read_Should_Handle_Empty_File()
    {
        // Arrange
        string tempPath = Path.Combine(Path.GetTempPath(), $"empty_{Guid.NewGuid()}.csv");
        File.WriteAllText(tempPath, string.Empty);

        // Act
        List<Person> result = _csvUtil.Read<Person>(tempPath);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        File.Delete(tempPath);
    }

    [Test]
    public void Write_Should_Create_File()
    {
        // Arrange
        string tempPath = Path.Combine(Path.GetTempPath(), $"write_{Guid.NewGuid()}.csv");
        var people = new List<Person>
        {
            new Person { Name = "Charlie", Age = 20, IsActive = true, BirthDate = DateTime.Today }
        };

        // Act
        _csvUtil.Write(people, tempPath);

        // Assert
        File.Exists(tempPath).Should().BeTrue();

        string content = File.ReadAllText(tempPath);
        content.Should().Contain("Charlie");

        File.Delete(tempPath);
    }
}
