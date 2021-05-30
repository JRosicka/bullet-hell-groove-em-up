using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rumia {
    /// <summary>
    /// An entity that we can schedule operations to be performed on via <see cref="RumiaMeasure"/>.
    /// This keeps a collection of <see cref="RumiaActions"/> that it creates based on its methods tagged with
    /// <see cref="RumiaActionAttribute"/>. This is for ease of use so that entity-specific behaviors can be defined
    /// in child classes to be easily invoked via <see cref="RumiaMeasure"/>s.
    /// </summary>
    public abstract class Pattern : MonoBehaviour {
        [SerializeField, HideInInspector] 
        private List<RumiaAction> RumiaActions;

        /// <summary>
        /// Spawn this pattern in the scene at the specified position.
        /// Note that this pattern will already have been instantiated at game start time for scheduling purposes. This
        /// method unhides it and sets its position.
        /// </summary>
        [RumiaAction]
        // ReSharper disable once UnusedMember.Global
        public void Spawn(Vector2 position) {
            transform.position = position;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Perform the <see cref="RumiaAction"/> that was scheduled
        /// </summary>
        public void InvokeRumiaAction(int id, string serializedParameter) {
            RumiaActions.First(e => e.ID == id).GetSubRumiaAction()?.InvokeRumiaAction(serializedParameter);
        }

        public List<RumiaAction> GetAllRumiaActions() {
            List<RumiaAction> allRumiaActions = new List<RumiaAction>(RumiaActions);

            return allRumiaActions;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Delete whatever data we have in <see cref="RumiaActions"/> and recreate it.
        /// - The first RumiaAction in the list is null
        /// - Additional entries are created and added for each method in this script with a
        ///   <see cref="RumiaActionAttribute"/> tag.
        ///
        /// Also sets the int IDs of each RumiaAction
        /// </summary>
        public void GenerateRumiaActions() {
            RumiaActions.Clear();

            // Add the null RumiaAction if it is not present 
            RumiaActions.Add(RumiaAction.CreateNullRumiaAction());

            MethodInfo[] methods = GetType()
                .GetMethods()
                .Where(t => t.GetCustomAttributes(typeof(RumiaActionAttribute), false).Length > 0)
                .ToArray();
            foreach (MethodInfo method in methods) {
                GenerateRumiaActionForMethod(method);
            }

            // Update the IDs in case any entries were added/removed/reordered
            for (int i = 0; i < RumiaActions.Count; i++) {
                RumiaActions[i].ID = i;
            }

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Create a pattern action for a single method. This determines what type of <see cref="RumiaAction.ISubRumiaAction"/> to use based on the
        /// type of the method's parameter. Only accepts methods with a single parameter or no parameters.
        /// Creates the new RumiaAction and adds it to <see cref="RumiaActions"/>.
        /// </summary>
        private void GenerateRumiaActionForMethod(MethodInfo method) {
            ParameterInfo[] parameters = method.GetParameters();

            // Only accept methods with zero or one parameter
            if (parameters.Length >= 2) {
                Debug.LogError("Too many parameters for method " + method.Name 
                                                                 + ". Methods with the [RumiaActionAttribute] can only have one parameter.");
                return;
            }

            RumiaAction.RumiaActionType actionType;
            if (parameters.Length == 0) {
                // No parameters, so use a BasicSubRumiaAction
                actionType = RumiaAction.RumiaActionType.Basic;
            } else {
                // Exactly one parameter. Get its mapped RumiaAction type.
                Type parameterType = parameters[0].ParameterType;
                actionType = RumiaAction.GetRumiaActionType(parameterType);
                if (actionType == RumiaAction.RumiaActionType.Undefined) {
                    Debug.LogError(
                        "No RumiaAction type assigned to typesDict for parameter type: " + parameterType);
                    return;
                }
            }

            // Create the RumiaAction based on the per-SubRumiaAction definition
            RumiaAction rumiaAction = RumiaAction.CreateRumiaAction(actionType);
            rumiaAction.ActionName = method.Name;
            rumiaAction.GetSubRumiaAction()?.GenerateRumiaActionEvent(method, this);

            RumiaActions.Add(rumiaAction);
        }
#endif
    }
}