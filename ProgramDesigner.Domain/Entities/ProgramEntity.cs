using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Domain.Entities
{
    public class ProgramEntity : BaseEntity
    {
        public string Name { get; set; } = null!;
        public Guid? RootGroupId { get; set; }
        public Group? RootGroup { get; set; }
    }
}
