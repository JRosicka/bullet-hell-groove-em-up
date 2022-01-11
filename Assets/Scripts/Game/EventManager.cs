using UnityEngine.Events;

public static class EventManager {
    public class IntEvent : UnityEvent<int> { }

    public static readonly IntEvent LifeCountChangeEvent = new IntEvent();
    public static readonly IntEvent BombCountChangeEvent = new IntEvent();
}