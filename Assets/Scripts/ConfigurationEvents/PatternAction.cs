using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A container used to assign a UnityEvent to be invoked later with optional specified parameters to invoke it with.
///
/// This contains a field of each type of <see cref="ISubPatternAction"/> in order to simulate polymorphism without
/// actually using polymorphism. Only one of these fields is relevant and is specified by the <see cref="type"/> value.
/// The reason for this is because the Unity editor does not work well with serializing collections of polymorphic data. 
/// </summary>
[Serializable]
public class PatternAction {
    public static PatternActionType GetPatternActionType(Type keyType) {
        PatternActionType ret;
        // The default value of Type is null, so we return null if we do not have a matching Type for keyType
        _typesDict.TryGetValue(keyType, out ret);
        return ret;
    }
    
    public static readonly string NoneString = "None";
    
    [HideInInspector]
    public int ID;
    public string ActionName = NoneString;
    public PatternActionType type = PatternActionType.None;

    #region Type-specific definitions
    
    public enum PatternActionType {
        Undefined, // Default type
        None,
        Basic,
        Vector
    }

    private static Dictionary<Type, PatternActionType> _typesDict = new Dictionary<Type, PatternActionType> {
        { typeof(Vector2), PatternActionType.Vector }
    };

    /// <summary>
    /// New <see cref="ISubPatternAction"/> fields be defined here.
    /// The field names must match their enum types. TODO enforce this.
    /// </summary>
    public BasicSubPatternAction Basic;
    public VectorSubPatternAction Vector;
    
    public void InvokePatternAction() {
        switch (type) {
            case PatternActionType.Basic:
                Basic.InvokePatternAction();
                break;
            case PatternActionType.Vector:
                Vector.InvokePatternAction();
                break;
            case PatternActionType.None:
                throw new Exception("Somehow, someway, we're attempting to invoke a base PatternAction :(");
        }
    }

    public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
        switch (type) {
            case PatternActionType.Basic:
                Basic.GeneratePatternActionEvent(method, target);
                break;
            case PatternActionType.Vector:
                Vector.GeneratePatternActionEvent(method, target);
                break;
            case PatternActionType.None:
                break;
        }
    }

    public static PatternAction CreatePatternAction(PatternActionType patternActionType) {
        switch (patternActionType) {
            case PatternActionType.Basic:
                return CreateBasicPatternAction();
            case PatternActionType.Vector:
                return CreateVectorPatternAction();
            case PatternActionType.None:
                return CreateNullPatternAction();
            default:
                throw new Exception("Failed to create PatternAction for type: " + patternActionType);
        }
    }
    
    private static PatternAction CreateBasicPatternAction() {
        PatternAction act = new PatternAction {type = PatternActionType.Basic, Basic = new BasicSubPatternAction()};
        return act;
    }

    private static PatternAction CreateVectorPatternAction() {
        PatternAction act = new PatternAction {type = PatternActionType.Vector, Vector = new VectorSubPatternAction()};
        return act;
    }

    public static PatternAction CreateNullPatternAction() {
        PatternAction act = new PatternAction();
        return act;
    }
    
    #endregion

    #region SubPatternActions
    
    interface ISubPatternAction {
        void InvokePatternAction();
        void GeneratePatternActionEvent(MethodInfo method, Pattern target);
    }

    [Serializable]
    public class BasicSubPatternAction : ISubPatternAction {
        public UnityEvent OnPatternAction;

        public void InvokePatternAction() {
            OnPatternAction.Invoke();
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
            UnityEvent newEvent = new UnityEvent();
            UnityAction action = (UnityAction) method.CreateDelegate(typeof(UnityAction), target);
            UnityEventTools.AddPersistentListener(newEvent, action);
            
            OnPatternAction = newEvent;
        }
    }

    // TODO apparently this isn't necessary in Unity 2020.1 since you can serialize generics there
    [Serializable]
    public class VectorEvent : UnityEvent<Vector2> { }

    [Serializable]
    public class VectorSubPatternAction : ISubPatternAction {
        public Vector2 Vector;
        public VectorEvent OnPatternAction;
        public void InvokePatternAction() {
            OnPatternAction.Invoke(Vector);
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
            VectorEvent newEvent = new VectorEvent();
            UnityAction<Vector2> action = (UnityAction<Vector2>)method.CreateDelegate(typeof(UnityAction<Vector2>), target);
            UnityEventTools.AddPersistentListener(newEvent, action);
            
            OnPatternAction = newEvent;
        }
    }
    
    #endregion
}