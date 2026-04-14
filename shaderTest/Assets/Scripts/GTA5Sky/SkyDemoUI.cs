using UnityEngine;

namespace GTA5Sky
{
    public class SkyDemoUI : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (FindFirstObjectByType<SkyDemoUI>() != null) return;
            GameObject go = new GameObject("SkyDemoUI");
            go.AddComponent<SkyDemoUI>();
            DontDestroyOnLoad(go);
        }

        void OnGUI()
        {
            DayNightCycle cycle = FindFirstObjectByType<DayNightCycle>();
            if (cycle == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 280, 260));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"Time: {cycle.TimeOfDay:F2}h");
            float newTime = GUILayout.HorizontalSlider(cycle.TimeOfDay, 0f, 24f);
            if (!Mathf.Approximately(newTime, cycle.TimeOfDay))
            {
                cycle.SetTimeOfDay(newTime);
            }

            GUILayout.Label($"Speed: {cycle.DaySpeed:F3}");
            cycle.DaySpeed = GUILayout.HorizontalSlider(cycle.DaySpeed, 0f, 2f);

            GUILayout.Space(8);
            GUILayout.Label("Weather:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear")) WeatherController.Instance?.SetWeatherImmediate(WeatherType.Clear);
            if (GUILayout.Button("Overcast")) WeatherController.Instance?.SetWeatherImmediate(WeatherType.Overcast);
            if (GUILayout.Button("Rainy")) WeatherController.Instance?.SetWeatherImmediate(WeatherType.Rainy);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Thunder")) WeatherController.Instance?.SetWeatherImmediate(WeatherType.Thunder);
            if (GUILayout.Button("Foggy")) WeatherController.Instance?.SetWeatherImmediate(WeatherType.Foggy);
            if (GUILayout.Button("Smog")) WeatherController.Instance?.SetWeatherImmediate(WeatherType.Smog);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
