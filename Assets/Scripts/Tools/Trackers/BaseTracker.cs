﻿using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AgentDebugTool.Scripts.Trackers
{
    /// <summary>
    /// Base updatable tracker class
    /// Utilizes the update coroutine in order to update the tracker in intervals
    /// </summary>
    public abstract class BaseTracker
    {
        protected const char NEW_LINE = '\n';

        protected readonly AgentDebugTool agentDebugTool;
        private readonly Text counterText;
        protected StringBuilder text;
        private bool isInitialized;
        private bool isActivated;
        protected bool dirty;

        /// <summary>
        /// Sets the reference to ml debug tool and subscribes to its events
        /// </summary>
        /// <param name="reference">The ML debug tool</param>
        /// <param name="counterText">The counter text filed to display the data</param>
        protected BaseTracker(AgentDebugTool reference, Text counterText)
        {
            agentDebugTool = reference;
            this.counterText = counterText;
        }

        #region Activation
        
        /// <summary>
        /// Simulates the OnEnable method
        /// Called on FPSCounter enabled
        /// </summary>
        public void Activate()
        {
            // Creates a new string builder or resets the existing one
            text = new StringBuilder(500);

            if (!isActivated)
            {
                // Invokes the activation actions
                // Must be implemented in the inherited class
                ActivationActions();
            }

            // Invokes the initialization actions
            // If wasn't initialized before 
            // Must be implemented in the inherited class
            if (!isInitialized)
            {
                InitActions();
            }
            
            // Updates and displays the value without force 
            UpdateValueAndDisplay(false);
            
            isActivated = true;
        }

        /// <summary>
        /// Method for overriden logic on counter activation
        /// </summary>
        protected virtual void ActivationActions() { }
        
        /// <summary>
        /// Method for overriden logic on counter initialization.
        /// Should call InitActions.base()
        /// </summary>
        protected virtual void InitActions()
        {
            isInitialized = true;
        }

        #endregion

        #region Update
        
        /// <summary>
        /// Invokes the update value method and displays the result text to the field
        /// </summary>
        /// <param name="force">If true will be forced to update the counter parameters</param>
        protected void UpdateValueAndDisplay(bool force)
        {
            // Updates the value with given force parameter
            UpdateValue(force);

            // Displays the counter text if the current operation mode is Normal and text field isn't null
            if (counterText != null)
            {
                DisplayText();
            }
        }

        /// <summary>
        /// Abstract update value method. All counter logic should be placed here
        /// </summary>
        /// <param name="force">If true will be forced to update the counter parameters</param>
        protected abstract void UpdateValue(bool force);
        
        /// <summary>
        /// Displays the current text value to the text field
        /// If the text field is null or current operation mode isn't normal will not proceed
        /// </summary>
        private void DisplayText()
        {
            counterText.text = text.ToString();
            dirty = false;
        }
        
        #endregion

        #region Deactivation

        /// <summary>
        /// Simulates OnDisable
        /// Called when MLDebugTool is disabled
        /// Should call InitActions.base()
        /// </summary>
        public void Deactivate()
        {
            // Return if wasn't initialized
            if (!isInitialized || !isActivated) 
                return;

            // Removes the text from the text field if exists
            text?.Remove(0, text.Length);

            // Clears the canvas texts
            ClearCanvasText();

            // Invokes the deactivation actions
            // Must be implemented in the inherited class
            DeactivationActions();

            // Sets as not initialized and not activated
            isInitialized = false;
            isActivated = false;
        }
        
        /// <summary>
        /// Clears the canvas text to empty string
        /// Should be overridden with the base reference
        /// </summary>
        public virtual void ClearCanvasText()
        {
            if(counterText != null)
            {
                counterText.text = "";
            }
        }

        /// <summary>
        /// Method for overriden logic on counter deactivation
        /// </summary>
        protected virtual void DeactivationActions() { }
        
        /// <summary>
        /// Simulates the OnDestroy method
        /// Called when MLDebugTool is destroyed
        /// Should call InitActions.base()
        /// </summary>
        public void Terminate()
        {
            if (text != null)
            {
                text.Remove(0, text.Length);
                text = null;
            }
        }
        
        #endregion
    }
}