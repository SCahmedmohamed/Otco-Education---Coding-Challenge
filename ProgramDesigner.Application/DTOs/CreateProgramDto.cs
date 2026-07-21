using System;

namespace ProgramDesigner.Application.DTOs
{
    public class CreateProgramDto
    {
        public string Name { get; set; }
        public ProgramNodeDto RootGroup { get; set; }
    }
}
