using System.Collections.Generic;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Surface", menuName = "UHFPS/Surface/Surface Definition")]
    public class SurfaceDefinition : ScriptableObject
    {
        public Tag SurfaceTag;
        public Texture2D[] SurfaceTextures;

        [Range(0f, 1f)]
        public float SurfaceFriction = 1f;

        [Range(0f, 1f)]
        public float FootstepsVolume = 1f;
        public List<AudioClip> SurfaceFootsteps = new();

        [Range(0f, 1f)]
        public float LandStepsVolume = 1f;
        public List<AudioClip> SurfaceLandSteps = new();

        [Range(0f, 1f)]
        public float BulletImpactVolume = 1f;
        public List<AudioClip> SurfaceBulletImpact = new();

        [Range(0f, 1f)]
        public float MeleeImpactVolume = 1f;
        public List<AudioClip> SurfaceMeleeImpact = new();

        public GameObject[] SurfaceBulletmarks;
        public GameObject[] SurfaceMeleemarks;
    }
}