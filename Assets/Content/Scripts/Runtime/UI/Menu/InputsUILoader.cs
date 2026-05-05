using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UHFPS.Input;
using ThunderWire.Attributes;
using static UHFPS.Input.InputManager;

namespace UHFPS.Runtime
{
    [InspectorHeader("Inputs UI Loader")]
    [Summary("Tự động tải và hiển thị danh sách các phím điều khiển từ InputManager lên giao diện người dùng.")]
    public class InputsUILoader : MonoBehaviour
    {
        [Tooltip("Transform cha chứa danh sách các phần tử gán phím.")]
        public Transform BindingsParent;
        [Tooltip("Prefab cho mỗi dòng gán phím.")]
        public GameObject BindingPrefab;

        [Header("Texts")]
        [Tooltip("Text hiển thị khi đang chờ người chơi nhấn phím mới.")]
        public string RebindText = "Press Button";
        [Tooltip("Text hiển thị khi hành động chưa được gán phím.")]
        public string NoneText = "None";

        [Header("Colors")]
        [Tooltip("Màu sắc bình thường của dòng gán phím.")]
        public Color RebindNormalColor = Color.white;
        [Tooltip("Màu sắc cảnh báo khi hành động chưa được gán phím.")]
        public Color RebindNoneColor = Color.red;

        private InputManager input;
        private string rebindText;
        private string noneText;

        private bool isInited;
        private bool applyBindings;

        private readonly List<BindingField> bindingFields = new();
        private readonly CompositeDisposable disposables = new();

        private void Awake()
        {
            input = InputManager.Instance;
            disposables.Add(input.OnInputsInit.Subscribe(OnInputsInit));
            disposables.Add(input.OnRebindEnd.Subscribe(OnRebindEnd));
            RebindText.SubscribeGloc(text => rebindText = text);
            NoneText.SubscribeGloc(text => noneText = text);
        }

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        public void ApplyBindingOverrides()
        {
            if (!applyBindings) return;
            ApplyInputRebindOverrides();
            applyBindings = false;
        }

        public void DiscardBindingOverrides()
        {
            if (!applyBindings) return;
            DiscardInputRebindOverrides();
            applyBindings = false;
        }

        public void ResetToDefaults()
        {
            ResetInputsToDefaults();
            applyBindings = true;
        }

        private void SetAllFieldButtons(bool state)
        {
            bindingFields.ForEach(field => field.RebindControlButton.interactable = state);
        }

        private void OnInputsInit(Unit _)
        {
            foreach (var action in input.Actions.Value)
            {
                foreach (var binding in action.bindings)
                {
                    GameObject bindingGO = Instantiate(BindingPrefab, Vector3.zero, Quaternion.identity, BindingsParent);
                    BindingField field = bindingGO.GetComponent<BindingField>();
                    Image fieldImage = bindingGO.GetComponent<Image>();
                    bindingFields.Add(field);

                    bindingGO.name = binding.Value.ToString();
                    binding.Value.ToString().SubscribeGloc(text => field.BindingName.text = text);

                    InputManagerE.ObserveBindingPath(action.action.name, binding.Value.bindingIndex, (apply, newPath) =>
                    {
                        if(newPath == NULL)
                        {
                            if(isInited && !apply) fieldImage.color = RebindNoneColor;
                            else fieldImage.color = RebindNormalColor;
                            field.BindingControl.text = noneText;
                            return;
                        }

                        fieldImage.color = RebindNormalColor;
                        InputBinding inputBinding = new(newPath);
                        field.BindingControl.text = inputBinding.ToDisplayString(InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                        applyBindings = isInited;
                    });

                    field.RebindControlButton.onClick.AddListener(() =>
                    {
                        StartRebindOperation(action.action.name, binding.Value.bindingIndex);
                        field.BindingControl.text = rebindText;
                        SetAllFieldButtons(false);
                    });
                }
            }

            isInited = true;
        }

        private void OnRebindEnd(bool state)
        {
            SetAllFieldButtons(true);
        }
    }
}