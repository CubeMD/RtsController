using Objects;
using Objects.Orders;
using Systems.Modules;
using UnityEngine;

namespace Systems.Templates.Modules
{
    public abstract class OrderExecutionModuleTemplate : ScriptableObject
    {
        public OrderType orderType;
        
        public abstract OrderExecutionModule GetOrderExecutionModule(Unit unit);
    }
}