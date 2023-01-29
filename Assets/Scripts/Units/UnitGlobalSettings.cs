using Systems.GlobalSetting;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace Units
{
    public class UnitGlobalSettings : GlobalSettings<UnitGlobalSettings>
    {
        [SerializeField]
        private SerializableDictionary<UnitType, UnitData> unitDataGroups = new SerializableDictionary<UnitType, UnitData>();
        public static SerializableDictionary<UnitType, UnitData> UnitDataGroups => Instance.unitDataGroups;

        public static UnitData GetUnitData(UnitType unitType)
        {
            if (UnitDataGroups.TryGetValue(unitType, out UnitData data))
            {
                return data;
            }

            Debug.LogError($"No unit data found for {unitType} in UnitGlobalSettings");
            return null;
        }
        
#if UNITY_EDITOR
        [UnityEditor.SettingsProvider]
        public static UnityEditor.SettingsProvider DisplayGeneralSettings()
        {
            return CreateGlobalSettingsProvider("Project/Strategy/Units");
        }
#endif
    }
}