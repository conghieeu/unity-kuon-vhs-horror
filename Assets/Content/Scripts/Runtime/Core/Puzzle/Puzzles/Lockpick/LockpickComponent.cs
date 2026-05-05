using System;
using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Thành phần 3D sinh ra trước Camera để xử lý logic xoay ổ khóa và thanh ghim (Bobby Pin) trong Lockpick Puzzle.")]
    public class LockpickComponent : MonoBehaviour
    {
        [Tooltip("AudioSource phát ra tiếng mở khóa hoặc gãy ghim.")]
        public AudioSource AudioSource;

        [Tooltip("Transform mô hình của thanh ghim (Bobby Pin).")]
        public Transform BobbyPin;

        [Tooltip("Transform mô hình của ổ khóa (Sẽ xoay khi thử bẻ).")]
        public Transform LockpickKeyhole;

        [Tooltip("Vị trí ảo dùng để gắn thanh ghim vào đúng tâm xoay của ổ khóa.")]
        public Transform KeyholeCopyLocation;

        [Tooltip("Trục xoay của thanh ghim (Thường là Z).")]
        public Axis BobbyPinForward;

        [Tooltip("Trục xoay của ổ khóa (Thường là Z).")]
        public Axis KeyholeForward;

        [Tooltip("Tốc độ di chuyển ghim khi di chuột.")]
        public float BobbyPinRotateSpeed = 20;

        [Tooltip("Thời gian chờ trước khi rút ghim mới ra sau khi ghim cũ bị gãy.")]
        public float BobbyPinResetTime = 1;

        [Tooltip("Biên độ rung lắc của thanh ghim khi bị xoay căng (Sai góc mở).")]
        public float BobbyPinShakeAmount = 3;

        [Tooltip("Góc xoay tối đa của ổ khóa khi được mở thành công (Thường là -90 hoặc 90).")]
        public float KeyholeUnlockAngle = -90;

        [Tooltip("Tốc độ xoay ổ khóa khi người chơi bấm nút Mở (Jump/Tương tác).")]
        public float KeyholeRotateSpeed = 2;

        [Tooltip("Âm thanh phát ra khi bẻ khóa thành công.")]
        public SoundClip Unlock;

        [Tooltip("Âm thanh phát ra khi bẻ gãy 1 thanh ghim.")]
        public SoundClip BobbyPinBreak;

        private PlayerManager playerManager;
        private LockpickInteract lockpick;
        private GameManager gameManager;

        private bool isActive;
        private bool isUnlocked;

        private MinMax keyholeLimits;
        private float bobbyPinAngle;
        private float keyholeAngle;
        private float keyholeTarget;

        private float keyholeTestRange;
        private float bobbyPinLifetime;
        private float bobbyPinUnlockDistance;
        private float keyholeUnlockTarget;

        private int bobbyPins;
        private float bobbyPinTime;
        private bool canUseBobbyPin;

        public void SetLockpick(LockpickInteract lockpick)
        {
            this.lockpick = lockpick;
            gameManager = lockpick.GameManager;
            playerManager = lockpick.PlayerManager;

            keyholeLimits = new MinMax(0, KeyholeUnlockAngle);
            keyholeTestRange = lockpick.KeyholeMaxTestRange;
            bobbyPinLifetime = lockpick.BobbyPinLifetime;
            bobbyPinUnlockDistance = lockpick.BobbyPinUnlockDistance;
            keyholeUnlockTarget = lockpick.KeyholeUnlockTarget;

            bobbyPinTime = bobbyPinLifetime;
            bobbyPins = lockpick.BobbyPinItem.Quantity;
            BobbyPin.gameObject.SetActive(bobbyPins > 0);

            UpdateLockpicksText();
            lockpick.LockpickUI.SetActive(true);

            canUseBobbyPin = true;
            isActive = true;
        }

        private void Update()
        {
            if (!isActive)
                return;

            if(KeyholeCopyLocation != null)
            {
                Vector3 copyPosiiton = KeyholeCopyLocation.position;
                Vector3 bobbyPinPosition = new Vector3(copyPosiiton.x, copyPosiiton.y, copyPosiiton.z);
                BobbyPin.position = bobbyPinPosition;
            }

            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE))
            {
                UnuseLockpick();
                return;
            }

            bool tryUnlock = InputManager.ReadButton(Controls.JUMP);
            keyholeTarget = tryUnlock ? KeyholeUnlockAngle : 0;

            float bobbyPinDiff = Mathf.Abs(lockpick.UnlockAngle - bobbyPinAngle);
            float bobbyPinNormalized = 0;
            float bobbyPinShake = 0;

            if (bobbyPins > 0 && canUseBobbyPin && !isUnlocked)
            {
                bool damageBobbyPin = false;
                if (!tryUnlock)
                {
                    Vector2 bobbyPinMove = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);
                    bobbyPinAngle += bobbyPinMove.x * BobbyPinRotateSpeed * Time.deltaTime;
                }
                else
                {
                    damageBobbyPin = true;
                    float randomShake = UnityEngine.Random.insideUnitCircle.x;
                    bobbyPinShake = UnityEngine.Random.Range(-randomShake, randomShake) * BobbyPinShakeAmount;

                    if (bobbyPinDiff <= keyholeTestRange)
                    {
                        bobbyPinNormalized = 1 - (bobbyPinDiff / keyholeTestRange);
                        bobbyPinNormalized = (float)Math.Round(bobbyPinNormalized, 2);
                        float targetDiff = Mathf.Abs(keyholeTarget - keyholeAngle);
                        float targetNormalized = targetDiff / keyholeTestRange;

                        if (bobbyPinNormalized >= (1 - bobbyPinUnlockDistance))
                        {
                            bobbyPinNormalized = 1;
                            damageBobbyPin = false;
                            bobbyPinShake = 0;

                            if (targetNormalized <= keyholeUnlockTarget)
                            {
                                StartCoroutine(OnUnlock());
                                isUnlocked = true;
                            }
                        }
                    }
                }

                if (damageBobbyPin && !lockpick.UnbreakableBobbyPin)
                {
                    if (bobbyPinTime > 0)
                    {
                        bobbyPinTime -= Time.deltaTime;
                    }
                    else
                    {
                        bobbyPins = Inventory.Instance.RemoveItem(lockpick.BobbyPinItem, 1);
                        BobbyPin.gameObject.SetActive(false);
                        bobbyPinTime = bobbyPinLifetime;
                        UpdateLockpicksText();

                        StartCoroutine(ResetBobbyPin());
                        AudioSource.PlayOneShotSoundClip(BobbyPinBreak);

                        canUseBobbyPin = false;
                        bobbyPinAngle = 0;
                    }
                }

                bobbyPinAngle = Mathf.Clamp(bobbyPinAngle, -90, 90);
                BobbyPin.localRotation = Quaternion.AngleAxis(bobbyPinAngle + bobbyPinShake, BobbyPinForward.Convert());
            }

            if (isUnlocked)
            {
                keyholeTarget = KeyholeUnlockAngle;
                bobbyPinNormalized = 1f;
            }

            keyholeTarget *= bobbyPinNormalized;
            keyholeAngle = Mathf.MoveTowardsAngle(keyholeAngle, keyholeTarget, Time.deltaTime * KeyholeRotateSpeed * 100);
            keyholeAngle = Mathf.Clamp(keyholeAngle, keyholeLimits.RealMin, keyholeLimits.RealMax);     
            LockpickKeyhole.localRotation = Quaternion.AngleAxis(keyholeAngle, KeyholeForward.Convert());
        }

        private void UnuseLockpick()
        {
            playerManager.PlayerItems.IsItemsUsable = true;
            gameManager.FreezePlayer(false);
            gameManager.SetBlur(false, true);
            gameManager.ShowPanel(GameManager.PanelType.MainPanel);
            gameManager.ShowControlsInfo(false, null);
            lockpick.LockpickUI.SetActive(false);
            Destroy(gameObject);
        }

        private void UpdateLockpicksText()
        {
            string text = lockpick.LockpicksText;
            text = text.RegexReplaceTag('[', ']', "count", bobbyPins.ToString());
            lockpick.LockpickText.text = text;
        }

        IEnumerator ResetBobbyPin()
        {
            yield return new WaitForSeconds(BobbyPinResetTime);

            if (bobbyPins > 0)
            {
                BobbyPin.gameObject.SetActive(true);
                canUseBobbyPin = true;
            }
            else UnuseLockpick();
        }

        IEnumerator OnUnlock()
        {
            lockpick.Unlock();

            if(Unlock.audioClip != null) 
                AudioSource.PlayOneShotSoundClip(Unlock);

            yield return new WaitForSeconds(1f);
            yield return new WaitUntil(() => !AudioSource.isPlaying);

            UnuseLockpick();
        }
    }
}