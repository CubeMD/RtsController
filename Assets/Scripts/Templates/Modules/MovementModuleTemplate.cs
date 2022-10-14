using Systems.Modules;
using UnityEngine;

namespace Templates.Modules
{
    [CreateAssetMenu(menuName = "Create ExecutingMoveOrderState", fileName = "ExecutingMoveOrderState")]
    public class MovementModuleTemplate : OrderModuleTemplate
    {
        public float defaultMovementSpeed;

        public override Module GetModule(Unit unit)
        {
            return new MovementOrderExecutionModule(this, unit);
        }
    }
}