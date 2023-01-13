using Objects;
using Systems.Modules;
using UnityEngine;

namespace Systems.Templates.Modules
{
    public abstract class ModuleTemplate : ScriptableObject
    {
        public abstract Module GetModule(Unit unit);
    }
}