namespace Systems.Modules
{
    public class Module
    {
        public Unit unit;
        public bool active;

        public Module(Unit unit)
        {
            this.unit = unit;
            active = false;
        }
        
        public virtual void Update() {}
    }
}