using System;
using UnityEngine.Rendering;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("CCTV Camera System")]
    public class CCTV_CameraSystem : MonoBehaviour
    {
        public enum CameraFeedEnum { NoFeed, LiveFeed, StaticFeed }
        public enum CameraPivotEnum { Static, Vertical, Horizontal, Both }

        public CameraFeedEnum CameraFeed = CameraFeedEnum.NoFeed;
        public CameraPivotEnum CameraPivot = CameraPivotEnum.Static;

        [Header("Camera Setup")]
        public Volume CCTVEffects;
        public CCTV_Camera[] Cameras;

        [Header("Camera Settings")]
        public float FadeViewSpeed = 3f;
        public float CameraSensitivity = 0.5f;

        [Header("Camera Monitor")]
        public CRTSimpleMonitor Monitor;
        public Vector2Int OutputTextureSize = new Vector2Int(500, 350);

        [Header("Enter Condition")]
        public ReflectionField Condition;

        private PlayerPresenceManager playerPresence;
        private PlayerItemsManager playerItems;
        private GameManager gameManager;

        private bool isActive;
        private bool isCameraView;
        private CCTV_Camera currentCamera;

        private RenderTexture outputTexture;
        private GameObject cctvOverlay;
        private TMP_Text camText;

        private void Awake()
        {
            playerPresence = PlayerPresenceManager.Instance;
            playerItems = playerPresence.PlayerManager.PlayerItems;
            gameManager = GameManager.Instance;

            var behaviours = gameManager.GraphicReferences.Value["CCTV"];
            cctvOverlay = behaviours[0].gameObject;
            camText = (TMP_Text)behaviours[1];
        }

        private void Start()
        {
            if (Cameras.Length > 0)
            {
                currentCamera = Cameras[0];
                SetMonitorCameraOutput();
            }
        }

        private void Update()
        {
            if (isCameraView)
            {
                if (isActive && InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE))
                    SwitchBack();

                if (Cameras.Length > 0 && InputManager.ReadInputOnce(this, Controls.AXIS_ARROWS, out Vector2 axis))
                    SwitchCamera((int)axis.x);

                if (CameraPivot != CameraPivotEnum.Static)
                    ControlCameras();
            }
        }

        private void ControlCameras()
        {
            Vector2 lookDelta = InputManager.ReadInput<Vector2>(Controls.LOOK);

            if (CameraPivot == CameraPivotEnum.Vertical || CameraPivot == CameraPivotEnum.Both)
            {
                Transform vertical = currentCamera.VerticalJoint;

                if (vertical != null)
                {
                    MinMax verticalLimits = currentCamera.VerticalLimits;
                    Axis verticalAxis = currentCamera.VerticalAxis;
                    Vector3 verticalDir = verticalAxis.Convert();

                    float currentVerticalAngle = vertical.localEulerAngles.Component(verticalAxis);
                    float desiredVerticalAngle = currentVerticalAngle - lookDelta.y * CameraSensitivity;
                    desiredVerticalAngle = ClampAngle(desiredVerticalAngle, verticalLimits.RealMin, verticalLimits.RealMax);

                    // apply the rotation
                    vertical.localRotation = Quaternion.AngleAxis(desiredVerticalAngle, verticalDir) * Quaternion.identity;
                }
            }

            if (CameraPivot == CameraPivotEnum.Horizontal || CameraPivot == CameraPivotEnum.Both)
            {
                Transform horizontal = currentCamera.HorizontalJoint;

                if (horizontal != null)
                {
                    MinMax horizontalLimits = currentCamera.HorizontalLimits;
                    Axis horizontalAxis = currentCamera.HorizontalAxis;
                    Vector3 horizontalDir = horizontalAxis.Convert();

                    float currentHorizontalAngle = horizontal.localEulerAngles.Component(horizontalAxis);
                    float desiredHorizontalAngle = currentHorizontalAngle + lookDelta.x * CameraSensitivity;
                    desiredHorizontalAngle = ClampAngle(desiredHorizontalAngle, horizontalLimits.RealMin, horizontalLimits.RealMax);

                    // apply the rotation
                    horizontal.localRotation = Quaternion.AngleAxis(desiredHorizontalAngle, horizontalDir) * Quaternion.identity;
                }
            }
        }

        private void SetMonitorCameraOutput()
        {
            if (Monitor == null || CameraFeed == CameraFeedEnum.NoFeed)
                return;

            foreach (var camera in Cameras)
            {
                camera.LiveCamera.targetTexture = null;
                camera.LiveCamera.gameObject.SetActive(false);
            }

            if (CameraFeed == CameraFeedEnum.LiveFeed)
            {
                if (outputTexture == null)
                {
                    outputTexture = new(OutputTextureSize.x, OutputTextureSize.y, 24);
                    outputTexture.name = "CameraLiveFeed";
                    outputTexture.Create();
                }

                currentCamera.LiveCamera.gameObject.SetActive(true);
                currentCamera.LiveCamera.targetTexture = outputTexture;
                Monitor.SetVideoInput(outputTexture);
            }
            else if(CameraFeed == CameraFeedEnum.StaticFeed)
            {
                Texture2D cameraView = CaptureCameraView();
                Monitor.SetTexture(cameraView);
            }
        }

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle > 180) angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }

        private void SwitchCamera(int axis)
        {
            int currentIndex = Array.IndexOf(Cameras, currentCamera);
            int nextCamera = GameTools.Wrap(currentIndex + axis, 0, Cameras.Length);

            currentCamera.VirtualCamera.gameObject.SetActive(false);
            currentCamera = Cameras[nextCamera];
            currentCamera.VirtualCamera.gameObject.SetActive(true);
            SetMonitorCameraOutput();

            if (cctvOverlay != null)
                camText.text = "CAM " + (nextCamera + 1);
        }

        public void StartCameraControl()
        {
            if (Condition.IsSet && !Condition.Value)
                return;

            playerPresence.FreezePlayer(true);
            playerPresence.SwitchActiveCamera(currentCamera.VirtualCamera.gameObject, FadeViewSpeed, OnBackgroundFade, () => { isActive = true; });
            playerItems.IsItemsUsable = false;
        }

        public void SwitchBack()
        {
            if (isCameraView)
            {
                if (CameraFeed == CameraFeedEnum.StaticFeed) SetMonitorCameraOutput();
                playerPresence.SwitchToPlayerCamera(FadeViewSpeed, OnBackgroundFade);
            }

            isActive = false;
        }

        private void OnBackgroundFade()
        {
            if (!isCameraView)
            {
                if (CCTVEffects != null) CCTVEffects.enabled = true;
                if (cctvOverlay != null)
                {
                    cctvOverlay.SetActive(true);
                    int currentIndex = Array.IndexOf(Cameras, currentCamera);
                    camText.text = "CAM " + (currentIndex + 1);
                }

                gameManager.DisableAllGamePanels();
                isCameraView = true;
            }
            else
            {
                if (CCTVEffects != null) CCTVEffects.enabled = false;
                if (cctvOverlay != null) cctvOverlay.SetActive(false);

                gameManager.ShowPanel(GameManager.PanelType.MainPanel);
                playerPresence.FreezePlayer(false);
                playerItems.IsItemsUsable = true;
                isCameraView = false;
            }
        }

        private Texture2D CaptureCameraView()
        {
            Camera camera = currentCamera.LiveCamera;
            camera.gameObject.SetActive(true);

            RenderTexture renderTexture = new(OutputTextureSize.x, OutputTextureSize.y, 24);
            renderTexture.antiAliasing = 8;

            camera.targetTexture = renderTexture;
            camera.Render();

            Texture2D texture = new(OutputTextureSize.x, OutputTextureSize.y, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;

            texture.ReadPixels(new Rect(0, 0, OutputTextureSize.x, OutputTextureSize.y), 0, 0);
            texture.Apply();

            camera.gameObject.SetActive(false);
            RenderTexture.active = null;
            camera.targetTexture = null;
            Destroy(renderTexture);

            return texture;
        }
    }
}