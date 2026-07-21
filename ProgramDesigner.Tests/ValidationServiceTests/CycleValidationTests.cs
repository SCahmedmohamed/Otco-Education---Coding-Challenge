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
    public class CycleValidationTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ProgramEntity>> _programRepoMock;
        private readonly ValidationService _sut;

        public CycleValidationTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _programRepoMock = new Mock<IGenericRepository<ProgramEntity>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<ProgramEntity>()).Returns(_programRepoMock.Object);
            _sut = new ValidationService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task OctoChallenge_DirectPrerequisiteCycle_ShouldBeInvalidWithCircularDependencyError()
        {
            // Arrange
            var program = TestDataFactory.CreateDirectCycleProgram();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Circular dependency detected"));
        }

        [Fact]
        public async Task ValidateProgramAsync_WithIndirectCycle_ShouldFailValidation()
        {
            // Arrange (A -> B -> C -> A)
            Guid stepAId = Guid.NewGuid();
            Guid stepBId = Guid.NewGuid();
            Guid stepCId = Guid.NewGuid();

            var stepA = new StepBuilder().WithId(stepAId).WithName("A").WithPrerequisiteId(stepCId).Build();
            var stepB = new StepBuilder().WithId(stepBId).WithName("B").WithPrerequisiteId(stepAId).Build();
            var stepC = new StepBuilder().WithId(stepCId).WithName("C").WithPrerequisiteId(stepBId).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(stepA)
                .AddChild(stepB)
                .AddChild(stepC)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Indirect Cycle")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Circular dependency detected"));
        }

        [Fact]
        public async Task ValidateProgramAsync_WithMultipleIndependentCycles_ShouldReportMultipleErrors()
        {
            // Arrange: Cycle 1 (A1 -> B1 -> A1) and Cycle 2 (A2 -> B2 -> A2)
            Guid a1Id = Guid.NewGuid(), b1Id = Guid.NewGuid();
            Guid a2Id = Guid.NewGuid(), b2Id = Guid.NewGuid();

            var a1 = new StepBuilder().WithId(a1Id).WithName("A1").WithPrerequisiteId(b1Id).Build();
            var b1 = new StepBuilder().WithId(b1Id).WithName("B1").WithPrerequisiteId(a1Id).Build();
            var a2 = new StepBuilder().WithId(a2Id).WithName("A2").WithPrerequisiteId(b2Id).Build();
            var b2 = new StepBuilder().WithId(b2Id).WithName("B2").WithPrerequisiteId(a2Id).Build();

            var group1 = new GroupBuilder().WithName("Group 1").AsInOrder().AddChild(a1).AddChild(b1).Build();
            var group2 = new GroupBuilder().WithName("Group 2").AsInOrder().AddChild(a2).AddChild(b2).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(group1)
                .AddChild(group2)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Multiple Cycles")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Errors.Should().Contain(e => e.Contains("Circular dependency detected for node 'A1'") || e.Contains("Circular dependency detected for node 'B1'"));
            result.Errors.Should().Contain(e => e.Contains("Circular dependency detected for node 'A2'") || e.Contains("Circular dependency detected for node 'B2'"));
        }

        [Fact]
        public async Task ValidateProgramAsync_WithCycleBetweenGroupAndStep_ShouldFailValidation()
        {
            // Arrange: Step in Group depends on Group, Group depends on Step
            Guid groupId = Guid.NewGuid();
            Guid stepId = Guid.NewGuid();

            var step = new StepBuilder().WithId(stepId).WithName("Child Step").WithPrerequisiteId(groupId).Build();

            var group = new GroupBuilder()
                .WithId(groupId)
                .WithName("Parent Group")
                .WithPrerequisiteId(stepId)
                .AsInOrder()
                .AddChild(step)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(group)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Group Step Cycle")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Circular dependency detected"));
        }
    }
}
