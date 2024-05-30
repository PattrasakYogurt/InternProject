namespace MinigameTemplate.Example
{
    using UnityEngine;

    public class MonoAnchor : MonoBehaviour
    {
        public enum UpdateType { Update, FixedUpdate, LateUpdate }

        [SerializeField] Transform source;
        [SerializeField] Vector3 offset;
        [SerializeField] UpdateType updateType = UpdateType.Update;

        [SerializeField] bool smoothing;
        [SerializeField] float smoothSpeed = 0.125f;

        private void Start()
        {
            transform.position = source.position + offset;
        }

        private void Update()
        {
            if (updateType != UpdateType.Update) return;
            MovementHandler();
        }
        private void FixedUpdate()
        {
            if (updateType != UpdateType.FixedUpdate) return;
            MovementHandler();
        }
        private void LateUpdate()
        {
            if (updateType != UpdateType.LateUpdate) return;
            MovementHandler();
        }

        private void MovementHandler()
        {
            if (source)
            {
                var nextPosition = source.position + offset;
                var movementInputSqrMagnitud = Vector3.Distance(transform.position.Y(0), nextPosition.Y(0));
                var moveTarget = movementInputSqrMagnitud > 0.01f;

                if (smoothing)
                {
                    transform.position = moveTarget ? Vector3.Lerp(transform.position, nextPosition, smoothSpeed) : transform.position;
                }
                else transform.position = moveTarget ? nextPosition : transform.position;
            }
        }
    }
}