using Systems.ObjectPooling;
using UnityEngine;

namespace Units
{
    public class UnitParameters
    {
        public UnitType unitType;
        public Transform teamParent;
        public string team;
        public string generatedID;
    }
    
    /// <summary>
    /// Persistent Unity Controller, that spawns and returns units by request using Object Pooler
    /// </summary>
    public class PersistentUnitController
    {
        /// <summary>
        /// Requests and preloads one of each NPC
        /// </summary>
        public static void PreloadEachUnit()
        {
            foreach (UnitData data in UnitGlobalSettings.UnitDataGroups.Values)
            {
                if (data.Prefab != null)
                {
                    ObjectPooler.PreloadObjects(data.Prefab, 1);
                }
            }
        }

        /// <summary>
        /// Requests and returns the unit. Assigns position & rotation
        /// </summary>
        /// <param name="unitParameters"></param>
        /// <param name="position">The world position</param>
        /// <param name="rotation">The world rotation</param>
        /// <param name="setInactive">If true will set the unit as inactive</param>
        /// <returns>Returns the requested unit</returns>
        public static Unit RequestUnit(UnitParameters unitParameters, Vector3 position, Quaternion rotation, bool setInactive)
        {
            if (SpawnUnit(unitParameters.unitType, unitParameters.teamParent, out Unit unit))
            {
                if (setInactive)
                {
                    unit.SetInactive();
                }
                
                unit.AssignUnitID(unitParameters.generatedID);
                unit.transform.position = position;
                unit.transform.rotation = rotation;
                return unit;
            }

            return null;
        }
        
        /// <summary>
        /// Spawns new Unit based on unit type with given name and parent.
        /// Returns true if was successful
        /// </summary>
        /// <param name="unitType">Unit type</param>
        /// <param name="parent">Parent transform</param>
        /// <param name="unit">Unit to assign</param>
        /// <returns>Returns true if unit spawn was successful</returns>
        public static bool SpawnUnit(UnitType unitType, Transform parent, out Unit unit)
        {
            Unit prefab = GetPrefabByUnitType(unitType, out UnitData data);

            if (prefab == null)
            {
                Debug.LogWarning($"{unitType} unit type group doesn't exist or prefab is null");
                unit = null;
                return false;
            }

            unit = ObjectPooler.Instantiate(prefab, parent);
           // unit.SetWingsuiterNameKey(data.CharacterNameKey, data.CharacterName);
            return true;
        }

        /// <summary>
        /// Gets the unit prefab by unit type
        /// </summary>
        /// <param name="unitType">Unit type</param>
        /// <param name="data">Unity data to assign</param>
        /// <returns>Returns the unit prefab associated with the given unit type. Can return null</returns>
        private static Unit GetPrefabByUnitType(UnitType unitType, out UnitData data)
        {
            data = UnitGlobalSettings.GetUnitData(unitType);
            Unit unit = data.Prefab;

            if (unit == null)
            {
                Debug.LogError($"Trying to get unit prefab by unit type, but such type isn't exist: {unitType}");
            }
            
            return unit;
        }

    }
}