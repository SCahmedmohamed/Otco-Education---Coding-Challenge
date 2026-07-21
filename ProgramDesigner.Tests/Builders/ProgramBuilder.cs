using ProgramDesigner.Domain.Entities;
using System;

namespace ProgramDesigner.Tests.Builders
{
    public class ProgramBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _name = "Default Program";
        private Group? _rootGroup;

        public ProgramBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public ProgramBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ProgramBuilder WithRootGroup(Group rootGroup)
        {
            _rootGroup = rootGroup;
            return this;
        }

        public ProgramEntity Build()
        {
            var program = new ProgramEntity
            {
                Id = _id,
                Name = _name,
                RootGroupId = _rootGroup?.Id,
                RootGroup = _rootGroup
            };

            if (_rootGroup != null)
            {
                _rootGroup.ProgramId = _id;
                FixProgramIds(_rootGroup, _id);
            }

            return program;
        }

        private static void FixProgramIds(ProgramNode node, Guid programId)
        {
            node.ProgramId = programId;
            if (node is Group group && group.Children != null)
            {
                foreach (var child in group.Children)
                {
                    child.ProgramId = programId;
                    child.ParentGroupId = group.Id;
                    FixProgramIds(child, programId);
                }
            }
        }
    }
}
