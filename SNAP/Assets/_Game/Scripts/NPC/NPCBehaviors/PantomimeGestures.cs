using System.Collections;
using UnityEngine;

namespace GPOyun.NPC
{
    /// <summary>
    /// Drives cozy, silent physical gestures using procedural scales and rotations.
    /// Used by NPCController to express states: Hugging, Fleeing, Chilling, Sadness, etc.
    /// </summary>
    public class PantomimeGestures : MonoBehaviour
    {
        private Vector3 _originalScale;
        private Vector3 _originalHeadPos;
        private Vector3 _originalBodyPos;

        private Transform _headTransform;
        private Transform _bodyTransform;

        private Coroutine _activeGesture;

        private void Awake()
        {
            _headTransform = transform.Find("Head");
            _bodyTransform = transform.Find("Body");

            _originalScale = transform.localScale;

            if (_headTransform != null) _originalHeadPos = _headTransform.localPosition;
            if (_bodyTransform != null) _originalBodyPos = _bodyTransform.localPosition;
        }

        private void StopActiveGesture()
        {
            if (_activeGesture != null)
            {
                StopCoroutine(_activeGesture);
                _activeGesture = null;
            }

            // Restore defaults
            transform.localScale = _originalScale;
            transform.localRotation = Quaternion.identity;

            if (_headTransform != null) _headTransform.localPosition = _originalHeadPos;
            if (_bodyTransform != null) _bodyTransform.localPosition = _originalBodyPos;
        }

        public void PlayJoy()
        {
            StopActiveGesture();
            _activeGesture = StartCoroutine(JoyCoroutine());
        }

        public void PlayFear()
        {
            StopActiveGesture();
            _activeGesture = StartCoroutine(FearCoroutine());
        }

        public void PlayAffection()
        {
            StopActiveGesture();
            _activeGesture = StartCoroutine(AffectionCoroutine());
        }

        public void SetSadness(bool active)
        {
            StopActiveGesture();
            if (active)
            {
                // Compress height (slouched look)
                transform.localScale = new Vector3(_originalScale.x * 1.05f, _originalScale.y * 0.82f, _originalScale.z * 1.05f);
            }
            else
            {
                transform.localScale = _originalScale;
            }
        }

        private IEnumerator JoyCoroutine()
        {
            // Rapid vertical scaling + jumping
            float elapsed = 0f;
            float duration = 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float wave = Mathf.Abs(Mathf.Sin(elapsed * 12f));

                // Jump position shift
                if (_bodyTransform != null)
                    _bodyTransform.localPosition = _originalBodyPos + Vector3.up * (wave * 0.4f);
                if (_headTransform != null)
                    _headTransform.localPosition = _originalHeadPos + Vector3.up * (wave * 0.4f);

                // Vertical squash/stretch
                float squash = 1f + (Mathf.Sin(elapsed * 24f) * 0.15f);
                transform.localScale = new Vector3(_originalScale.x / squash, _originalScale.y * squash, _originalScale.z / squash);

                yield return null;
            }

            StopActiveGesture();
        }

        private IEnumerator FearCoroutine()
        {
            // Rapid shivering jitter
            float elapsed = 0f;
            float duration = 2.0f;

            // Squash slightly (cower)
            transform.localScale = new Vector3(_originalScale.x * 0.9f, _originalScale.y * 0.85f, _originalScale.z * 0.9f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float jitter = Mathf.Sin(elapsed * 45f) * 0.08f;

                // Shake body locally
                if (_bodyTransform != null)
                    _bodyTransform.localPosition = _originalBodyPos + new Vector3(jitter, 0, 0);
                if (_headTransform != null)
                    _headTransform.localPosition = _originalHeadPos + new Vector3(-jitter, 0, 0);

                yield return null;
            }

            StopActiveGesture();
        }

        private IEnumerator AffectionCoroutine()
        {
            // Inward rotational tilting (soft bow / lean)
            float elapsed = 0f;
            float duration = 2.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // Tilt rotation back and forth slowly
                float angle = Mathf.Sin(elapsed * 3f) * 12f;
                transform.localRotation = Quaternion.Euler(0, 0, angle);

                // Slight heartbeat scaling
                float heart = 1f + Mathf.PingPong(elapsed * 4f, 0.08f);
                transform.localScale = new Vector3(_originalScale.x * heart, _originalScale.y * heart, _originalScale.z * heart);

                yield return null;
            }

            StopActiveGesture();
        }
    }
}
