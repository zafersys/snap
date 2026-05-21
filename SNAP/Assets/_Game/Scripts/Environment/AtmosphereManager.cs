using UnityEngine;
using GPOyun.Managers;

namespace GPOyun.Environment
{
    /// <summary>
    /// Adjusts the visual mood of the scene based on the current DayPhase.
    /// Handles skybox colors and directional light intensity.
    /// </summary>
    public class AtmosphereManager : MonoBehaviour
    {
        [Header("Global References")]
        [SerializeField] private Light mainDirectionalLight;
        
        [Header("Phase Settings")]
        [SerializeField] private Color morningColor   = new Color(0.9f, 0.9f, 1.0f); // Soft Light
        [SerializeField] private Color middayColor    = new Color(1.0f, 1.0f, 0.9f); // Bright
        [SerializeField] private Color afternoonColor = new Color(1.0f, 0.8f, 0.6f); // Warm
        [SerializeField] private Color eveningColor   = new Color(1.0f, 0.4f, 0.2f); // Sunset
        [SerializeField] private Color nightColor     = new Color(0.1f, 0.15f, 0.3f); // Dark Moonlight

        public void Initialize(Light mainLight)
        {
            mainDirectionalLight = mainLight;
        }

        private void Update()
        {
            if (TimeManager.Instance == null || mainDirectionalLight == null) return;

            UpdateSunPosition();
        }

        private void UpdateSunPosition()
        {
            float progress = TimeManager.Instance.GetPhaseProgress();
            DayPhase current = TimeManager.Instance.CurrentPhase;
            
            // Map DayPhase to a continuous 0-1 daily cycle
            float dayProgress = ((int)current + progress) / 5f;
            
            // Rotate the sun 360 degrees (continuous loop)
            float rotationAngle = (dayProgress * 360f); 
            mainDirectionalLight.transform.rotation = Quaternion.Euler(rotationAngle, -30f, 0);
            
            // Forces the sun to be infinite-distance parallel light
            mainDirectionalLight.transform.position = Vector3.up * 100f; 

            // Shadows: Standard "Obvious" Basics for premium look
            mainDirectionalLight.shadows = LightShadows.Soft;
            mainDirectionalLight.shadowStrength = 0.85f;

            // Interpolate Color
            Color startColor = GetColorForPhase(current);
            Color endColor = GetColorForPhase((DayPhase)(((int)current + 1) % 5));
            mainDirectionalLight.color = Color.Lerp(startColor, endColor, progress);

            // Intensity: Dynamic range for Mediterranean sun
            float intensity = 1.2f;
            if (current == DayPhase.Evening) intensity = Mathf.Lerp(1.2f, 0.4f, progress);
            else if (current == DayPhase.Night) intensity = 0.15f;
            else if (current == DayPhase.Morning) intensity = Mathf.Lerp(0.15f, 1.2f, progress);

            mainDirectionalLight.intensity = intensity;
            
            // Ambient balance
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = mainDirectionalLight.color * (intensity * 0.3f);
        }

        private Color GetColorForPhase(DayPhase phase)
        {
            return phase switch
            {
                DayPhase.Morning   => morningColor,
                DayPhase.Midday    => middayColor,
                DayPhase.Afternoon => afternoonColor,
                DayPhase.Evening   => eveningColor,
                DayPhase.Night     => nightColor,
                _                  => morningColor
            };
        }
    }
}
