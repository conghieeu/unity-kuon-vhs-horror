using System;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering.Universal;

namespace UHFPS.Runtime
{
    [Serializable]
    // 0 - None, 1 - FXAA, 2 - SMAA, 3 - TMAA
    public class OptionAntialiasingMode : OptionModule
    {
        public override string ContextName => "URP/Antialiasing Mode";

        public GString[] AntialiasingModes =
        {
            new("*None", ""),
            new("*FXAA", ""),
            new("*SMAA", ""),
            new("*TAA", ""),
        };

        public override void OnApplyOption()
        {
            int antialiasing = (int)Value;
            if (IsChanged)
            {
                AntialiasingMode mode = (AntialiasingMode)antialiasing;
                Options.CameraData.antialiasing = mode;
            }

            Options.SerializableData[Name] = new(antialiasing);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Integer, out int antialiasing))
            {
                Options.CameraData.antialiasing = (AntialiasingMode)antialiasing;
                Behaviour.SetOptionValue(antialiasing);
                return;
            }

            AntialiasingMode mode = Options.CameraData.antialiasing;
            Behaviour.SetOptionValue((int)mode);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "options", AntialiasingModes } });
        }
    }

    [Serializable]
    // 0 - Low, 1 - Medium, 2 - High
    // Only TAA, SMAA
    public class OptionAntialiasingQuality : OptionModule
    {
        public override string ContextName => "URP/Antialiasing Quality";

        public GString[] QualityLevels =
{
            new("*VeryLow", ""),
            new("*Low", ""),
            new("*Medium", ""),
            new("*High", ""),
            new("*VeryHigh", ""),
        };

        [Tooltip("The previous option must be of type OptionAntialiasingMode, otherwise it will cause unwanted behavior!")]
        public bool AutoDisableQuality = true;

        public override void OnApplyOption()
        {
            int quality = (int)Value;
            if (IsChanged && quality >= 0)
            {
                if (Options.CameraData.antialiasing == AntialiasingMode.TemporalAntiAliasing)
                {
                    Options.CameraData.taaSettings.quality = ConvertTAAQuality(quality);
                }
                else if (Options.CameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing)
                {
                    Options.CameraData.antialiasingQuality = (AntialiasingQuality)quality;
                }
                else if (Options.CameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing)
                {
                    quality = -1;
                }
            }

            Options.SerializableData[Name] = new(quality);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Integer, out int value))
            {
                if (value >= 0)
                {
                    if (Options.CameraData.antialiasing == AntialiasingMode.TemporalAntiAliasing)
                        Options.CameraData.taaSettings.quality = ConvertTAAQuality(value);
                    else if (Options.CameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing)
                        Options.CameraData.antialiasingQuality = (AntialiasingQuality)value;
                    Behaviour.SetOptionValue(value);
                }
                return;
            }

            int quality = 0;
            if (Options.CameraData.antialiasing == AntialiasingMode.TemporalAntiAliasing)
                quality = GetTAAQualityValue(Options.CameraData.taaSettings.quality);
            else if (Options.CameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing)
                quality = (int)Options.CameraData.antialiasingQuality;

            Behaviour.SetOptionValue(quality);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "options", QualityLevels } });

            if (AutoDisableQuality)
            {
                if (!behaviour.TryGetComponent(out CanvasGroup canvasGroup))
                    canvasGroup = behaviour.gameObject.AddComponent<CanvasGroup>();

                var uiDisabled = behaviour.gameObject.AddComponent<AntialiasingQualityUIDisabler>();
                uiDisabled.AntialiasingQuality = canvasGroup;

                var previous = GetPrevious();
                if(previous != null)
                {
                    if(previous is OptionsRadio radio)
                    {
#if UNITY_EDITOR
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(radio.OnChange, uiDisabled.OnAntialiasingChange);
#endif
                    }
                }
                else
                {
                    Debug.LogError("Previous option is null!");
                }
            }
        }

        private TemporalAAQuality ConvertTAAQuality(int value)
        {
            if (value == 0) return TemporalAAQuality.Low;
            else if (value == 1) return TemporalAAQuality.Medium;
            else if (value == 2) return TemporalAAQuality.High;
            return TemporalAAQuality.Medium;
        }

        private int GetTAAQualityValue(TemporalAAQuality quality)
        {
            if (quality == TemporalAAQuality.Low) return 0;
            else if (quality == TemporalAAQuality.Medium) return 1;
            else if (quality == TemporalAAQuality.High) return 2;
            return 1;
        }
    }

    [Serializable]
    // 0 - 0m (Disabled), 1 - 25m (Very Low), 2 - 40m (Low), 3 - 55m (Medium), 4 - 70m (High), 5 - 85m (Very High), 6 - 100m (Max)
    public class OptionShadowDistance : OptionModule
    {
        public override string ContextName => "URP/ShadowDistance";

        public NameValue<float>[] ShadowDistances =
        {
            new("*Disabled", 0f),
            new("*Very Low", 25f),
            new("*Low", 40f),
            new("*Medium", 55f),
            new("*High", 70f),
            new("*Very High", 85f),
            new("*Max", 100f)
        };

        public override void OnApplyOption()
        {
            int value = (int)Value;
            if (IsChanged && value >= 0)
            {
                float distance = ShadowDistances[value].Value;
                Options.URPAsset.shadowDistance = distance;
            }

            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 6);
                float distance = ShadowDistances[value].Value;
                Options.URPAsset.shadowDistance = distance;
                Behaviour.SetOptionValue(value);
                return;
            }

            float shadowDistance = Options.URPAsset.shadowDistance;
            int index = ClosestIndex(ShadowDistances, (int)shadowDistance);
            Behaviour.SetOptionValue(index);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            GString[] gStrings = ShadowDistances.Select(x => x.Name).ToArray();
            behaviour.SetOptionData(new StorableCollection() { { "options", gStrings } });
        }

        private int ClosestIndex(NameValue<float>[] array, float targetValue)
        {
            int closestIndex = -1;
            float smallestDifference = float.MaxValue;

            for (int i = 0; i < array.Length; i++)
            {
                float difference = Mathf.Abs(array[i].Value - targetValue);
                if (difference < smallestDifference)
                {
                    smallestDifference = difference;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
    }
}