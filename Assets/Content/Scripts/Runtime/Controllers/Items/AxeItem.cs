using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Scriptable;
using ThunderWire.Attributes;
using static UHFPS.Scriptable.SurfaceDefinitionSet;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    [Summary("Điều khiển vật phẩm Rìu, cho phép người chơi tấn công cận chiến và gây sát thương.")]
    public class AxeItem : PlayerItemBehaviour
    {
        [Header("Surface Settings")]
        [Tooltip("Tập hợp định nghĩa các loại bề mặt khi bị rìu chém trúng.")]
        public SurfaceDefinitionSet SurfaceDefinitionSet;
        [Tooltip("Phương pháp phát hiện bề mặt khi chém trúng.")]
        public SurfaceDetection SurfaceDetection;
        [Tooltip("Thẻ (Tag) dùng để nhận diện bề mặt là da thịt (kẻ địch).")]
        public Tag FleshTag;

        [Header("Attack Setup")]
        [Tooltip("Lớp mạng (LayerMask) sẽ bị rìu chém trúng.")]
        public LayerMask RaycastMask;
        [Tooltip("Góc độ chém của rìu.")]
        public MinMax AttackAngle;
        [Tooltip("Tầm đánh của rìu (Khoảng cách từ người chơi tới mục tiêu).")]
        public MinMax AttackRange;
        [Tooltip("Số lượng tia (Raycast) được bắn ra để phát hiện va chạm trong một lần chém.")]
        public uint RaycastCount = 11;
        [Tooltip("Thời gian trễ trước khi tia va chạm đầu tiên được bắn ra (khớp với hoạt ảnh chém).")]
        public float AttackDelay;
        [Tooltip("Thời gian trễ giữa mỗi tia va chạm được bắn ra.")]
        public float RaycastDelay;
        [Tooltip("Hiển thị đường đạn (Gizmos) của các tia chém trong Editor để dễ gỡ lỗi.")]
        public bool ShowAttackGizmos;

        [Header("Damage")]
        [Tooltip("Sát thương gây ra ngẫu nhiên trong khoảng (Min-Max) mỗi lần chém trúng.")]
        public MinMaxInt AttackDamage;
        [Tooltip("Thời gian chờ tối thiểu giữa mỗi lần chém tiếp theo.")]
        public float NextAttackTime;

        [Header("Animations")]
        [Tooltip("Tên trạng thái hoạt ảnh khi rút rìu ra.")]
        public string DrawState = "AxeDraw";
        [Tooltip("Tên trạng thái hoạt ảnh khi cất rìu đi.")]
        public string HideState = "AxeHide";
        [Tooltip("Tên trạng thái hoạt ảnh khi rìu ở trạng thái nghỉ chờ.")]
        public string IdleState = "AxeIdle";

        [Header("Triggers")]
        [Tooltip("Tham số Trigger gọi hoạt ảnh cất rìu.")]
        public string HideTrigger = "Hide";
        [Tooltip("Tham số Trigger gọi hoạt ảnh chém rìu.")]
        public string AttackTrigger = "Attack";

        [Header("Sounds")]
        [Tooltip("Âm thanh phát ra khi rút rìu.")]
        public SoundClip AxeDraw;
        [Tooltip("Âm thanh phát ra khi cất rìu.")]
        public SoundClip AxeHide;
        [Tooltip("Âm thanh phát ra khi chém rìu vào không khí.")]
        public SoundClip AxeSlash;

        private AudioSource audioSource;
        private Coroutine attack;

        private float attackTime;
        private bool isEquipped;
        private bool isBusy;

        public override string Name => "Axe";
        public override bool IsBusy() => !isEquipped || isBusy;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public override void OnUpdate()
        {
            if (!isEquipped || !CanInteract || isBusy)
                return;

            if (attackTime > 0f)
                attackTime -= Time.deltaTime;

            if (InputManager.ReadButtonOnce("Fire", Controls.FIRE) && attackTime <= 0)
            {
                ApplyEffect("Kickback");

                if (attack != null) StopCoroutine(attack);
                audioSource.PlayOneShotSoundClip(AxeSlash);
                attack = StartCoroutine(OnAttack());
                Animator.SetTrigger(AttackTrigger);
                attackTime = NextAttackTime;
            }
        }

        IEnumerator OnAttack()
        {
            yield return new WaitForSeconds(AttackDelay);
            float step = (AttackAngle.RealMax - AttackAngle.RealMin) / (RaycastCount - 1);
            float mid = (AttackAngle.RealMin + AttackAngle.RealMax) / 2f;

            for (int i = 0; i < RaycastCount; i++)
            {
                float angle = AttackAngle.RealMax - (step * i);
                float dir = GameTools.InverseLerp3(AttackAngle.RealMin, mid, AttackAngle.RealMax, angle);
                float distance = Mathf.Lerp(AttackRange.RealMin, AttackRange.RealMax, dir);

                Vector3 upward = PlayerItems.transform.up;
                Vector3 forward = PlayerItems.transform.forward;
                Vector3 direction = Quaternion.AngleAxis(angle, upward) * forward;
                Ray ray = new(PlayerItems.transform.position, direction);

                if (Physics.Raycast(ray, out RaycastHit hit, distance, RaycastMask))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    Vector3 hitPoint = hit.point;

                    bool isFlesh = false;
                    if (hit.collider.TryGetComponent(out IDamagable damagable))
                    {
                        int damage = AttackDamage.Random();
                        damagable.OnApplyDamage(damage, PlayerManager.transform);
                        isFlesh = damagable is NPCBodyPart or IHealthEntity;
                    }

                    SurfaceDefinition surfaceDefinition = isFlesh
                        ? SurfaceDefinitionSet.GetSurface(FleshTag)
                        : SurfaceDefinitionSet.GetSurface(hitObject, hitPoint, SurfaceDetection);

                    if (surfaceDefinition != null)
                    {
                        if (surfaceDefinition.SurfaceMeleemarks.Length > 0)
                        {
                            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                            GameObject hitPrefab = surfaceDefinition.SurfaceMeleemarks.Random();
                            GameObject bulletmark = Instantiate(hitPrefab, hitPoint, hitRotation);
                            bulletmark.transform.SetParent(hit.transform);
                        }

                        if(surfaceDefinition.SurfaceMeleeImpact.Count > 0)
                        {
                            AudioClip audio = surfaceDefinition.SurfaceMeleeImpact.ToArray().Random();
                            AudioSource.PlayClipAtPoint(audio, hitPoint, surfaceDefinition.MeleeImpactVolume);
                        }
                    }

                    ApplyEffect("Hit");
                    break;
                }

                if(ShowAttackGizmos) Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, 1f);
                yield return new WaitForSeconds(RaycastDelay);
            }
        }

        public override void OnItemSelect()
        {
            audioSource.PlayOneShotSoundClip(AxeDraw);
            ItemObject.SetActive(true);
            StartCoroutine(OnShow());
        }

        IEnumerator OnShow()
        {
            yield return new WaitForAnimatorClip(Animator, DrawState);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            audioSource.PlayOneShotSoundClip(AxeHide);
            StopAllCoroutines();
            StartCoroutine(OnHide());
            Animator.SetTrigger(HideTrigger);
            isBusy = true;
        }

        IEnumerator OnHide()
        {
            yield return new WaitForAnimatorClip(Animator, HideState);
            ItemObject.SetActive(false);
            isEquipped = false;
            isBusy = false;
        }

        public override void OnItemActivate()
        {
            StopAllCoroutines();
            ItemObject.SetActive(true);
            Animator.Play(IdleState);
            isEquipped = true;
            isBusy = false;
        }

        public override void OnItemDeactivate()
        {
            StopAllCoroutines();
            ItemObject.SetActive(false);
            isEquipped = false;
            isBusy = false;
        }
    }
}