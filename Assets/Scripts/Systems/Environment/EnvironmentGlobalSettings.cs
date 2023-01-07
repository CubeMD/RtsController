using Systems.GlobalSetting;
using UnityEngine;

namespace Systems.Environment
{
    public class EnvironmentGlobalSettings : GlobalSettings<EnvironmentGlobalSettings>
    {
        [SerializeField] [GlobalSettingsGroup("General")]
        private Vector2 groundSize = new Vector2(200, 200);
        public static Vector2 GroundSize => Instance.groundSize;
        
        [SerializeField] [GlobalSettingsGroup("Mass")]
        private Mass mass;
        public static Mass Mass => Instance.mass;

        [SerializeField] [GlobalSettingsGroup("Mass")]
        private int initialMassAmount = 75;
        public static int InitialMassAmount => Instance.initialMassAmount;
        
        [SerializeField] [GlobalSettingsGroup("Mass")]
        private Vector2 massAmountMinMax;
        public static Vector2 MassAmountMinMax => Instance.massAmountMinMax;
        
        [SerializeField] [GlobalSettingsGroup("Mass")]
        private Gradient massColorGradient;
        public static Gradient MassColorGradient => Instance.massColorGradient;

        public static Color GetMassGradientColor(float massAmount)
        {
            return MassColorGradient.Evaluate(Mathf.InverseLerp(MassAmountMinMax.x, MassAmountMinMax.y, massAmount));
        }
        
#if UNITY_EDITOR
        [UnityEditor.SettingsProvider]
        public static UnityEditor.SettingsProvider DisplayGeneralSettings()
        {
            return CreateGlobalSettingsProvider("Project/Strategy/Environment/General", "General");
        }
        
        [UnityEditor.SettingsProvider]
        public static UnityEditor.SettingsProvider DisplayMassSettings()
        {
            return CreateGlobalSettingsProvider("Project/Strategy/Environment/Mass", "Mass");
        }
#endif
    }
}