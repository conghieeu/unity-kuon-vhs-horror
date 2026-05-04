using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public enum PowerType { None, Output, Input }
    public enum PartDirection { Up, Down, Left, Right }

    [RequireComponent(typeof(AudioSource))]
    public class ElectricalCircuitPuzzle : PuzzleBase, ISaveable
    {
        [Serializable]
        public sealed class PowerComponent
        {
            public PowerType PowerType;
            public PartDirection PowerDirection;
            public int PowerID;
            public int ConnectPowerID;
        }

        [Serializable]
        public sealed class ComponentFlow
        {
            public ElectricalCircuitComponent Component;
            public int Rotation;
        }

        [Serializable]
        public sealed class PowerInputEvents
        {
            public PowerComponent PowerComponent;
            public ElectricalCircuitLights InputLight;

            public UnityEvent<int> OnConnected;
            public UnityEvent<int> OnDisconnected;
        }

        public ushort Rows;
        public ushort Columns;
        public PowerComponent[] PowerFlow;
        public ElectricalCircuitComponent[] CircuitComponents;

        public Transform ComponentsParent;
        public float ComponentsSpacing = 0f;
        public float ComponentsSize = 1f;

        public bool DisableWhenConnected = true;
        public float PowerConnectedWaitTime = 1f;

        public ComponentFlow[] ComponentsFlow;
        public List<ElectricalCircuitComponent> Components = new();
        public List<PowerInputEvents> InputEvents = new();

        public SoundClip RotateComponent;
        public SoundClip PowerConnected;
        public SoundClip PowerDisconnected;

        public UnityEvent OnConnected;
        public UnityEvent OnDisconnected;
        public bool isConnected;

        private AudioSource audioSource;

        private void OnValidate()
        {
            if(PowerFlow == null || PowerFlow.Length <= 0)
                PowerFlow = new PowerComponent[Rows * Columns];

            if (ComponentsFlow == null || ComponentsFlow.Length <= 0)
                ComponentsFlow = new ComponentFlow[Rows * Columns];
        }

        public override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (!SaveGameManager.GameWillLoad)
            {
                PowerAllOutputs();
                CheckAllInputs();
            }
        }

        public void ReinitializeCircuit()
        {
            if (DisableWhenConnected && isConnected)
                return;

            RemoveAllPowerIDs();
            PowerAllOutputs();
            CheckPowerStates();
            CheckAllInputs();

            audioSource.PlayOneShotSoundClip(RotateComponent);
        }

        public void PowerAllOutputs()
        {
            for (int i = 0; i < PowerFlow.Length; i++)
            {
                PowerComponent powerComponent = PowerFlow[i];
                if (powerComponent.PowerType == PowerType.Output)
                {
                    ElectricalCircuitComponent component = Components[i];
                    PartDirection fromDirection = ToOppositeDirection(powerComponent.PowerDirection);
                    component.SetPowerFlow(fromDirection, powerComponent.PowerID, null);
                }
            }
        }

        public void CheckAllInputs()
        {
            Dictionary<PowerComponent, ElectricalCircuitComponent> inputs = new();
            Dictionary<int, List<int>> outputPairs = new();
            int inputsCount = 0;

            for (int i = 0; i < PowerFlow.Length; i++)
            {
                PowerComponent powerComponent = PowerFlow[i];
                if (powerComponent.PowerType == PowerType.Output)
                {
                    if (!outputPairs.ContainsKey(powerComponent.ConnectPowerID))
                    {
                        outputPairs[powerComponent.ConnectPowerID] = new List<int> { powerComponent.PowerID };
                    }
                    else
                    {
                        outputPairs[powerComponent.ConnectPowerID].Add(powerComponent.PowerID);
                    }
                }
                else if (powerComponent.PowerType == PowerType.Input)
                {
                    ElectricalCircuitComponent component = Components[i];
                    inputs[powerComponent] = component;
                    inputsCount++;
                }
            }

            int connectedInputs = 0;
            foreach (var input in inputs)
            {
                PowerInputEvents events = InputEvents.FirstOrDefault(x =>
                {
                    var powerComponent = x.PowerComponent;
                    return powerComponent.PowerType == PowerType.Input
                        && powerComponent.PowerID == input.Key.PowerID;
                });

                if (events == null) 
                    continue;

                PartDirection oppositeDirection = ToOppositeDirection(input.Key.PowerDirection);
                var oppositeFlow = input.Value.GetOppositePowerFlow(oppositeDirection);
                int reqCount = outputPairs.TryGetValue(input.Key.PowerID, out var requiredConnections) ? requiredConnections.Count : 0;

                int connected = 0;
                if (oppositeFlow != null && requiredConnections != null)
                {
                    foreach (var connection in requiredConnections)
                    {
                        if (oppositeFlow.PowerFlows.Contains(connection))
                        {
                            if (events.InputLight != null)
                                events.InputLight.OnConnected(connection);

                            events.OnConnected?.Invoke(connection);
                            connected++;
                        }
                        else
                        {
                            if (events.InputLight != null)
                                events.InputLight.OnDisconnected(connection);

                            events.OnDisconnected?.Invoke(connection);
                        }
                    }
                }

                if (connected == reqCount)
                    connectedInputs++;
            }

            if (connectedInputs == inputsCount)
            {
                if (!SaveGameManager.GameWillLoad)
                    audioSource.PlayOneShotSoundClip(PowerConnected);

                if (DisableWhenConnected)
                {
                    canManuallySwitch = false;
                    if (isActive) StartCoroutine(OnPowerConnected());
                    else DisableInteract();
                }

                OnConnected?.Invoke();
                isConnected = true;
            }
            else if (isConnected)
            {
                if (!SaveGameManager.GameWillLoad)
                    audioSource.PlayOneShotSoundClip(PowerDisconnected);

                OnDisconnected?.Invoke();
                isConnected = false;
            }
        }

        IEnumerator OnPowerConnected()
        {
            yield return new WaitForSeconds(PowerConnectedWaitTime);
            SwitchBack();
            DisableInteract();
        }

        public void RemoveAllPowerIDs()
        {
            foreach (var component in Components)
            {
                foreach (var flow in component.PowerFlows)
                {
                    flow.PowerFlows.Clear();
                }
            }
        }

        public void CheckPowerStates()
        {
            foreach (var component in Components)
            {
                foreach (var flow in component.PowerFlows)
                {
                    if (!flow.PowerFlows.Any())
                    {
                        component.SetFlowState(flow, false);
                    }
                }
            }
        }

        public int CoordsToIndex(Vector2Int coords)
        {
            return coords.y * Columns + coords.x;
        }

        public bool IsCoordsValid(Vector2Int coords)
        {
            return coords.x >= 0 && coords.x < Columns
                && coords.y >= 0 && coords.y < Rows;
        }

        public static Vector2Int DirectionToVector(PartDirection direction)
        {
            return direction switch
            {
                PartDirection.Up => new Vector2Int(0, -1),
                PartDirection.Down => new Vector2Int(0, 1),
                PartDirection.Left => new Vector2Int(-1, 0),
                PartDirection.Right => new Vector2Int(1, 0),
                _ => Vector2Int.zero,
            };
        }

        public static bool IsOppositeDirection(PartDirection lhs, PartDirection rhs)
        {
            if (lhs == PartDirection.Up && rhs == PartDirection.Down) return true;
            else if (lhs == PartDirection.Down && rhs == PartDirection.Up) return true;
            else if (lhs == PartDirection.Left && rhs == PartDirection.Right) return true;
            else if (lhs == PartDirection.Right && rhs == PartDirection.Left) return true;
            return false;
        }

        public static PartDirection ToOppositeDirection(PartDirection direction)
        {
            return direction switch
            {
                PartDirection.Up => PartDirection.Down,
                PartDirection.Down => PartDirection.Up,
                PartDirection.Left => PartDirection.Right,
                PartDirection.Right => PartDirection.Left,
                _ => direction
            };
        }

        public StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new();

            for (int i = 0; i < Components.Count; i++)
            {
                saveableBuffer.Add("component_" + i, Components[i].OnCustomSave());
            }

            return saveableBuffer;
        }

        public void OnLoad(JToken data)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnCustomLoad(data["component_" + i]);
            }

            PowerAllOutputs();
            CheckAllInputs();
            CheckPowerStates();
        }
    }
}