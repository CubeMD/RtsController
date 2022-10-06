using System.Collections.Generic;
using System.Linq;

namespace Systems.Orders
{
    public class Transition
    {
        public readonly Order previousOrder;
        public readonly Order nextOrder;
        public readonly List<Unit> assignedUnits;

        public Transition(Order previousOrder, Order nextOrder, List<Unit> assignedUnits)
        {
            this.previousOrder = previousOrder;
            this.nextOrder = nextOrder;
            this.assignedUnits = assignedUnits;
        }
    }
    
    /// <summary>
    /// UnitsNodes are always inputs in the edges
    /// </summary>
    public class OrderGraph
    {
        private readonly Dictionary<Order, List<Transition>> orderTransitionsTable = new Dictionary<Order, List<Transition>>();

        public void AddOrder(Order order)
        {
            orderTransitionsTable.Add(order, new List<Transition>());
        }

        public void AddUnitToTransitionOrCreateNew(Order previousOrder, Order nextOrder, Unit unit)
        {
            foreach (Transition transition in orderTransitionsTable[previousOrder])
            {
                if (transition.nextOrder == nextOrder)
                {
                    transition.assignedUnits.Add(unit);
                    return;
                }
            }

            Transition newTransition = new Transition(previousOrder, nextOrder, new List<Unit>{unit});
            AddTransition(newTransition);
        }

        private void AddTransition(Transition transition)
        {
            orderTransitionsTable[transition.previousOrder].Add(transition);
            orderTransitionsTable[transition.nextOrder].Add(transition);
        }
        
        public void RemoveOrder(Order order)
        {
            if (orderTransitionsTable.TryGetValue(order, out List<Transition> transitions))
            {
                foreach (Transition transition in transitions.ToList())
                {
                    RemoveTransition(transition);
                }
                
                orderTransitionsTable.Remove(order);
                //CheckOrderForDestruction(order);
            }
        }

        private void RemoveUnitFromTransition(Order previousOrder, Order nextOrder, Unit unit)
        {
            foreach (Transition transition in orderTransitionsTable[previousOrder].ToList())
            {
                if (transition.nextOrder == nextOrder)
                {
                    transition.assignedUnits.Remove(unit);
                    
                    if (transition.assignedUnits.Count < 1)
                    {
                        RemoveTransition(transition);
                    }

                    return;
                }
            }
        }

        private void RemoveTransition(Transition transition)
        {
            orderTransitionsTable[transition.previousOrder].Remove(transition);
            orderTransitionsTable[transition.nextOrder].Remove(transition);
            
            CheckOrderForDestruction(transition.nextOrder);
        }

        public void RemoveUnitFromAllTransitions(Unit unit)
        {
            TryGetUnitTransitions(unit, out List<Transition> transitions);
                
            foreach (Transition transition in transitions)
            {
                RemoveUnitFromTransition(transition.previousOrder, transition.nextOrder, unit);
            }
        }

        public bool TryGetUnitTransitions(Unit unit, out List<Transition> transitions)
        {
            transitions = new List<Transition>();
            
            if (unit.activeOrder == null)
            {
                return false;
            }
            
            Order currentOrder = unit.activeOrder;

            while (orderTransitionsTable.TryGetValue(currentOrder, out List<Transition> orderTransitions))
            {
                Transition transition = orderTransitions.FirstOrDefault(orderTransition =>
                    orderTransition.previousOrder == currentOrder &&
                    orderTransition.assignedUnits.Contains(unit));

                if (transition == null)
                {
                    break;
                }
                
                transitions.Add(transition);
                currentOrder = transition.nextOrder;
            }
            
            return transitions.Count > 0;
        }

        public Order LastOrderForUnit(Unit unit)
        {
            Order currentOrder = unit.activeOrder;

            while (true)
            {
                Transition transition = orderTransitionsTable[currentOrder].FirstOrDefault(orderTransition =>
                    orderTransition.previousOrder == currentOrder &&
                    orderTransition.assignedUnits.Contains(unit));

                if (transition == null)
                {
                    break;
                }
                
                currentOrder = transition.nextOrder;
            }
            
            return currentOrder;
        }

        public bool TryGetTransition(Unit unit, out Transition transition, Order fromOrder)
        {
            if (orderTransitionsTable.TryGetValue(fromOrder, out List<Transition> transitions))
            {
                transition = transitions.FirstOrDefault(orderTransition =>
                    orderTransition.previousOrder == fromOrder &&
                    orderTransition.assignedUnits.Contains(unit));
                
                return transition != null;
            }

            transition = null;
            return false;
        }

        public void TransitionUnitToNextOrder(Unit unit)
        {
            TryGetTransition(unit, out Transition transition, unit.activeOrder);
            unit.UnAssignActiveOrder();
            
            if (transition != null)
            {
                unit.TryAssignActiveOrder(transition.nextOrder);
                RemoveUnitFromTransition(transition.previousOrder, transition.nextOrder, unit);
            }
        }

        private void CheckOrderForDestruction(Order order)
        {
            if (orderTransitionsTable.ContainsKey(order) &&
                orderTransitionsTable[order].All(t => t.nextOrder != order) &&
                !order.CheckHasAssignedUnits()) 
            {
                RemoveOrder(order);
            }
        }

        public bool HasTransitionsToOrder(Order order)
        {
            if (orderTransitionsTable.TryGetValue(order, out List<Transition> transitions))
            {
                return transitions.Any(transition => transition.nextOrder == order);
            }

            return false;
        }
    }
}