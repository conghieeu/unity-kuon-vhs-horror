using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Flickering Light")]
    public class FlickeringLight : MonoBehaviour
    {
        public Light Light;

        [Header("Settings")]
        public float FlickerChance = 0.5f;
        public MinMax FlickerRate = new(0.1f, 0.2f);
        public MinMax FlickerOffRate = new(0.1f, 0.2f);

        [Header("Sounds")]
        public AudioClip[] BallastBuzz;
        [Range(0f, 1f)] public float BallastVolume = 1f;
        public float MaxSoundDistance = 50f;

        [Header("Emission")]
        public bool EnableEmission;
        public string EmissionKeyword = "_EMISSION";
        public RendererMaterial Material;

        private float timer;
        private float targetRate;
        private bool lightState;

        private void Awake()
        {
            targetRate = FlickerRate.Random();
        }

        private void Update()
        {
            if(timer < targetRate)
            {
                timer += Time.deltaTime;
            }
            else if(lightState)
            {
                if (PickTrueWithProbability(FlickerChance))
                {
                    SwitchLight(false);
                    targetRate = FlickerOffRate.Random();
                }

                timer = 0f;
            }
            else
            {
                SwitchLight(true);
                targetRate = FlickerRate.Random();
                timer = 0f;
            }
        }

        private void SwitchLight(bool state)
        {
            Light.enabled = state;
            lightState = state;

            if (state && BallastBuzz.Length > 0)
            {
                AudioClip buzz = BallastBuzz.Random();
                GameTools.PlayOneShot3D(Light.transform.position, buzz, MaxSoundDistance, volume: BallastVolume, name: "BallastBuzz");
            }

            if (EnableEmission && Material.IsAssigned)
            {
                if(state) Material.ClonedMaterial.EnableKeyword(EmissionKeyword);
                else Material.ClonedMaterial.DisableKeyword(EmissionKeyword);
            }
        }

        private bool PickTrueWithProbability(double probability)
        {
            System.Random rand = new();
            double randValue = rand.NextDouble();
            return randValue < probability;
        }
    }
}