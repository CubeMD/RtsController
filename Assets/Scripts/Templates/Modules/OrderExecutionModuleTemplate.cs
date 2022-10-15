using Systems.Modules;
using Systems.Orders;
using UnityEngine;

namespace Templates.Modules
{
    public abstract class OrderExecutionModuleTemplate : ScriptableObject
    {
        public OrderType orderType;
        
        public abstract OrderExecutionModule GetOrderExecutionModule(Unit unit);
    }
}