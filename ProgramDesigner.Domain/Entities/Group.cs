using ProgramDesigner.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Domain.Entities
{
    public class Group : ProgramNode
    {
        public GroupRule Rule { get; set; }
        public int? PickCount { get; set; }
        public ICollection<ProgramNode> Children { get; set; } = new List<ProgramNode>();
        public ProgramEntity? Program { get; set; }
    }
}
