using ProgramDesigner.Domain.Entities;
using System;

namespace ProgramDesigner.Tests.Builders
{
    public class StepBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _name = "Default Step";
        private Guid _programId = Guid.NewGuid();
        private Guid? _parentGroupId;
        private Guid? _prerequisiteId;

        public StepBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public StepBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public StepBuilder WithProgramId(Guid programId)
        {
            _programId = programId;
            return this;
        }

        public StepBuilder WithParentGroupId(Guid? parentGroupId)
        {
            _parentGroupId = parentGroupId;
            return this;
        }

        public StepBuilder WithPrerequisiteId(Guid? prerequisiteId)
        {
            _prerequisiteId = prerequisiteId;
            return this;
        }

        public Step Build()
        {
            return new Step
            {
                Id = _id,
                Name = _name,
                ProgramId = _programId,
                ParentGroupId = _parentGroupId,
                PrerequisiteId = _prerequisiteId
            };
        }
    }
}
