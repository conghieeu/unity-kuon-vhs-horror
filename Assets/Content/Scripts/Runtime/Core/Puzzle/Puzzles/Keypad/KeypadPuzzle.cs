using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using TMPro;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class KeypadPuzzle : PuzzleBase, ISaveable
    {
        public enum Button { Number0, Number1, Number2, Number3, Number4, Number5, Number6, Number7, Number8, Number9, Remove, Confirm }

        public string AccessCode = "0000";
        public uint MaxCodeLength = 4;
        public bool UseInteract = false;
        public float AccessUpdateWaitTime = 1f;
        public float SleepWaitTime = 10f;

        public TextMeshPro DisplayTextMesh;
        public string GrantedText = "ACCESS GRANTED";
        public string DeniedText = "ACCESS DENIED";

        public float TextFontSize = 20;
        public float CodeFontSize = 25;

        public Color DefaultColor = Color.white;
        public Color GrantedColor = Color.green;
        public Color DeniedColor = Color.red;

        public bool UseLights = true;
        public Light KeypadLight;
        public Color GrantedLightColor = Color.green;
        public Color DeniedLightColor = Color.red;

        public bool UseEmission = true;
        public MeshRenderer KeypadRenderer;
        public string EmissionKeyword = "_EMISSION";

        public SoundClip ButtonPressSound;
        public SoundClip AccessGrantedSound;
        public SoundClip AccessDeniedSound;

        public UnityEvent OnAccessGranted;
        public UnityEvent OnAccessDenied;
        public UnityEvent<int> OnButtonPressed;

        /// <summary>
        /// Granted status of the code lock.
        /// </summary>
        public bool AccessGranted => accessGranted;

        private AudioSource audioSource;
        private string displayText = "";
        private bool accessGranted;
        private bool notUsable;

        private bool confirmPressed;
        private float sleepTime;

        public override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
            DisplayTextMesh.color = DefaultColor;
            DisplayTextMesh.fontSize = CodeFontSize;
            DisplayTextMesh.text = "";
            SetRendererEmission(false);

            if (UseInteract)
            {
                foreach (var collider in CollidersEnable)
                {
                    collider.enabled = true;
                }
            }
        }

        public void OnPressButton(Button button)
        {
            if (accessGranted || notUsable)
                return;

            audioSource.PlayOneShotSoundClip(ButtonPressSound);
            confirmPressed = false;

            if (button == Button.Confirm)
            {
                if(displayText == AccessCode)
                {
                    SetAccessGranted();
                    accessGranted = true;
                }
                else
                {
                    SetAccessDenied();
                    displayText = "";
                }

                confirmPressed = true;
            }
            else if(button == Button.Remove)
            {
                if (displayText.Length > 0)
                {
                    displayText = displayText.Remove(displayText.Length - 1);
                    DisplayTextMesh.text = displayText;
                }
            }
            else if(displayText.Length < MaxCodeLength)
            {
                displayText += (int)button;
                DisplayTextMesh.text = displayText;
                OnButtonPressed?.Invoke((int)button);
            }

            if (UseInteract)
            {
                sleepTime = SleepWaitTime;
                SetRendererEmission(true);
            }
        }

        public override void InteractStart()
        {
            if(!UseInteract && !accessGranted) 
                base.InteractStart();
        }

        public override void Update()
        {
            if (!UseInteract) base.Update();
            else if (!isActive && confirmPressed)
            {
                if(sleepTime > 0) sleepTime -= Time.deltaTime;
                else
                {
                    displayText = "";
                    DisplayTextMesh.text = "";
                    SetRendererEmission(false);
                    confirmPressed = false;
                }
            }
        }

        public override void OnBackgroundFade()
        {
            base.OnBackgroundFade();

            if (!UseInteract && UseEmission)
            {
                if(isActive) SetRendererEmission(true);
                else SetRendererEmission(false);
            }
        }

        public void SetAccessGranted()
        {
            audioSource.PlayOneShotSoundClip(AccessGrantedSound);
            OnAccessGranted?.Invoke();

            DisplayTextMesh.text = GrantedText;
            DisplayTextMesh.color = GrantedColor;
            DisplayTextMesh.fontSize = TextFontSize;

            if (UseLights)
            {
                KeypadLight.color = GrantedLightColor;
                KeypadLight.enabled = true;
            }

            notUsable = true;
            StartCoroutine(OnAccessUpdated(true));
        }

        public void SetAccessDenied()
        {
            audioSource.PlayOneShotSoundClip(AccessDeniedSound);
            OnAccessDenied?.Invoke();

            DisplayTextMesh.text = DeniedText;
            DisplayTextMesh.color = DeniedColor;
            DisplayTextMesh.fontSize = TextFontSize;

            if (UseLights)
            {
                KeypadLight.color = DeniedLightColor;
                KeypadLight.enabled = true;
            }

            notUsable = true;
            StartCoroutine(OnAccessUpdated(false));
        }

        private void SetRendererEmission(bool state)
        {
            if (!UseEmission) 
                return;

            if(state) KeypadRenderer.material.EnableKeyword(EmissionKeyword);
            else KeypadRenderer.material.DisableKeyword(EmissionKeyword);
        }

        IEnumerator OnAccessUpdated(bool granted)
        {
            yield return new WaitForSeconds(AccessUpdateWaitTime);

            if (UseLights) KeypadLight.enabled = false;

            displayText = "";
            DisplayTextMesh.text = "";
            DisplayTextMesh.color = DefaultColor;
            DisplayTextMesh.fontSize = CodeFontSize;

            if (granted)
            {
                if (!UseInteract) SwitchBack();
                else
                {
                    SetRendererEmission(false);
                    confirmPressed = false;
                    sleepTime = 0f;
                }

                DisableInteract();
            }

            notUsable = false;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(accessGranted), accessGranted }
            };
        }

        public void OnLoad(JToken data)
        {
            accessGranted = (bool)data[nameof(accessGranted)];
            if(accessGranted) DisableInteract();

            if (UseLights) KeypadLight.enabled = false;
            SetRendererEmission(false);
            DisplayTextMesh.text = "";
            displayText = "";
        }
    }
}