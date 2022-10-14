using System;
using Systems.Interfaces;
using Systems.Orders;

namespace Systems.Modules
{
    public abstract class OrderExecutionModule : Module
    {
        public event Action<OrderExecutionModule, OrderData> OnOrderDestroyed;
        public event Action<OrderExecutionModule, OrderData> OnOrderCompleted;
        
        public OrderType orderType;
        public OrderData orderData;
        
        public OrderExecutionModule(Unit unit) : base(unit) { }

        public virtual void SetActiveOrder(OrderData activeOrderData)
        {
            orderData = activeOrderData;
            activeOrderData.order.OnDestroyableDestroy += HandleOrderDestroyed;
        }

        public void ClearActiveOrder()
        {
            orderData = null;
        }
        
        private void HandleOrderDestroyed(IDestroyable iDestroyable)
        {
            ClearActiveOrder();
            BroadcastOrderDestroyed();
        }

        protected void BroadcastOrderDestroyed()
        {
            OnOrderDestroyed?.Invoke(this, orderData);
        }

        protected void BroadcastOrderCompleted()
        {
            orderData.order.OnDestroyableDestroy -= HandleOrderDestroyed;
            OnOrderCompleted?.Invoke(this, orderData);
        }
    }
}