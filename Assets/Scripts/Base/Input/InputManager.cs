using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public delegate void InputCallback(IInput input);

    public interface IInput
    {
        event InputCallback OnPressEvent;
        event InputCallback OnHoldEvent;
        event InputCallback OnReleaseEvent;
        event InputCallback OnChangeEvent;

        T GetValue<T>() where T : struct;

        void AddInputBlocker(string id);
        void RemoveInputBlocker(string id);
    }

    public class InputManager : Singleton<InputManager>
    {
        private class Input : IInput
        {
            private readonly InputAction inputAction;
            private readonly InputManager inputManager;

            public event InputCallback OnPressEvent;
            public event InputCallback OnHoldEvent;
            public event InputCallback OnReleaseEvent;
            public event InputCallback OnChangeEvent;

            private Coroutine HoldCoroutine
            {
                set
                {
                    if (holdCoroutine != null) inputManager.StopCoroutine(holdCoroutine);
                    holdCoroutine = value;
                }
            }
            private Coroutine holdCoroutine;

            public bool IsBlocked
            {
                set
                {
                    isBlocked = value;
                    if (isBlocked)
                    {
                        inputAction.Disable();
                    }
                    else
                    {
                        inputAction.Enable();
                    }
                }
            }
            private bool isBlocked;

            private HashSet<string> inputBlockers = new();

            public Input(InputAction inputAction, InputManager inputManager)
            {
                this.inputAction = inputAction;
                this.inputManager = inputManager;

                inputAction.started += OnPress;
                inputAction.canceled += OnRelease;
                inputAction.performed += OnChange;

                inputAction.Enable();
            }

            private void OnPress(InputAction.CallbackContext context)
            {
                OnPressEvent?.Invoke(this);
                HoldCoroutine = inputManager.StartCoroutine(OnHold());
            }

            private IEnumerator OnHold()
            {
                while (inputAction.IsPressed())
                {
                    OnHoldEvent?.Invoke(this);
                    yield return null;
                }
            }

            private void OnRelease(InputAction.CallbackContext context)
            {
                OnReleaseEvent?.Invoke(this);
            }

            private void OnChange(InputAction.CallbackContext context)
            {
                OnChangeEvent?.Invoke(this);
            }

            public T GetValue<T>() where T : struct
            {
                return inputAction.ReadValue<T>();
            }

            public void AddInputBlocker(string id)
            {
                inputBlockers.Add(id);
                IsBlocked = inputBlockers.Count > 0;
            }

            public void RemoveInputBlocker(string id)
            {
                inputBlockers.Remove(id);
                IsBlocked = inputBlockers.Count == 0;
            }
        }

        [SerializeField]
        private InputActionAsset inputActions;

        private Dictionary<string, Input> inputs = new();

        public IInput SwitchAction => GetInput("SwitchAction");
        public IInput EndTurn => GetInput("EndTurn");
        public IInput RotateCamera => GetInput("RotateCamera");
        public IInput PointerPosition => GetInput("PointerPosition");
        public IInput PointerSelect => GetInput("PointerSelect");

        protected override void HandleAwake()
        {
            base.HandleAwake();

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            foreach (var action in inputActions)
            {
                inputs.TryAdd(action.name, new(action, this));
            }
        }

        public IInput GetInput(string name)
        {
            return inputs.GetValueOrDefault(name);
        }
    }
}
