using ProgramDesigner.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ProgramDesigner.Application.DTOs
{
    public class ProgramNodeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? PrerequisiteId { get; set; }
        public Guid? ParentGroupId { get; set; }
        
        public bool IsGroup { get; set; }
        
        public GroupRule? Rule { get; set; }
        public int? PickCount { get; set; }
        public List<ProgramNodeDto> Children { get; set; } = new List<ProgramNodeDto>();
    }
}
