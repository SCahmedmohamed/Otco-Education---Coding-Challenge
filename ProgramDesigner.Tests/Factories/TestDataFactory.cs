using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Domain.Enums;
using ProgramDesigner.Tests.Builders;
using System;

namespace ProgramDesigner.Tests.Factories
{
    public static class TestDataFactory
    {
        public static readonly Guid ComputerScienceProgramId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid CsRootGroupId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid ProgrammingBasicsStepId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid DataStructuresStepId = Guid.Parse("20000000-0000-0000-0000-000000000002");

        /// <summary>
        /// Octo Challenge Required Scenario 1:
        /// Computer Science program with Programming Basics and Data Structures (Data Structures depends on Programming Basics).
        /// </summary>
        public static ProgramEntity CreateComputerScienceProgram()
        {
            var step1 = new StepBuilder()
                .WithId(ProgrammingBasicsStepId)
                .WithName("Programming Basics")
                .WithPrerequisiteId(null)
                .Build();

            var step2 = new StepBuilder()
                .WithId(DataStructuresStepId)
                .WithName("Data Structures")
                .WithPrerequisiteId(ProgrammingBasicsStepId)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithId(CsRootGroupId)
                .WithName("Computer Science")
                .AsInOrder()
                .AddChild(step1)
                .AddChild(step2)
                .Build();

            return new ProgramBuilder()
                .WithId(ComputerScienceProgramId)
                .WithName("Computer Science")
                .WithRootGroup(rootGroup)
                .Build();
        }

        /// <summary>
        /// Octo Challenge Required Scenario 2:
        /// Direct prerequisite cycle (A -> B and B -> A).
        /// </summary>
        public static ProgramEntity CreateDirectCycleProgram()
        {
            Guid stepAId = Guid.Parse("40000000-0000-0000-0000-000000000001");
            Guid stepBId = Guid.Parse("40000000-0000-0000-0000-000000000002");

            var stepA = new StepBuilder()
                .WithId(stepAId)
                .WithName("A")
                .WithPrerequisiteId(stepBId)
                .Build();

            var stepB = new StepBuilder()
                .WithId(stepBId)
                .WithName("B")
                .WithPrerequisiteId(stepAId)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithId(Guid.Parse("10000000-0000-0000-0000-000000000003"))
                .WithName("Root")
                .AsInOrder()
                .AddChild(stepA)
                .AddChild(stepB)
                .Build();

            return new ProgramBuilder()
                .WithId(Guid.NewGuid())
                .WithName("Circular")
                .WithRootGroup(rootGroup)
                .Build();
        }

        /// <summary>
        /// Octo Challenge Required Scenario 3:
        /// Self prerequisite (Programming Basics depends on Programming Basics).
        /// </summary>
        public static ProgramEntity CreateSelfPrerequisiteProgram()
        {
            Guid stepId = Guid.Parse("30000000-0000-0000-0000-300000000001");

            var step = new StepBuilder()
                .WithId(stepId)
                .WithName("Programming Basics")
                .WithPrerequisiteId(stepId)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithId(Guid.Parse("10000000-0000-0000-0000-000000000002"))
                .WithName("Root")
                .AsInOrder()
                .AddChild(step)
                .Build();

            return new ProgramBuilder()
                .WithId(Guid.NewGuid())
                .WithName("Self Reference Program")
                .WithRootGroup(rootGroup)
                .Build();
        }

        /// <summary>
        /// Octo Challenge Required Scenario 4:
        /// Reachability Warning:
        ///   Structure:
        ///     rootGroup (Choice, PickCount=1)
        ///       └─ subGroup (InOrder)     ← NOT Course A's direct Choice parent
        ///            └─ Course A (prerequisite → Course B)
        ///       └─ Course B (no prerequisite)
        ///
        /// Course A's active set contains: Course A, subGroup, rootGroup, Course B
        /// From rootGroup's perspective: subGroup + Course B = 2 items → exceeds PickCount 1
        /// Since rootGroup is NOT Course A's direct parent (subGroup is), this is a WARNING only.
        /// </summary>
        public static ProgramEntity CreateReachabilityWarningProgram()
        {
            Guid courseAId = Guid.Parse("80000000-0000-0000-0000-000000000001");
            Guid courseBId = Guid.Parse("80000000-0000-0000-0000-000000000002");
            Guid subGroupId = Guid.Parse("90000000-0000-0000-0000-000000000001");
            Guid rootGroupId = Guid.Parse("10000000-0000-0000-0000-000000000007");

            // Course A (in subGroup) depends on Course B (which is a sibling of subGroup in rootGroup)
            var courseA = new StepBuilder()
                .WithId(courseAId)
                .WithName("Course A")
                .WithPrerequisiteId(courseBId)
                .Build();

            // SubGroup contains Course A
            var subGroup = new GroupBuilder()
                .WithId(subGroupId)
                .WithName("SubGroup")
                .AsInOrder()
                .AddChild(courseA)
                .Build();

            // Course B has no prerequisite — direct child of the Choice rootGroup
            var courseB = new StepBuilder()
                .WithId(courseBId)
                .WithName("Course B")
                .WithPrerequisiteId(null)
                .Build();

            // Choice root group: PickCount = 1
            // Direct children: subGroup, Course B
            // Course A active set from rootGroup perspective: [subGroup, Course B] = 2 items → exceeds PickCount 1
            // But rootGroup is NOT Course A's DIRECT parent (subGroup is) → Reachability Warning, not Error
            var rootGroup = new GroupBuilder()
                .WithId(rootGroupId)
                .WithName("Electives")
                .AsChoice(pickCount: 1)
                .AddChild(subGroup)
                .AddChild(courseB)
                .Build();

            return new ProgramBuilder()
                .WithId(Guid.NewGuid())
                .WithName("Reachability Warning Program")
                .WithRootGroup(rootGroup)
                .Build();
        }
    }
}
