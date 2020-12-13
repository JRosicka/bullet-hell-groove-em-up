using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PatternAction {
    public enum PatternActionType {
        None,
        Basic,
        Vector
    }

    public static readonly string NoneString = "None";
    
    [HideInInspector]
    public int ID;
    public string ActionName = NoneString;
    public PatternActionType type = PatternActionType.None;

    // Name must match its enum type. TODO enforce this
    public BasicSubPatternAction Basic;
    public VectorSubPatternAction Vector;
    // TODO: More PatternAction types go here
    
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
    
    interface ISubPatternAction {
        void InvokePatternAction();
    }

    [Serializable]
    public class BasicSubPatternAction : ISubPatternAction {
        public UnityEvent OnPatternAction;

        public void InvokePatternAction() {
            OnPatternAction.Invoke();
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
    }
}