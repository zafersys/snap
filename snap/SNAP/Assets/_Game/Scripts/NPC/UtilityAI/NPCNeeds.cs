using UnityEngine;

namespace GPOyun.NPC.UtilityAI
{
    public class NPCNeeds : MonoBehaviour
    {
        [Header("Dynamic Needs (0 to 100)")]
        [Range(0, 100)] public float Boredom = 0f;
        [Range(0, 100)] public float SocialDesire = 0f;
        [Range(0, 100)] public float Introversion = 0f;
        [Range(0, 100)] public float Energy = 100f;

        [Header("Context Flags (Event Driven)")]
        public bool HasPendingNews = false;
        public bool IsNightTime = false;
        public bool IsPlayerHostile = false;
        public bool IsPlayerNearby = false;

        [Header("Rates (Per Second)")]
        public float BoredomRate = 1.5f;
        public float SocialDecayRate = 0.5f;
        public float IntroversionRate = 0.3f;
        public float EnergyDrainRate = 0.2f;

        private void Start()
        {
            RandomizeStartingNeeds();
        }

        public void RandomizeStartingNeeds()
        {
            // Give each NPC a unique starting state!
            Boredom = Random.Range(10f, 60f);
            SocialDesire = Random.Range(5f, 80f);
            Introversion = Random.Range(10f, 90f);
            Energy = Random.Range(50f, 100f);

            // Also slightly randomize their rates so they diverge over time
            BoredomRate *= Random.Range(0.8f, 1.2f);
            SocialDecayRate *= Random.Range(0.8f, 1.2f);
            IntroversionRate *= Random.Range(0.8f, 1.2f);
        }

        private void Update()
        {
            // Needs naturally shift over time
            Boredom = Mathf.Clamp(Boredom + BoredomRate * Time.deltaTime, 0, 100);
            SocialDesire = Mathf.Clamp(SocialDesire + SocialDecayRate * Time.deltaTime, 0, 100);
            Introversion = Mathf.Clamp(Introversion + IntroversionRate * Time.deltaTime, 0, 100);
            Energy = Mathf.Clamp(Energy - EnergyDrainRate * Time.deltaTime, 0, 100);
        }

        public void SatisfyBoredom(float amount)
        {
            Boredom = Mathf.Clamp(Boredom - amount, 0, 100);
        }

        public void SatisfySocial(float amount)
        {
            SocialDesire = Mathf.Clamp(SocialDesire - amount, 0, 100);
        }
        
        public void RestoreEnergy(float amount)
        {
            Energy = Mathf.Clamp(Energy + amount, 0, 100);
        }

        public void SatisfyIntroversion(float amount)
        {
            Introversion = Mathf.Clamp(Introversion - amount, 0, 100);
        }
    }
}
