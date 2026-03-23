using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UHFPS.Editors
{
    public class AudioWaveformWrapper
    {
        private readonly MethodInfo WaveformPreviewCreate;
        private readonly Type WaveformPreviewClass;

        public AudioWaveformWrapper()
        {
            Assembly unityEditorAssembly = typeof(Editor).Assembly;
            Type waveformPreviewFactory = unityEditorAssembly.GetType("UnityEditor.WaveformPreviewFactory");
            WaveformPreviewClass = unityEditorAssembly.GetType("UnityEditor.WaveformPreview");
            WaveformPreviewCreate = waveformPreviewFactory.GetMethod("Create", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public WaveformPreview Create(int initialSize, AudioClip clip)
        {
            object result = WaveformPreviewCreate.Invoke(null, new object[] { initialSize, clip });
            return new WaveformPreview(result, WaveformPreviewClass);
        }

        public sealed class WaveformPreview
        {
            public enum ChannelMode
            {
                MonoSum,
                Separate,
                SpecificChannel
            }

            private readonly object target;
            private readonly Type targetType;

            private readonly PropertyInfo p_backgroundColor;
            public Color backgroundColor
            {
                get => (Color)p_backgroundColor.GetValue(target);
                set => p_backgroundColor.SetValue(target, value);
            }

            private readonly PropertyInfo p_waveColor;
            public Color waveColor
            {
                get => (Color)p_waveColor.GetValue(target);
                set => p_waveColor.SetValue(target, value);
            }

            private readonly MethodInfo m_dispose;
            private readonly MethodInfo m_render;
            private readonly MethodInfo m_apply;
            private readonly MethodInfo m_setChannel;
            private readonly MethodInfo m_optimize;
            private readonly MethodInfo m_setTime;
            private readonly MethodInfo m_setWave;

            public WaveformPreview(object target, Type waveformPreview)
            {
                this.target = target;
                targetType = target.GetType();

                p_backgroundColor = targetType.GetProperty("backgroundColor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                p_waveColor = targetType.GetProperty("waveColor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                m_dispose = targetType.GetMethod("Dispose", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                m_render = targetType.GetMethod("Render", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                m_apply = targetType.GetMethod("ApplyModifications", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                Type channelModeType = waveformPreview.GetNestedType("ChannelMode", BindingFlags.Public);
                m_setChannel = targetType.GetMethod("SetChannelMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { channelModeType, typeof(int) }, null);
                m_optimize = targetType.GetMethod("OptimizeForSize", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                m_setTime = targetType.GetMethod("SetTimeInfo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                m_setWave = targetType.GetMethod("SetMMWaveData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            public void Dispose()
            {
                m_dispose.Invoke(target, null);
            }

            public void Render(Rect rect)
            {
                m_render.Invoke(target, new object[] { rect });
            }

            public void ApplyModifications()
            {
                m_apply.Invoke(target, null);
            }

            public void SetChannelMode(ChannelMode mode, int specificChannelToRender)
            {
                m_setChannel.Invoke(target, new object[] { (int)mode, specificChannelToRender });
            }

            public void SetChannelMode(ChannelMode mode)
            {
                SetChannelMode(mode, 0);
            }

            public void OptimizeForSize(Vector2 newSize)
            {
                m_optimize.Invoke(target, new object[] { newSize });
            }

            public void SetTimeInfo(double start, double length)
            {
                m_setTime.Invoke(target, new object[] { start, length });
            }

            public void SetMMWaveData(int interleavedOffset, float[] data)
            {
                m_setWave.Invoke(target, new object[] { interleavedOffset, data });
            }
        }
    }
}