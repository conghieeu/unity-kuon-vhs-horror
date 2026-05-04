using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Scriptable;
using static UHFPS.Scriptable.SurfaceDefinitionSet;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class KnifeItem : PlayerItemBehaviour
    {
        public SurfaceDefinitionSet SurfaceDefinitionSet;
        public SurfaceDetection SurfaceDetection;
        public Tag FleshTag;

        public LayerMask RaycastMask;
        public MinMax AttackAngle;
        public MinMax AttackRange;
        public uint RaycastCount = 10;
        public float RaycastDelay;
        public bool ShowAttackGizmos;

        public MinMaxInt AttackDamage;
        public float NextAttackDelay;
        [Range(0f, 1f)]
        public float AttackTimeOffset = 0f;

        public string DrawState = "KnifeDraw";
        public string HideState = "KnifeHide";
        public string IdleState = "KnifeIdle";

        public string SlashRState = "KnifeSlash_R";
        public string SlashLState = "KnifeSlash_L";

        public string AttackBool = "Attack";
        public string SlashTrigger = "Slash";
        public string HideTrigger = "Hide";

        public SoundClip KnifeDraw;
        public SoundClip KnifeHide;
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