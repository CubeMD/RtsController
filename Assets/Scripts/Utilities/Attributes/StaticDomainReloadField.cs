using System;
using UnityEngine;

namespace Utilities.Attributes
{
    /// <summary>
    /// Automatically resets a field/property/event on reload, with some custom options.
    /// Used for resetting/setting values when Enter Play Mode options are enabled and
    /// Domain Reload is disabled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
    public class StaticDomainReloadField : PropertyAttribute
    {
        public readonly object resetValue = null;
        public readonly bool instantiateNew = false;
        
        /// <summary>
        /// Tags a field to be reset manually when domain reload is disabled in Enter Play Mode options.
        /// Will reset to the default value for the field type when no arguments are provided.
        /// </summary>
        /// <param name="resetValue">A value to reset the static field to on enter play mode. Must match the field type.</param>
        public StaticDomainReloadField(object resetValue)
        {
            this.resetValue = resetValue;
        }

        /// <summary>
        /// Tags a field to be reset manually when domain reload is disabled in Enter Play Mode options.
        /// Will reset to the default value for the field type when no arguments are provided.
        /// </summary>
        /// <param name="instantiateNew">Should a new instance of this type be created on enter play mode to populate the field? (e.g. private Vector3 v = new Vector3(); )</param>
        public StaticDomainReloadField(bool instantiateNew)
        {
            this.instantiateNew = instantiateNew;
        }

        /// <summary>
        /// Tags a field to be reset manually when domain reload is disabled in Enter Play Mode options.
        /// Will reset to the default value for the field type when no arguments are provided.
        /// </summary>
        public StaticDomainReloadField()
        {}
    }
}