using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

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
        Vector,
        Bool,
        Int,
        Float,
        String
    }

    private static Dictionary<Type, PatternActionType> _typesDict = new Dictionary<Type, PatternActionType> {
        { typeof(Vector2), PatternActionType.Vector },
        { typeof(bool), PatternActionType.Bool },
        { typeof(int), PatternActionType.Int },
        { typeof(float), PatternActionType.Float },
        { typeof(string), PatternActionType.String }
    };

    /// <summary>
    /// New <see cref="ISubPatternAction"/> fields be defined here.
    /// The field names must match their enum types. TODO enforce this.
    /// </summary>
    public BasicSubPatternAction Basic;
    public VectorSubPatternAction Vector;
    public BoolSubPatternAction Bool;
    public IntSubPatternAction Int;
    public FloatSubPatternAction Float;
    public StringSubPatternAction String;
    
    public ISubPatternAction GetSubPatternAction() {
        switch (type) {
            case PatternActionType.Basic:
                return Basic;
            case PatternActionType.Vector:
                return Vector;
            case PatternActionType.Bool:
                return Bool;
            case PatternActionType.Int:
                return Int;
            case PatternActionType.Float:
                return Float;
            case PatternActionType.String:
                return String;
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
            case PatternActionType.Bool:
                return CreateBoolPatternAction();
            case PatternActionType.Int:
                return CreateIntPatternAction();
            case PatternActionType.Float:
                return CreateFloatPatternAction();
            case PatternActionType.String:
                return CreateStringPatternAction();
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

    private static PatternAction CreateBoolPatternAction() {
        PatternAction act = new PatternAction {type = PatternActionType.Bool, Bool = new BoolSubPatternAction()};
        return act;
    }

    private static PatternAction CreateIntPatternAction() {
        PatternAction act = new PatternAction {type = PatternActionType.Int, Int = new IntSubPatternAction()};
        return act;
    }

    private static PatternAction CreateFloatPatternAction() {
        PatternAction act = new PatternAction {type = PatternActionType.Float, Float = new FloatSubPatternAction()};
        return act;
    }

    private static PatternAction CreateStringPatternAction() {
        PatternAction act = new PatternAction {type = PatternActionType.String, String = new StringSubPatternAction()};
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
            OnPatternAction.Invoke();    // TODO: Not the most relevant place to put this, BUT when we have a PatternConfiguration configured and then later add in a new pattern action type for that pattern, we currently need to look at all of the scripts in order to update the IDs to the correct values. This can be tough to catch sometimes. We should fix this so that the values are updated for everything whenever we add new pattern action methods (or whatever they're called)
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
            UnityEvent newEvent = new UnityEvent();
            UnityAction action = (UnityAction) method.CreateDelegate(typeof(UnityAction), target);
            UnityEventTools.AddPersistentListener(newEvent, action);

            OnPatternAction = newEvent;
#endif
        }

        public Type GetParameterType() {
            return null;
        }
    }
    
    // TODO apparently this isn't necessary in Unity 2020.1 since you can serialize generics there
    [Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    [Serializable]
    public class BoolSubPatternAction : ISubPatternAction {
        public BoolEvent OnPatternAction;
        public void InvokePatternAction(string serializedParameter) {
            OnPatternAction.Invoke(DeserializeParameter(serializedParameter));
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
            BoolEvent newEvent = new BoolEvent();
            UnityAction<bool> action = (UnityAction<bool>) method.CreateDelegate(typeof(UnityAction<bool>), target);
            UnityEventTools.AddPersistentListener(newEvent, action);

            OnPatternAction = newEvent;
#endif
        }

        public Type GetParameterType() {
            return typeof(bool);
        }

        public static string SerializeParameter(bool b) {
            return b.ToString();
        }

        public static bool DeserializeParameter(string serializedParameter) {
            if (serializedParameter == null || serializedParameter.Equals(""))
                return default;
            if (serializedParameter.Equals("True"))
                return true;
            if (serializedParameter.Equals("False"))
                return false;
            throw new Exception("BoolSubPatternAction: Unexpected serialized parameter value '" + serializedParameter + "'");
        }
    }
    
    [Serializable]
    public class IntEvent : UnityEvent<int> { }
    [Serializable]
    public class IntSubPatternAction : ISubPatternAction {
        public IntEvent OnPatternAction;
        public void InvokePatternAction(string serializedParameter) {
            OnPatternAction.Invoke(DeserializeParameter(serializedParameter));
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
            IntEvent newEvent = new IntEvent();
            UnityAction<int> action = (UnityAction<int>) method.CreateDelegate(typeof(UnityAction<int>), target);
            UnityEventTools.AddPersistentListener(newEvent, action);

            OnPatternAction = newEvent;
#endif
        }

        public Type GetParameterType() {
            return typeof(int);
        }

        public static string SerializeParameter(int b) {
            return b.ToString();
        }

        public static int DeserializeParameter(string serializedParameter) {
            if (serializedParameter == null || serializedParameter.Equals(""))
                return default;
            int ret;
            if (int.TryParse(serializedParameter, out ret))
                return ret;
            throw new Exception("IntSubPatternAction: Unexpected serialized parameter value '" + serializedParameter + "'");
        }
    }
    
    [Serializable]
    public class FloatEvent : UnityEvent<float> { }
    [Serializable]
    public class FloatSubPatternAction : ISubPatternAction {
        public FloatEvent OnPatternAction;
        public void InvokePatternAction(string serializedParameter) {
            OnPatternAction.Invoke(DeserializeParameter(serializedParameter));
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
            FloatEvent newEvent = new FloatEvent();
            UnityAction<float> action = (UnityAction<float>) method.CreateDelegate(typeof(UnityAction<float>), target);
            UnityEventTools.AddPersistentListener(newEvent, action);

            OnPatternAction = newEvent;
#endif
        }

        public Type GetParameterType() {
            return typeof(float);
        }

        public static string SerializeParameter(float b) {
            return $"{b:N4}";    // 4 decimal places
        }

        public static float DeserializeParameter(string serializedParameter) {
            if (serializedParameter == null || serializedParameter.Equals(""))
                return default;
            float ret;
            if (float.TryParse(serializedParameter, out ret))
                return ret;
            throw new Exception("FloatSubPatternAction: Unexpected serialized parameter value '" + serializedParameter + "'");
        }
    }
    
    [Serializable]
    public class StringEvent : UnityEvent<string> { }
    [Serializable]
    public class StringSubPatternAction : ISubPatternAction {
        public StringEvent OnPatternAction;
        public void InvokePatternAction(string serializedParameter) {
            OnPatternAction.Invoke(DeserializeParameter(serializedParameter));
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
            StringEvent newEvent = new StringEvent();
            UnityAction<string> action =
                (UnityAction<string>) method.CreateDelegate(typeof(UnityAction<string>), target);
            UnityEventTools.AddPersistentListener(newEvent, action);

            OnPatternAction = newEvent;
#endif
        }

        public Type GetParameterType() {
            return typeof(string);
        }

        public static string SerializeParameter(string b) {
            return b;
        }

        public static string DeserializeParameter(string serializedParameter) {
            if (serializedParameter == null || serializedParameter.Equals(""))
                return default;
            return serializedParameter;
        }
    }
    
    [Serializable]
    public class VectorEvent : UnityEvent<Vector2> { }
    [Serializable]
    public class VectorSubPatternAction : ISubPatternAction {
        public VectorEvent OnPatternAction;
        public void InvokePatternAction(string serializedParameter) {
            OnPatternAction.Invoke(DeserializeParameter(serializedParameter));
        }

        public void GeneratePatternActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
            VectorEvent newEvent = new VectorEvent();
            UnityAction<Vector2> action =
                (UnityAction<Vector2>) method.CreateDelegate(typeof(UnityAction<Vector2>), target);
            UnityEventTools.AddPersistentListener(newEvent, action);

            OnPatternAction = newEvent;
#endif
        }

        public Type GetParameterType() {
            return typeof(Vector2);
        }
        
        public static string SerializeParameter(Vector2 vector) {
            return $"{vector.x} {vector.y}";
        }
        
        public static Vector2 DeserializeParameter(string serializedParameter) {
            if (serializedParameter == null || serializedParameter.Equals(""))
                return default;
            string[] vectorComponents = serializedParameter.Split(' ');
            float x = float.Parse(vectorComponents[0]);
            float y = float.Parse(vectorComponents[1]);
            return new Vector2(x, y);
        } 

    }
    
    #endregion
}