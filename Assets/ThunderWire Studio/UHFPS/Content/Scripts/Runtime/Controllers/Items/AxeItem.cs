using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Scriptable;
using static UHFPS.Scriptable.SurfaceDefinitionSet;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class AxeItem : PlayerItemBehaviour
    {
        public SurfaceDefinitionSet SurfaceDefinitionSet;
        public SurfaceDetection SurfaceDetection;
        public Tag FleshTag;

        public LayerMask RaycastMask;
        public MinMax AttackAngle;
        public MinMax AttackRange;
        public uint RaycastCount = 11;
        public float AttackDelay;
        public float RaycastDelay;
        public bool ShowAttackGizmos;

        public MinMaxInt AttackDamage;
        public float NextAttackTime;

        public string DrawState = "AxeDraw";
        public string HideState = "AxeHide";
        public string IdleState = "AxeIdle";

        public string HideTrigger = "Hide";
        public string AttackTrigger = "Attack";

        public SoundClip AxeDraw;
        public SoundClip AxeHide;
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