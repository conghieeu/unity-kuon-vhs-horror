using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UnityEngine.UI;
using static UHFPS.Runtime.InteractableItem;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(InteractController))]
    [Summary("Hệ thống kiểm tra (Examine) vật phẩm, cho phép người chơi nhặt vật phẩm lên, xoay, phóng to, đọc văn bản và tương tác chi tiết.")]
    public class ExamineController : PlayerComponent
    {
        public sealed class ExaminedObject
        {
            public InteractableItem InteractableItem;
            public ExaminePutter.PutSettings PutSettings;
            public Vector3 HoldPosition;
            public Vector3 StartPosition;
            public Quaternion StartRotation;
            public Vector3 ControlPoint;
            public float ExamineDistance;
            public float Velocity;
            public float t;

            public GameObject GameObject => InteractableItem.gameObject;
        }

        [Header("Examine Rendering")]
        [Tooltip("Các layer bị bỏ qua (culling) khi render đối tượng đang được examine.")]
        public LayerMask FocusCullLayes;
        [Tooltip("Layer dành riêng cho vật phẩm khi đang được examine (thường dùng để tách khỏi môi trường).")]
        public Layer FocusLayer;
        [Tooltip("Rendering Layer Mask sử dụng cho URP/HDRP khi examine.")]
        public uint FocusRenderingLayer;

        [Header("Examine Light & UI")]
        [Tooltip("Nguồn sáng trợ sáng dành riêng cho vật phẩm đang examine.")]
        public Light ExamineLight;
        [Tooltip("Prefab của điểm tương tác (Hotspot) hiển thị trên bề mặt vật phẩm.")]
        public GameObject HotspotPrefab;

        [Header("Controls Info")]
        [Tooltip("Cấu hình phím bấm: Cất vật phẩm.")]
        public ControlsContext ControlPutBack;
        [Tooltip("Cấu hình phím bấm: Đọc giấy/tài liệu.")]
        public ControlsContext ControlRead;
        [Tooltip("Cấu hình phím bấm: Nhặt vật phẩm vào kho.")]
        public ControlsContext ControlTake;
        [Tooltip("Cấu hình phím bấm: Xoay vật phẩm.")]
        public ControlsContext ControlRotate;
        [Tooltip("Cấu hình phím bấm: Phóng to/thu nhỏ.")]
        public ControlsContext ControlZoom;

        [Header("Examine Settings")]
        [Tooltip("Thời gian làm mượt khi xoay vật phẩm.")]
        public float RotateTime = 0.1f;
        [Tooltip("Hệ số nhân tốc độ xoay.")]
        public float RotateMultiplier = 3f;
        [Tooltip("Hệ số nhân tốc độ phóng to/thu nhỏ.")]
        public float ZoomMultiplier = 0.1f;
        [Tooltip("Thời gian chờ trước khi xem xong vật phẩm (để hiện tiêu đề/thoại).")]
        public float TimeToExamine = 2f;

        [Header("Positions")]
        [Tooltip("Tọa độ rớt vật phẩm xuống đất (tương đối so với Camera).")]
        public Vector3 DropOffset;
        [Tooltip("Tọa độ thu vật phẩm vào kho đồ (tương đối so với Camera).")]
        public Vector3 InventoryOffset;
        [Tooltip("Hiển thị nhãn vị trí trên Gizmos (trong Editor).")]
        public bool ShowLabels = true;

        [Header("Animations")]
        [Tooltip("Đường cong làm mượt vị trí khi bắt đầu nhấc vật phẩm lên để examine.")]
        public AnimationCurve PickUpCurve = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [Tooltip("Hệ số nhân thời gian của PickUpCurve.")]
        public float PickUpCurveMultiplier = 1f;
        [Tooltip("Thời gian nhấc vật phẩm.")]
        public float PickUpTime = 0.2f;

        [Tooltip("Đường cong làm mượt vị trí khi cất vật phẩm lại chỗ cũ.")]
        public AnimationCurve PutPositionCurve = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [Tooltip("Hệ số nhân thời gian của PutPositionCurve.")]
        public float PutPositionCurveMultiplier = 1f;
        [Tooltip("Thời gian cất vật phẩm (vị trí).")]
        public float PutPositionCurveTime = 0.1f;

        [Tooltip("Đường cong làm mượt góc xoay khi cất vật phẩm lại chỗ cũ.")]
        public AnimationCurve PutRotationCurve = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [Tooltip("Hệ số nhân thời gian của PutRotationCurve.")]
        public float PutRotationCurveMultiplier = 1f;
        [Tooltip("Thời gian cất vật phẩm (góc xoay).")]
        public float PutRotationCurveTime = 0.1f;

        [Header("Sounds")]
        [Tooltip("Âm thanh gợi ý khi xem xong vật phẩm (nếu vật phẩm không có âm thanh riêng).")]
        public SoundClip ExamineHintSound;

        public Vector3 DropPosition => transform.TransformPoint(DropOffset);
        public Vector3 InventoryPosition => transform.TransformPoint(InventoryOffset);

        public bool IsExamining { get; private set; }

        private GameManager gameManager;
        private InteractController interactController;

        private readonly Stack<ExaminedObject> examinedObjects = new();
        private ExaminedObject currentExamine;
        private Image examineHotspot;

        private bool isInventoryExamine;
        private bool isPointerShown;
        private bool isReadingPaper;
        private bool isHotspotPressed;

        private float defaultLightIntensity;
        private Color defaultLightColor;

        private Vector2 examineRotate;
        private Vector2 rotateVelocity;

        private void Awake()
        {
            gameManager = GameManager.Instance;
            interactController = GetComponent<InteractController>();

            defaultLightIntensity = ExamineLight.intensity;
            defaultLightColor = ExamineLight.color;
        }

        private void Start()
        {
            ControlPutBack.InteractName.SubscribeGloc();
            ControlRead.InteractName.SubscribeGloc();
            ControlTake.InteractName.SubscribeGloc();
            ControlRotate.InteractName.SubscribeGloc();
            ControlZoom.InteractName.SubscribeGloc();
        }

        private void Update()
        {
            if (gameManager.IsInventoryShown || gameManager.IsPaused)
                return;

            if (interactController.RaycastObject != null || IsExamining)
            {
                if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE))
                {
                    if (!IsExamining)
                    {
                        GameObject raycastObj = interactController.RaycastObject;
                        if (!raycastObj.GetComponent<ExaminePutter>())
                            StartExamine(raycastObj);
                    }
                    else
                    {
                        PopExaminedObject();
                    }
                }
            }

            if (IsExamining) ExamineHold();
        }

        public void SetExamineLight(float intensity)
        {
            ExamineLight.intensity = intensity;
        }

        public void SetExamineLight(Color color)
        {
            ExamineLight.color = color;
        }

        public void SetExamineLight(float intensity, Color color)
        {
            ExamineLight.intensity = intensity;
            ExamineLight.color = color;
        }

        public void ResetExamineLight()
        {
            ExamineLight.intensity = defaultLightIntensity;
            ExamineLight.color = defaultLightColor;
        }

        public void ExamineFromInventory(GameObject obj)
        {
            isInventoryExamine = true;
            StartExamine(obj);
        }

        private void StartExamine(GameObject obj)
        {
            if (obj.TryGetComponent(out InteractableItem interactableItem))
            {
                if (interactableItem.ExamineType == ExamineTypeEnum.None)
                    return;

                ExamineObject(interactableItem);
                gameManager.SetBlur(true, true);
                gameManager.FreezePlayer(true);
                gameManager.DisableAllGamePanels();

                ShowBottomControls(interactableItem);
                IsExamining = true;
            }
        }

        private void ShowBottomControls(InteractableItem interactableItem)
        {
            List<ControlsContext> controls = new()
            {
                ControlPutBack // default put back button info
            };

            // read paper or take object info
            if (interactableItem.IsPaper) controls.Add(ControlRead);
            else if (interactableItem.TakeFromExamine) controls.Add(ControlTake);

            // rotate object info
            if (interactableItem.ExamineRotate != ExamineRotateEnum.Static)
                controls.Add(ControlRotate);

            // zoom object info
            if (interactableItem.UseExamineZooming)
                controls.Add(ControlZoom);

            gameManager.ShowControlsInfo(true, controls.ToArray());
        }

        private void ExamineObject(InteractableItem interactableItem)
        {
            if (interactableItem == null) return;
            currentExamine?.GameObject.SetLayerRecursively(interactController.InteractLayer);

            Vector3 controlPoint = interactableItem.UseControlPoint ? interactableItem.ControlPoint : Vector3.zero;
            Vector3 controlOffset = Quaternion.LookRotation(MainCamera.transform.forward) * controlPoint;
            Vector3 holdPosition = MainCamera.transform.position + MainCamera.transform.forward * interactableItem.ExamineDistance;

            // transform settings
            var transformSettings = new ExaminePutter.TransformSettings(
                interactableItem.transform.position,
                interactableItem.transform.rotation,
                controlOffset
            );

            // curve settings
            var curveSettings = new ExaminePutter.CurveSettings
            (
                new ExaminePutter.PutCurve(PutPositionCurve)
                {
                    EvalMultiply = PutPositionCurveMultiplier,
                    CurveTime = PutPositionCurveTime
                },
                new ExaminePutter.PutCurve(PutRotationCurve)
                {
                    EvalMultiply = PutRotationCurveMultiplier,
                    CurveTime = PutRotationCurveTime
                }
             );

            // rigidbody settings
            ExaminePutter.RigidbodySettings rigidbodySettings = null;
            if (interactableItem.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbodySettings = new(rigidbody);
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }

            // put settings
            var putSettings = new ExaminePutter.PutSettings(
                interactableItem.transform,
                transformSettings,
                curveSettings,
                rigidbodySettings,
                examinedObjects.Count > 0
            );

            // push data to stack
            examinedObjects.Push(currentExamine = new ExaminedObject()
            {
                InteractableItem = interactableItem,
                PutSettings = putSettings,
                HoldPosition = holdPosition,
                StartPosition = interactableItem.transform.position,
                StartRotation = interactableItem.transform.rotation,
                ControlPoint = interactableItem.transform.position + controlOffset,
                ExamineDistance = interactableItem.ExamineDistance
            });

            foreach (Collider collider in interactableItem.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(collider, PlayerCollider, true);
            }

            if (interactableItem.IsCustomExamine)
            {
                foreach (var col in interactableItem.CollidersEnable)
                {
                    col.enabled = true;
                }

                foreach (var col in interactableItem.CollidersDisable)
                {
                    col.enabled = false;
                }
            }

            if (interactableItem.ShowExamineTitle)
            {
                StopAllCoroutines();
                StartCoroutine(ExamineItemAndShowInfo(interactableItem));
            }

            if (interactableItem.ExamineType == ExamineTypeEnum.CustomObject && interactableItem.ExamineHotspot.HotspotTransform != null)
            {
                // clear previous active hotspot
                if (examineHotspot != null)
                {
                    Destroy(examineHotspot.gameObject);
                    examineHotspot = null;
                }

                // add new hotspot
                GameObject hotspotGo = Instantiate(HotspotPrefab, Vector3.zero, Quaternion.identity, gameManager.ExamineHotspots);
                Image hotspotImage = hotspotGo.GetComponent<Image>();
                hotspotImage.Alpha(0f);
                examineHotspot = hotspotImage;
            }

            // show interaction pointer 
            if (interactableItem.AllowCursorExamine)
            {
                isPointerShown = true;
                gameManager.ShowPointer(FocusCullLayes, FocusLayer, (hit, _) =>
                {
                    if (!isReadingPaper && hit.collider.gameObject.TryGetComponent(out InteractableItem interactableItem))
                    {
                        ExamineObject(interactableItem);
                        gameManager.HidePointer();
                        isPointerShown = false;
                    }
                });
            }

            GameTools.PlayOneShot2D(transform.position, interactableItem.ExamineSound, "ExamineSound");
            interactableItem.gameObject.SetLayerRecursively(FocusLayer);
            interactableItem.gameObject.SetRenderingLayer(FocusRenderingLayer);
            interactableItem.OnExamineStartEvent?.Invoke();
            PlayerManager.PlayerItems.IsItemsUsable = false;
        }

        IEnumerator ExamineItemAndShowInfo(InteractableItem item)
        {
            if (!item.IsExamined)
            {
                yield return new WaitForSeconds(TimeToExamine);
                item.IsExamined = true;

                SoundClip examineHintSound = ExamineHintSound;
                if (item.ExamineHintSound != null)
                    examineHintSound = item.ExamineHintSound;

                GameTools.PlayOneShot2D(transform.position, examineHintSound, "ExamineInfo");
            }

            string title = item.ExamineTitle;
            if (item.ExamineInventoryTitle)
            {
                Item inventoryItem = item.PickupItem.GetItem();
                title = inventoryItem.Title;
            }

            gameManager.ShowExamineInfo(true, false, title);
        }

        private void PopExaminedObject()
        {
            ExaminedObject obj = examinedObjects.Pop();
            obj.InteractableItem.OnExamineEndEvent?.Invoke();

            // destroy an object if there are no other objects examined and the object is examined from the inventory
            if (examinedObjects.Count <= 0 && isInventoryExamine)
            {
                Destroy(obj.GameObject);
            }
            // otherwise return the object to its original location
            else
            {
                obj.GameObject.AddComponent<ExaminePutter>().Put(obj.PutSettings);
                obj.GameObject.SetRenderingLayer(FocusRenderingLayer, false);
            }

            // if the number of examined objects is greater than zero, peek the previous object
            if (examinedObjects.Count > 0)
            {
                currentExamine = examinedObjects.Peek();
                currentExamine.GameObject.SetLayerRecursively(FocusLayer);
            }
            // otherwise reset examined object and unlock player
            else
            {
                ResetExamine(obj);
                currentExamine = null;
            }

            // if it's a custom examine, enable/disable custom colliders
            if (obj.InteractableItem.IsCustomExamine)
            {
                foreach (var col in obj.InteractableItem.CollidersEnable)
                {
                    col.enabled = false;
                }

                foreach (var col in obj.InteractableItem.CollidersDisable)
                {
                    col.enabled = true;
                }
            }

            // disable pointer
            if (isPointerShown) gameManager.HidePointer();
            gameManager.ShowPaperInfo(false, true);
            isReadingPaper = false;
            isPointerShown = false;
        }

        private void ExamineHold()
        {
            InteractableItem currentItem = currentExamine.InteractableItem;

            // hold position
            foreach (var obj in examinedObjects)
            {
                Vector3 holdPos = MainCamera.transform.position + MainCamera.transform.forward * obj.ExamineDistance;
                obj.HoldPosition = Vector3.Lerp(obj.HoldPosition, holdPos, Time.deltaTime * 5);
                float speedMultiplier = PickUpCurve.Evaluate(obj.t) * PickUpCurveMultiplier;
                obj.t = Mathf.SmoothDamp(obj.t, 1f, ref obj.Velocity, PickUpTime + speedMultiplier);
                obj.InteractableItem.transform.position = VectorE.QuadraticBezier(obj.StartPosition, obj.HoldPosition, obj.ControlPoint, obj.t);
            }

            // rotation
            if (currentItem.UseFaceRotation && currentExamine.t <= 0.99f)
            {
                Vector3 faceRotation = currentItem.FaceRotation;
                Quaternion faceRotationQ = Quaternion.LookRotation(MainCamera.transform.forward) * Quaternion.Euler(faceRotation);
                currentItem.transform.rotation = Quaternion.Slerp(currentExamine.StartRotation, faceRotationQ, currentExamine.t);
            }
            else if (!gameManager.IsPointerHolding && !isReadingPaper && InputManager.ReadButton(Controls.FIRE))
            {
                Vector2 rotateValue = InputManager.ReadInput<Vector2>(Controls.LOOK) * RotateMultiplier;
                examineRotate = Vector2.SmoothDamp(examineRotate, rotateValue, ref rotateVelocity, RotateTime);

                switch (currentItem.ExamineRotate)
                {
                    case ExamineRotateEnum.Horizontal:
                        currentItem.transform.Rotate(MainCamera.transform.up, -examineRotate.x, Space.World);
                        break;
                    case ExamineRotateEnum.Vertical:
                        currentItem.transform.Rotate(MainCamera.transform.right, examineRotate.y, Space.World);
                        break;
                    case ExamineRotateEnum.Both:
                        currentItem.transform.Rotate(MainCamera.transform.up, -examineRotate.x, Space.World);
                        currentItem.transform.Rotate(MainCamera.transform.right, examineRotate.y, Space.World);
                        break;
                }
            }

            // examine zooming
            if (!isReadingPaper && currentItem.UseExamineZooming)
            {
                Vector2 scroll = InputManager.ReadInput<Vector2>(Controls.SCROLL_WHEEL);
                float nextZoom = currentExamine.ExamineDistance + scroll.normalized.y * ZoomMultiplier;
                currentExamine.ExamineDistance = Mathf.Clamp(nextZoom, currentItem.ExamineZoomLimits.RealMin, currentItem.ExamineZoomLimits.RealMax);
            }

            // examine hotspots
            bool isHotspotShown = false;
            if (examineHotspot != null && currentItem.ExamineHotspot.HotspotTransform != null)
            {
                if (currentItem.ExamineType == ExamineTypeEnum.CustomObject
                && currentItem.ExamineHotspot.HotspotTransform.gameObject.activeInHierarchy
                && currentExamine.t > 0.99f)
                {
                    var hotspot = currentItem.ExamineHotspot;
                    Vector3 mainCamera = MainCamera.transform.position;
                    Vector3 hotspotPos = currentItem.ExamineHotspot.HotspotTransform.position;

                    Vector3 screenPointPos = MainCamera.WorldToScreenPoint(hotspotPos);
                    examineHotspot.transform.position = screenPointPos;

                    Vector3 direction = hotspotPos - mainCamera;
                    direction -= direction.normalized * 0.01f;

                    float alpha = examineHotspot.color.a;
                    {
                        if (!Physics.Raycast(mainCamera, direction, out _, direction.magnitude, FocusCullLayes, QueryTriggerInteraction.Ignore) && currentItem.ExamineHotspot.Enabled)
                        {
                            alpha = Mathf.MoveTowards(alpha, 1f, Time.deltaTime * 10f);
                            isHotspotShown = true;

                            if (InputManager.ReadButtonOnce(this, Controls.USE))
                            {
                                hotspot.HotspotAction?.Invoke();
                                if (hotspot.ResetHotspot)
                                    isHotspotPressed = !isHotspotPressed;
                            }
                        }
                        else
                        {
                            alpha = Mathf.MoveTowards(alpha, 0f, Time.deltaTime * 10f);
                        }
                    }
                    examineHotspot.Alpha(alpha);
                }
                else
                {
                    examineHotspot.Alpha(0f);
                }
            }

            // paper reading
            if (!isHotspotShown) // if the hotspot is not shown, you can read the paper or take the item
            {
                if (currentItem.InteractableType == InteractableTypeEnum.ExamineItem && currentItem.IsPaper && !string.IsNullOrEmpty(currentItem.PaperText))
                {
                    if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.USE))
                    {
                        isReadingPaper = !isReadingPaper;
                        gameManager.ShowPaperInfo(isReadingPaper, false, currentItem.PaperText);
                    }
                }
                else if (currentItem.InteractableType == InteractableTypeEnum.InventoryItem && currentItem.TakeFromExamine)
                {
                    if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.USE))
                    {
                        ResetExamine(currentExamine, true);
                        interactController.Interact(currentExamine.GameObject);
                        currentExamine = null;
                        return;
                    }
                }
            }
        }

        private void ResetExamine(ExaminedObject examine, bool examineTake = false)
        {
            gameManager.SetBlur(false, true);
            gameManager.FreezePlayer(false);
            gameManager.ShowPanel(GameManager.PanelType.MainPanel);
            gameManager.ShowControlsInfo(false, new ControlsContext[0]);
            gameManager.ShowExamineInfo(false, true);
            PlayerManager.PlayerItems.IsItemsUsable = true;

            StopAllCoroutines();
            examinedObjects.Clear();

            if(!isInventoryExamine)
                examine.GameObject.SetLayerRecursively(interactController.InteractLayer);

            if (!examineTake)
            {
                if (examineHotspot != null)
                {
                    var hotspot = examine.InteractableItem.ExamineHotspot;
                    if (hotspot.ResetHotspot && isHotspotPressed)
                    {
                        hotspot.HotspotAction?.Invoke();
                        isHotspotPressed = false;
                    }

                    Destroy(examineHotspot.gameObject);
                    examineHotspot = null;
                }
            }
            else
            {
                if (examineHotspot != null)
                {
                    Destroy(examineHotspot.gameObject);
                    examineHotspot = null;
                }

                Vector3 position = examine.PutSettings.TransformData.Position;
                Quaternion rotation = examine.PutSettings.TransformData.Rotation;
                examine.GameObject.transform.SetPositionAndRotation(position, rotation);
                examine.GameObject.SetActive(false);
            }

            isInventoryExamine = false;
            IsExamining = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(InventoryPosition, 0.01f);
            if(ShowLabels) GizmosE.DrawCenteredLabel(InventoryPosition, "Inventory Position");

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(DropPosition, 0.01f);
            if (ShowLabels) GizmosE.DrawCenteredLabel(DropPosition, "Drop Position");
        }
    }
}