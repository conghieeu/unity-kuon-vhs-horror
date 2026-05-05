using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UHFPS.Editors
{
    public static class AudioUtilWrapper
    {
        public static Type AudioUtil
        {
            get
            {
                Assembly unityEditorAssembly = typeof(Editor).Assembly;
                return unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            }
        }

        public static void PlayPreviewClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            MethodInfo method = AudioUtil.GetMethod("PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null);

            method.Invoke(null, new object[] { clip, startSample, loop });
        }

        public static void PausePreviewClip()
        {
            MethodInfo method = AudioUtil.GetMethod("PausePreviewClip", BindingFlags.Static | BindingFlags.Public);
            method.Invoke(null, null);
        }

        public static void ResumePreviewClip()
        {
            MethodInfo method = AudioUtil.GetMethod("ResumePreviewClip", BindingFlags.Static | BindingFlags.Public);
            method.Invoke(null, null);
        }

        public static void LoopPreviewClip(bool on)
        {
            MethodInfo method = AudioUtil.GetMethod("LoopPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(bool) },
                null);

            method.Invoke(null, new object[] { on });
        }

        public static bool IsPreviewClipPlaying()
        {
            MethodInfo method = AudioUtil.GetMethod("IsPreviewClipPlaying", BindingFlags.Static | BindingFlags.Public);
            return (bool)method.Invoke(null, null);
        }

        public static void StopAllPreviewClips()
        {
            MethodInfo method = AudioUtil.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public);
            method.Invoke(null, null);
        }

        public static float GetPreviewClipPosition()
        {
            MethodInfo method = AudioUtil.GetMethod("GetPreviewClipPosition", BindingFlags.Static | BindingFlags.Public);
            return (float)method.Invoke(null, null);
        }       
        
        public static int GetPreviewClipSamplePosition()
        {
            MethodInfo method = AudioUtil.GetMethod("GetPreviewClipSamplePosition", BindingFlags.Static | BindingFlags.Public);
            return (int)method.Invoke(null, null);
        }

        public static void SetPreviewClipSamplePosition(AudioClip clip, int iSamplePosition)
        {
            MethodInfo method = AudioUtil.GetMethod("SetPreviewClipSamplePosition",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int) },
                null);

            method.Invoke(null, new object[] { clip, iSamplePosition });
        }

        public static int GetSampleCount(AudioClip clip)
        {
            MethodInfo method = AudioUtil.GetMethod("GetSampleCount",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip) },
                null);

            return (int)method.Invoke(null, new object[] { clip });
        }

        public static int GetChannelCount(AudioClip clip)
        {
            MethodInfo method = AudioUtil.GetMethod("GetChannelCount",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip) },
                null);

            return (int)method.Invoke(null, new object[] { clip });
        }

        public static int GetBitRate(AudioClip clip)
        {
            MethodInfo method = AudioUtil.GetMethod("GetBitRate",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip) },
                null);

            return (int)method.Invoke(null, new object[] { clip });
        }

        public static int GetBitsPerSample(AudioClip clip)
        {
            MethodInfo method = AudioUtil.GetMethod("GetBitsPerSample",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip) },
                null);

            return (int)method.Invoke(null, new object[] { clip });
        }

        public static int GetFrequency(AudioClip clip)
        {
            MethodInfo method = AudioUtil.GetMethod("GetFrequency",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip) },
                null);

            return (int)method.Invoke(null, new object[] { clip });
        }

        public static int GetSoundSize(AudioClip clip)
        {
            MethodInfo method = AudioUtil.GetMethod("GetSoundSize",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip) },
                null);

            return (int)method.Invoke(null, new object[] { clip });
        }

        public static double GetDuration(AudioClip clip)
        {
            MethodInfo method = AudioUtil.GetMethod("GetDuration",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip) },
                null);

            return (double)method.Invoke(null, new object[] { clip });
        }
    }
}