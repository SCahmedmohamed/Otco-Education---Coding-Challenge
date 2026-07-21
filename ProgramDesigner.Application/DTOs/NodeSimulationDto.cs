using ProgramDesigner.Domain.Enums;
using System;

namespace ProgramDesigner.Application.DTOs
{

    public class NodeSimulationDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string NodeType { get; set; } = null!;

        public NodeStatus Status { get; set; }


        public string? Reason { get; set; }
    }
}
