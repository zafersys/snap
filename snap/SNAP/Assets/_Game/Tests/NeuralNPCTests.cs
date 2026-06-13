using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GPOyun.NPC;
using GPOyun.NPC.Data;
using GPOyun.NPC.Appraisal;
using GPOyun.NPC.Memory;

namespace GPOyun.Tests
{
    public class NeuralNPCTests
    {
        private GameObject _npcObj;
        private NPCAppraisalEngine _appraisal;
        private NPCMemoryStream _memory;

        [SetUp]
        public void Setup()
        {
            _npcObj = new GameObject("TestNPC");
            _appraisal = _npcObj.AddComponent<NPCAppraisalEngine>();
            _memory = _npcObj.AddComponent<NPCMemoryStream>();

            // Setup mock personality
            var personality = ScriptableObject.CreateInstance<NPCPersonalityData>();
            personality.Neuroticism = 0.8f;
            personality.Agreeableness = 0.2f;
            personality.Extraversion = 0.1f;
            _appraisal.Initialize(personality);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_npcObj);
        }

        [Test]
        public void AppraisalEngine_HighNeuroticism_ReactsAngryToCamera()
        {
            var stimulus = new Stimulus(StimulusType.CameraShutter, Vector3.zero, 0, 1f);
            var result = _appraisal.Evaluate(stimulus, EmotionType.Neutral);

            Assert.AreEqual(EmotionType.Angry, result.NewEmotion);
            Assert.Less(result.RelationshipDelta, 0);
        }

        [Test]
        public void MemoryStream_LogsEventSuccessfully()
        {
            var evt = new MemoryEvent(Time.time, StimulusType.CameraFlash, 0, EmotionType.Angry);
            _memory.AddMemory(evt);
            
            // Generate reflections based on memory (simulate Night)
            _memory.SynthesizeNightlyReflections();
            
            // Should not generate belief if < 3 instances
            var beliefs = _memory.GetBeliefs();
            Assert.AreEqual(0, beliefs.Count);

            // Add 3 more to trigger stalking belief
            _memory.AddMemory(new MemoryEvent(Time.time, StimulusType.CameraShutter, 0, EmotionType.Angry));
            _memory.AddMemory(new MemoryEvent(Time.time, StimulusType.CameraShutter, 0, EmotionType.Angry));
            _memory.AddMemory(new MemoryEvent(Time.time, StimulusType.CameraShutter, 0, EmotionType.Angry));

            _memory.SynthesizeNightlyReflections();
            beliefs = _memory.GetBeliefs();

            Assert.AreEqual(1, beliefs.Count);
            Assert.AreEqual("The photographer is obsessed with me and keeps stalking me.", beliefs[0].NarrativeBias);
        }
    }
}
