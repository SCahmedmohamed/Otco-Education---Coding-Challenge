using System;

namespace ProgramDesigner.Application.DTOs
{
    public class ProgramDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? RootGroupId { get; set; }
        public ProgramNodeDto RootGroup { get; set; }
    }
}
