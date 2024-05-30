namespace MinigameTemplate.Example
{
    using UnityEngine;

    public class Bullet : MonoBehaviour
    {
        [SerializeField, ShowOnly] float damage;
        [SerializeField, ShowOnly] float speed;
        [SerializeField, ShowOnly] Transform source;
        [SerializeField] RuntimeSet.Path sourceTeam_;
        RuntimeSet.Path? sourceTeam;

        [SerializeField] GameObject particleEffectSpawn;
        [SerializeField] GameObject particleEffectHitTarget;
        [SerializeField] GameObject particleEffectHitAny;

        bool consumed;
        private void Awake()
        {
            GetComponent<Rigidbody>();
        }

        public void SetupBullet(Vector3 position, Vector3 direction, float speed, float damage, Transform source)
        {
            transform.position = position;
            transform.forward = direction;

            this.damage = damage;
            this.speed = speed;
            this.source = source;

            if (RuntimeSet.TryGetPathsByGroup(source, RuntimeSet.Group.Team, out var paths))
            {
                this.sourceTeam_ = paths.First.Value;
                this.sourceTeam = paths.First.Value;
            }

            if (TryGetComponent<Rigidbody>(out var rigidbody))
            {
                rigidbody.AddForce(transform.forward * this.speed);
            }

            var effect = Instantiate(particleEffectSpawn);

            effect.transform.forward = transform.forward;
            effect.transform.position = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (consumed) return;

            var target = other.transform;

            if (target == source) return;

            if (RuntimeSet.Contain(target) == false)
            {
                Kill(particleEffectHitAny);
                return;
            }

            if (sourceTeam.HasValue)
            {
                if (RuntimeSet.ContainPath(target, sourceTeam.Value)) return;
            }

            if (target.TryGetComponent<HealthComponent>(out var healthComponent) == false) return;

            healthComponent.TakeDamage(damage);

            consumed = true;

            Kill(particleEffectHitTarget);
        }

        private void Kill(GameObject particleEffect)
        {
            var effect = Instantiate(particleEffect);

            effect.transform.position = transform.position;

            Destroy(gameObject);
        }
    }
}