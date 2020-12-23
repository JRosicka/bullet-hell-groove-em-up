using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// An entity that we can schedule operations to be performed on via <see cref="PatternMeasure"/>.
/// This keeps a collection of <see cref="PatternActions"/> that it creates based on its methods tagged with
/// <see cref="PatternActionAttribute"/>. This is for ease of use so that entity-specific behaviors can be defined
/// in child classes to be easily invoked via <see cref="PatternMeasure"/>s.
/// </summary>
public abstract class Pattern : MonoBehaviour {
    [SerializeField, HideInInspector]
    private List<PatternAction> PatternActions;

    /// <summary>
    /// Instantiate this pattern in the scene at a specified position
    /// </summary>
    [PatternActionAttribute]
    public void Spawn(Vector2 position) {
        Transform spawner = GameController.Instance.EnemyManager.transform;
        Instantiate(this, position, Quaternion.identity, spawner);
    }
    
    /// <summary>
    /// Perform the <see cref="PatternAction"/> that was scheduled
    /// </summary>
    public void InvokePatternAction(int id, string serializedParameter) {
        PatternActions.First(e => e.ID == id).GetSubPatternAction()?.InvokePatternAction(serializedParameter);
    }

    public List<PatternAction> GetAllPatternActions() {
        List<PatternAction> allPatternActions = new List<PatternAction>(PatternActions); 
        
        return allPatternActions;
    }
    
    #if UNITY_EDITOR
    
    /// <summary>
    /// Delete whatever data we have in <see cref="PatternActions"/> and recreate it.
    /// - The first PatternAction in the list is a NullPatternAction
    /// - Additional entries are created and added for each method in this script with a
    ///   <see cref="PatternActionAttribute"/> tag.
    ///
    /// Also sets the int IDs of each PatternAction
    /// </summary>
    public void GeneratePatternActions() {
        PatternActions.Clear();
            
        // Add the null PatternAction if it is not present 
        PatternActions.Add(PatternAction.CreateNullPatternAction());

        MethodInfo[] methods = GetType()
            .GetMethods()
            .Where(t => t.GetCustomAttributes(typeof(PatternActionAttribute), false).Length > 0)
            .ToArray();
        foreach (MethodInfo method in methods) {
            GeneratePatternActionForMethod(method);
        }
        
        // Update the IDs in case any entries were added/removed/reordered
        for (int i = 0; i < PatternActions.Count; i++) {
            PatternActions[i].ID = i;
        }
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Create a pattern action for a single method. This determines what type of SubPatternAction to use based on the
    /// type of the method's parameter. Only accepts methods with a single parameter or no parameters.
    /// Creates the new PatternAction and adds it to <see cref="PatternActions"/>.
    /// </summary>
    private void GeneratePatternActionForMethod(MethodInfo method) {
        ParameterInfo[] parameters = method.GetParameters();
        
        // Only accept methods with zero or one parameter
        if (parameters.Length >= 2) {
            Debug.LogError("Too many parameters for method " + method.Name + ". Methods with the [PatternActionAttribute] can only have one parameter.");
            return;
        }

        PatternAction.PatternActionType actionType;
        if (parameters.Length == 0) {
            // No parameters, so use a BasicSubPatternAction
            actionType = PatternAction.PatternActionType.Basic;
        } else {
            // Exactly one parameter. Get its mapped PatternAction type.
            Type parameterType = parameters[0].ParameterType;
            actionType = PatternAction.GetPatternActionType(parameterType);
            if (actionType == PatternAction.PatternActionType.Undefined) {
                Debug.LogError("No SubPatternAction type assigned to typesDict for parameter type: " + parameterType);
                return;
            }
        }

        // Create the PatternAction based on the per-SubPatternAction definition
        PatternAction patternAction = PatternAction.CreatePatternAction(actionType);
        patternAction.ActionName = method.Name;
        patternAction.GetSubPatternAction()?.GeneratePatternActionEvent(method, this);
        
        PatternActions.Add(patternAction);
    }
#endif
}