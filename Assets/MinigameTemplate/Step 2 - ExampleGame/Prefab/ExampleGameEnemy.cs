namespace MinigameTemplate.Example
{
    using Boom.Patterns.Broadcasts;
    using UnityEngine;
    using UnityEngine.AI;

    public class ExampleGameEnemy : MonoBehaviour
    {
        [SerializeField] GameObject rotatingPart;
        [SerializeField] GameObject asset;
        [SerializeField] Collider bodyCollider;

        [SerializeField] float fovAngleChance = 360;
        [SerializeField] float fovAngleAttack = 90;

        [SerializeField] float sightRange = 20;
        [SerializeField] float attackRange = 5;


        [SerializeField, ShowOnly] ConfigurableCharacter configurableCharacter;
        [SerializeField, ShowOnly] MonoDictionaryEvent animatorEventSystem;
        [SerializeField, ShowOnly] Gun gun;

        [SerializeField] ExampleGameManager.ScoreType onDeathPoints;

        private int speedAnimHash;
        private int attackAnimHash;

        private int frameCount = 0;

        [SerializeField, ShowOnly] Transform target;
        [SerializeField, ShowOnly] bool playerInSightRange;
        [SerializeField, ShowOnly] bool playerInAttackRange;
        [SerializeField, ShowOnly] bool attaking;

        private NavMeshAgent agent;
        private HealthComponent healthComponent;

        ExampleGameManager.GameState gameState;


        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            healthComponent = GetComponent<HealthComponent>();

            healthComponent.OnRevive.AddListener(OnReviveHandler);
            healthComponent.OnDeath.AddListener(OnDeathHandler);

            BroadcastState.Register<ExampleGameManager.MinigameState>(StateChangeHandler, new BroadcastState.BroadcastSetting() { invokeOnRegistration = true });

            speedAnimHash = Animator.StringToHash("Speed");
            attackAnimHash = Animator.StringToHash("Action"); //Attack

            if (TryGetComponent<ConfigurableCharacter>(out configurableCharacter) == false)
            {
                Debug.LogError($"Character doesn't not have compoment of type {nameof(ConfigurableCharacter)}");
                return;
            }

            if (asset.TryGetComponent<MonoDictionaryEvent>(out animatorEventSystem) == false)
            {
                Debug.LogError($"Character doesn't not have compoment of type {nameof(MonoDictionaryEvent)}");
                return;
            }

            configurableCharacter.OnSkinChange.AddListener(OnSkinChangeHandler);
        }

        private void Start()
        {
            agent.enabled = true;
        }

        public void SetupEnemy(AnimatorOverrideController animatorOverride, GameObject characterModel, GameObject weaponModel, GameObject reviveVFX, GameObject deathVFX, float health, float speed, Gun.Settings gunSetting)
        {
            var model = Instantiate(characterModel);

            var weapon = Instantiate(weaponModel);

            configurableCharacter.SetupSkin(model);

            configurableCharacter.SetupHandWeapon(weapon, false);

            gun = configurableCharacter.weaponRight.GetComponent<Gun>();

            gun.SetSource(transform);
            gun.UpdateSettings(gunSetting);

            if (TryGetComponent<HealthComponent>(out var healthComponent))
            {
                healthComponent.UpdateSettings(new HealthComponent.HealthSetting() { maxHealth = health, currentHealth = health });
            }

            agent.speed = speed;

            configurableCharacter.SetupEffects(reviveVFX, deathVFX);

            if (animatorOverride != null)
            {
                configurableCharacter.Animator.runtimeAnimatorController = animatorOverride;
            }
        }

        private void OnSkinChangeHandler(GameObject arg0)
        {
            Debug.Log("Agent Register to Anim Events");
            animatorEventSystem.AddListener("onActionStart", StartAttackHandler);
            animatorEventSystem.AddListener("onActionEnd", EndAttackHandler);
        }

        private void OnDestroy()
        {
            healthComponent.OnRevive.RemoveListener(OnReviveHandler);

            healthComponent.OnDeath.RemoveListener(OnDeathHandler);

            BroadcastState.Unregister<ExampleGameManager.MinigameState>(StateChangeHandler);


            animatorEventSystem.RemoveListener("onActionStart", StartAttackHandler);
            animatorEventSystem.RemoveListener("onActionEnd", EndAttackHandler);
        }


        private void StateChangeHandler(ExampleGameManager.MinigameState state)
        {
            gameState = state.gameState;

        }


        private void OnDeathHandler()
        {
            bodyCollider.enabled = false;
            agent.enabled = false;
            asset.SetActive(false);

            configurableCharacter.Animator.enabled = false;

            CoroutineManagerUtil.DelayAction(() =>
            {
                Destroy(gameObject);

            }, 2, transform);

            Broadcast.Invoke(new ExampleGameManager.AddScore(onDeathPoints));
        }

        private void OnReviveHandler()
        {
            agent.enabled = true;
            asset.SetActive(true);
        }


        private void Update()
        {
            ++frameCount;

            if (rotatingPart.gameObject.activeSelf == false) rotatingPart.gameObject.SetActive(true);

            if (configurableCharacter.Animator == null) return;


            if (frameCount % 1 == 0)
            {
                if (gameState != ExampleGameManager.GameState.Playing)
                {
                    configurableCharacter.Animator.SetFloat(speedAnimHash, 0);
                    //if (agent.isOnNavMesh && agent.enabled) agent.isStopped = true;
                    return;
                }

                configurableCharacter.Animator.SetFloat(speedAnimHash, Vector3.Distance(transform.position, agent.destination) > 0.25f && !playerInAttackRange ? 1 : 0);


                target = RuntimeSet.FindClosest(RuntimeSet.Group.Player, RuntimeSet.Channel.A, transform, sightRange, fovAngleChance);

                playerInSightRange = target;

                if (!playerInSightRange)
                {
                    return;
                }

                playerInAttackRange = target.IsInView(transform, attackRange, fovAngleAttack);

                if (agent.enabled == false) return;

                if (!playerInAttackRange)
                {
                    if (!attaking) SetDestination(target);
                }
                else
                {
                    agent.velocity = Vector3.zero;

                    DoAttack();
                    transform.rotation = transform.rotation.RotateSlerp((target.position - transform.position).normalized, 5);
                }
            }
        }


        private void SetDestination(Transform target)
        {
            agent.SetDestination(target.position);
        }

        private void DoAttack()
        {
            if (attaking || gun.CanFire() == false) return;
            attaking = true;
            configurableCharacter.Animator.SetBool(attackAnimHash, true);
        }

        private void StartAttackHandler()
        {
            gun.Fire();
        }
        private void EndAttackHandler()
        {
            attaking = false;
            configurableCharacter.Animator.SetBool(attackAnimHash, false);
        }



        private void OnDrawGizmos()
        {
            if (agent == null) return;
            if (agent.enabled == false) return;
            if (gun == null) return;

            // Set up the gizmo color for field of view visualization
            Gizmos.color = Color.yellow;
            // Draw the field of view using a wire sphere
            Gizmos.DrawWireSphere(transform.position, sightRange);
            // Draw the field of view angle using lines
            DrawFieldOfView(sightRange, fovAngleChance, Color.yellow);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            DrawFieldOfView(attackRange, fovAngleAttack, Color.blue);
        }

        void DrawFieldOfView(float range, float fovAngle, Color color)
        {
            Vector3 fovLine1 = Quaternion.Euler(0, fovAngle / 2, 0) * transform.forward * range;
            Vector3 fovLine2 = Quaternion.Euler(0, -fovAngle / 2, 0) * transform.forward * range;

            Gizmos.color = color;
            Gizmos.DrawRay(transform.position, fovLine1);
            Gizmos.DrawRay(transform.position, fovLine2);
        }
    }
}
