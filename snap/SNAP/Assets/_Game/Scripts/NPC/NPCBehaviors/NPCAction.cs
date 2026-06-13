using System.Collections;
using UnityEngine;

namespace GPOyun.NPC.UtilityAI
{
    public abstract class NPCAction : MonoBehaviour
    {
        public string ActionName;
        public float BaseUtility = 10f;
        
        protected NPCController Controller;
        protected NPCNeeds Needs;

        public virtual void Initialize(NPCController controller, NPCNeeds needs)
        {
            Controller = controller;
            Needs = needs;
        }

        /// <summary>
        /// Returns the dynamic utility score (0-100+) of this action given the current context.
        /// </summary>
        public abstract float CalculateUtility();

        /// <summary>
        /// The main coroutine logic for executing the action.
        /// </summary>
        public abstract IEnumerator Execute();

        /// <summary>
        /// Called if another action suddenly spikes in utility and overrides this one.
        /// </summary>
        public abstract void Interrupt();
    }
}
