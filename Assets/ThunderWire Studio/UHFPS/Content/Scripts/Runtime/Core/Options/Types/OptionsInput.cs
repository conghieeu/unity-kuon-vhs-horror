using System;
using System.Reactive;
using System.Reactive.Disposables;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;
using TMPro;
using static UHFPS.Input.InputManager;

namespace UHFPS.Runtime
{
    [InspectorHeader("Options Input")]
    public class OptionsInput : MonoBehaviour
    {
        public InputReference InputReference;

        [Header("References")]
        public Button Binding;
        public TMP_Text InputText;

        [Header("Texts")]
        public GString RebindText;
        public GString NoneText;

        private InputManager input;
        private readonly CompositeDisposable disposables = new();

        private bool isRebinding;
        private string prevName;

        private void Awake()
        {
            input = InputManager.Instance;
            input.OnRebindStart.Subscribe(OnRebindStart).AddTo(disposables);
            input.OnRebindEnd.Subscribe(OnRebindEnd).AddTo(disposables);

            RebindText.SubscribeGloc();
            NoneText.SubscribeGloc();
        }

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        private void Start()
        {
            InputManagerE.ObserveBindingPath(InputReference.ActionName, InputReference.BindingIndex, (apply, newPath) =>
            {
                if (newPath == NULL)
                {
                    InputText.text = NoneText;
                    return;
                }

                InputBinding inputBinding = new(newPath);
                InputText.text = inputBinding.ToDisplayString(InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            });
        }

        public void StartRebind()
        {
            prevName = InputText.text;
            Binding.interactable = false;

            StartRebindOperation(InputReference.ActionName, InputReference.BindingIndex);
            InputText.text = RebindText;
            isRebinding = true;
        }

        private void OnRebindStart(Unit _)
        {
            Binding.interactable = false;
        }

        private void OnRebindEnd(bool completed)
        {
            if (!completed && isRebinding) 
                InputText.text = prevName;

            Binding.interactable = true;
            isRebinding = false;
        }
    }
}