using System.Collections.Generic;
using Templates.Modules;
using UnityEngine;

namespace Templates
{
    [CreateAssetMenu(menuName = "Create UnitTemplate", fileName = "UnitTemplate")]
    public class UnitTemplate : ScriptableObject
    {
        public List<OrderExecutionModuleTemplate> orderExecutionModuleTemplates;
    }
}