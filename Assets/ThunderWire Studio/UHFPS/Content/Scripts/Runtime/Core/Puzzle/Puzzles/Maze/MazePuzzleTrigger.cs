using UnityEngine;

namespace UHFPS.Runtime
{
    public class MazePuzzleTrigger : MonoBehaviour, IInteractStart
    {
        public MazePuzzle.TriggerType TriggerType = MazePuzzle.TriggerType.PutBall;

        private MazePuzzle mazePuzzle;

        private void Awake()
        {
            mazePuzzle = GetComponentInParent<MazePuzzle>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (mazePuzzle == null || TriggerType != MazePuzzle.TriggerType.WrongHole
                && TriggerType != MazePuzzle.TriggerType.FinishHole)
                return;

            if (other.TryGetComponent<MazePuzzleBall>(out _))
                mazePuzzle.OnMazeTrigger(TriggerType);
        }

        private void OnDrawGizmosSelected()
        {
            if (TriggerType != MazePuzzle.TriggerType.WrongHole && TriggerType != MazePuzzle.TriggerType.FinishHole)
                return;

            if (TryGetComponent(out BoxCollider box)) 
            {
                Color color = TriggerType == MazePuzzle.TriggerType.WrongHole
                    ? Color.red : Color.green;

                color.a = 0.2f;
                Gizmos.color = color;
                Gizmos.DrawCube(transform.position, box.size);
            }
        }

        public void InteractStart()
        {
            mazePuzzle.OnMazeTrigger(TriggerType);
        }
    }
}