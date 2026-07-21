using FluentAssertions;
using Moq;
using ProgramDesigner.Application.Services;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Tests.Builders;
using ProgramDesigner.Tests.Factories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ProgramDesigner.Tests.ValidationServiceTests
{
    public class SelfReferenceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ProgramEntity>> _programRepoMock;
        private readonly ValidationService _sut;

        public SelfReferenceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _programRepoMock = new Mock<IGenericRepository<ProgramEntity>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<ProgramEntity>()).Returns(_programRepoMock.Object);
            _sut = new ValidationService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task OctoChallenge_SelfPrerequisite_ShouldBeInvalidWithSelfDependError()
        {
            // Arrange
            var program = TestDataFactory.CreateSelfPrerequisiteProgram();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            // The ValidationService detects both self-dependency and circular dependency for self-referencing nodes
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Node 'Programming Basics' cannot depend on itself.");
            result.Errors.Should().Contain(e => e.Contains("Circular dependency detected for node 'Programming Basics'"));
        }

        [Fact]
        public async Task ValidateProgramAsync_WhenStepDependsOnItself_ShouldFailValidation()
        {
            // Arrange
            Guid stepId = Guid.NewGuid();
            var step = new StepBuilder()
                .WithId(stepId)
                .WithName("Physics 101")
                .WithPrerequisiteId(stepId)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(step)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Self Step Dependency")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Node 'Physics 101' cannot depend on itself.");
        }

        [Fact]
        public async Task ValidateProgramAsync_WhenGroupDependsOnItself_ShouldFailValidation()
        {
            // Arrange
            Guid groupId = Guid.NewGuid();
            var subGroup = new GroupBuilder()
                .WithId(groupId)
                .WithName("Core Electives")
                .WithPrerequisiteId(groupId)
                .AsInOrder()
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(subGroup)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Self Group Dependency")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Node 'Core Electives' cannot depend on itself.");
        }

        [Fact]
        public async Task ValidateProgramAsync_WhenDeepNestedStepDependsOnItself_ShouldFailValidation()
        {
            // Arrange
            Guid stepId = Guid.NewGuid();
            var selfStep = new StepBuilder()
                .WithId(stepId)
                .WithName("Deep Self Step")
                .WithPrerequisiteId(stepId)
                .Build();

            var level2Group = new GroupBuilder().WithName("Level 2").AsInOrder().AddChild(selfStep).Build();
            var level1Group = new GroupBuilder().WithName("Level 1").AsInOrder().AddChild(level2Group).Build();
            var rootGroup = new GroupBuilder().WithName("Root").AsInOrder().AddChild(level1Group).Build();

            var program = new ProgramBuilder()
                .WithName("Deep Self Dependency")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Node 'Deep Self Step' cannot depend on itself.");
        }
    }
}
