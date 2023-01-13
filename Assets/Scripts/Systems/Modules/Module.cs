using Objects;

namespace Systems.Modules
{
    public class Module
    {
        public Unit unit;

        public Module(Unit unit)
        {
            this.unit = unit;
        }
        
        public virtual void Update() {}
    }
}