using FluentAssertions;
using Moq;
using ProgramDesigner.Application.DTOs;
using ProgramDesigner.Application.Services;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Domain.Enums;
using ProgramDesigner.Tests.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProgramDesigner.Tests.SimulationServiceTests
{
    /// <summary>
    /// Unit tests for <see cref="SimulationService.SimulateAsync"/>.
    /// Every test builds an in-memory program tree using the existing fluent Builders,
    /// mocks <see cref="IUnitOfWork"/> to return it, then asserts the expected node states.
    /// </summary>
    public class SimulationServiceTests
    {
        // ─── Shared test infrastructure ──────────────────────────────────────────
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ProgramEntity>> _programRepoMock;
        private readonly SimulationService _sut;

        public SimulationServiceTests()
        {
            _unitOfWorkMock    = new Mock<IUnitOfWork>();
            _programRepoMock   = new Mock<IGenericRepository<ProgramEntity>>();
            _unitOfWorkMock
                .Setup(u => u.GetRepository<ProgramEntity>())
                .Returns(_programRepoMock.Object);
            _sut = new SimulationService(_unitOfWorkMock.Object);
        }

        // ─── Request factory helpers ─────────────────────────────────────────────

        private static SimulationRequestDto EmptyRequest() => new SimulationRequestDto();

        private static SimulationRequestDto RequestWith(
            IEnumerable<Guid>? completedItems = null,
            Dictionary<Guid, List<Guid>>? selectedChoices = null)
        {
            return new SimulationRequestDto
            {
                CompletedItems  = completedItems?.ToList() ?? new List<Guid>(),
                SelectedChoices = selectedChoices         ?? new Dictionary<Guid, List<Guid>>()
            };
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 1 — Happy path: every node is completed
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_HappyPath_AllNodesCompleted_ReturnsAllCompleted()
        {
            // Arrange
            var step1 = new StepBuilder().WithName("Step 1").Build();
            var step2 = new StepBuilder().WithName("Step 2").WithPrerequisiteId(step1.Id).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(step1)
                .AddChild(step2)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Participant has completed everything — group included
            var request = RequestWith(completedItems: new[] { rootGroup.Id, step1.Id, step2.Id });

            // Act
            var result = await _sut.SimulateAsync(program.Id, request);

            // Assert
            result.Should().NotBeNull();
            result.Nodes.Should().AllSatisfy(n => n.Status.Should().Be(NodeStatus.Completed));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 2 — Completed path: partial completion, next step becomes Unlocked
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_CompletedPath_Step1Done_Step2Unlocked_Step3Blocked()
        {
            // Arrange
            var step1 = new StepBuilder().WithName("Foundations").Build();
            var step2 = new StepBuilder().WithName("Intermediate").WithPrerequisiteId(step1.Id).Build();
            var step3 = new StepBuilder().WithName("Advanced").WithPrerequisiteId(step2.Id).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(step1)
                .AddChild(step2)
                .AddChild(step3)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            var request = RequestWith(completedItems: new[] { step1.Id });

            // Act
            var result = await _sut.SimulateAsync(program.Id, request);

            // Assert
            result.Nodes.Should().ContainSingle(n => n.Id == step1.Id)
                .Which.Status.Should().Be(NodeStatus.Completed);

            result.Nodes.Should().ContainSingle(n => n.Id == step2.Id)
                .Which.Status.Should().Be(NodeStatus.Unlocked);

            var step3State = result.Nodes.Single(n => n.Id == step3.Id);
            step3State.Status.Should().Be(NodeStatus.Blocked);
            step3State.Reason.Should().Be("Prerequisite 'Intermediate' is not completed.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 3 — Blocked prerequisite with human-readable reason
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_BlockedPrerequisite_ReasonContainsPrerequisiteName()
        {
            // Arrange
            var step1 = new StepBuilder().WithName("AI Capstone").Build();
            var step2 = new StepBuilder().WithName("Final Project").WithPrerequisiteId(step1.Id).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(step1)
                .AddChild(step2)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Participant has completed nothing
            var request = EmptyRequest();

            // Act
            var result = await _sut.SimulateAsync(program.Id, request);

            // Assert
            var finalProject = result.Nodes.Single(n => n.Id == step2.Id);
            finalProject.Status.Should().Be(NodeStatus.Blocked);
            finalProject.Reason.Should().Be("Prerequisite 'AI Capstone' is not completed.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 4 — Choice Group selection: AI chosen, IT and Programming Skipped
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_ChoiceGroup_AISelected_OtherBranchesSkipped()
        {
            // Arrange
            var ai          = new StepBuilder().WithName("AI").Build();
            var it          = new StepBuilder().WithName("IT").Build();
            var programming = new StepBuilder().WithName("Programming").Build();

            var majorGroup = new GroupBuilder()
                .WithName("Major")
                .AsChoice(pickCount: 1)
                .AddChild(ai)
                .AddChild(it)
                .AddChild(programming)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(majorGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            var request = RequestWith(selectedChoices: new Dictionary<Guid, List<Guid>>
            {
                [majorGroup.Id] = new List<Guid> { ai.Id }
            });

            // Act
            var result = await _sut.SimulateAsync(program.Id, request);

            // Assert
            result.Nodes.Should().ContainSingle(n => n.Id == ai.Id)
                .Which.Status.Should().Be(NodeStatus.Unlocked);

            result.Nodes.Should().ContainSingle(n => n.Id == it.Id)
                .Which.Status.Should().Be(NodeStatus.Skipped);

            result.Nodes.Should().ContainSingle(n => n.Id == programming.Id)
                .Which.Status.Should().Be(NodeStatus.Skipped);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 5 — Multiple independent Choice Groups, each respects its own selection
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_MultipleChoiceGroups_EachGroupRespectsItsOwnSelection()
        {
            // Arrange: Major (AI or IT) + Electives (CV or NLP), independent of each other
            var ai  = new StepBuilder().WithName("AI").Build();
            var it  = new StepBuilder().WithName("IT").Build();

            var majorGroup = new GroupBuilder()
                .WithName("Major")
                .AsChoice(pickCount: 1)
                .AddChild(ai)
                .AddChild(it)
                .Build();

            var vision = new StepBuilder().WithName("Computer Vision").Build();
            var nlp    = new StepBuilder().WithName("NLP").Build();

            var electivesGroup = new GroupBuilder()
                .WithName("Electives")
                .AsChoice(pickCount: 1)
                .AddChild(vision)
                .AddChild(nlp)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(majorGroup)
                .AddChild(electivesGroup)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            var request = RequestWith(selectedChoices: new Dictionary<Guid, List<Guid>>
            {
                [majorGroup.Id]    = new List<Guid> { ai.Id },
                [electivesGroup.Id] = new List<Guid> { vision.Id }
            });

            // Act
            var result = await _sut.SimulateAsync(program.Id, request);

            // Assert — Major group
            result.Nodes.Single(n => n.Id == ai.Id).Status.Should().Be(NodeStatus.Unlocked);
            result.Nodes.Single(n => n.Id == it.Id).Status.Should().Be(NodeStatus.Skipped);

            // Assert — Electives group
            result.Nodes.Single(n => n.Id == vision.Id).Status.Should().Be(NodeStatus.Unlocked);
            result.Nodes.Single(n => n.Id == nlp.Id).Status.Should().Be(NodeStatus.Skipped);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 6 — Nested Choice Groups: outer selection controls inner group access
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_NestedChoiceGroups_CorrectSkipPropagationThroughSubtree()
        {
            // Arrange:
            // outerChoice (Choice, pick 1)
            //   ├── innerChoice (Choice, pick 1)
            //   │     ├── AI
            //   │     └── IT        ← skipped (innerChoice is active, AI selected)
            //   └── Math            ← skipped (outerChoice selects innerChoice branch)
            var ai = new StepBuilder().WithName("AI").Build();
            var it = new StepBuilder().WithName("IT").Build();

            var innerChoice = new GroupBuilder()
                .WithName("Tech Electives")
                .AsChoice(pickCount: 1)
                .AddChild(ai)
                .AddChild(it)
                .Build();

            var math = new StepBuilder().WithName("Math").Build();

            var outerChoice = new GroupBuilder()
                .WithName("Track")
                .AsChoice(pickCount: 1)
                .AddChild(innerChoice)
                .AddChild(math)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(outerChoice).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            var request = RequestWith(selectedChoices: new Dictionary<Guid, List<Guid>>
            {
                [outerChoice.Id] = new List<Guid> { innerChoice.Id }, // select Tech branch
                [innerChoice.Id] = new List<Guid> { ai.Id }           // within Tech, select AI
            });

            // Act
            var result = await _sut.SimulateAsync(program.Id, request);

            // Assert
            result.Nodes.Single(n => n.Id == ai.Id).Status.Should().Be(NodeStatus.Unlocked);
            result.Nodes.Single(n => n.Id == it.Id).Status.Should().Be(NodeStatus.Skipped);
            result.Nodes.Single(n => n.Id == math.Id).Status.Should().Be(NodeStatus.Skipped);
            // innerChoice group itself is in the active branch of outerChoice → NOT skipped
            result.Nodes.Single(n => n.Id == innerChoice.Id).Status.Should().NotBe(NodeStatus.Skipped);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 7 — Skipped branches are never evaluated (even if they have unmet prereqs)
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_SkippedBranch_BlockedPrerequisiteInsideBranchStaysSkipped()
        {
            // Arrange:
            // root (InOrder)
            //   ├── foundation (no prereq) — active
            //   └── major (Choice, pick 1)
            //         ├── ai             — selected
            //         └── itGroup (InOrder)  — skipped
            //               └── itAdvanced (requires foundation, NOT completed)
            //                   → must remain Skipped, not Blocked
            var foundation = new StepBuilder().WithName("Foundation").Build();
            var ai         = new StepBuilder().WithName("AI").Build();
            var itAdvanced = new StepBuilder()
                .WithName("IT Advanced")
                .WithPrerequisiteId(foundation.Id) // prereq not completed
                .Build();

            var itGroup = new GroupBuilder()
                .WithName("IT Track")
                .AsInOrder()
                .AddChild(itAdvanced)
                .Build();

            var majorGroup = new GroupBuilder()
                .WithName("Major")
                .AsChoice(pickCount: 1)
                .AddChild(ai)
                .AddChild(itGroup)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(foundation)
                .AddChild(majorGroup)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Foundation not completed; AI selected → IT branch is skipped
            var request = RequestWith(selectedChoices: new Dictionary<Guid, List<Guid>>
            {
                [majorGroup.Id] = new List<Guid> { ai.Id }
            });

            // Act
            var result = await _sut.SimulateAsync(program.Id, request);

            // Assert — itAdvanced and itGroup must be Skipped, NOT Blocked
            result.Nodes.Single(n => n.Id == itAdvanced.Id).Status.Should().Be(NodeStatus.Skipped);
            result.Nodes.Single(n => n.Id == itGroup.Id).Status.Should().Be(NodeStatus.Skipped);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 8 — Deep recursion: 20-level linear chain, no stack overflow
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_DeepRecursion_20LevelChain_CorrectStatusesWithoutOverflow()
        {
            // Arrange: Step1 → Step2 → Step3 → ... → Step20 (linear prerequisite chain)
            var steps = new List<Step>();
            Step? prev = null;

            for (int i = 0; i < 20; i++)
            {
                var step = new StepBuilder()
                    .WithName($"Step {i + 1}")
                    .WithPrerequisiteId(prev?.Id)
                    .Build();
                steps.Add(step);
                prev = step;
            }

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChildren(steps)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Only the first step is completed
            var request = RequestWith(completedItems: new[] { steps[0].Id });

            // Act — must not throw StackOverflowException
            var result = await _sut.SimulateAsync(program.Id, request);

            // Assert
            result.Should().NotBeNull();
            result.Nodes.Single(n => n.Id == steps[0].Id).Status.Should().Be(NodeStatus.Completed);
            result.Nodes.Single(n => n.Id == steps[1].Id).Status.Should().Be(NodeStatus.Unlocked);

            // Steps 3–20 are Blocked because their immediate prerequisite is not completed
            for (int i = 2; i < 20; i++)
            {
                var nodeState = result.Nodes.Single(n => n.Id == steps[i].Id);
                nodeState.Status.Should().Be(NodeStatus.Blocked,
                    because: $"Step {i + 1} requires Step {i} which is not completed");
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 9 — Empty program (no RootGroup): returns empty Nodes list
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_EmptyProgram_NoRootGroup_ReturnsEmptyNodeList()
        {
            // Arrange
            var program = new ProgramBuilder().Build(); // deliberately no root group
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.SimulateAsync(program.Id, EmptyRequest());

            // Assert
            result.Should().NotBeNull();
            result.Nodes.Should().BeEmpty();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 10 — Invalid completedItem IDs: throws ArgumentException
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_CompletedItemNotInProgram_ThrowsArgumentException()
        {
            // Arrange
            var step = new StepBuilder().WithName("Step 1").Build();
            var rootGroup = new GroupBuilder().WithName("Root").AsInOrder().AddChild(step).Build();
            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            var bogusId = Guid.NewGuid();
            var request = RequestWith(completedItems: new[] { bogusId });

            // Act
            Func<Task> act = () => _sut.SimulateAsync(program.Id, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Completed item '{bogusId}' does not belong to this program.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 11 — Invalid selectedChoices key (group not in program): throws
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_SelectedChoiceGroupNotInProgram_ThrowsArgumentException()
        {
            // Arrange
            var step = new StepBuilder().WithName("Step 1").Build();
            var rootGroup = new GroupBuilder().WithName("Root").AsInOrder().AddChild(step).Build();
            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            var bogusGroupId = Guid.NewGuid();
            var request = RequestWith(selectedChoices: new Dictionary<Guid, List<Guid>>
            {
                [bogusGroupId] = new List<Guid> { Guid.NewGuid() }
            });

            // Act
            Func<Task> act = () => _sut.SimulateAsync(program.Id, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Selected choice group '{bogusGroupId}' does not belong to this program.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 12 — selectedChoices references a non-Choice group: throws
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_SelectedChoicesKeyIsNotChoiceGroup_ThrowsArgumentException()
        {
            // Arrange — rootGroup is InOrder, not a Choice group
            var step = new StepBuilder().WithName("Step 1").Build();
            var rootGroup = new GroupBuilder().WithName("Root").AsInOrder().AddChild(step).Build();
            var program = new ProgramBuilder().WithRootGroup(rootGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Participant incorrectly references the InOrder group as if it were a Choice group
            var request = RequestWith(selectedChoices: new Dictionary<Guid, List<Guid>>
            {
                [rootGroup.Id] = new List<Guid> { step.Id }
            });

            // Act
            Func<Task> act = () => _sut.SimulateAsync(program.Id, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Node '{rootGroup.Name}' ('{rootGroup.Id}') is not a Choice group.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 13 — Program not found: returns null (controller maps to 404)
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_ProgramNotFound_ReturnsNull()
        {
            // Arrange
            var missingId = Guid.NewGuid();
            _programRepoMock
                .Setup(r => r.GetAsync(missingId))
                .ReturnsAsync((ProgramEntity)null!);

            // Act
            var result = await _sut.SimulateAsync(missingId, EmptyRequest());

            // Assert — null signals 404 to the controller, matching ProgramService convention
            result.Should().BeNull();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 14 — Node type is correctly set in the response
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_NodeTypes_GroupAndStepAreLabelledCorrectly()
        {
            // Arrange
            var step = new StepBuilder().WithName("My Step").Build();

            var group = new GroupBuilder()
                .WithName("My Group")
                .AsInOrder()
                .AddChild(step)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(group).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.SimulateAsync(program.Id, EmptyRequest());

            // Assert
            result.Nodes.Single(n => n.Id == group.Id).NodeType.Should().Be("Group");
            result.Nodes.Single(n => n.Id == step.Id).NodeType.Should().Be("Step");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 15 — Unresolved Choice Group (not in selectedChoices): all children Skipped
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SimulateAsync_ChoiceGroupAbsentFromSelectedChoices_AllChildrenSkipped()
        {
            // Arrange — Choice group exists but participant provided no selectedChoices entry for it
            var ai = new StepBuilder().WithName("AI").Build();
            var it = new StepBuilder().WithName("IT").Build();

            var majorGroup = new GroupBuilder()
                .WithName("Major")
                .AsChoice(pickCount: 1)
                .AddChild(ai)
                .AddChild(it)
                .Build();

            var program = new ProgramBuilder().WithRootGroup(majorGroup).Build();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act — no selectedChoices provided at all
            var result = await _sut.SimulateAsync(program.Id, EmptyRequest());

            // Assert — all children are Skipped because the choice is unresolved
            result.Nodes.Single(n => n.Id == ai.Id).Status.Should().Be(NodeStatus.Skipped);
            result.Nodes.Single(n => n.Id == it.Id).Status.Should().Be(NodeStatus.Skipped);
        }
    }
}
