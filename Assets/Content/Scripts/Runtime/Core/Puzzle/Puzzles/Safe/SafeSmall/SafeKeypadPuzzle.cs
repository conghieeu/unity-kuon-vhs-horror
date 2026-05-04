using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class SafeKeypadPuzzle : PuzzleBase, ISaveable
    {
        public enum Button { Number0, Number1, Number2, Number3, Number4, Number5, Number6, Number7, Number8, Number9, Cancel, Confirm }
        private const int MAX_INDICATORS = 4;

        public string AccessCode = "0000";

        public Animator Animator;
        public string UnlockTrigger = "Unlock";
        public string ResetTrigger = "Reset";

        public bool UseIndicators = true;
        public MeshRenderer[] Indicators;
        public string EmissionKeyword = "_EMISSION";
        public string EmissionColor = "_EmissionColor";
        public Color DefaultLightColor = Color.red;
        public Color EnterLightColor = Color.green;

        public SoundClip ButtonPressSound;
        public SoundClip AccessGrantedSound;
        public SoundClip AccessDeniedSound;

        public bool LoadCallEvent;
        public UnityEvent OnAccessGranted;
        public UnityEvent OnAccessDenied;
        public UnityEvent<int> OnButtonPressed;

        private AudioSource audioSource;
        private string enteredCode = "";

        private bool isUnlocked;
        private bool notUsable;

        public override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
        }

        public void OnPressButton(Button button)
        {
            if (isUnlocked || notUsable)
                return;

            if (button == Button.Confirm)
            {
                if (enteredCode == AccessCode)
                {
                    SetAccessGranted();
                    isUnlocked = true;
                }
                else
                {
                    SetAccessDenied();
                    enteredCode = "";
                }
            }
            else if(button == Button.Cancel)
            {
                enteredCode = "";
                SetIndicator();
            }
            else if (enteredCode.Length < 4)
            {
                enteredCode += (int)button;
                OnButtonPressed?.Invoke((int)button);
                SetIndicator();
            }

            audioSource.PlayOneShotSoundClip(ButtonPressSound);
        }

        public override void OnBackgroundFade()
        {
            base.OnBackgroundFade();

            if (isActive) gameObject.layer = DisabledLayer;
            else if (!isUnlocked)
            {
                gameObject.layer = InteractLayer;
                enteredCode = "";

                for (int i = 0; i < MAX_INDICATORS; i++)
                {
                    Indicators[i].material.SetColor(EmissionColor, DefaultLightColor);
                }
            }
            else if(isUnlocked)
            {
                Animator.SetTrigger(UnlockTrigger);
            }
        }

        private void SetIndicator()
        {
            if (!UseIndicators)
                return;

            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Color color = i < enteredCode.Length ? EnterLightColor : DefaultLightColor;
                Indicators[i].material.SetColor(EmissionColor, color);
            }
        }

        public void SetAccessGranted()
        {
            audioSource.PlayOneShotSoundClip(AccessGrantedSound);
            OnAccessGranted?.Invoke();

            notUsable = true;
            StartCoroutine(OnAccessUpdated(true));
        }

        public void SetAccessDenied()
        {
            audioSource.PlayOneShotSoundClip(AccessDeniedSound);
            OnAccessDenied?.Invoke();

            notUsable = true;
            StartCoroutine(OnAccessUpdated(false));
        }

        IEnumerator OnAccessUpdated(bool granted)
        {
            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Indicators[i].material.DisableKeyword(EmissionKeyword);
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Color color = granted ? EnterLightColor : DefaultLightColor;
                Indicators[i].material.SetColor(EmissionColor, color);
                Indicators[i].material.EnableKeyword(EmissionKeyword);
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Indicators[i].material.DisableKeyword(EmissionKeyword);
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Indicators[i].material.EnableKeyword(EmissionKeyword);
            }

            yield return new WaitForSeconds(0.5f);

            if (notUsable = granted)
                SwitchBack();
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isUnlocked), isUnlocked }
            };
        }

        public void OnLoad(JToken data)
        {
            isUnlocked = (bool)data[nameof(isUnlocked)];

            if (isUnlocked)
            {
                gameObject.layer = DisabledLayer;
                if (Animator != null) Animator.SetTrigger(ResetTrigger);
                if (LoadCallEvent) OnAccessGranted?.Invoke();

                for (int i = 0; i < MAX_INDICATORS; i++)
                {
                    Indicators[i].material.SetColor(EmissionColor, EnterLightColor);
                }
            }
        }
    }
}