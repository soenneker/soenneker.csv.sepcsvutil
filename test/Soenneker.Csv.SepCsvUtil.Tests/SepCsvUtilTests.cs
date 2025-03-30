using Soenneker.Csv.SepCsvUtil.Abstract;
using Soenneker.Tests.FixturedUnit;
using System;
using System.Collections.Generic;
using System.IO;
using Soenneker.Csv.SepCsvUtil.Tests.Dtos;
using Xunit;
using FluentAssertions;

namespace Soenneker.Csv.SepCsvUtil.Tests;

[Collection("Collection")]
public class SepCsvUtilTests : FixturedUnitTest
{
    private readonly ISepCsvUtil _csvUtil;

    public SepCsvUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _csvUtil = Resolve<ISepCsvUtil>(true);
    }

    [Fact]
    public void Default()
    {
        // This test is intentionally left blank.
    }

    [Fact]
    public void Write_And_Read_Should_Preserve_Data()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"people_{Guid.NewGuid()}.csv");

        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30, IsActive = true, BirthDate = new DateTime(1994, 4, 1) },
            new Person { Name = "Bob", Age = 45, IsActive = false, BirthDate = new DateTime(1979, 7, 15) },
        };

        // Act
        _csvUtil.Write(people, tempPath);
        var result = _csvUtil.Read<Person>(tempPath);

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

    [Fact]
    public void Read_Should_Handle_Empty_File()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"empty_{Guid.NewGuid()}.csv");
        File.WriteAllText(tempPath, string.Empty);

        // Act
        var result = _csvUtil.Read<Person>(tempPath);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        File.Delete(tempPath);
    }

    [Fact]
    public void Write_Should_Create_File()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"write_{Guid.NewGuid()}.csv");
        var people = new List<Person>
        {
            new Person { Name = "Charlie", Age = 20, IsActive = true, BirthDate = DateTime.Today }
        };

        // Act
        _csvUtil.Write(people, tempPath);

        // Assert
        File.Exists(tempPath).Should().BeTrue();

        var content = File.ReadAllText(tempPath);
        content.Should().Contain("Charlie");

        File.Delete(tempPath);
    }
}
