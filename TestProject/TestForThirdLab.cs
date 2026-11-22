using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DomainClassLibrary;

namespace Contracts.Tests
{
    public class ThirdLabTest
    {
        //  Вспомогательные методы
        private static List<CycleModeInfo> GetModes() => CyclePresets.GetModes();

        private static CycleModeInfo GetMode(string name) =>
            GetModes().First(m => m.Name == name);

        private static List<CycleStepResult> RunFullCycle(
            int[] array, int threshold, CycleModeInfo mode)
        {
            var verifier = new CycleVerifier(array, threshold, mode);
            var steps = new List<CycleStepResult>();

            while (!verifier.IsFinished())
            {
                steps.Add(verifier.ExecuteStep());
            }
            return steps;
        }

        //  Источники данных (MemberData)
        public static IEnumerable<object[]> GetVariantDecreasingData()
        {
            yield return new object[] { "PrefixSum", new int[] { 1, 2, 3 }, 0 };
            yield return new object[] { "PrefixSum", new int[] { -5, 10, 0, 7 }, 0 };
            yield return new object[] { "PrefixSum", new int[] { 42 }, 0 };
            yield return new object[] { "Count > T", new int[] { 1, 5, 3, 8, 2 }, 4 };
            yield return new object[] { "Count > T", new int[] { 10, 20, 30 }, 15 };
            yield return new object[] { "Count > T", new int[] { 1, 2, 3 }, 100 };
            yield return new object[] { "PrefixMax", new int[] { 3, 1, 4, 1, 5 }, 0 };
            yield return new object[] { "PrefixMax", new int[] { -10, -20, -5 }, 0 };
            yield return new object[] { "PrefixMax", new int[] { 7 }, 0 };
        }

        public static IEnumerable<object[]> GetPrefixSumPostConditionData()
        {
            yield return new object[] { new int[] { 1, 2, 3, 4, 5 }, 0 };
            yield return new object[] { new int[] { -1, -2, -3 }, 0 };
            yield return new object[] { new int[] { 10 }, 0 };
            yield return new object[] { new int[] { 0, 0, 0, 0 }, 0 };
        }

        public static IEnumerable<object[]> GetCountGreaterThanTPostConditionData()
        {
            yield return new object[] { new int[] { 1, 5, 3, 8, 2 }, 4, 2 };
            yield return new object[] { new int[] { 10, 20, 30 }, 15, 2 };
            yield return new object[] { new int[] { 1, 2, 3 }, 100, 0 };
            yield return new object[] { new int[] { 5, 5, 5 }, 4, 3 };
            yield return new object[] { new int[] { 5, 5, 5 }, 5, 0 };
        }

        public static IEnumerable<object[]> GetPrefixMaxPostConditionData()
        {
            yield return new object[] { new int[] { 3, 1, 4, 1, 5 }, 5 };
            yield return new object[] { new int[] { -10, -20, -5 }, -5 };
            yield return new object[] { new int[] { 7 }, 7 };
            yield return new object[] { new int[] { 1, 2, 3, 2, 1 }, 3 };
        }

        public static IEnumerable<object[]> GetInvariantHoldsData()
        {
            yield return new object[] { "PrefixSum", new int[] { 1, 2, 3, 4 }, 0 };
            yield return new object[] { "Count > T", new int[] { 1, 5, 3, 8 }, 3 };
            yield return new object[] { "PrefixMax", new int[] { 3, 1, 4, 1 }, 0 };
        }

        //  1. Тесты убывания вариант-функции t = N - j (на малых массивах)
        [Theory]
        [MemberData(nameof(GetVariantDecreasingData))]
        public void VariantFunction_IsStrictlyDecreasing(
            string modeName, int[] array, int threshold)
        {
            var mode = GetMode(modeName);
            var steps = RunFullCycle(array, threshold, mode);

            Assert.NotEmpty(steps);

            for (int i = 0; i < steps.Count; i++)
            {
                Assert.True(steps[i].IsVariantDecreasing,
                    $"Шаг {i}: вариант-функция не убывает. Mode={modeName}");
            }

            for (int i = 1; i < steps.Count; i++)
            {
                Assert.True(steps[i].VariantValue < steps[i - 1].VariantValue,
                    $"Шаг {i}: VariantValue {steps[i].VariantValue} >= {steps[i - 1].VariantValue}");
            }
        }

        [Fact]
        public void VariantFunction_SingleElementArray_DecreasesOnce()
        {
            var steps = RunFullCycle(new int[] { 42 }, 0, GetMode("PrefixSum"));
            Assert.Single(steps);
            Assert.Equal(1, steps[0].VariantValue);
            Assert.True(steps[0].IsVariantDecreasing);
        }

        [Fact]
        public void VariantFunction_EmptyArray_NoSteps()
        {
            var steps = RunFullCycle(Array.Empty<int>(), 0, GetMode("PrefixSum"));
            Assert.Empty(steps);
        }

        //  2. Тесты Post-условия
        [Theory]
        [MemberData(nameof(GetPrefixSumPostConditionData))]
        public void PostCondition_PrefixSum_CorrectResult(int[] array, int threshold)
        {
            var verifier = new CycleVerifier(array, threshold, GetMode("PrefixSum"));
            while (!verifier.IsFinished()) verifier.ExecuteStep();

            long expected = array.Sum(x => (long)x);
            Assert.True(verifier.CheckPostCondition(),
                $"Post-условие нарушено. Array=[{string.Join(",", array)}]");
            Assert.Equal(expected, verifier.GetFinalResult());
        }

        [Theory]
        [MemberData(nameof(GetCountGreaterThanTPostConditionData))]
        public void PostCondition_CountGreaterThanT_CorrectResult(
            int[] array, int threshold, int expectedCount)
        {
            var verifier = new CycleVerifier(array, threshold, GetMode("Count > T"));
            while (!verifier.IsFinished()) verifier.ExecuteStep();

            Assert.True(verifier.CheckPostCondition(),
                $"Post-условие нарушено. Array=[{string.Join(",", array)}], T={threshold}");
            Assert.Equal(expectedCount, verifier.GetFinalResult());
        }

        [Theory]
        [MemberData(nameof(GetPrefixMaxPostConditionData))]
        public void PostCondition_PrefixMax_CorrectResult(int[] array, int expectedMax)
        {
            var verifier = new CycleVerifier(array, 0, GetMode("PrefixMax"));
            while (!verifier.IsFinished()) verifier.ExecuteStep();

            Assert.True(verifier.CheckPostCondition(),
                $"Post-условие нарушено. Array=[{string.Join(",", array)}]");
            Assert.Equal(expectedMax, verifier.GetFinalResult());
        }

        //  3. Тесты инварианта на каждом шаге
        [Theory]
        [MemberData(nameof(GetInvariantHoldsData))]
        public void Invariant_HoldsOnEveryStep(
            string modeName, int[] array, int threshold)
        {
            var steps = RunFullCycle(array, threshold, GetMode(modeName));

            foreach (var step in steps)
            {
                Assert.True(step.IsInvariantTrueBefore,
                    $"Инвариант нарушен ПЕРЕД шагом j={step.CurrentJ}");
                Assert.True(step.IsInvariantTrueAfter,
                    $"Инвариант нарушен ПОСЛЕ шага j={step.CurrentJ}");
            }
        }

        //  4. Тесты инициализации и завершения
        [Fact]
        public void Initialization_PrefixSum_ResIsZero()
        {
            var verifier = new CycleVerifier(new int[] { 1, 2, 3 }, 0, GetMode("PrefixSum"));
            Assert.Equal(0L, verifier.GetFinalResult());
            Assert.False(verifier.IsFinished());
        }

        [Fact]
        public void Initialization_PrefixMax_ResIsMinValue()
        {
            var verifier = new CycleVerifier(new int[] { 1, 2, 3 }, 0, GetMode("PrefixMax"));
            Assert.Equal(long.MinValue, verifier.GetFinalResult());
        }

        [Fact]
        public void Finish_EmptyArray_ImmediatelyFinished()
        {
            var verifier = new CycleVerifier(Array.Empty<int>(), 0, GetMode("PrefixSum"));
            Assert.True(verifier.IsFinished());
            Assert.Equal(0L, verifier.GetFinalResult());
        }

        //  5. Тесты CyclePresets и WpEngine
        [Fact]
        public void CyclePresets_ReturnsThreeModes()
        {
            var modes = CyclePresets.GetModes();
            Assert.Equal(3, modes.Count);
            Assert.Contains(modes, m => m.Name == "PrefixSum");
            Assert.Contains(modes, m => m.Name == "Count > T");
            Assert.Contains(modes, m => m.Name == "PrefixMax");
        }

        [Theory]
        [InlineData("PrefixSum")]
        [InlineData("Count > T")]
        [InlineData("PrefixMax")]
        public void WpExplanation_ContainsRequiredInfo(string modeName)
        {
            var verifier = new CycleVerifier(new int[] { 1, 2, 3 }, 0, GetMode(modeName));
            var explanation = verifier.GetWpExplanation();

            Assert.Contains(modeName, explanation);
            Assert.Contains("wp(S, R)", explanation);
            Assert.Contains("Инвариант", explanation);
        }


        //  6. Тесты на малых массивах и с отрицательными числами
        [Fact]
        public void SmallArray_Size3_AllModes_VariantDecreases()
        {
            var array = new int[] { 5, 2, 8 };
            foreach (var mode in GetModes())
            {
                var steps = RunFullCycle(array, 3, mode);
                Assert.Equal(3, steps.Count);
                Assert.Equal(3, steps[0].VariantValue);
                Assert.Equal(2, steps[1].VariantValue);
                Assert.Equal(1, steps[2].VariantValue);

                foreach (var step in steps)
                    Assert.True(step.IsVariantDecreasing,
                        $"Вариант не убывает для {mode.Name}");
            }
        }

        [Fact]
        public void NegativeNumbers_PrefixMax_Correct()
        {
            var verifier = new CycleVerifier(new int[] { -10, -3, -7 }, 0, GetMode("PrefixMax"));
            while (!verifier.IsFinished()) verifier.ExecuteStep();

            Assert.Equal(-3L, verifier.GetFinalResult());
            Assert.True(verifier.CheckPostCondition());
        }

        [Fact]
        public void NegativeNumbers_PrefixSum_Correct()
        {
            var verifier = new CycleVerifier(new int[] { -1, -2, -3 }, 0, GetMode("PrefixSum"));
            while (!verifier.IsFinished()) verifier.ExecuteStep();

            Assert.Equal(-6L, verifier.GetFinalResult());
            Assert.True(verifier.CheckPostCondition());
        }

        [Fact]
        public void NegativeNumbers_CountGreaterThanT_Correct()
        {
            var verifier = new CycleVerifier(new int[] { -1, -5, 0, 3 }, -2, GetMode("Count > T"));
            while (!verifier.IsFinished()) verifier.ExecuteStep();

            Assert.Equal(3L, verifier.GetFinalResult());
            Assert.True(verifier.CheckPostCondition());
        }
    }
}