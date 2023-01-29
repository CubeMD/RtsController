using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Units
{
    public enum UnitType
    {
        Engineer,
    }
    
    [Serializable]
    public class UnitData
    {
        [SerializeField]
        private UnitType unitType;
        public UnitType UnitType => unitType;
        
        [SerializeField]
        private Unit prefab;
        public Unit Prefab => prefab;
        
        [SerializeField]
        private Color unitColor = Color.white;
        public Color UnitColor => unitColor;
    }
}