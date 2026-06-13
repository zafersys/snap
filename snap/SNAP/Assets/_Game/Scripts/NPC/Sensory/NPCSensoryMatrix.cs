using UnityEngine;
using System;
using System.Collections.Generic;
using GPOyun.NPC.Data;

namespace GPOyun.NPC.Sensory
{
    public class NPCSensoryMatrix : MonoBehaviour
    {
        public float visionRadius = 15f;
        public float visionAngle = 120f;
        public float hearingRadius = 10f;
        public LayerMask occlusionLayers;

        public event Action<Stimulus> OnStimulusDetected;

        private float _scanTimer = 0f;
        private float _scanInterval = 0.5f;

        private void Update()
        {
            _scanTimer += Time.deltaTime;
            if (_scanTimer >= _scanInterval)
            {
                _scanTimer = 0f;
                ScanVision();
                // Audio listening is largely event-driven (e.g. from CameraController), 
                // but we can poll for continuous sounds here if needed.
            }
        }

        private void ScanVision()
        {
            // For now, we only look for the Player
            var player = FindAnyObjectByType<Player.PlayerController>();
            if (player == null) return;

            Vector3 dirToPlayer = player.transform.position - transform.position;
            float dist = dirToPlayer.magnitude;

            if (dist <= visionRadius)
            {
                float angle = Vector3.Angle(transform.forward, dirToPlayer);
                if (angle <= visionAngle / 2f)
                {
                    // Inside cone, check occlusion
                    if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer.normalized, dist, occlusionLayers))
                    {
                        // Check if player is aiming camera
                        // Assuming PlayerController has a way to expose this or we check CameraController
                        var camCtrl = FindAnyObjectByType<GPOyun.CameraSystem.CameraController>();
                        if (camCtrl != null && camCtrl.IsViewfinderActive())
                        {
                            var stimulus = new Stimulus(StimulusType.PlayerAimedCamera, player.transform.position, 0, 1f);
                            OnStimulusDetected?.Invoke(stimulus);
                        }
                    }
                }
            }
        }

        public void HearAudioEvent(StimulusType type, Vector3 sourceLocation, int sourceId)
        {
            float dist = Vector3.Distance(transform.position, sourceLocation);
            if (dist <= hearingRadius)
            {
                var stimulus = new Stimulus(type, sourceLocation, sourceId, 1f - (dist / hearingRadius));
                OnStimulusDetected?.Invoke(stimulus);
            }
        }
    }
}
