using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Domain.Entities
{
    public abstract class ProgramNode : BaseEntity
    {
        public string Name { get; set; }
        public Guid ProgramId { get; set; }
        public ProgramEntity Program {  get; set; } 
        public Guid? ParentGroupId { get; set; }
        public Group? ParentGroup { get; set; }
        public Guid? PrerequisiteId { get; set; }
        public ProgramNode? Prerequisite { get; set; }
    }
}
