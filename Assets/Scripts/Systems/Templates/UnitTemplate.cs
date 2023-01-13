using System.Collections.Generic;
using Systems.Templates.Modules;
using UnityEngine;

namespace Systems.Templates
{
    [CreateAssetMenu(menuName = "Create UnitTemplate", fileName = "UnitTemplate")]
    public class UnitTemplate : ScriptableObject
    {
        public float size;
        public List<OrderExecutionModuleTemplate> orderExecutionModuleTemplates;
    }
}