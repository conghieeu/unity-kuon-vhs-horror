using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Flickering Light")]
    [Summary("Tạo hiệu ứng đèn nhấp nháy. Hỗ trợ âm thanh ballast và hiệu ứng Emission cho vật liệu.")]
    public class FlickeringLight : MonoBehaviour
    {
        [Tooltip("Đèn mục tiêu.")]
        public Light Light;

        [Header("Settings")]
        [Tooltip("Tỉ lệ (0-1) để đèn bị tắt đi.")]
        public float FlickerChance = 0.5f;
        [Tooltip("Tốc độ nhấp nháy khi đèn đang bật.")]
        public MinMax FlickerRate = new(0.1f, 0.2f);
        [Tooltip("Thời gian đèn bị tắt trước khi bật lại.")]
        public MinMax FlickerOffRate = new(0.1f, 0.2f);

        [Header("Sounds")]
        [Tooltip("Âm thanh rè của chấn lưu.")]
        public AudioClip[] BallastBuzz;
        [Range(0f, 1f)] 
        [Tooltip("Âm lượng âm thanh rè.")]
        public float BallastVolume = 1f;
        [Tooltip("Khoảng cách âm thanh tối đa.")]
        public float MaxSoundDistance = 50f;

        [Header("Emission")]
        [Tooltip("Bật/tắt hiệu ứng Emission trên vật liệu.")]
        public bool EnableEmission;
        [Tooltip("Từ khóa shader cho Emission.")]
        public string EmissionKeyword = "_EMISSION";
        [Tooltip("Vật liệu cần thay đổi Emission.")]
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