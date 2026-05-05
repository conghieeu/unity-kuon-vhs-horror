using UnityEngine;
using UHFPS.Runtime;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UHFPS.Tools
{
    public static class HandlesDrawing
    {
        /*
        public static void DrawLimits(Vector3 position, MinMax limits, Vector3 forward, Vector3 upward, bool bothSides = true, bool flip = false, float radius = 1f)
        {
#if UNITY_EDITOR
            Vector3 from = Quaternion.AngleAxis(bothSides || !flip ? limits.min : limits.min - limits.min, upward) * forward;

            float angle = bothSides
                ? limits.max - limits.min % 360f
                : !flip ? -limits.min % 360 : limits.max % 360;

            Handles.color = Color.white;
            Handles.DrawWireArc(position, upward, from, angle, radius);

            Handles.color = Color.white.Alpha(0.1f);
            Handles.DrawSolidArc(position, upward, from, angle, radius);
#endif
        }
        */

        public static void DrawLimits(Vector3 position, MinMax openLimits, Vector3 forward, Vector3 upward, bool bothSides = true, bool flip = false, float radius = 1f)
        {
#if UNITY_EDITOR
            if (bothSides)
            {
                // double–sided limits situation
                if (openLimits.RealMax > 0)
                {
                    Handles.color = Color.white;
                    Handles.DrawWireArc(position, upward, forward, openLimits.RealMax, radius);
                    Handles.color = Color.white.Alpha(0.1f);
                    Handles.DrawSolidArc(position, upward, forward, openLimits.RealMax, radius);
                }

                if (openLimits.RealMin < 0)
                {
                    Handles.color = Color.white;
                    Handles.DrawWireArc(position, upward, forward, openLimits.RealMin, radius);
                    Handles.color = Color.white.Alpha(0.1f);
                    Handles.DrawSolidArc(position, upward, forward, openLimits.RealMin, radius);
                }
            }
            else
            {
                // one–sided limits situation
                float closedAngle = flip ? openLimits.RealMax : openLimits.RealMin;
                float targetAngle = flip ? openLimits.RealMin : openLimits.RealMax;
                float arcAngle = targetAngle - closedAngle;
                Vector3 startDir = Quaternion.AngleAxis(closedAngle, upward) * forward;

                Handles.color = Color.white;
                Handles.DrawWireArc(position, upward, startDir, arcAngle, radius);
                Handles.color = Color.white.Alpha(0.1f);
                Handles.DrawSolidArc(position, upward, startDir, arcAngle, radius);
            }
#endif
        }

        public static void DrawLimitsArc(Vector3 position, MinMax limits, Vector3 forward, Vector3 upward, Color color, bool bothSides = true, bool flip = false, float radius = 1f)
        {
#if UNITY_EDITOR
            Vector3 from = Quaternion.AngleAxis(bothSides || !flip ? limits.min : limits.min - limits.min, upward) * forward;

            float angle = bothSides
                ? limits.max - limits.min % 360f
                : !flip ? -limits.min % 360 : limits.max % 360;

            Handles.color = color;
            Handles.DrawWireArc(position, upward, from, angle, radius);
#endif
        }
    }
}