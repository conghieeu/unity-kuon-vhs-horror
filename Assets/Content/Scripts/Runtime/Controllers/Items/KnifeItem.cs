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
    [Summary("Điều khiển vật phẩm Dao găm (Pocket Knife), cho phép người chơi chém và gây sát thương cận chiến.")]
    public class KnifeItem : PlayerItemBehaviour
    {
        [Header("Surface Settings")]
        [Tooltip("Tập hợp định nghĩa bề mặt khi bị dao chém trúng.")]
        public SurfaceDefinitionSet SurfaceDefinitionSet;
        [Tooltip("Phương pháp phát hiện bề mặt khi chém trúng.")]
        public SurfaceDetection SurfaceDetection;
        [Tooltip("Thẻ (Tag) nhận diện bề mặt da thịt (kẻ địch).")]
        public Tag FleshTag;

        [Header("Attack Setup")]
        [Tooltip("Lớp mạng (LayerMask) bị dao chém trúng.")]
        public LayerMask RaycastMask;
        [Tooltip("Góc độ chém của dao.")]
        public MinMax AttackAngle;
        [Tooltip("Tầm đánh của dao (Khoảng cách từ người chơi tới mục tiêu).")]
        public MinMax AttackRange;
        [Tooltip("Số lượng tia (Raycast) bắn ra để phát hiện va chạm trong một lần chém.")]
        public uint RaycastCount = 10;
        [Tooltip("Thời gian trễ giữa mỗi tia va chạm bắn ra.")]
        public float RaycastDelay;
        [Tooltip("Hiển thị đường chém (Gizmos) trong Editor.")]
        public bool ShowAttackGizmos;

        [Header("Damage")]
        [Tooltip("Sát thương gây ra ngẫu nhiên (Min-Max) mỗi lần chém trúng.")]
        public MinMaxInt AttackDamage;
        [Tooltip("Thời gian chờ tối thiểu giữa mỗi lần chém.")]
        public float NextAttackDelay;
        [Range(0f, 1f)]
        [Tooltip("Độ trễ (bù trừ) khi kết thúc hoạt ảnh chém.")]
        public float AttackTimeOffset = 0f;

        [Header("Animations")]
        [Tooltip("Trạng thái hoạt ảnh rút dao ra.")]
        public string DrawState = "KnifeDraw";
        [Tooltip("Trạng thái hoạt ảnh cất dao đi.")]
        public string HideState = "KnifeHide";
        [Tooltip("Trạng thái hoạt ảnh khi dao ở trạng thái nghỉ.")]
        public string IdleState = "KnifeIdle";

        [Tooltip("Trạng thái hoạt ảnh chém từ phải sang trái.")]
        public string SlashRState = "KnifeSlash_R";
        [Tooltip("Trạng thái hoạt ảnh chém từ trái sang phải.")]
        public string SlashLState = "KnifeSlash_L";

        [Header("Triggers")]
        [Tooltip("Tham số Boolean điều khiển trạng thái tấn công.")]
        public string AttackBool = "Attack";
        [Tooltip("Tham số Trigger gọi hoạt ảnh chém dao.")]
        public string SlashTrigger = "Slash";
        [Tooltip("Tham số Trigger gọi hoạt ảnh cất dao.")]
        public string HideTrigger = "Hide";

        [Header("Sounds")]
        [Tooltip("Âm thanh khi rút dao.")]
        public SoundClip KnifeDraw;
        [Tooltip("Âm thanh khi cất dao.")]
        public SoundClip KnifeHide;
        [Tooltip("Âm thanh khi chém dao vào không khí.")]
        public SoundClip KnifeSlash;

        private AudioSource audioSource;
        private Coroutine attack;

        private float attackTime;
        private bool isAttack;
        private bool isAttackEnd;

        private bool isEquipped;
        private bool isBusy;

        public override string Name => "Pocket Knife";

        public override bool IsBusy() => !isEquipped || isBusy;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public override void OnUpdate()
        {
            if (!isEquipped || isBusy)
                return;

            if (isAttack && isAttackEnd && attackTime <= 0)
            {
                Animator.SetTrigger(SlashTrigger);
                isAttackEnd = false;
            }
            else if (attackTime > 0f)
            {
                attackTime -= Time.deltaTime;
            }

            if (!CanInteract)
                return;

            if (InputManager.ReadButton(Controls.FIRE))
            {
                if (!isAttack && attackTime <= 0)
                {
                    Animator.SetBool(AttackBool, true);
                    isAttackEnd = false;
                    isAttack = true;
                }
            }
            else
            {
                Animator.SetBool(AttackBool, false);
                isAttack = false;
            }
        }

        /// <summary>
        /// Called from the animation event.
        /// </summary>
        public void Slash(bool isLeftSlash)
        {
            attackTime = NextAttackDelay;
            audioSource.PlayOneShotSoundClip(KnifeSlash);
            Animator.ResetTrigger(SlashTrigger);

            if (attack != null) StopCoroutine(attack);
            attack = StartCoroutine(OnAttack(isLeftSlash));

            ApplyEffect(isLeftSlash ? "SlashL" : "SlashR");
        }




        IEnumerator OnAttack(bool isLeftSlash)
        {
            float step = (AttackAngle.RealMax - AttackAngle.RealMin) / (RaycastCount - 1);
            float mid = (AttackAngle.RealMin + AttackAngle.RealMax) / 2f;

            for (int i = 0; i < RaycastCount; i++)
            {
                float angle = isLeftSlash 
                    ? AttackAngle.RealMin + (step * i)
                    : AttackAngle.RealMax - (step * i);

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

                        if (surfaceDefinition.SurfaceMeleeImpact.Count > 0)
                        {
                            AudioClip audio = surfaceDefinition.SurfaceMeleeImpact.ToArray().Random();
                            AudioSource.PlayClipAtPoint(audio, hitPoint, surfaceDefinition.MeleeImpactVolume);
                        }
                    }

                    ApplyEffect("Hit");
                    break;
                }

                if (ShowAttackGizmos) Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, 1f);
                yield return new WaitForSeconds(RaycastDelay);
            }

            yield return new WaitForAnimatorStateEnd(Animator, isLeftSlash ? SlashLState : SlashRState, AttackTimeOffset);
            isAttackEnd = true;
        }

        public override void OnItemSelect()
        {
            ItemObject.SetActive(true);
            StartCoroutine(OnShow());
            audioSource.PlayOneShotSoundClip(KnifeDraw);
        }

        IEnumerator OnShow()
        {
            yield return new WaitForAnimatorClip(Animator, DrawState);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            StopAllCoroutines();
            StartCoroutine(OnHide());
            audioSource.PlayOneShotSoundClip(KnifeHide);

            Animator.SetTrigger(HideTrigger);
            Animator.ResetTrigger(SlashTrigger);
            Animator.SetBool(AttackBool, false);

            attackTime = 0f;
            isAttackEnd = false;
            isAttack = false;
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