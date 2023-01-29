using System.Linq;
using UnityEngine;

namespace Utilities.Extensions
{   
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            T foundComponent = component.GetComponent<T>();

            if (foundComponent != null)
            {
                return foundComponent;
            }

            return component.gameObject.AddComponent<T>();
        }

        public static T GetComponentInChildrenExcludeSelf<T>(this Component component) where T : Component
        {
            return component.GetComponentsInChildren<T>().FirstOrDefault(c => c.gameObject != component.gameObject);
        }
    }
}