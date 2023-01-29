using Systems.StateMachine;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Attributes;
using Utilities.Extensions;

namespace Units
{
    public abstract class Unit : MonoBehaviour
    {
        [SerializeField] [ReadOnly]
        private string id;
        
        private StateMachine stateMachine;
        public StateMachine StateMachine => stateMachine == null ? stateMachine = this.GetOrAddComponent<StateMachine>() : stateMachine;
        
        public virtual void SetInactive()
        {
            ReturnToDefaultState();
        }

        public void AssignUnitID(string id)
        {
            this.id = id;
        }
        
        protected virtual void ReturnToDefaultState()
        {
            StateMachine.SetActiveState(new EmptyState());
        }
    }
}