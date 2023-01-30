using UnityEngine;

namespace Units.States.UnitStateParameters
{
    [System.Serializable]
    public class EngineerParameters
    {
        [SerializeField] private float range;
        public float Range => range;

        [SerializeField] private float power;
        public float Power => power;
    }
}