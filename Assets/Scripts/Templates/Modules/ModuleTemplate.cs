using Systems.Modules;
using UnityEngine;

namespace Templates.Modules
{
    public abstract class ModuleTemplate : ScriptableObject
    {
        public abstract Module GetModule(Unit unit);
    }
}