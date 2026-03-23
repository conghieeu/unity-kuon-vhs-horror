using System;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public class OptionLanguage : OptionModule
    {
        public override string ContextName => "General/Language";

        public GString[] Languages = { new("*English", "") };

        public override void OnApplyOption()
        {
            int value = (int)Value;
            if(IsChanged) GameLocalization.Instance.ChangeLanguage(value);
            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Integer, out int value))
            {
                GameLocalization.Instance.ChangeLanguage(value);
                Behaviour.SetOptionValue(value);
                return;
            }

            int language = GameLocalization.Instance.CurrentLanguageIndex;
            Behaviour.SetOptionValue(language);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "options", Languages } });
        }
    }

    [Serializable]
    // 0 - First, N - Last
    public class OptionDisplayMonitor : OptionModule
    {
        public override string ContextName => "Screen/Display Monitor";

        public override void OnApplyOption()
        {
            int value = (int)Value;
            if (IsChanged) Options.CurrentDisplay.Value = Options.DisplayInfos[value];
            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Integer, out int value))
            {
                if (value < Display.displays.Length)
                {
                    Options.CurrentDisplay.SilentValue = Options.DisplayInfos[value];
                    Display.displays[value].Activate();
                    Behaviour.SetOptionValue(value);
                    return;
                }
            }

            Options.CurrentDisplay.SilentValue = Screen.mainWindowDisplayInfo;
            int display = Options.DisplayInfos.IndexOf(Options.CurrentDisplay.Value);
            Behaviour.SetOptionValue(display);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            if (behaviour is OptionsRadio radio)
                radio.OptionsCustom = true;
        }

        public override void OnBuildOptionRuntime()
        {
            string[] displays = Options.DisplayInfos.Select(x => x.name).ToArray();
            GString[] gStrings = displays.Select(x => new GString($"{x}")).ToArray();
            Behaviour.SetOptionData(new StorableCollection() { { "options", gStrings } });
        }
    }

    [Serializable]
    // 0 - Min Resolution, N - Max Resolution
    public class OptionResolution : OptionModule
    {
        public override string ContextName => "Screen/Resolution";

        public override void OnApplyOption()
        {
            var resolution = Options.Resolutions[(int)Value];
            if (IsChanged) Options.CurrentResolution.Value = resolution;
            Options.SerializableData[$"{Name}_width"] = new(resolution.width);
            Options.SerializableData[$"{Name}_height"] = new(resolution.height);
        }

        public override void OnLoadOption(bool fromFile)
        {
            var resolutions = Options.Resolutions;
            if (fromFile)
            {
                bool val1 = CheckOption($"{Name}_width", JTokenType.Integer, out int width);
                bool val2 = CheckOption($"{Name}_height", JTokenType.Integer, out int height);

                if (val1 && val2)
                {
                    int index = resolutions.FindIndex(x => x.width == width && x.height == height);
                    if (index <= -1) index = resolutions.Count - 1;

                    Options.CurrentResolution.SilentValue = Screen.currentResolution;
                    Options.CurrentResolution.Value = resolutions[index];
                    Behaviour.SetOptionValue(index);
                    return;
                }
            }

            Options.CurrentResolution.SilentValue = Screen.currentResolution;
            int value = resolutions.IndexOf(Options.CurrentResolution.Value);
            Behaviour.SetOptionValue(value);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            if (behaviour is OptionsRadio radio)
                radio.OptionsCustom = true;
        }

        public override void OnBuildOptionRuntime()
        {
            string[] resolutions = Options.Resolutions.Select(x => $"{x.width}x{x.height}").ToArray();
            GString[] gStrings = resolutions.Select(x => new GString($"{x}")).ToArray();
            Behaviour.SetOptionData(new StorableCollection() { { "options", gStrings } });
        }
    }

    [Serializable]
    // 0 - Windowed, 1 - Fullscreen
    public class OptionFullscreen : OptionModule
    {
        public override string ContextName => "Screen/Fullscreen Mode";

        public GString[] ModeNames = { 
            new("*Windowed", ""),
            new("*Borderless Window", "") 
        };

        public override void OnApplyOption()
        {
            int fullscreen = (int)Value;
            if (IsChanged)
            {
                FullScreenMode mode = fullscreen == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                Options.CurrentFullscreen.Value = mode;
            }

            Options.SerializableData[Name] = new(fullscreen == 1);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Boolean, out bool fullscreen))
            {
                Options.CurrentFullscreen.SilentValue = Screen.fullScreenMode;
                Options.CurrentFullscreen.Value = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                Behaviour.SetOptionValue(fullscreen ? 1 : 0);
                return;
            }

            Options.CurrentFullscreen.Value = Screen.fullScreenMode;
            int value = Options.CurrentFullscreen.Value == FullScreenMode.FullScreenWindow ? 1 : 0;
            Behaviour.SetOptionValue(value);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "options", ModeNames } });
        }
    }

    [Serializable]
    // 0 - 30FPS, 1 - 60FPS, 2 - 120FPS, 3 - Variable
    public class OptionFramerate : OptionModule
    {
        public override string ContextName => "General/Framerate";

        public NameValue<int>[] Framerates =
        {
            new("*30FPS", 30),
            new("*60FPS", 60),
            new("*120FPS", 120),
            new("*Variable", -1),
        };

        public override void OnApplyOption()
        {
            int value = (int)Value;
            int framerate = Framerates[value].Value;

            if(IsChanged) Application.targetFrameRate = framerate;
            Options.SerializableData[Name] = new(framerate);
        }

        public override void OnLoadOption(bool fromFile)
        {
            int framerate = Application.targetFrameRate;
            if (fromFile && CheckOption(JTokenType.Integer, out int value))
            {
                if(Framerates.Any(x => x.Value == value))
                    Application.targetFrameRate = framerate = value;
            }

            int fIndex = Array.FindIndex(Framerates, x => x.Value == framerate);
            Behaviour.SetOptionValue(fIndex >= 0 ? fIndex : Framerates.Length);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            GString[] gStrings = Framerates.Select(x => x.Name).ToArray();
            behaviour.SetOptionData(new StorableCollection() { { "options", gStrings } });
        }
    }

    [Serializable]
    // 0 - Don't Sync, 1 = Every V Blank
    public class OptionVSync : OptionModule
    {
        public override string ContextName => "General/VSync";

        public GString[] VSyncNames = { 
            new("*OFF", ""),
            new("*ON", "") 
        };

        public override void OnApplyOption()
        {
            int value = (int)Value;
            if (IsChanged) QualitySettings.vSyncCount = value;
            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 1);
                QualitySettings.vSyncCount = value;
                Behaviour.SetOptionValue(value);
                return;
            }

            int vsync = QualitySettings.vSyncCount;
            Behaviour.SetOptionValue(vsync);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "options", VSyncNames } });
        }
    }

    [Serializable]
    // 0 - Disable, 1 - Enable, 2 - Force Enable
    public class OptionAnisotropic : OptionModule
    {
        public override string ContextName => "General/Anisotropic";

        public GString[] AnisotropicNames = { 
            new("*Disable", ""),
            new("*Enable", ""),
            new("*Force Enable", "") 
        };

        public override void OnApplyOption()
        {
            int value = (int)Value;
            if (IsChanged) QualitySettings.anisotropicFiltering = (AnisotropicFiltering)value;
            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 2);
                QualitySettings.anisotropicFiltering = (AnisotropicFiltering)value;
                Behaviour.SetOptionValue(value);
                return;
            }

            int anisotropic = (int)QualitySettings.anisotropicFiltering;
            Behaviour.SetOptionValue(anisotropic);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "options", AnisotropicNames } });
        }
    }

    [Serializable]
    // 0 - Eighth Size, 1 - Quarter Size, 2 - Half Size, 3 - Normal
    public class OptionTextureQuality : OptionModule
    {
        public override string ContextName => "General/Texture Quality";

        public GString[] TextureQuality = { 
            new("*Eighth Size", ""),
            new("*Quarter Size", ""),
            new("*Half Size", ""),
            new("*Normal", "") 
        };

        public override void OnApplyOption()
        {
            int value = (int)Value;
            int quality = 3 - value;
            if (IsChanged) QualitySettings.globalTextureMipmapLimit = quality;
            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 3);
                QualitySettings.globalTextureMipmapLimit = 3 - value;
                Behaviour.SetOptionValue(value);
                return;
            }

            int texQuality = QualitySettings.globalTextureMipmapLimit;
            texQuality = 3 - texQuality;
            Behaviour.SetOptionValue(texQuality);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection() { { "options", TextureQuality } });
        }
    }

    // 0 - Min Volume, 1 - Max Volume
    [Serializable]
    public class OptionVolumeGeneral : OptionModule
    {
        public override string ContextName => "General/Volume";

        public float DefaultValue = 1f;
        public MinMax SliderLimits = new(0, 1);
        public bool UseSnapping = true;
        public float SnapValue = 0.05f;

        public override void OnApplyOption()
        {
            float value = Mathf.Clamp01((float)Value);
            if (IsChanged) AudioListener.volume = value;
            Options.SerializableData[Name] = new(value);
        }

        public override void OnLoadOption(bool fromFile)
        {
            if (fromFile && CheckOption(JTokenType.Float, out float value))
            {
                value = Mathf.Clamp01(value);
                AudioListener.volume = value;
                Behaviour.SetOptionValue(value);
                return;
            }

            float globalVolume = AudioListener.volume;
            Behaviour.SetOptionValue(globalVolume);
        }

        public override void OnBuildOption(OptionBehaviour behaviour)
        {
            behaviour.SetOptionData(new StorableCollection()
            {
                { "settings", new object[]
                    {
                        OptionsSlider.SliderTypeEnum.FloatSlider,
                        SliderLimits,
                        UseSnapping,
                        SnapValue
                    }
                },
                { "defaultValue", DefaultValue }
            });
        }
    }
}
