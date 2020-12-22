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
    
    public ISubPatternAction GetSubPatternAction() {
        switch (type) {
            case PatternActionType.Basic:
                return Basic;
            case PatternActionType.Vector:
                return Vector;
            case PatternActionType.None:
                return null;
            default:
                throw new Exception("PatternAction type is Undefined");
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
    
    public interface ISubPatternAction {
        void InvokePatternAction(string serializedParameter);
        void GeneratePatternActionEvent(MethodInfo method, Pattern target);
        Type GetParameterType();
    }

    [Serializable]
    public class BasicSubPatternAction : ISubPatternAction {
        public UnityEvent OnPatternAction;

        public void InvokePatternAction(string serializedParameter) {
            OnPatternAction.Invoke();
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
            UnityEvent newEvent = new UnityEvent();
            UnityAction action = (UnityAction) method.CreateDelegate(typeof(UnityAction), target);
            UnityEventTools.AddPersistentListener(newEvent, action);
            
            OnPatternAction = newEvent;
        }
        
        public Type GetParameterType() {
            return null;
        }
    }

    // TODO apparently this isn't necessary in Unity 2020.1 since you can serialize generics there
    [Serializable]
    public class VectorEvent : UnityEvent<Vector2> { }

    [Serializable]
    public class VectorSubPatternAction : ISubPatternAction {
        public VectorEvent OnPatternAction;
        public void InvokePatternAction(string serializedParameter) {
            OnPatternAction.Invoke(DeserializeVector2(serializedParameter));
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
            VectorEvent newEvent = new VectorEvent();
            UnityAction<Vector2> action = (UnityAction<Vector2>)method.CreateDelegate(typeof(UnityAction<Vector2>), target);
            UnityEventTools.AddPersistentListener(newEvent, action);
            
            OnPatternAction = newEvent;
        }
        
        public Type GetParameterType() {
            return typeof(Vector2);
        }
        
        public static string SerializeVector2(Vector2 vector) {
            return $"{vector.x} {vector.y}";
        }
        
        public static Vector2 DeserializeVector2(string serializedVector) {
            if (serializedVector == null || serializedVector.Equals(""))
                return default;
            string[] vectorComponents = serializedVector.Split(' ');
            float x = float.Parse(vectorComponents[0]);
            float y = float.Parse(vectorComponents[1]);
            return new Vector2(x, y);
        } 

    }
    
    #endregion
}