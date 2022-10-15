using Systems.Modules;
using UnityEngine;

namespace Templates.Modules
{
    [CreateAssetMenu(menuName = "Create EngineeringOrderExecutionModuleTemplate", fileName = "EngineeringOrderExecutionModuleTemplate", order = 0)]
    public class EngineeringOrderExecutionModuleTemplate : OrderExecutionModuleTemplate
    {
        public float range;
        public float power;


        public override OrderExecutionModule GetOrderExecutionModule(Unit unit)
        {
            return new ReclaimOrderExecutionModule(this, unit);
        }
    }
}