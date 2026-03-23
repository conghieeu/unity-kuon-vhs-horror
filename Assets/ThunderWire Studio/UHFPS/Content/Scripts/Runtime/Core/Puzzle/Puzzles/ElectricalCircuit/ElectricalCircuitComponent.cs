using System.Collections.Generic;
using System;
using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class ElectricalCircuitComponent : MonoBehaviour, IInteractStart, ISaveableCustom
    {
        [Serializable]
        public sealed class FlowDirection
        {
            public List<PartDirection> FlowDirections = new();
            public RendererMaterial[] FlowRenderer;
        }

        [Serializable]
        public sealed class PowerFlow
        {
            public PartDirection[] FlowDirections;
            public RendererMaterial[] FlowRenderer;
            public List<int> PowerFlows = new();
        }

        public ElectricalCircuitPuzzle ElectricalCircuit;

        public Texture2D ComponentIcon;
        public MeshFilter ComponentMesh;
        public Axis ComponentUp;

        public Vector2Int Coords;
        public float Angle;

        public List<FlowDirection> FlowDirections = new();
        public PowerFlow[] PowerFlows;

        private void Awake()
        {
            PowerFlows = new PowerFlow[FlowDirections.Count];

            for (int i = 0; i < FlowDirections.Count; i++)
            {
                var direction = FlowDirections[i];
                var flow = PowerFlows[i] = new PowerFlow();
                flow.FlowDirections = new PartDirection[direction.FlowDirections.Count];
                flow.FlowRenderer = direction.FlowRenderer;

                for (int j = 0; j < direction.FlowDirections.Count; j++)
                {
                    flow.FlowDirections[j] = direction.FlowDirections[j];
                }
            }

            if(!SaveGameManager.GameWillLoad)
                InitializeDirections();
        }

        public void InitializeDirections()
        {
            int angleTimes = (ushort)(Angle / 90);
            RotateDirections(angleTimes);
        }

        public void InteractStart()
        {
            if (ElectricalCircuit.DisableWhenConnected && ElectricalCircuit.isConnected)
                return;

            Angle = (Angle + 90) % 360;
            SetComponentAngle();

            RotateDirections(1);
            ElectricalCircuit.ReinitializeCircuit();
        }

        public void SetComponentAngle()
        {
            Vector3 newRotation = transform.localEulerAngles.SetComponent(ComponentUp, Angle);
            transform.localEulerAngles = newRotation;
        }

        public void RotateDirections(int times)
        {
            foreach (var flow in PowerFlows)
            {
                for (int i = 0; i < flow.FlowDirections.Length; i++)
                {
                    flow.FlowDirections[i] = RotatePartDirection(flow.FlowDirections[i], times);
                }
            }
        }

        public void SetPowerFlow(PartDirection fromDirection, int powerID, List<PowerFlow> visited)
        {
            // get power flow that is connected from direction
            PowerFlow inputFlow = GetOppositePowerFlow(fromDirection);
            if (inputFlow == null) return;

            if(visited == null)
                visited = new List<PowerFlow>();

            if (visited.Contains(inputFlow)) return;
            else visited.Add(inputFlow);

            foreach (var direction in inputFlow.FlowDirections)
            {
                if (!ElectricalCircuitPuzzle.IsOppositeDirection(fromDirection, direction))
                {
                    if (GetDirectionComponent(direction, out var component))
                    {
                        if (component.GetOppositePowerFlow(direction) != null)
                        {
                            component.SetPowerFlow(direction, powerID, visited);
                        }
                    }
                }
            }

            if (!inputFlow.PowerFlows.Contains(powerID))
                inputFlow.PowerFlows.Add(powerID);

            SetFlowState(inputFlow, true);
        }

        public void SetFlowState(PowerFlow powerFlow, bool state)
        {
            foreach (var renderer in powerFlow.FlowRenderer)
            {
                if(state) renderer.ClonedMaterial.EnableKeyword("_EMISSION");
                else renderer.ClonedMaterial.DisableKeyword("_EMISSION");
            }
        }

        public bool GetDirectionComponent(PartDirection direction, out ElectricalCircuitComponent component)
        {
            Vector2Int dirOutput = ElectricalCircuitPuzzle.DirectionToVector(direction);
            Vector2Int newCoords = Coords + dirOutput;

            if (ElectricalCircuit.IsCoordsValid(newCoords))
            {
                int compIndex = ElectricalCircuit.CoordsToIndex(newCoords);
                component = ElectricalCircuit.Components[compIndex];
                return true;
            }

            component = null;
            return false;
        }

        public PowerFlow GetOppositePowerFlow(PartDirection oppositeDir)
        {
            foreach (var flow in PowerFlows)
            {
                foreach (var direction in flow.FlowDirections)
                {
                    if (ElectricalCircuitPuzzle.IsOppositeDirection(direction, oppositeDir))
                        return flow;
                }
            }

            return null;
        }

        private PartDirection RotatePartDirection(PartDirection direction, int times)
        {
            if (times <= 0) 
                return direction;

            return RotatePartDirection(direction switch
            {
                PartDirection.Up => PartDirection.Right,
                PartDirection.Down => PartDirection.Left,
                PartDirection.Left => PartDirection.Up,
                PartDirection.Right => PartDirection.Down,
                _ => direction
            }, --times);
        }

        public StorableCollection OnCustomSave()
        {
            List<string> partDirections = new();
            foreach (var flow in PowerFlows)
            {
                string dirCode = "";
                for (int i = 0; i < flow.FlowDirections.Length; i++)
                {
                    int code = (int)flow.FlowDirections[i];
                    dirCode += code;
                }

                partDirections.Add(dirCode);
            }

            return new StorableCollection()
            {
                { "angle", Angle },
                { "partDirections", partDirections }
            };
        }

        public void OnCustomLoad(JToken data)
        {
            Angle = (float)data["angle"];
            SetComponentAngle();

            string[] partDirections = data["partDirections"].ToObject<string[]>();
            if (partDirections.Length == PowerFlows.Length)
            {
                for (int i = 0; i < partDirections.Length; i++)
                {
                    var flow = PowerFlows[i];
                    for (int j = 0; j < flow.FlowDirections.Length; j++)
                    {
                        int code = int.Parse(partDirections[i][j].ToString());
                        flow.FlowDirections[j] = (PartDirection)code;
                    }
                }
            }
            else
            {
                Debug.LogError("Saved 'partDirections' length does not match 'PowerFlows' length!");
            }
        }
    }
}