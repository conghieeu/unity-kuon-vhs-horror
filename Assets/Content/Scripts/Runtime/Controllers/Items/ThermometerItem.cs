using System.Globalization;
using System.Collections;
using System;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class ThermometerItem : PlayerItemBehaviour
    {
        public GameObject DisplayCanvas;
        public TMP_Text Temperature;
        public string DisplayFormat = "<mspace=0.5em>{0}</mspace>.<mspace=0.5em>{1}</mspace>";

        public RendererMaterial Display;
        public string EmissionKeyword = "_EMISSION";

        public bool SetBaseTemp = true;
        public float BaseTemperature = 26f;

        public LayerMask RaycastMask;
        public float RaycastDistance;

        public float TempGetInterval;
        public float TempNoiseScale;
        public float TempNoiseSpeed;

        public float TempGainSpeed;
        public float TempDropSpeed;

        public string ThermometerDrawState = "ThermometerDraw";
        public string ThermometerHideState = "ThermometerHide";
        public string ThermometerHideTrigger = "Hide";

        private float tempInterval;
        private float targetTemp;
        private float currTemp;

        private float baseTemp;
        private float defaultTemp;

        private bool isEquipped;
        private bool tempEnabled;

        public override string Name => "Thermometer";

        public override bool IsBusy() => !isEquipped;

        public override bool CanCombine() => isEquipped;

        private void Awake()
        {
            if(!SaveGameManager.GameWillLoad && SetBaseTemp)
                SetResetTemp(BaseTemperature);
        }

        public void SetResetTemp(float temperature)
        {
            baseTemp = temperature;
            currTemp = temperature;
            targetTemp = temperature;
            defaultTemp = temperature;
        }

        public void SetTemperature(float temperature)
        {
            targetTemp = temperature;
        }

        public void SetBaseTemperature(float baseTemperature)
        {
            baseTemp = baseTemperature;
        }

        public void ResetTemperature()
        {
            baseTemp = defaultTemp;
            targetTemp = defaultTemp;
        }

        public override void OnUpdate()
        {
            if (!tempEnabled)
                return;

            if (Physics.Raycast(CameraRay, out RaycastHit hit, RaycastDistance, RaycastMask))
            {
                if (hit.collider.TryGetComponent(out ThermometerTemp temp))
                {
                    if(!temp.IsBaseTrigger)
                        targetTemp = temp.Temperature;
                }
                else
                {
                    targetTemp = baseTemp;
                }
            }
            else
            {
                targetTemp = baseTemp;
            }

            if (tempInterval > 0) tempInterval -= Time.deltaTime;
            float noise = Mathf.PerlinNoise1D(Time.time * TempNoiseSpeed) * TempNoiseScale;

            if(currTemp > targetTemp) currTemp = Mathf.Lerp(currTemp, targetTemp, Time.deltaTime * TempGainSpeed);
            else currTemp = Mathf.Lerp(currTemp, targetTemp, Time.deltaTime * TempDropSpeed);

            if (tempInterval <= 0)
            {
                float temp = currTemp + noise;
                DisplayTemperature(temp);
                tempInterval = TempGetInterval;
            }
        }

        private void DisplayTemperature(float temp)
        {
            double roundedNumber = Math.Round(temp, 1);
            string mgText = roundedNumber.ToString("0.0", CultureInfo.InvariantCulture);

            if (mgText.Contains('.'))
            {
                string[] parts = mgText.Split('.');
                string wholePart = parts[0].TrimStart('0');

                if (string.IsNullOrEmpty(wholePart))
                    wholePart = "0";

                string final = string.Format(DisplayFormat, wholePart, parts[1]);
                Temperature.text = final;
            }
            else
            {
                string final = string.Format(DisplayFormat, 0, 0);
                Temperature.text = final;
            }
        }

        private void SetDisplay(bool state)
        {
            if (state)
            {
                Display.ClonedMaterial.EnableKeyword(EmissionKeyword);
                DisplayCanvas.SetActive(true);
            }
            else
            {
                Display.ClonedMaterial.DisableKeyword(EmissionKeyword);
                DisplayCanvas.SetActive(false);
            }
        }

        public override void OnItemSelect()
        {
            tempEnabled = true;
            ItemObject.SetActive(true);
            SetDisplay(true);

            StartCoroutine(ShowThermometer());
        }

        IEnumerator ShowThermometer()
        {
            yield return new WaitForAnimatorClip(Animator, ThermometerDrawState);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            StopAllCoroutines();
            StartCoroutine(HideThermometer());

            SetDisplay(false);
            tempEnabled = false;
            Animator.SetTrigger(ThermometerHideTrigger);
        }

        IEnumerator HideThermometer()
        {
            yield return new WaitForAnimatorClip(Animator, ThermometerHideState);

            ItemObject.SetActive(false);
            isEquipped = false;
        }

        public override void OnItemActivate()
        {
            ItemObject.SetActive(true);
            Animator.Play(ThermometerDrawState, 0, 1f);
            SetDisplay(true);

            tempEnabled = true;
            isEquipped = true;
        }

        public override void OnItemDeactivate()
        {
            ItemObject.SetActive(false);
            SetDisplay(false);

            tempEnabled = false;
            isEquipped = false;
        }

        public override StorableCollection OnCustomSave()
        {
            return new StorableCollection()
            {
                { nameof(baseTemp), baseTemp }
            };
        }

        public override void OnCustomLoad(JToken data)
        {
            float baseTemp = (float)data[nameof(baseTemp)];
            SetResetTemp(baseTemp);
        }
    }
}