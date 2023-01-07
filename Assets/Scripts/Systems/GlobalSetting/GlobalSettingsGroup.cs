using UnityEngine;

namespace Systems.GlobalSetting
{
    /// <summary>
    /// Identifies a field as belonging to a settings group.
    /// Groups can be displayed separately within ProjectSettings
    /// </summary>
    public class GlobalSettingsGroup : PropertyAttribute
    {
        public string groupID;

        public GlobalSettingsGroup(string groupID)
        {
            this.groupID = groupID;
        }
    }
}