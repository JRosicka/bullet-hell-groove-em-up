using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace Rumia {
    /// <summary>
    /// A container used to assign a UnityEvent to be invoked later with optional specified parameters to invoke it with.
    ///
    /// This contains a field of each type of <see cref="ISubRumiaAction"/> in order to simulate polymorphism without
    /// actually using polymorphism. Only one of these fields is relevant and is specified by the <see cref="type"/> value.
    /// The reason for this is because the Unity editor does not work well with serializing collections of polymorphic data. 
    /// </summary>
    [Serializable]
    public class RumiaAction {
        public static RumiaActionType GetRumiaActionType(Type keyType) {
            // The default value of Type is null, so we return null if we do not have a matching Type for keyType
            _typesDict.TryGetValue(keyType, out RumiaActionType ret);
            return ret;
        }

        public static readonly string NoneString = "None";

        [HideInInspector] public int ID;
        public string ActionName = NoneString;
        public RumiaActionType type = RumiaActionType.None;

        #region Type-specific definitions

        public enum RumiaActionType {
            Undefined, // Default type
            None,
            Basic,
            Vector,
            Bool,
            Int,
            Float,
            String
        }

        private static Dictionary<Type, RumiaActionType> _typesDict = new Dictionary<Type, RumiaActionType> {
            {typeof(Vector2), RumiaActionType.Vector},
            {typeof(bool), RumiaActionType.Bool},
            {typeof(int), RumiaActionType.Int},
            {typeof(float), RumiaActionType.Float},
            {typeof(string), RumiaActionType.String}
        };

        /// <summary>
        /// New <see cref="ISubRumiaAction"/> fields be defined here.
        /// The field names must match their enum types. TODO enforce this.
        /// </summary>
        public BasicSubRumiaAction Basic;
        public VectorSubRumiaAction Vector;
        public BoolSubRumiaAction Bool;
        public IntSubRumiaAction Int;
        public FloatSubRumiaAction Float;
        public StringSubRumiaAction String;

        public ISubRumiaAction GetSubRumiaAction() {
            switch (type) {
                case RumiaActionType.Basic:
                    return Basic;
                case RumiaActionType.Vector:
                    return Vector;
                case RumiaActionType.Bool:
                    return Bool;
                case RumiaActionType.Int:
                    return Int;
                case RumiaActionType.Float:
                    return Float;
                case RumiaActionType.String:
                    return String;
                case RumiaActionType.None:
                    return null;
                default:
                    throw new Exception("RumiaAction type is Undefined");
            }
        }

        public static RumiaAction CreateRumiaAction(RumiaActionType rumiaActionType) {
            switch (rumiaActionType) {
                case RumiaActionType.Basic:
                    return CreateBasicRumiaAction();
                case RumiaActionType.Vector:
                    return CreateVectorRumiaAction();
                case RumiaActionType.Bool:
                    return CreateBoolRumiaAction();
                case RumiaActionType.Int:
                    return CreateIntRumiaAction();
                case RumiaActionType.Float:
                    return CreateFloatRumiaAction();
                case RumiaActionType.String:
                    return CreateStringRumiaAction();
                case RumiaActionType.None:
                    return CreateNullRumiaAction();
                default:
                    throw new Exception("Failed to create RumiaAction for type: " + rumiaActionType);
            }
        }

        private static RumiaAction CreateBasicRumiaAction() {
            RumiaAction act = new RumiaAction {type = RumiaActionType.Basic, Basic = new BasicSubRumiaAction()};
            return act;
        }

        private static RumiaAction CreateVectorRumiaAction() {
            RumiaAction act = new RumiaAction {type = RumiaActionType.Vector, Vector = new VectorSubRumiaAction()};
            return act;
        }

        private static RumiaAction CreateBoolRumiaAction() {
            RumiaAction act = new RumiaAction {type = RumiaActionType.Bool, Bool = new BoolSubRumiaAction()};
            return act;
        }

        private static RumiaAction CreateIntRumiaAction() {
            RumiaAction act = new RumiaAction {type = RumiaActionType.Int, Int = new IntSubRumiaAction()};
            return act;
        }

        private static RumiaAction CreateFloatRumiaAction() {
            RumiaAction act = new RumiaAction {type = RumiaActionType.Float, Float = new FloatSubRumiaAction()};
            return act;
        }

        private static RumiaAction CreateStringRumiaAction() {
            RumiaAction act = new RumiaAction {type = RumiaActionType.String, String = new StringSubRumiaAction()};
            return act;
        }

        public static RumiaAction CreateNullRumiaAction() {
            RumiaAction act = new RumiaAction();
            return act;
        }

        #endregion

        #region SubRumiaActions

        public interface ISubRumiaAction {
            void InvokeRumiaAction(string serializedParameter);
            void GenerateRumiaActionEvent(MethodInfo method, Pattern target);
            Type GetParameterType();
        }

        [Serializable]
        public class BasicSubRumiaAction : ISubRumiaAction {
            public UnityEvent OnRumiaAction;

            public void InvokeRumiaAction(string serializedParameter) {
                OnRumiaAction
                    .Invoke();
            }

            public void GenerateRumiaActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
                UnityEvent newEvent = new UnityEvent();
                UnityAction action = (UnityAction) method.CreateDelegate(typeof(UnityAction), target);
                UnityEventTools.AddPersistentListener(newEvent, action);

                OnRumiaAction = newEvent;
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
        public class BoolSubRumiaAction : ISubRumiaAction {
            public BoolEvent OnRumiaAction;

            public void InvokeRumiaAction(string serializedParameter) {
                OnRumiaAction.Invoke(DeserializeParameter(serializedParameter));
            }

            public void GenerateRumiaActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
                BoolEvent newEvent = new BoolEvent();
                UnityAction<bool> action = (UnityAction<bool>) method.CreateDelegate(typeof(UnityAction<bool>), target);
                UnityEventTools.AddPersistentListener(newEvent, action);

                OnRumiaAction = newEvent;
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
                throw new Exception("BoolSubRumiaAction: Unexpected serialized parameter value '"
                                    + serializedParameter + "'");
            }
        }

        [Serializable]
        public class IntEvent : UnityEvent<int> { }

        [Serializable]
        public class IntSubRumiaAction : ISubRumiaAction {
            public IntEvent OnRumiaAction;

            public void InvokeRumiaAction(string serializedParameter) {
                OnRumiaAction.Invoke(DeserializeParameter(serializedParameter));
            }

            public void GenerateRumiaActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
                IntEvent newEvent = new IntEvent();
                UnityAction<int> action = (UnityAction<int>) method.CreateDelegate(typeof(UnityAction<int>), target);
                UnityEventTools.AddPersistentListener(newEvent, action);

                OnRumiaAction = newEvent;
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
                if (int.TryParse(serializedParameter, out int ret))
                    return ret;
                throw new Exception("IntSubRumiaAction: Unexpected serialized parameter value '" + serializedParameter
                                                                                                   + "'");
            }
        }

        [Serializable]
        public class FloatEvent : UnityEvent<float> { }

        [Serializable]
        public class FloatSubRumiaAction : ISubRumiaAction {
            public FloatEvent OnRumiaAction;

            public void InvokeRumiaAction(string serializedParameter) {
                OnRumiaAction.Invoke(DeserializeParameter(serializedParameter));
            }

            public void GenerateRumiaActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
                FloatEvent newEvent = new FloatEvent();
                UnityAction<float> action =
                    (UnityAction<float>) method.CreateDelegate(typeof(UnityAction<float>), target);
                UnityEventTools.AddPersistentListener(newEvent, action);

                OnRumiaAction = newEvent;
#endif
            }

            public Type GetParameterType() {
                return typeof(float);
            }

            public static string SerializeParameter(float b) {
                return $"{b:N4}"; // 4 decimal places
            }

            public static float DeserializeParameter(string serializedParameter) {
                if (serializedParameter == null || serializedParameter.Equals(""))
                    return default;
                if (float.TryParse(serializedParameter, out float ret))
                    return ret;
                throw new Exception("FloatSubRumiaAction: Unexpected serialized parameter value '"
                                    + serializedParameter + "'");
            }
        }

        [Serializable]
        public class StringEvent : UnityEvent<string> { }

        [Serializable]
        public class StringSubRumiaAction : ISubRumiaAction {
            public StringEvent OnRumiaAction;

            public void InvokeRumiaAction(string serializedParameter) {
                OnRumiaAction.Invoke(DeserializeParameter(serializedParameter));
            }

            public void GenerateRumiaActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
                StringEvent newEvent = new StringEvent();
                UnityAction<string> action =
                    (UnityAction<string>) method.CreateDelegate(typeof(UnityAction<string>), target);
                UnityEventTools.AddPersistentListener(newEvent, action);

                OnRumiaAction = newEvent;
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
        public class VectorSubRumiaAction : ISubRumiaAction {
            public VectorEvent OnRumiaAction;

            public void InvokeRumiaAction(string serializedParameter) {
                OnRumiaAction.Invoke(DeserializeParameter(serializedParameter));
            }

            public void GenerateRumiaActionEvent(MethodInfo method, Pattern target) {
#if UNITY_EDITOR
                VectorEvent newEvent = new VectorEvent();
                UnityAction<Vector2> action =
                    (UnityAction<Vector2>) method.CreateDelegate(typeof(UnityAction<Vector2>), target);
                UnityEventTools.AddPersistentListener(newEvent, action);

                OnRumiaAction = newEvent;
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
}