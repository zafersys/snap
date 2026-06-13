using System.Collections.Generic;
using UnityEngine;
using GPOyun.NPC;

namespace GPOyun.Core
{
    public class SocialGroup
    {
        public string GroupId { get; private set; }
        public Vector3 CenterPosition { get; private set; }
        public List<NPCController> Members { get; private set; }
        public int MaxMembers { get; private set; }

        public SocialGroup(string id, Vector3 position, int maxMembers = 3)
        {
            GroupId = id;
            CenterPosition = position;
            MaxMembers = maxMembers;
            Members = new List<NPCController>();
        }

        public bool IsFull => Members.Count >= MaxMembers;

        public void AddMember(NPCController npc)
        {
            if (!Members.Contains(npc)) Members.Add(npc);
        }

        public void RemoveMember(NPCController npc)
        {
            Members.Remove(npc);
        }

        public void UpdateCenter(Vector3 position)
        {
            CenterPosition = position;
        }
    }

    public class SocialGroupManager : MonoBehaviour
    {
        private static SocialGroupManager _instance;
        public static SocialGroupManager Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<SocialGroupManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[SINGLETON] SocialGroupManager");
                        _instance = go.AddComponent<SocialGroupManager>();
                    }
                }
                return _instance;
            } 
        }

        private List<SocialGroup> _activeGroups = new List<SocialGroup>();
        private int _groupIdCounter = 0;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        public SocialGroup CreateGroup(NPCController initiator, Vector3 position)
        {
            var group = new SocialGroup($"Group_{_groupIdCounter++}", position);
            group.AddMember(initiator);
            _activeGroups.Add(group);
            Debug.Log($"[SocialGroup] {initiator.NpcName} started a new social group {group.GroupId}.");
            return group;
        }

        public void DisbandGroup(SocialGroup group)
        {
            if (_activeGroups.Contains(group))
            {
                _activeGroups.Remove(group);
            }
        }

        public SocialGroup TryJoinNearbyGroup(NPCController joiner, float maxDistance = 15f)
        {
            SocialGroup bestGroup = null;
            float highestAffinity = -999f;

            foreach (var group in _activeGroups)
            {
                if (group.IsFull) continue;

                float dist = Vector3.Distance(joiner.transform.position, group.CenterPosition);
                if (dist > maxDistance) continue;

                // Affinity is based on relationship with existing members
                float affinity = 0f;
                foreach (var member in group.Members)
                {
                    if (RelationshipMatrix.Instance != null)
                    {
                        affinity += RelationshipMatrix.Instance.GetRelationship(joiner.NpcId, member.NpcId);
                    }
                }

                if (affinity > highestAffinity)
                {
                    highestAffinity = affinity;
                    bestGroup = group;
                }
            }

            if (bestGroup != null && highestAffinity >= 0) // Don't join groups full of enemies
            {
                bestGroup.AddMember(joiner);
                Debug.Log($"[SocialGroup] {joiner.NpcName} joined existing group {bestGroup.GroupId}.");
                return bestGroup;
            }

            return null;
        }

        public void LeaveGroup(NPCController npc, SocialGroup group)
        {
            if (group == null) return;
            group.RemoveMember(npc);
            
            if (group.Members.Count == 0)
            {
                DisbandGroup(group);
            }
        }
    }
}
