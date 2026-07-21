using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ProgramDesigner.Tests.Builders
{
    public class GroupBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _name = "Default Group";
        private Guid _programId = Guid.NewGuid();
        private Guid? _parentGroupId;
        private Guid? _prerequisiteId;
        private GroupRule _rule = GroupRule.InOrder;
        private int? _pickCount;
        private readonly List<ProgramNode> _children = new();

        public GroupBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public GroupBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public GroupBuilder WithProgramId(Guid programId)
        {
            _programId = programId;
            return this;
        }

        public GroupBuilder WithParentGroupId(Guid? parentGroupId)
        {
            _parentGroupId = parentGroupId;
            return this;
        }

        public GroupBuilder WithPrerequisiteId(Guid? prerequisiteId)
        {
            _prerequisiteId = prerequisiteId;
            return this;
        }

        public GroupBuilder WithRule(GroupRule rule)
        {
            _rule = rule;
            return this;
        }

        public GroupBuilder WithPickCount(int? pickCount)
        {
            _pickCount = pickCount;
            return this;
        }

        public GroupBuilder AsInOrder()
        {
            _rule = GroupRule.InOrder;
            _pickCount = null;
            return this;
        }

        public GroupBuilder AsChoice(int pickCount = 1)
        {
            _rule = GroupRule.Choice;
            _pickCount = pickCount;
            return this;
        }

        public GroupBuilder AddChild(ProgramNode child)
        {
            _children.Add(child);
            return this;
        }

        public GroupBuilder AddChildren(IEnumerable<ProgramNode> children)
        {
            _children.AddRange(children);
            return this;
        }

        public Group Build()
        {
            var group = new Group
            {
                Id = _id,
                Name = _name,
                ProgramId = _programId,
                ParentGroupId = _parentGroupId,
                PrerequisiteId = _prerequisiteId,
                Rule = _rule,
                PickCount = _pickCount,
                Children = new List<ProgramNode>()
            };

            foreach (var child in _children)
            {
                child.ParentGroupId = _id;
                child.ProgramId = _programId;
                group.Children.Add(child);
            }

            return group;
        }
    }
}
