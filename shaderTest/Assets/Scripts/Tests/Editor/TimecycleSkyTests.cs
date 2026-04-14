using NUnit.Framework;
using UnityEngine;
using GTA5Sky;

namespace GTA5Sky.Tests
{
    [TestFixture]
    public class TimecycleSkyTests
    {
        WeatherTransition.WeatherState clearState;

        [SetUp]
        public void Setup()
        {
            var settings = WeatherSettings.CreateRuntimeDefaults();
            var profile = settings.GetProfileOrDefault(WeatherType.Clear);
            clearState = WeatherTransition.WeatherState.FromProfile(profile);
        }

        [Test]
        public void Build_Deterministic_SameInputSameOutput()
        {
            var a = GTA5TimecycleSky.Build(clearState, 12f, 100f);
            var b = GTA5TimecycleSky.Build(clearState, 12f, 100f);

            Assert.AreEqual(a.SunDirection, b.SunDirection, "SunDirection mismatch");
            Assert.AreEqual(a.MoonDirection, b.MoonDirection, "MoonDirection mismatch");
            Assert.AreEqual(a.SkyHdrIntensity, b.SkyHdrIntensity, "SkyHdrIntensity mismatch");
            Assert.AreEqual(a.DayNightBalance, b.DayNightBalance, "DayNightBalance mismatch");
        }

        [Test]
        public void Build_Noon_SunAboveHorizon()
        {
            var snap = GTA5TimecycleSky.Build(clearState, 12f, 0f);
            Assert.Greater(snap.SunDirection.y, 0f, "Sun should be above horizon at noon");
            Assert.Greater(snap.SunFade, 0.9f, "SunFade should be near 1 at noon");
            Assert.Less(snap.MoonFade, 0.01f, "MoonFade should be near 0 at noon");
        }

        [Test]
        public void Build_Midnight_SunBelowHorizon()
        {
            var snap = GTA5TimecycleSky.Build(clearState, 0f, 0f);
            Assert.Less(snap.SunDirection.y, 0f, "Sun should be below horizon at midnight");
            Assert.Greater(snap.MoonFade, 0.5f, "MoonFade should be significant at midnight");
        }

        [Test]
        public void Build_DayNightBalance_RangeValid()
        {
            for (float h = 0f; h < 24f; h += 0.5f)
            {
                var snap = GTA5TimecycleSky.Build(clearState, h, 0f);
                Assert.GreaterOrEqual(snap.DayNightBalance, 0f, $"DayNightBalance < 0 at hour {h}");
                Assert.LessOrEqual(snap.DayNightBalance, 1f, $"DayNightBalance > 1 at hour {h}");
            }
        }

        [Test]
        public void Build_AllColors_NonNegative()
        {
            for (float h = 0f; h < 24f; h += 1f)
            {
                var snap = GTA5TimecycleSky.Build(clearState, h, 0f);
                AssertColorNonNegative(snap.AzimuthEastColor, $"AzimuthEast at {h}h");
                AssertColorNonNegative(snap.ZenithColor, $"Zenith at {h}h");
                AssertColorNonNegative(snap.SunColor, $"SunColor at {h}h");
                Assert.GreaterOrEqual(snap.SkyHdrIntensity, 0f, $"SkyHdr < 0 at {h}h");
            }
        }

        [Test]
        public void Build_Performance_Under1ms()
        {
            var sw = new System.Diagnostics.Stopwatch();
            const int iterations = 1000;

            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                GTA5TimecycleSky.Build(clearState, (i * 0.024f) % 24f, i * 0.1f);
            }
            sw.Stop();

            double avgMs = sw.Elapsed.TotalMilliseconds / iterations;
            Assert.Less(avgMs, 1.0, $"Build averaged {avgMs:F4}ms, expected < 1ms");
            UnityEngine.Debug.Log($"[PerfTest] GTA5TimecycleSky.Build: {avgMs:F4}ms avg over {iterations} calls");
        }

        [Test]
        public void WeatherLerp_Midpoint_BetweenStates()
        {
            var settings = WeatherSettings.CreateRuntimeDefaults();
            var clear = WeatherTransition.WeatherState.FromProfile(settings.GetProfileOrDefault(WeatherType.Clear));
            var rainy = WeatherTransition.WeatherState.FromProfile(settings.GetProfileOrDefault(WeatherType.Rainy));

            var mid = WeatherTransition.WeatherState.Lerp(clear, rainy, 0.5f);

            float expectedFog = Mathf.Lerp(clear.FogDensity, rainy.FogDensity, 0.5f);
            Assert.AreEqual(expectedFog, mid.FogDensity, 0.0001f, "Fog density lerp mismatch");
        }

        static void AssertColorNonNegative(Color c, string label)
        {
            Assert.GreaterOrEqual(c.r, 0f, $"{label}.r < 0");
            Assert.GreaterOrEqual(c.g, 0f, $"{label}.g < 0");
            Assert.GreaterOrEqual(c.b, 0f, $"{label}.b < 0");
        }
    }
}
