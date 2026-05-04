using UnityEngine;
using UnityEngine.UI;
using UHFPS.Input;
using UHFPS.Tools;
using TMPro;
using System.Collections;

namespace UHFPS.Runtime
{
    public class SafeWheel : MonoBehaviour
    {
        public Transform Wheel;
        public Axis WheelRotateAxis = Axis.Z;

        public ushort MaxWheelNumber = 99;
        public float FastRotateWaitTime = 2f;
        public float FastRotateNumberTime = 1f;
        public float RotateSmoothing = 0.1f;
        public float UnlockWaitTime = 1f;

        public AudioSource AudioSource;
        public SoundClip DialTurn;
        public SoundClip SafeUnlock;

        private SafePuzzle safePuzzle;
        private GameManager gameManager;
        private PlayerManager playerManager;

        private Image number1Panel;
        private TMP_Text number1;
        private Image number2Panel;
        private TMP_Text number2;
        private Image number3Panel;
        private TMP_Text number3;

        private float targetAngle;
        private int prevDirection;
        private int wheelIndex;

        private int solution1;
        private int solution2;
        private int solution3;

        private float fastTime;
        private float oneNumberAngle;
        private float rotateVelocity;

        private bool isActive;
        private bool isInitial;
        private bool isOneNumber;
        private bool isFastRotate;

        public void SetSafe(SafePuzzle safePuzzle)
        {
            this.safePuzzle = safePuzzle;
            gameManager = safePuzzle.GameManager;
            playerManager = safePuzzle.PlayerManager;

            oneNumberAngle = (float)360 / (MaxWheelNumber + 1);

            safePuzzle.SafeLockPanel.SetActive(true);
            number1 = safePuzzle.Number1;
            number2 = safePuzzle.Number2;
            number3 = safePuzzle.Number3;

            number1Panel = number1.transform.parent.GetComponent<Image>();
            number2Panel = number2.transform.parent.GetComponent<Image>();
            number3Panel = number3.transform.parent.GetComponent<Image>();

            SetActiveSolutionColor();

            prevDirection = 0;
            isInitial = false;
            isActive = true;
        }

        private void Update()
        {
            if (!isActive) return;

            UpdateWheelRotation();
            HandleOtherInputs();
            HandleDialInput();
            CheckForSolutionCompletion();
        }

        private void UpdateWheelRotation()
        {
            Vector3 wheelRotation = Wheel.localEulerAngles;
            float currentAngle = wheelRotation.Component(WheelRotateAxis);
            currentAngle = Mathf.SmoothDampAngle(currentAngle, -targetAngle, ref rotateVelocity, RotateSmoothing);
            Wheel.localEulerAngles = wheelRotation.SetComponent(WheelRotateAxis, currentAngle);
        }

        private void HandleOtherInputs()
        {
            // handle quit lock view
            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE))
            {
                QuitPuzzle();
                return;
            }

            // handle reset solution
            if (isInitial && InputManager.ReadButtonOnce(GetInstanceID(), Controls.RELOAD))
            {
                ResetSolution();
                SetActiveSolutionColor();
            }
        }

        private void HandleDialInput()
        {
            if (InputManager.ReadInput(Controls.AXIS_ARROWS, out Vector2 axis))
            {
                int direction = (int)axis.x;

                if (!isInitial)
                {
                    prevDirection = direction;
                    isInitial = true;
                }
                else if (wheelIndex == 2 && direction != prevDirection)
                {
                    return;
                }

                if (!isOneNumber)
                {
                    AudioSource.PlayOneShotSoundClip(DialTurn);
                    targetAngle = (targetAngle + direction * oneNumberAngle) % 360f;
                    isOneNumber = true;
                }

                if (!isFastRotate)
                {
                    if (fastTime < FastRotateWaitTime)
                    {
                        fastTime += Time.deltaTime;
                    }
                    else
                    {
                        isFastRotate = true;
                        fastTime = 0;
                    }
                }
                else
                {
                    if (fastTime < FastRotateNumberTime)
                    {
                        fastTime += Time.deltaTime;
                    }
                    else
                    {
                        isOneNumber = false;
                        fastTime = 0;
                    }
                }

                if (direction != prevDirection)
                {
                    wheelIndex++;
                    prevDirection = direction;
                }

                int currentNumber = Mathf.RoundToInt(targetAngle / oneNumberAngle);
                if (currentNumber < 0) currentNumber += MaxWheelNumber + 1;

                switch (wheelIndex)
                {
                    case 0:
                        solution1 = currentNumber;
                        break;
                    case 1:
                        solution2 = currentNumber;
                        break;
                    case 2:
                        solution3 = currentNumber;
                        break;
                }

                SetSolutionText();
                SetActiveSolutionColor();
            }
            else
            {
                isFastRotate = false;
                isOneNumber = false;
                fastTime = 0;
            }
        }

        private void CheckForSolutionCompletion()
        {
            if (wheelIndex == 2 && InputManager.ReadButtonOnce(GetInstanceID(), Controls.JUMP))
            {
                string wantedCode = safePuzzle.UnlockCode;
                string actualCode = $"{solution1:00}{solution2:00}{solution3:00}";
                
                if(actualCode == wantedCode)
                {
                    isActive = false;
                    AudioSource.PlayOneShotSoundClip(SafeUnlock);
                    StartCoroutine(OnUnlockWait());
                }
            }
        }

        IEnumerator OnUnlockWait()
        {
            yield return new WaitForSeconds(UnlockWaitTime);
            safePuzzle.SetUnlocked();
            QuitPuzzle();
        }

        private void SetSolutionText()
        {
            number1.text = solution1.ToString("00");
            number2.text = solution2.ToString("00");
            number3.text = solution3.ToString("00");
        }

        private void SetActiveSolutionColor()
        {
            Color[] colors = { safePuzzle.SolutionNormalColor, safePuzzle.SolutionNormalColor, safePuzzle.SolutionNormalColor };
            colors[wheelIndex] = safePuzzle.SolutionCurrentColor;

            number1Panel.color = colors[0];
            number2Panel.color = colors[1];
            number3Panel.color = colors[2];
        }

        private void ResetSolution()
        {
            solution1 = 0;
            solution2 = 0;
            solution3 = 0;

            number1.text = "00";
            number2.text = "00";
            number3.text = "00";

            prevDirection = 1;
            targetAngle = 0;
            wheelIndex = 0;
            fastTime = 0;

            Vector3 wheelRotation = Wheel.localEulerAngles;
            Wheel.localEulerAngles = wheelRotation.SetComponent(WheelRotateAxis, targetAngle);

            isFastRotate = false;
            isOneNumber = false;
            isInitial = false;
        }

        private void QuitPuzzle()
        {
            playerManager.PlayerItems.IsItemsUsable = true;
            gameManager.FreezePlayer(false);
            gameManager.SetBlur(false, true);
            gameManager.ShowPanel(GameManager.PanelType.MainPanel);
            gameManager.ShowControlsInfo(false, null);
            safePuzzle.OnPuzzleQuit();
            ResetSolution();
            Destroy(gameObject);
        }
    }
}