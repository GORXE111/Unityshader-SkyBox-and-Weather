using UnityEngine;

namespace GTA5Sky
{
    [DisallowMultipleComponent]
    public sealed class DayNightCycle : MonoBehaviour
    {
        private const float HoursPerDay = 24f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (Object.FindFirstObjectByType<DayNightCycle>() != null)
            {
                return;
            }

            WeatherController controller = Object.FindFirstObjectByType<WeatherController>();
            if (controller == null)
            {
                GameObject go = new GameObject("WeatherSystem");
                controller = go.AddComponent<WeatherController>();
                DontDestroyOnLoad(go);
            }

            GameObject host = controller.gameObject;
            if (host.GetComponent<DayNightCycle>() == null)
            {
                host.AddComponent<DayNightCycle>();
            }

            DontDestroyOnLoad(host);
        }

        [SerializeField, Range(0f, HoursPerDay)] private float timeOfDay = 12f;
        [SerializeField] private float daySpeed = 0.1f;

        private WeatherController weatherController;

        public float TimeOfDay => timeOfDay;
        public float SunElevation => ComputeSunElevationFromTimeOfDay(timeOfDay);
        public float DaySpeed
        {
            get => daySpeed;
            set => daySpeed = value;
        }

        private void Awake()
        {
            ResolveWeatherController();
        }

        private void OnEnable()
        {
            ApplyCurrentTime();
        }

        private void Update()
        {
            if (Mathf.Approximately(daySpeed, 0f))
            {
                return;
            }

            SetTime(timeOfDay + (daySpeed * Time.unscaledDeltaTime));
        }

        public void SetTime(float nextTimeOfDay)
        {
            float wrappedTime = Mathf.Repeat(nextTimeOfDay, HoursPerDay);
            if (!Mathf.Approximately(wrappedTime, timeOfDay))
            {
                timeOfDay = wrappedTime;
            }

            ApplyCurrentTime();
        }

        public void SetTimeOfDay(float nextTimeOfDay)
        {
            SetTime(nextTimeOfDay);
            WeatherController.Instance?.ForceRefreshSky();
        }

        public void ApplyCurrentTime()
        {
            if (!ResolveWeatherController())
            {
                return;
            }

            float normalizedTime = timeOfDay / HoursPerDay;
            float elevation = ComputeSunElevationFromNormalizedTime(normalizedTime) * 80f;
            float azimuth = normalizedTime * 360f;
            weatherController.SetSolarParameters(azimuth, elevation);
        }

        private static float ComputeSunElevationFromTimeOfDay(float currentTimeOfDay)
        {
            float normalizedTime = Mathf.Repeat(currentTimeOfDay, HoursPerDay) / HoursPerDay;
            return ComputeSunElevationFromNormalizedTime(normalizedTime);
        }

        private static float ComputeSunElevationFromNormalizedTime(float normalizedTime)
        {
            return Mathf.Sin((Mathf.Repeat(normalizedTime, 1f) * Mathf.PI * 2f) - (Mathf.PI * 0.5f));
        }

        private bool ResolveWeatherController()
        {
            if (weatherController != null)
            {
                return true;
            }

            weatherController = GetComponent<WeatherController>();
            if (weatherController != null)
            {
                return true;
            }

            weatherController = Object.FindFirstObjectByType<WeatherController>(FindObjectsInactive.Exclude);
            return weatherController != null;
        }
    }
}
