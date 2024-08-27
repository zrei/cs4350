using UnityEngine;

public delegate void IntEvent(int _);
public delegate void VoidEvent();
public delegate void FloatEvent(float _);
public delegate void Vector3Event(Vector3 _);

public static class GlobalEvents {

    public static class UI {

    }

    public static class Battle {
        public delegate void UnitEvent(Unit _);
        public static UnitEvent UnitDefeatedEvent;
    }
}