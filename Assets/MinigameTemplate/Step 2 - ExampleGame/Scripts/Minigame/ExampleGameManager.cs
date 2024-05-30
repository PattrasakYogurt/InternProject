namespace MinigameTemplate.Example
{
    using Boom.Utility;
    using UnityEngine;

    using Boom;
    using System;
    using Boom.Patterns.Broadcasts;
    using System.Collections;
    using System.Collections.Generic;

    [System.Serializable]
    public class ExampleMinigameConfig
    {
        public string name;
        public string coverImage;
        public string description;
        public string sceneName;

        public string missionTitle;
        public string missionLogo;

        public int missionGoal;
        public float missionDurationSeconds;

        public int playerHealth;
        public string playerModel;
        public string playerWeaponModel;

        public string missionControlAction;

        public ExampleMinigameConfig() { }
        public ExampleMinigameConfig(Dictionary<string, string> mainConfig_)
        {

            //Name

            if (mainConfig_.TryGetValue("name", out string fieldTextValue) == false)
            {
                fieldTextValue = "Minigame";
            }

            name = fieldTextValue;


            //URL

            mainConfig_.TryGetValue("cover_image", out fieldTextValue);

            coverImage = fieldTextValue;


            //Description

            if (mainConfig_.TryGetValue("description", out fieldTextValue) == false)
            {
                fieldTextValue = "";
            }

            description = fieldTextValue;


            //SceneName

            if (mainConfig_.TryGetValue("scene_name", out fieldTextValue) == false)
            {
                fieldTextValue = "";
            }

            sceneName = fieldTextValue;

            //GoalTitle

            if (mainConfig_.TryGetValue("mission_title", out fieldTextValue) == false)
            {
                fieldTextValue = "Get 20 points";
            }

            missionTitle = fieldTextValue;


            //GoalScore

            if (mainConfig_.TryGetValue("mission_goal", out fieldTextValue) == false)
            {
                fieldTextValue = "20";
            }

            if (int.TryParse(fieldTextValue, out missionGoal) == false)
            {
                missionGoal = 20;
            }

            //MissionLogo

            if (mainConfig_.TryGetValue("mission_logo", out fieldTextValue) == false)
            {
                fieldTextValue = "MissionLogoKill";
            }

            missionLogo = fieldTextValue;

            //MissionControl

            if (mainConfig_.TryGetValue("mission_control_action", out fieldTextValue) == false)
            {
                fieldTextValue = "USE WASD OR ARROWS <color=green>TO MOVE</color>\nPRESS SPACEBAR TO <color=green>ATTACK</color> ENEMIES";
            }

            missionControlAction = fieldTextValue;

            //DurationSeconds

            if (mainConfig_.TryGetValue("mission_duration_seconds", out fieldTextValue) == false)
            {
                fieldTextValue = "180.0";
            }

            if (float.TryParse(fieldTextValue, out missionDurationSeconds) == false)
            {
                missionDurationSeconds = 180;
            }

            //PlayerHealth

            if (mainConfig_.TryGetValue("player_health", out fieldTextValue) == false)
            {
                fieldTextValue = "260";
            }

            if (int.TryParse(fieldTextValue, out playerHealth) == false)
            {
                playerHealth = 260;
            }


            //PlayerModel

            if (mainConfig_.TryGetValue("player_model", out fieldTextValue) == false)
            {
                fieldTextValue = "SkinMoonwalkerDefault";
            }

            playerModel = fieldTextValue;


            //PlayerWeaponModel

            if (mainConfig_.TryGetValue("player_weapon_model", out fieldTextValue) == false)
            {
                fieldTextValue = "Hammer";
            }

            playerWeaponModel = fieldTextValue;
        }

        public ExampleMinigameConfig(string name, string coverImage, string description, string sceneName, string missionTitle, string missionLogo, int missionGoal, float missionDurationSeconds, int playerHealth, string playerModel, string playerWeaponModel, string missionControlAction)
        {
            this.name = name;
            this.coverImage = coverImage;
            this.description = description;
            this.sceneName = sceneName;
            this.missionTitle = missionTitle;
            this.missionLogo = missionLogo;
            this.missionGoal = missionGoal;
            this.missionDurationSeconds = missionDurationSeconds;
            this.playerHealth = playerHealth;
            this.playerModel = playerModel;
            this.playerWeaponModel = playerWeaponModel;
            this.missionControlAction = missionControlAction;
        }
    }
    public class ExampleGameManager : MonoBehaviour
    {
        #region Types

        public enum GameState
        {
            None,
            Initiate,
            Initiating,
            Playing,
            Paused,
            GameOverSuccess,
            GameOverFailure,
            Quit
        }
        public enum GameplayDurationState
        {
            Initiating,
            Depleting,
        }
        public enum ScoreType
        {
            x1 = 1,
            x2 = 2,
            x3 = 3,
            x5 = 5,
            x10 = 10,
            x15 = 15,
            x25 = 25,
            x50 = 50,
            x100 = 100,
        }

        public struct AddScore : IBroadcast
        {
            public float score;

            public AddScore(ScoreType score)
            {
                this.score = (int)score;
            }
            public AddScore(float score)
            {
                this.score = score;
            }
        }
        public struct ScoreState : IBroadcastState
        {
            public float missionGoal;
            public float missionGoalProgress;

            public ScoreState(float missionGoalProgress, float missionGoal)
            {
                this.missionGoalProgress = missionGoalProgress;
                this.missionGoal = missionGoal;
            }

            public int MaxSavedStatesCount()
            {
                return 0;
            }
        }
        public struct MinigameState : IBroadcastState
        {
            public GameState gameState;
            public int countDown;
            public double? reward;

            public MinigameState(GameState gameState, int countDown, double? reward = null)
            {
                this.gameState = gameState;
                this.countDown = countDown;
                this.reward = reward;
            }

            public int MaxSavedStatesCount()
            {
                return 0;
            }
        }
        public struct GameplayTimeLeft : IBroadcast
        {
            public float currentTimeLeft;
            public GameplayDurationState state;

            public GameplayTimeLeft(float currentDuration, GameplayDurationState gameplayDurationState)
            {
                this.currentTimeLeft = currentDuration;
                this.state = gameplayDurationState;
            }
        }

        #endregion

        [SerializeField] GameObject playerModel, enemyModel1, enemyModel2, reviveVFX, deathVFX, weaponPrefab, bulletPrefab;

        protected ExampleMinigameConfig config = null;

        public int missionGoal = 2;
        public float missionDurationSeconds = 180;

        [SerializeField, ShowOnly] public GameState gameState;

        [SerializeField, ShowOnly] int currentStartCountdownDuration = 0;

        [SerializeField, ShowOnly] protected Transform player;

        [field: SerializeField] public static bool MinigameManagerExist { get; private set; }

        private bool isTimerInitialized = false;
        private GameState prevGameState;

        //

        [SerializeField] ExampleGameEnemy enemyRangeFollow;
        [SerializeField] AnimatorOverrideController enemyRangeFollowAnimatorOverride;

        [SerializeField, ShowOnly] Gun playerGun;

        [SerializeField, ShowOnly] private ExampleGamePlayerController playerControllerTopdown;
        [SerializeField, ShowOnly] private HealthComponent playerHealthComponent;
        [SerializeField] private AnimatorOverrideController playerAnimatorOverride;

        [SerializeField] ExampleGameplayPanel exampleGameplayPanel;
        [SerializeField] GameObject successPanel;
        [SerializeField] GameObject failurePanel;
        [SerializeField] GameObject pausePanel;


        #region WIN & LOSE
        protected void UserWonMatch()
        {
            if (gameState != GameState.Playing) return;


            ActionUtil.Guilds.UserWonMatch();

            gameState = GameState.GameOverSuccess;
            BroadcastState.Invoke(new MinigameState(gameState, currentStartCountdownDuration));

            successPanel.SetActive(true);
        }
        protected void UserLostMatch()
        {
            if (gameState != GameState.Playing) return;

            ActionUtil.Guilds.UserLostMatch();

            gameState = GameState.GameOverFailure;
            BroadcastState.Invoke(new MinigameState(gameState, currentStartCountdownDuration));

            failurePanel.SetActive(true);
        }
        #endregion

        #region MONO
        private void Awake()
        {
            InputMaster.RegisterToKeyPress(InputMaster.InputType.Menu, OnPauseHandler);

            Broadcast.Register<AddScore>(AddScoreHandler);
            BroadcastState.Register<ScoreState>(ScoreStateChangeHandler);
            BroadcastState.Register<MinigameState>(StateChangeHandler);

            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginStateChangeHandler);
        }

        private void Start()
        {
            MinigameManagerExist = true;

            Time.timeScale = 1;

            //SET UP GAMEPLAY CONFIGS
            config = new(
                "Coin Blast",
                "CoverImageMinigame3",
                "Defeat Pepe and Doge",
                "SceneMinigame_3",
                "<size=110%><color=#FA27FF><i>DEFEAT</i> <color=\"white\"><size=80%>PEPE AND DOGE",
                "MissionLogoKill",
                2,
                360,
                390,
                "SkinMoonwalkerDefault",
                "CannonBall",
                "USE WASD OR ARROWS <color=green>TO MOVE</color>\nPRESS SPACEBAR TO <color=green>ATTACK</color> ENEMIES"
                );

            BroadcastState.Invoke(new ScoreState(0, missionGoal));

            gameState = GameState.None;
            BroadcastState.Invoke(new MinigameState(gameState, currentStartCountdownDuration));

            //FIND PLAYER ON SCENE

            player = RuntimeSet.FindFirst(RuntimeSet.Group.Player, RuntimeSet.Channel.A);

            if (player == null)
            {
                $"Could not find player!".Error();
                return;
            }

            PlayerFoundHandler(player);

            SpawnEnemies();
        }
        protected void OnDestroy()
        {
            InputMaster.RegisterToKeyPress(InputMaster.InputType.Menu, OnPauseHandler);

            Broadcast.Register<AddScore>(AddScoreHandler);
            BroadcastState.Register<ScoreState>(ScoreStateChangeHandler);
            BroadcastState.Register<MinigameState>(StateChangeHandler);

            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginStateChangeHandler);

            //

            if (playerControllerTopdown)
            {
                playerControllerTopdown.OnStartActionHandler.AddListener(OnPlayerActionStartHandler);
            }

            if (playerHealthComponent)
            {
                playerHealthComponent.OnDeath.RemoveListener(UserLostMatch);
            }
        }

        protected virtual void Update()
        {
            if (isTimerInitialized == false)
            {
                isTimerInitialized = true;

                Broadcast.Invoke(new GameplayTimeLeft(missionDurationSeconds, GameplayDurationState.Initiating));
            }

            if (gameState != GameState.Playing) return;

            TryDepleteDuration();
        }
        #endregion

        #region PLAYER SET UP

        protected void PlayerFoundHandler(Transform player)
        {
            playerControllerTopdown = player.GetComponent<ExampleGamePlayerController>();

            if (playerControllerTopdown == null)
            {
                $"Could not find component: {nameof(ExampleGamePlayerController)} on the player!".Error();
                return;
            }

            playerControllerTopdown.OnStartActionHandler.AddListener(OnPlayerActionStartHandler);


            playerHealthComponent = player.GetComponent<HealthComponent>();

            if (playerHealthComponent == null)
            {
                $"Could not find component: {nameof(HealthComponent)} on the player!".Error();
                return;
            }

            playerHealthComponent.OnDeath.AddListener(UserLostMatch);
            playerHealthComponent.UpdateSettings(new HealthComponent.HealthSetting() { maxHealth = config.playerHealth, currentHealth = config.playerHealth });

            SetupPlayerModelAndWeaponModel(player, SetupWeapon);

            SetupPlayerPosition(player);
        }

        private void SetupPlayerModelAndWeaponModel(Transform player, Action<Transform> onComplete = null)
        {
            var _playerModel = Instantiate(playerModel);

            if (player.TryGetComponent<ConfigurableCharacter>(out var configurableCharacter) == false)
            {
                $"Failure to find component of name: {nameof(ConfigurableCharacter)}".Error();

                return;
            }

            configurableCharacter.SetupSkin(_playerModel);
            configurableCharacter.SetupEffects(reviveVFX, deathVFX);

            if (playerAnimatorOverride != null)
            {
                Debug.Log("Override Player Animator");
                configurableCharacter.Animator.runtimeAnimatorController = playerAnimatorOverride;
            }

            var _weaponPrefab = Instantiate(weaponPrefab);
            configurableCharacter.SetupHandWeapon(_weaponPrefab, false);

            onComplete?.Invoke(player);
        }

        private static void SetupPlayerPosition(Transform player)
        {
            var spawnPoint = RuntimeSet.GetRandom(RuntimeSet.Group.PlayerSpawnpoint, RuntimeSet.Channel.A, e => true);
            spawnPoint.gameObject.SetActive(false);

            player.position = spawnPoint.position;
            player.forward = Vector3.back;
        }

        private void SetupWeapon(Transform player)
        {
            var projectileDamage = 10f;
            var projectileSpeed = 400f;
            var gunFireRate = 0.5f;
            var gunSpread = 0f;
            var burstCount = 0;
            var burstRate = 0f;

            if (player.TryGetComponent<ConfigurableCharacter>(out var configurableCharacter))
            {
                if (configurableCharacter.weaponRight)
                {
                    playerGun = configurableCharacter.weaponRight.GetComponent<Gun>();
                    playerGun.SetSource(player.transform);

                    playerGun.UpdateSettings(new(bulletPrefab, projectileDamage, projectileSpeed, gunFireRate, gunSpread, new(new(burstCount, burstRate), burstCount > 0)));
                }
                else
                {
                    Debug.LogError("Player doesn't have a bazooka on the rigth hand");
                }
            }
            else
            {
                Debug.LogError($"Player doesn't have a {nameof(ConfigurableCharacter)} component");

            }
        }
        #endregion

        #region ENEMIES SET UP

        private void SpawnEnemies()
        {
            var newEnemy1 = Instantiate(enemyRangeFollow);

            var spawnPoint = RuntimeSet.GetRandom(RuntimeSet.Group.PlayerSpawnpoint, RuntimeSet.Channel.A, e => true);
            spawnPoint.gameObject.SetActive(false);

            SetupEnemy(newEnemy1, spawnPoint, enemyModel1);

            var newEnemy2 = Instantiate(enemyRangeFollow);

            spawnPoint = RuntimeSet.GetRandom(RuntimeSet.Group.PlayerSpawnpoint, RuntimeSet.Channel.A, e => true);
            spawnPoint.gameObject.SetActive(false);
            SetupEnemy(newEnemy2, spawnPoint, enemyModel2);
        }

        private void SetupEnemy(ExampleGameEnemy newEnemy, Transform enemySpawnPoint, GameObject characterModel)
        {
            var health = 45;
            var speed = 3.5f;

            var projectileDamage = 15f;
            var projectileSpeed = 400;
            var gunFireRate = 1f;
            var gunSpread = 0.125f;

            var burstCount = 0;
            var burstRate = 0f;

            newEnemy.transform.position = enemySpawnPoint.transform.position;
            newEnemy.transform.forward = Vector3.back;

            if (newEnemy.TryGetComponent<ExampleGameEnemy>(out var agentComp))
            {
                agentComp.SetupEnemy(enemyRangeFollowAnimatorOverride, characterModel, weaponPrefab, reviveVFX, deathVFX, health, speed, new(bulletPrefab, projectileDamage, projectileSpeed, gunFireRate, gunSpread, new(new(burstCount, burstRate), burstCount > 0)));
            }
        }


        #endregion

        #region START GAME

        private void LoginStateChangeHandler(MainDataTypes.LoginData data)
        {
            if (data.state != MainDataTypes.LoginData.State.LoggedIn) return;

            exampleGameplayPanel.Setup(new ExampleGameplayPanel.WindowData(config.missionTitle, config.missionGoal, config.missionControlAction, config.missionLogo));

            BroadcastState.Invoke(new MinigameState(GameState.None, currentStartCountdownDuration));
            StartGame();
        }

        private void StartGame()
        {
            if (gameState != GameState.None) return;

            StartCoroutine(StartGameRoutine());
        }

        IEnumerator StartGameRoutine()
        {
            yield return new WaitForSeconds(3f);

            gameState = GameState.Initiate;

            BroadcastState.Invoke(new MinigameState(gameState, currentStartCountdownDuration));

            yield return new WaitForSeconds(.5f);

            gameState = GameState.Initiating;

            currentStartCountdownDuration = 3;

            BroadcastState.Invoke(new MinigameState(gameState, currentStartCountdownDuration));

            yield return new WaitForSeconds(1.25f);

            currentStartCountdownDuration = 2;

            BroadcastState.Invoke(new MinigameState(gameState, currentStartCountdownDuration));

            yield return new WaitForSeconds(1.25f);

            currentStartCountdownDuration = 1;

            BroadcastState.Invoke(new MinigameState(gameState, currentStartCountdownDuration));

            yield return new WaitForSeconds(1.25f);

            gameState = GameState.Playing;

            BroadcastState.Invoke(new MinigameState(gameState, currentStartCountdownDuration));

            currentStartCountdownDuration = 0;
        }

        #endregion


        #region STATE CHANGE
        private void StateChangeHandler(MinigameState state)
        {
            if (state.gameState == GameState.Initiating)
            {
                Debug.Log("*** Initiating: " + state.countDown);
            }
            else if (state.gameState == GameState.Playing)
            {
                Debug.Log("*** Playing!");

            }
            else if (state.gameState == GameState.GameOverSuccess)
            {
                Debug.Log("*** You Won!");

            }
            else if (state.gameState == GameState.GameOverFailure)
            {
                Debug.Log("*** You Lost!");
            }

            prevGameState = state.gameState;
        }

        private void AddScoreHandler(AddScore score)
        {
            BroadcastState.Invoke<ScoreState>(e => new ScoreState(e.missionGoalProgress + (int)score.score, missionGoal));
        }

        private void ScoreStateChangeHandler(ScoreState state)
        {
            if (state.missionGoalProgress < missionGoal) return;

            UserWonMatch();
        }

        #endregion

        public void OnPauseHandler()
        {
            if (pausePanel.activeSelf)
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else
            {
                if (gameState != GameState.Playing) return;

                pausePanel.SetActive(true);
                Time.timeScale = 0.0f;
            }
        }

        private void TryDepleteDuration()
        {
            float _newCurrentGameplayDuration = missionDurationSeconds - Time.deltaTime;
            if (_newCurrentGameplayDuration < 0) _newCurrentGameplayDuration = 0;

            missionDurationSeconds = _newCurrentGameplayDuration;

            if (missionDurationSeconds != 0)
            {

                Broadcast.Invoke(new GameplayTimeLeft(missionDurationSeconds, GameplayDurationState.Depleting));
            }
            else
            {
                UserLostMatch();
            }
        }

        protected void OnPlayerActionStartHandler()
        {
            playerGun.Fire();
        }

        public void CloseApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        if (Application.isPlaying)
        {
            Application.Quit();
        }
#endif
        }
    }

}