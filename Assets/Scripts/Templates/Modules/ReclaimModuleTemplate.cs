using Systems.Modules;
using UnityEngine;

namespace Templates.Modules
{
    [CreateAssetMenu(menuName = "Create ExecutingReclaimOrderState", fileName = "ExecutingReclaimOrderState")]
    public class ReclaimModuleTemplate : OrderModuleTemplate
    {
        public float defaultReclaimRange;
        public float defaultReclaimPower;
        public override Module GetModule()
        {
            return new ReclaimOrderExecutionModule(this);
        }
    }
}