namespace MinigameTemplate.Example
{
    using Boom.Patterns.Broadcasts;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;

    public class Gun : MonoBehaviour
    {
        [System.Serializable]
        public struct BurstSetting
        {
            public int burstCount;
            public float burstRate;

            public BurstSetting(int burstCount, float burstRate)
            {
                this.burstCount = burstCount;
                this.burstRate = burstRate;
            }
        }
        [System.Serializable]
        public struct Settings
        {
            public GameObject bulletPrefab;

            public float damage;
            public float speed;
            public float fireRate;
            public float spread;
            public ToggableValue<BurstSetting> burstSetting;

            public Settings(GameObject bulletPrefab, float damage, float speed, float fireRate, float spread, ToggableValue<BurstSetting> burstSetting)
            {
                this.bulletPrefab = bulletPrefab;
                this.damage = damage;
                this.speed = speed;
                this.fireRate = fireRate;
                this.spread = spread;
                this.burstSetting = burstSetting;
            }
        }



        [SerializeField] Transform spawnpoint;
        [SerializeField] Settings settings = new(null, 5, 50, 1, 0, default);

        public UnityEvent onReloadStateChange;
        public UnityEvent onReloadComplete;

        [SerializeField, ShowOnly] Transform source;
        [SerializeField, ShowOnly] bool isFiring;

        Coroutine fireRoutine;

        private void Awake()
        {
            BroadcastState.Register<ExampleGameManager.MinigameState>(StateChangeHandler);
        }

        private void OnDestroy()
        {
            BroadcastState.Unregister<ExampleGameManager.MinigameState>(StateChangeHandler);

            StopAllCoroutines();
        }

        private void OnEnable()
        {
        }

        private void StateChangeHandler(ExampleGameManager.MinigameState state)
        {
            if (state.gameState == ExampleGameManager.GameState.GameOverSuccess || state.gameState == ExampleGameManager.GameState.GameOverFailure)
            {
                if (fireRoutine != null) CoroutineManager.Instance.StopCoroutine(fireRoutine);
                isFiring = false;
            }
        }

        public void SetSource(Transform source)
        {
            this.source = source;
        }

        public bool CanFire()
        {
            return isFiring == false;
        }
        public bool Fire()
        {
            if (isFiring) return false;

            fireRoutine = CoroutineManager.Instance.StartCoroutine(FireRoutine());

            return true;
        }

        IEnumerator FireRoutine()
        {
            isFiring = true;

            int burstCount = settings.burstSetting.Value.burstCount;

            bool validBurstRate = burstCount > 1;

            if (settings.burstSetting.Enabled && validBurstRate)
            {
                int currentBurstCount = burstCount;
                float burstRate = settings.burstSetting.Value.burstRate;

                while (currentBurstCount > 0)
                {
                    SpawnProjectile();

                    --currentBurstCount;
                    yield return new WaitForSeconds(burstRate);
                }
            }
            else
            {
                SpawnProjectile();
            }

            yield return new WaitForSeconds(settings.fireRate);


            isFiring = false;
        }

        private void SpawnProjectile()
        {
            var projectile = Instantiate(settings.bulletPrefab);

            if (projectile.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.SetupBullet(spawnpoint.position, GenerateDirection(spawnpoint.forward), settings.speed, settings.damage, source);
            }
        }

        private Vector3 GenerateDirection(Vector3 desiredDirection)
        {
            var spread = spawnpoint.rotation * (Random.insideUnitCircle * settings.spread) + desiredDirection;
            spread.Normalize();

            return spread;
        }

        public void UpdateSettings(Settings newSettings)
        {
            settings = newSettings;
        }
    }
}