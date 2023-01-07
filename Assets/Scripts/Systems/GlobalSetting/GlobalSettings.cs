using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Utilities.Attributes;

namespace Systems.GlobalSetting
{
     /// <summary>
    /// Abstract class for all GlobalSettings objects. Must be placed in Assets/Resources/GlobalSettings/
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GlobalSettings<T> : ScriptableObject where T : ScriptableObject
    {
        [StaticDomainReloadField(null)]
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    string directoryString = "GlobalSettings" + System.IO.Path.DirectorySeparatorChar;
                    T[] loadedSettings = Resources.LoadAll<T>(directoryString);
                    instance = loadedSettings.Length <= 0 ? null : loadedSettings[0];
                    
#if UNITY_EDITOR
                    if (instance == null)
                    {
                        instance = CreateInstance<T>();
                        string savePath = string.Format("Assets{0}Resources{0}{1}{0}{2}.asset",
                            System.IO.Path.DirectorySeparatorChar,
                            directoryString,
                            instance.GetType().Name);
                        string debugString = string.Format("{0} not found in {1}, creating new {0} at {2}",
                            instance.GetType().Name, directoryString, savePath);
                        Debug.LogWarning(debugString, instance);
                        AssetDatabase.CreateAsset(instance, savePath);
                        AssetDatabase.SaveAssets();
                    }
#endif
                }

                return instance;
            }
        }
        
#if UNITY_EDITOR
        protected static SettingsProvider CreateGlobalSettingsProvider(string path, params string[] acceptedGroupIDs)
        {
            return CreateGlobalSettingsProvider(path, SettingsScope.Project, acceptedGroupIDs);
        }
        
        protected static SettingsProvider CreateGlobalSettingsProvider(string path, SettingsScope settingsScope, params string[] acceptedGroupIDs)
        {
            SerializedObject settings = new SerializedObject(Instance);
            FieldInfo[] classFields = Instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Dictionary<string, FieldInfo> infoByProperty = new Dictionary<string, FieldInfo>();
            HashSet<SerializedProperty> permittedProperties = new HashSet<SerializedProperty>();

            foreach (FieldInfo fieldInfo in classFields)
            {
                infoByProperty.Add(fieldInfo.Name, fieldInfo);
            }
            
            List<string> displayNames = new List<string>();
            SerializedProperty propertyIterator =  settings.GetIterator();

            if (propertyIterator.NextVisible(true))
            {
                do
                {
                    displayNames.Add(propertyIterator.displayName);

                    if (acceptedGroupIDs == null ||
                        acceptedGroupIDs.Length <= 0 ||
                        (infoByProperty.ContainsKey(propertyIterator.name) &&
                         FieldHasAPermittedAttribute(infoByProperty[propertyIterator.name], acceptedGroupIDs)))
                    {
                        permittedProperties.Add(propertyIterator.Copy());
                    }
                } while (propertyIterator.NextVisible(false));
            }
            

            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            SettingsProvider provider = new SettingsProvider(path, settingsScope)
            {
                titleBarGuiHandler = () =>
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUIUtility.labelWidth *= 2f;
                    GUIStyle style = new GUIStyle(EditorStyles.label);
                    style.alignment = TextAnchor.MiddleRight;
                    style.clipping = TextClipping.Overflow;
                    style.stretchWidth = true;
                    EditorGUILayout.LabelField(path, style);
                    EditorGUIUtility.labelWidth *= 0.5f;
                    EditorGUI.EndDisabledGroup();
                },
                guiHandler = (searchContext) =>
                {
                    string trimmedSearchContext = new string(searchContext.Where(char.IsLetterOrDigit).ToArray());
                    
                    EditorGUIUtility.labelWidth *= 2f;

                    foreach (SerializedProperty property in permittedProperties)
                    {
                        if (property.displayName.
                                IndexOf(trimmedSearchContext, StringComparison.InvariantCultureIgnoreCase) >= 0 &&
                            permittedProperties.Contains(property))
                        {
                            EditorGUILayout.PropertyField(property, true);
                        }
                    }

                    EditorGUIUtility.labelWidth *= 0.5f;
                    settings.ApplyModifiedProperties();
                },
                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = displayNames.ToArray()
            };

            return provider;
        }

        public static void SetGlobalSettingDirty()
        {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(Instance);
        }

        private static bool FieldHasAPermittedAttribute(FieldInfo fieldInfo, string[] acceptedGroupIDs)
        {
            foreach (Attribute attribute in fieldInfo.GetCustomAttributes())
            {
                if (attribute is GlobalSettingsGroup globalSettingsGroup)
                {
                    foreach (string acceptedGroupID in acceptedGroupIDs)
                    {
                        if (globalSettingsGroup.groupID == acceptedGroupID)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
#endif
    }
}