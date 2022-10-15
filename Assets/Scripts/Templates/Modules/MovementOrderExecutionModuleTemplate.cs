using Systems.Modules;
using UnityEngine;

namespace Templates.Modules
{
    [CreateAssetMenu(menuName = "Create MovementOrderExecutionModuleTemplate", fileName = "MovementOrderExecutionModuleTemplate", order = 0)]
    public class MovementOrderExecutionModuleTemplate : OrderExecutionModuleTemplate
    {
        public float speed;
        
        public override OrderExecutionModule GetOrderExecutionModule(Unit unit)
        {
            return new MovementOrderExecutionModule(this, unit);
        }
    }
}