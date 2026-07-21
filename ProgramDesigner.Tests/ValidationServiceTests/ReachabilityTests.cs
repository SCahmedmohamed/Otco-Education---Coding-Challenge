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
    public class ReachabilityTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ProgramEntity>> _programRepoMock;
        private readonly ValidationService _sut;

        public ReachabilityTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _programRepoMock = new Mock<IGenericRepository<ProgramEntity>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<ProgramEntity>()).Returns(_programRepoMock.Object);
            _sut = new ValidationService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task OctoChallenge_ReachabilityWarning_ShouldReturnWarningOnlyAndRemainValid()
        {
            // Arrange
            var program = TestDataFactory.CreateReachabilityWarningProgram();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            // Course A is inside SubGroup (child of Choice 'Electives' PickCount=1).
            // Course A's prereq chain pulls in Course B (also a direct child of 'Electives').
            // Active items from 'Electives': SubGroup + Course B = 2 > PickCount 1 → Warning.
            // 'Electives' is NOT Course A's DIRECT parent (SubGroup is) → Warning, not Error.
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().HaveCount(1);
            result.Warnings.Should().ContainSingle()
                .Which.Should().Be("Reachability warning: 'Course A' is unreachable because it requires 2 nodes from Choice group 'Electives' (PickCount: 1).");
        }

        [Fact]
        public async Task ValidateProgramAsync_SafeCrossBranchPrerequisite_NoChoiceViolation_ShouldHaveNoWarnings()
        {
            // Arrange (Root Choice Group PickCount 2; Course A in SubGroup requires Course B; Course B is in Root Choice Group; Active set count for Choice group is 2 -> allowed!)
            Guid courseAId = Guid.NewGuid();
            Guid courseBId = Guid.NewGuid();

            var courseA = new StepBuilder().WithId(courseAId).WithName("Course A").WithPrerequisiteId(courseBId).Build();
            var courseB = new StepBuilder().WithId(courseBId).WithName("Course B").WithPrerequisiteId(null).Build();

            var subGroup = new GroupBuilder().WithName("SubGroup").AsInOrder().AddChild(courseA).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root Choice")
                .AsChoice(pickCount: 2)
                .AddChild(subGroup)
                .AddChild(courseB)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Safe Cross Branch")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateProgramAsync_MultipleUnreachableNodes_ShouldGenerateMultipleWarnings()
        {
            // Arrange (Choice root pickCount 1; Node X and Node Y both in separate sub-groups requiring multiple nodes from root choice group)
            Guid nodeBId = Guid.NewGuid(), nodeCId = Guid.NewGuid(), nodeDId = Guid.NewGuid(), nodeEId = Guid.NewGuid();

            var nodeB = new StepBuilder().WithId(nodeBId).WithName("Node B").Build();
            var nodeC = new StepBuilder().WithId(nodeCId).WithName("Node C").Build();
            var nodeD = new StepBuilder().WithId(nodeDId).WithName("Node D").Build();
            var nodeE = new StepBuilder().WithId(nodeEId).WithName("Node E").Build();

            var stepX = new StepBuilder().WithName("Step X").WithPrerequisiteId(nodeBId).Build();
            var stepY = new StepBuilder().WithName("Step Y").WithPrerequisiteId(nodeDId).Build();

            var subGroup1 = new GroupBuilder().WithName("Sub1").AsInOrder().AddChild(stepX).Build();
            var subGroup2 = new GroupBuilder().WithName("Sub2").AsInOrder().AddChild(stepY).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root Choice")
                .AsChoice(pickCount: 1)
                .AddChild(subGroup1)
                .AddChild(subGroup2)
                .AddChild(nodeB)
                .AddChild(nodeC)
                .AddChild(nodeD)
                .AddChild(nodeE)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Multi Warning Program")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().HaveCount(2);
            result.Warnings.Should().Contain(w => w.Contains("'Step X' is unreachable"));
            result.Warnings.Should().Contain(w => w.Contains("'Step Y' is unreachable"));
        }
    }
}
