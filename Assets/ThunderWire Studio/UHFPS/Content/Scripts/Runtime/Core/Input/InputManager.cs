using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UHFPS.Scriptable;
using UHFPS.Runtime;
using ThunderWire.Attributes;
using NameAndParameters = UnityEngine.InputSystem.Utilities.NameAndParameters;

namespace UHFPS.Input
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/managing-inputs")]
    public class InputManager : Singleton<InputManager>
    {
        public const string EXTENSION = ".xml";
        public const string NULL = "null";
        private const string IGNORE_TAG = "*";

        public Dictionary<string, ActionMap> actionMap = new();
        public List<RebindContext> preparedRebinds = new();
        public CompositeDisposable disposables = new();

        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        private readonly Dictionary<string, bool> toggledActions = new();
        private readonly List<string> pressedActions = new();
        private bool isApplyPending;

        public InputActionAsset inputActions;
        public InputSpritesAsset inputSpritesAsset;
        public bool debugMode;

        public ReplaySubject<Unit> OnInputsInit = new();
        public Subject<bool> OnApply = new();

        public Subject<RebindContext> OnRebindPrepare = new();
        public Subject<Unit> OnRebindStart = new();
        public Subject<bool> OnRebindEnd = new();

        public static InputActionAsset ActionsAsset
        {
            get => Instance.inputActions;
        }

        public static InputSpritesAsset SpritesAsset
        {
            get => Instance.inputSpritesAsset;
        }

        public Lazy<IList<ActionMap.Action>> Actions { get; } = new(() =>
        {
            var actions = new List<ActionMap.Action>();
            var playerMap = Instance.actionMap.First();

            foreach (var action in playerMap.Value.actions)
            {
                if (!action.Key.Contains(IGNORE_TAG))
                    actions.Add(action.Value);
            }

            return actions;
        });

        private string InputsFilename => SerializationUtillity.SerializationAsset.InputsFilename + EXTENSION;

        private string InputsPath
        {
            get
            {
                string configPath = SerializationUtillity.SerializationAsset.GetConfigPath();
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                return configPath + "/" + InputsFilename;
            }
        }

        private async void Awake()
        {
            if (!inputActions) throw new NullReferenceException("InputActionAsset is not assigned!");

            foreach (var map in inputActions.actionMaps)
            {
                actionMap.Add(map.name, new ActionMap(map));
            }

            if (File.Exists(InputsPath))
            {
                await ReadInputOverrides();
                if (debugMode) Debug.Log($"[InputManager] {InputsFilename} readed successfully.");
            }

            OnInputsInit.OnNext(Unit.Default);
            inputActions.Enable();
        }

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        /// <summary>
        /// Subscribe listening to the performed input action event.
        /// </summary>
        public static void Performed(string name, Action<InputAction.CallbackContext> performed)
        {
            InputAction action = Action(name);
            var observable = PerformedObservable(name).Subscribe(performed);
            Instance.disposables.Add(observable);
        }

        /// <summary>
        /// Get observable from performed input action event.
        /// </summary>
        public static IObservable<InputAction.CallbackContext> PerformedObservable(string name)
        {
            InputAction action = Action(name);
            return Observable.FromEvent<InputAction.CallbackContext>(
                handler => action.performed += handler,
                handler => action.performed -= handler);
        }

        /// <summary>
        /// Observe the input binding path change.
        /// </summary>
        public static IObservable<(bool apply, string path)> ObserveBindingPath(string actionName, int bindingIndex)
        {
            return Observable.Merge
            (
                Instance.OnRebindPrepare
                    .Where(ctx => ctx.action.name == actionName && ctx.bindingIndex == bindingIndex)
                    .Select(ctx => (false, ctx.overridePath)),
                Instance.OnApply.Select(_ => (true, GetBindingPath(actionName, bindingIndex).EffectivePath)),
                Instance.OnInputsInit.Select(_ => (true, GetBindingPath(actionName, bindingIndex).EffectivePath))
            );
        }

        /// <summary>
        /// Find InputAction reference by name.
        /// </summary>
        public static InputAction FindAction(string name)
        {
            return Instance.inputActions.FindAction(name);
        }

        /// <summary>
        /// Get InputAction reference by name.
        /// </summary>
        public static InputAction Action(string name)
        {
            foreach (var map in Instance.actionMap)
            {
                if (map.Value.actions.TryGetValue(name, out ActionMap.Action action))
                    return action.action;
            }

            Debug.LogError(new NullReferenceException($"[InputManager] Could not find input action with name \"{name}\"!").ToString());
            return null;
        }

        /// <summary>
        /// Get action map action by name.
        /// </summary>
        public static ActionMap.Action ActionMapAction(string name)
        {
            foreach (var map in Instance.actionMap)
            {
                if (map.Value.actions.TryGetValue(name, out ActionMap.Action action))
                    return action;
            }

            Debug.LogError(new NullReferenceException($"[InputManager] Could not find action map action with name \"{name}\"!").ToString());
            return null;
        }

        /// <summary>
        /// Read input value as Type.
        /// </summary>
        public static T ReadInput<T>(string actionName) where T : struct
        {
            InputAction inputAction = Action(actionName);
            return inputAction.ReadValue<T>();
        }

        /// <summary>
        /// Read input value as object.
        /// </summary>
        public static object ReadInput(string actionName)
        {
            InputAction inputAction = Action(actionName);
            return inputAction.ReadValueAsObject();
        }

        /// <summary>
        /// Check whether the button is pressed and return its value.
        /// </summary>
        public static bool ReadInput<T>(string actionName, out T value) where T : struct
        {
            InputAction inputAction = Action(actionName);
            if (inputAction.IsPressed())
            {
                value = inputAction.ReadValue<T>();
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Check whether the button is pressed once and return its value.
        /// </summary>
        public static bool ReadInputOnce<T>(UnityEngine.Object obj, string actionName, out T value) where T : struct
        {
            string inputKey = actionName + "." + obj.GetInstanceID().ToString();
            InputAction inputAction = Action(actionName);

            if (inputAction.IsPressed())
            {
                if (!Instance.pressedActions.Contains(inputKey))
                {
                    Instance.pressedActions.Add(inputKey);
                    value = inputAction.ReadValue<T>();
                    return true;
                }
            }
            else
            {
                Instance.pressedActions.Remove(inputKey);
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Check whether the button is pressed and return its value.
        /// </summary>
        public static bool ReadInput(string actionName, out object value)
        {
            InputAction inputAction = Action(actionName);
            if (inputAction.IsPressed())
            {
                value = inputAction.ReadValueAsObject();
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Read input value as Button.
        /// </summary>
        public static bool ReadButton(string actionName)
        {
            InputAction inputAction = Action(actionName);
            if (inputAction.type == InputActionType.Button)
                return Convert.ToBoolean(inputAction.ReadValueAsObject());

            throw new NotSupportedException("[InputManager] The Input Action must be a button type!");
        }

        /// <summary>
        /// Read input as a button once.
        /// </summary>
        public static bool ReadButtonOnce(UnityEngine.Object obj, string actionName)
        {
            return ReadButtonOnce(obj.GetInstanceID().ToString(), actionName);
        }

        /// <summary>
        /// Read input as a button once.
        /// </summary>
        public static bool ReadButtonOnce(int instanceID, string actionName)
        {
            return ReadButtonOnce(instanceID.ToString(), actionName);
        }

        /// <summary>
        /// Read input as a button once.
        /// </summary>
        public static bool ReadButtonOnce(string key, string actionName)
        {
            string inputKey = actionName + "." + key;

            if (ReadButton(actionName))
            {
                if (!Instance.pressedActions.Contains(inputKey))
                {
                    Instance.pressedActions.Add(inputKey);
                    return true;
                }
            }
            else
            {
                Instance.pressedActions.Remove(inputKey);
            }

            return false;
        }

        /// <summary>
        /// Read input button as toggle (on/off).
        /// </summary>
        public static bool ReadButtonToggle(UnityEngine.Object obj, string actionName)
        {
            return ReadButtonToggle(obj.GetInstanceID().ToString(), actionName);
        }

        /// <summary>
        /// Read input button as toggle (on/off).
        /// </summary>
        public static bool ReadButtonToggle(int instanceID, string actionName)
        {
            return ReadButtonToggle(instanceID.ToString(), actionName);
        }

        /// <summary>
        /// Read input button as toggle (on/off).
        /// </summary>
        public static bool ReadButtonToggle(string key, string actionName)
        {
            string inputKey = actionName + "." + key;

            if (ReadButtonOnce(key, actionName))
            {
                if (!Instance.toggledActions.ContainsKey(inputKey))
                {
                    Instance.toggledActions.Add(inputKey, true);
                }
                else if (!Instance.toggledActions[inputKey])
                {
                    Instance.toggledActions.Remove(inputKey);
                }
            }
            else if (Instance.toggledActions.ContainsKey(inputKey))
            {
                Instance.toggledActions[inputKey] = false;
            }

            return Instance.toggledActions.ContainsKey(inputKey);
        }

        /// <summary>
        /// Reset toggled button.
        /// </summary>
        public static void ResetToggledButton(string key, string actionName)
        {
            string inputKey = actionName + "." + key;
            if (Instance.toggledActions.ContainsKey(inputKey))
                Instance.toggledActions.Remove(inputKey);
        }

        /// <summary>
        /// Reset toggled button.
        /// </summary>
        public static void ResetToggledButton(string actionName)
        {
            foreach (var toggledAction in Instance.toggledActions)
            {
                if (toggledAction.Key.Contains(actionName))
                {
                    Instance.toggledActions.Remove(toggledAction.Key);
                    break;
                }
            }
        }

        /// <summary>
        /// Reset all toggled buttons.
        /// </summary>
        public static void ResetToggledButtons()
        {
            Instance.toggledActions.Clear();
        }

        /// <summary>
        /// Check if any button is pressed.
        /// </summary>
        public static bool AnyKeyPressed()
        {
            Mouse mouse = Mouse.current;
            return Keyboard.current.anyKey.isPressed
                || mouse.leftButton.isPressed
                || mouse.rightButton.isPressed;
        }

        /// <summary>
        /// Get binding path of action.
        /// </summary>
        public static BindingPath GetBindingPath(string actionName, int bindingIndex = 0)
        {
            ActionMap.Action action = ActionMapAction(actionName);

            if (action == null) 
                return null;

            ActionMap.Action.Binding binding = action.bindings[bindingIndex];
            BindingPath bindingPath = binding.bindingPath;
            bindingPath.GetGlyphPath();
            return bindingPath;
        }

        /// <summary>
        /// Start action rebinding operation.
        /// </summary>
        public static void StartRebindOperation(string actionName, int bindingIndex = 0)
        {
            InputAction action = Action(actionName);

            Instance.inputActions.Disable();
            Instance.PerformInteractiveRebinding(action, bindingIndex);
            Instance.OnRebindStart.OnNext(Unit.Default);
            if (Instance.debugMode) Debug.Log("[InputManager] Rebind Started - Press any control.");
        }

        /// <summary>
        /// Start action rebinding operation.
        /// </summary>
        public static void StartRebindOperation(InputActionReference actionReference, int bindingIndex = 0)
        {
            InputAction action = actionReference.action;

            Instance.inputActions.Disable();
            Instance.PerformInteractiveRebinding(action, bindingIndex);
            Instance.OnRebindStart.OnNext(Unit.Default);
            if (Instance.debugMode) Debug.Log("[InputManager] Rebind Started - Press any control.");
        }

        /// <summary>
        /// Final apply of input overrides. (Without serialization)
        /// </summary>
        public static void SetInputRebindOverrides()
        {
            if (Instance.preparedRebinds.Count > 0)
            {
                foreach (var rebind in Instance.preparedRebinds)
                {
                    if (rebind.action.bindings[rebind.bindingIndex].path == rebind.overridePath || string.IsNullOrEmpty(rebind.overridePath))
                    {
                        rebind.action.RemoveBindingOverride(rebind.bindingIndex);
                    }
                    else
                    {
                        rebind.action.ApplyBindingOverride(rebind.bindingIndex, rebind.overridePath);
                    }

                    GetBindingPath(rebind.action.name, rebind.bindingIndex)
                        .EffectivePath = rebind.overridePath;
                }

                // write overrides
                Instance.preparedRebinds.Clear();
            }

            if (Instance.debugMode) Debug.Log("[InputManager] Bindings Applied");
            Instance.OnApply.OnNext(true);
            Instance.isApplyPending = false;
        }

        /// <summary>
        /// Final apply and serialization of input overrides.
        /// </summary>
        public static async void ApplyInputRebindOverrides()
        {
            if (Instance.preparedRebinds.Count > 0)
            {
                foreach (var rebind in Instance.preparedRebinds)
                {
                    if (rebind.action.bindings[rebind.bindingIndex].path == rebind.overridePath || string.IsNullOrEmpty(rebind.overridePath))
                    {
                        rebind.action.RemoveBindingOverride(rebind.bindingIndex);
                    }
                    else
                    {
                        rebind.action.ApplyBindingOverride(rebind.bindingIndex, rebind.overridePath);
                    }

                    GetBindingPath(rebind.action.name, rebind.bindingIndex)
                        .EffectivePath = rebind.overridePath;
                }

                // write overrides
                Instance.preparedRebinds.Clear();
                await Instance.PackAndWriteOverrides();
            }

            if (Instance.debugMode) Debug.Log("[InputManager] Bindings Applied");
            Instance.OnApply.OnNext(true);
            Instance.isApplyPending = false;
        }

        /// <summary>
        /// Discard prepared input overrides.
        /// </summary>
        public static void DiscardInputRebindOverrides()
        {
            if (!Instance.isApplyPending)
                return;

            if (Instance.debugMode) Debug.Log("[InputManager] Bindings Discarded");
            Instance.preparedRebinds.Clear();
            Instance.OnApply.OnNext(false);
        }

        /// <summary>
        /// Discard prepared input overrides and reset them to default values.
        /// </summary>
        public static void ResetInputsToDefaults()
        {
            if (!Instance.isApplyPending)
                return;

            Instance.preparedRebinds.Clear();
            foreach (var map in Instance.actionMap)
            {
                foreach (var action in map.Value.actions)
                {
                    foreach (var binding in action.Value.bindings)
                    {
                        Instance.PrepareRebind(new(action.Value.action, binding.Value.bindingIndex, null));
                    }
                }
            }
        }

        private void PerformInteractiveRebinding(InputAction action, int bindingIndex)
        {
            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnApplyBinding((operation, path) =>
                {
                    // if there is a prepared binding with the same override path
                    if (AnyPreparedRebind(path, action, bindingIndex, out var dupPath))
                    {
                        PrepareRebind(new(action, bindingIndex, path));
                        PrepareRebind(new(dupPath.action, dupPath.bindingIndex, NULL));
                    }
                    // if a binding path with the same override path exists in the action map
                    else if (AnyBindingPath(path, action, bindingIndex, out var duplicate))
                    {
                        PrepareRebind(new(action, bindingIndex, path));
                        if (!preparedRebinds.Any(x => x.bindingIndex == duplicate.bindingIndex))
                            PrepareRebind(new(duplicate.action, duplicate.bindingIndex, NULL));
                    }
                    // normal rebind
                    else
                    {
                        PrepareRebind(new(action, bindingIndex, path));
                    }
                })
                .OnComplete(_ =>
                {
                    if (debugMode) Debug.Log("[InputManager] Rebind Completed");
                    inputActions.Enable();
                    OnRebindEnd.OnNext(true);
                    CleanRebindOperation();
                })
                .OnCancel(_ =>
                {
                    if (debugMode) Debug.Log("[InputManager] Rebind Cancelled");
                    inputActions.Enable();
                    OnRebindEnd.OnNext(false);
                    CleanRebindOperation();
                })
                .WithCancelingThrough("<Keyboard>/escape")
                .Start();
        }

        private bool AnyPreparedRebind(string bindingPath, InputAction currentAction, int currentIndex, out RebindContext duplicate) 
        {
            foreach (var context in preparedRebinds)
            {
                if (bindingPath == context.overridePath && (context.action != currentAction || context.action == currentAction && context.bindingIndex != currentIndex))
                {
                    duplicate = context;
                    return true;
                }
            }

            duplicate = null;
            return false;
        }

        private bool AnyBindingPath(string bindingPath, InputAction currentAction, int currentIndex, out (InputAction action, int bindingIndex) duplicate)
        {
            foreach (var map in actionMap)
            {
                foreach (var action in map.Value.actions)
                {
                    foreach (var binding in action.Value.bindings)
                    {
                        if (action.Value.action == currentAction && binding.Value.bindingIndex == currentIndex)
                            continue;

                        if (binding.Value.bindingPath.EffectivePath == bindingPath)
                        {
                            duplicate = (action.Value.action, binding.Value.bindingIndex);
                            return true;
                        }
                    }
                }
            }

            duplicate = default;
            return false;
        }

        private void PrepareRebind(RebindContext context)
        {
            preparedRebinds.RemoveAll(x => x == context);
            var bindingPath = GetBindingPath(context.action.name, context.bindingIndex);

            // if context override path is null, set override path to default binding path
            if (string.IsNullOrEmpty(context.overridePath))
                context.overridePath = bindingPath.bindingPath;

            // if context override path is not same as binding effective path, add prepared rebind
            if (bindingPath.EffectivePath != context.overridePath)
                preparedRebinds.Add(context);

            // send prepare rebind event
            OnRebindPrepare.OnNext(context);
            isApplyPending = true;
        }

        private XmlDocument WriteOverridesToXML()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("InputBinding");

            foreach (var map in actionMap.Take(2))
            {
                XmlNode node_map = xmlDoc.CreateElement("Map");

                foreach (var action in map.Value.actions.Values)
                {
                    XmlNode node_action = xmlDoc.CreateElement("Action");
                    XmlAttribute attr_name = xmlDoc.CreateAttribute("Name");

                    attr_name.Value = action.action.name;
                    node_action.Attributes.Append(attr_name);

                    foreach (var binding in action.bindings)
                    {
                        XmlNode node_binding = xmlDoc.CreateElement("Binding");
                        XmlAttribute attr_index = xmlDoc.CreateAttribute("Index");
                        XmlAttribute attr_value = xmlDoc.CreateAttribute("Path");

                        attr_index.Value = binding.Value.bindingIndex.ToString();
                        node_binding.Attributes.Append(attr_index);

                        attr_value.Value = binding.Value.bindingPath.EffectivePath;
                        node_binding.Attributes.Append(attr_value);

                        node_action.AppendChild(node_binding);
                    }

                    if (node_action.HasChildNodes)
                    {
                        node_map.AppendChild(node_action);
                    }
                }

                if (node_map.HasChildNodes)
                {
                    rootNode.AppendChild(node_map);
                }
            }

            xmlDoc.AppendChild(rootNode);
            return xmlDoc;
        }

        private async Task PackAndWriteOverrides()
        {
            XmlDocument xml = WriteOverridesToXML();
            StringWriter sw = new();
            XmlTextWriter xtw = new(sw);

            xtw.Formatting = Formatting.Indented;
            xml.WriteTo(xtw);

            if (!Directory.Exists(SerializationUtillity.SerializationAsset.GetConfigPath()))
                Directory.CreateDirectory(SerializationUtillity.SerializationAsset.GetConfigPath());

            using StreamWriter stream = new(InputsPath);
            await stream.WriteAsync(sw.ToString());
        }

        private async Task ReadInputOverrides()
        {
            using StreamReader sr = new(InputsPath);
            string xmlData = await sr.ReadToEndAsync();

            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlData);

            foreach (XmlNode mapNode in xmlDoc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode actionNode in mapNode.ChildNodes)
                {
                    string actionName = actionNode.Attributes["Name"].Value;

                    foreach (XmlNode bindingNode in actionNode.ChildNodes)
                    {
                        int bindingIndex = int.Parse(bindingNode.Attributes["Index"].Value);
                        string bindingPath = bindingNode.Attributes["Path"].Value;

                        var action = ActionMapAction(actionName);
                        var binding = action.bindings[bindingIndex];

                        if (binding.bindingPath.EffectivePath != bindingPath)
                        {
                            action.action.ApplyBindingOverride(bindingIndex, bindingPath);
                            binding.bindingPath.EffectivePath = bindingPath;
                        }
                    }
                }
            }
        }

        private void CleanRebindOperation()
        {
            rebindOperation?.Dispose();
            rebindOperation = null;
        }

        private struct ToggleInstance
        {
            public string action;
            public bool unpressed;

            public ToggleInstance(string action)
            {
                this.action = action;
                unpressed = false;
            }
        }

        public class RebindContext
        {
            public InputAction action;
            public int bindingIndex;
            public string overridePath;

            public RebindContext(InputAction action, int bindingIndex, string overridePath)
            {
                this.action = action;
                this.bindingIndex = bindingIndex;
                this.overridePath = overridePath;
            }

            public static bool operator ==(RebindContext left, RebindContext right)
            {
                return left.action.name == right.action.name && left.bindingIndex == right.bindingIndex;
            }

            public static bool operator !=(RebindContext left, RebindContext right) => !(left == right);

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if(obj is RebindContext context)
                {
                    return action.name == context.action.name
                        && bindingIndex == context.bindingIndex;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return (action.name, bindingIndex, overridePath).GetHashCode();
            }

            public override string ToString()
            {
                string actionName = action.name;
                string bindingName = action.bindings[bindingIndex].name;

                if (!string.IsNullOrEmpty(bindingName))
                    actionName += "." + bindingName;

                return actionName;
            }
        }

        public class ActionMap
        {
            public string name;
            public Dictionary<string, Action> actions = new Dictionary<string, Action>();

            public ActionMap(InputActionMap map)
            {
                name = map.name;

                foreach (var action in map.actions)
                {
                    actions.Add(action.name, new Action(action));
                }
            }

            public class Action
            {
                public InputAction action;
                public Dictionary<int, Binding> bindings = new();

                public Action(InputAction action)
                {
                    this.action = action;

                    int bindingsCount = action.bindings.Count;
                    for (int bindingIndex = 0; bindingIndex < bindingsCount; bindingIndex++)
                    {
                        if (action.bindings[bindingIndex].isComposite)
                        {
                            int firstPartIndex = bindingIndex + 1;
                            int lastPartIndex = firstPartIndex;
                            while (lastPartIndex < bindingsCount && action.bindings[lastPartIndex].isPartOfComposite)
                                ++lastPartIndex;

                            int partCount = lastPartIndex - firstPartIndex;
                            for (int i = 0; i < partCount; i++)
                            {
                                int bindingPartIndex = firstPartIndex + i;
                                InputBinding binding = action.bindings[bindingPartIndex];
                                AddBinding(binding, bindingPartIndex);
                            }

                            bindingIndex += partCount;
                        }
                        else
                        {
                            InputBinding binding = action.bindings[bindingIndex];
                            AddBinding(binding, bindingIndex);
                        }
                    }

                    void AddBinding(InputBinding binding, int bindingIndex)
                    {
                        string[] groups = binding.groups.Split(InputBinding.Separator);

                        string partString = string.Empty;
                        if (!string.IsNullOrEmpty(binding.name))
                        {
                            NameAndParameters nameParameters = NameAndParameters.Parse(binding.name);
                            partString = nameParameters.name;
                        }

                        bindings.Add(bindingIndex, new Binding()
                        {
                            name = binding.name,
                            parentAction = action.name,
                            compositePart = partString,
                            bindingIndex = bindingIndex,
                            group = groups,
                            bindingPath = new BindingPath(binding.path, binding.overridePath),
                            inputBinding = binding
                        });
                    }
                }

                public struct Binding
                {
                    public string name;
                    public string parentAction;
                    public string compositePart;
                    public int bindingIndex;
                    public string[] group;
                    public BindingPath bindingPath;
                    public InputBinding inputBinding;

                    public override string ToString()
                    {
                        string actionName = parentAction;
                        if (!string.IsNullOrEmpty(name))
                            actionName += "." + name;
                        return actionName;
                    }
                }
            }
        }

        public sealed class BindingPath
        {
            public string bindingPath;
            public string overridePath;
            public InputGlyph inputGlyph;

            private readonly BehaviorSubject<Unit> observer;

            public BindingPath(string bindingPath, string overridePath)
            {
                this.bindingPath = bindingPath;
                this.overridePath = overridePath;

                GetGlyphPath();
                observer = new(Unit.Default);
            }

            /// <summary>
            /// Currently used binding path.
            /// </summary>
            public string EffectivePath
            {
                get
                {
                    return !string.IsNullOrEmpty(overridePath)
                        ? overridePath : bindingPath;
                }
                set
                {
                    overridePath = bindingPath == value
                        ? string.Empty : value;

                    GetGlyphPath();
                    observer.OnNext(Unit.Default);
                }
            }

            /// <summary>
            /// Currently used binding path, but observable.
            /// </summary>
            public IObservable<string> EffectivePathObservable
            {
                get => observer.Select(_ => EffectivePath);
            }

            /// <summary>
            /// Current input glyph, but observable.
            /// </summary>
            public IObservable<InputGlyph> InputGlyphObservable
            {
                get => observer.Select(_ => inputGlyph);
            }

            /// <summary>
            /// Current glyph path, but observable.
            /// </summary>
            public IObservable<string> GlyphPathObservable
            {
                get => observer.Select(_ => inputGlyph.GlyphPath);
            }

            /// <summary>
            /// Current glyph sprite, but observable.
            /// </summary>
            public IObservable<Sprite> GlyphSpriteObservable
            {
                get => observer.Select(_ => inputGlyph.GlyphSprite);
            }

            /// <summary>
            /// Update the glyph path.
            /// </summary>
            public void GetGlyphPath()
            {
                inputGlyph = SpritesAsset.GetInputGlyph(EffectivePath);
            }

            /// <summary>
            /// Format the glyph path.
            /// </summary>
            public string Format(string format)
            {
                return string.Format(format, inputGlyph.GlyphPath);
            }
        }
    }

    public static class InputManagerE
    {
        public static void ObserveEffectivePath(this InputManager.BindingPath bindingPath, Action<string> effectivePath)
        {
            CompositeDisposable disposables = InputManager.Instance.disposables;
            disposables.Add(bindingPath.EffectivePathObservable.Subscribe(effectivePath));
        }

        public static void ObserveGlyphPath(this InputManager.BindingPath bindingPath, Action<string> glyphPath)
        {
            CompositeDisposable disposables = InputManager.Instance.disposables;
            disposables.Add(bindingPath.GlyphPathObservable.Subscribe(glyphPath));
        }

        public static void ObserveGlyphPath(string actionName, int bindingIndex, Action<string> glyphPath)
        {
            CompositeDisposable disposables = InputManager.Instance.disposables;
            var bindingPath = InputManager.GetBindingPath(actionName, bindingIndex);
            if (bindingPath != null) disposables.Add(bindingPath.GlyphPathObservable.Subscribe(glyphPath));
        }

        public static void ObserveInputGlyph(string actionName, int bindingIndex, Action<InputGlyph> inputGlyph)
        {
            CompositeDisposable disposables = InputManager.Instance.disposables;
            var bindingPath = InputManager.GetBindingPath(actionName, bindingIndex);
            if (bindingPath != null) disposables.Add(bindingPath.InputGlyphObservable.Subscribe(inputGlyph));
        }

        public static void ObserveBindingPath(string actionName, int bindingIndex, Action<bool, string> bindingPath)
        {
            CompositeDisposable disposables = InputManager.Instance.disposables;
            disposables.Add(InputManager.ObserveBindingPath(actionName, bindingIndex).Subscribe(evt => bindingPath?.Invoke(evt.apply, evt.path)));
        }
    }
}