using System;

namespace Soenneker.Csv.SepCsvUtil.Tests.Dtos
{
    public class Person
    {
        public string Name { get; set; } = default!;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
