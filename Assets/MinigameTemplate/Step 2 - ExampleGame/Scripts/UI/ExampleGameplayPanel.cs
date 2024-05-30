namespace MinigameTemplate.Example
{
    using Boom.Patterns.Broadcasts;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class ExampleGameplayPanel : MonoBehaviour
    {
        public class WindowData
        {
            public string missionTitle;
            public int missionGoal;
            public string missionControlAction;
            public string missionLogo;
            public WindowData(string missionTitle, int missionGoal, string missionControlAction, string missionLogo)
            {
                this.missionTitle = missionTitle;
                this.missionGoal = missionGoal;
                this.missionControlAction = missionControlAction;
                this.missionLogo = missionLogo;
                this.missionLogo = missionLogo;
            }
        }


        [SerializeField] GameObject getReadyText;
        [SerializeField] GameObject mission;
        [SerializeField] GameObject gameplayInformation;
        [SerializeField] GameObject control;

        [SerializeField] TextMeshProUGUI goalTitleText;
        [SerializeField] TextMeshProUGUI goalScoreText;

        [SerializeField] TextMeshProUGUI goalGameplayScoreText;

        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] TextMeshProUGUI timerText;

        [SerializeField] TextMeshProUGUI controlActionText;

        [SerializeField] Animator countdownAnimator;

        [SerializeField] List<TransformTweening> animationsToPlayOnGameStart;

        protected void Awake()
        {
            BroadcastState.Register<ExampleGameManager.MinigameState>(StateChangeHandler, new BroadcastState.BroadcastSetting() { invokeOnRegistration = true });
            BroadcastState.Register<ExampleGameManager.ScoreState>(ScoreStateHandler, new BroadcastState.BroadcastSetting() { invokeOnRegistration = true });
            Broadcast.Register<ExampleGameManager.GameplayTimeLeft>(GameplayDurationHandler);
        }



        private void Start()
        {
            countdownText.text = "GET READY!";
            control.SetActive(false);
            gameplayInformation.SetActive(false);

            countdownAnimator.enabled = true;
        }
        protected void OnDestroy()
        {
            BroadcastState.Unregister<ExampleGameManager.MinigameState>(StateChangeHandler);
            BroadcastState.Unregister<ExampleGameManager.ScoreState>(ScoreStateHandler);
            Broadcast.Unregister<ExampleGameManager.GameplayTimeLeft>(GameplayDurationHandler);
        }

        private void StateChangeHandler(ExampleGameManager.MinigameState state)
        {

            if (state.gameState == ExampleGameManager.GameState.Initiate)
            {
                animationsToPlayOnGameStart.ForEach(e => e.PlayFoward());
            }
            else if (state.gameState == ExampleGameManager.GameState.Initiating)
            {
                control.SetActive(true);

                if (!countdownAnimator.enabled) countdownAnimator.enabled = true;

                countdownText.text = $"{state.countDown}";

                countdownAnimator.SetTrigger("shirnk");
            }
            else if (state.gameState == ExampleGameManager.GameState.Playing)
            {
                countdownText.text = "GO!";

                countdownAnimator.SetTrigger("shirnk");
                timerText.text = "60:00";

                gameplayInformation.SetActive(true);
                //control.SetActive(false);
                if (!countdownAnimator.enabled) countdownAnimator.enabled = true;

                CoroutineManagerUtil.DelayAction(() => { countdownAnimator.enabled = false; }, 2.5f, transform);
            }
        }
        private void ScoreStateHandler(ExampleGameManager.ScoreState state)
        {
            goalGameplayScoreText.text = $"{state.missionGoalProgress}/{state.missionGoal}";
        }

        private void GameplayDurationHandler(ExampleGameManager.GameplayTimeLeft duration)
        {
            timerText.text = $"{DisplayDeepTime((long)(duration.currentTimeLeft * 1000))}";
        }


        public void Setup(WindowData data)
        {
            if (data == null)
            {
                Debug.Log($"Window of name {gameObject.name}, requires data, data cannot be null");
                return;
            }
            goalTitleText.text = data.missionTitle;
            goalScoreText.text = $"x{data.missionGoal}";
            goalGameplayScoreText.text = $"{0}/{data.missionGoal}";
            controlActionText.text = data.missionControlAction;
        }

        public string DisplayTime(long timeMs, bool includeHours = true)
        {
            long timeSeconds = timeMs / 1000;
            int seconds = Mathf.FloorToInt(timeSeconds % 60);
            int minutes = Mathf.FloorToInt(timeSeconds / 60) % 60;
            int hours = Mathf.FloorToInt(timeSeconds / (60 * 60));

            if (includeHours)
            {
                return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
            else
            {
                return string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        public string DisplayDeepTime(long timeMs, bool singleDigit = false, bool includeHours = false)
        {
            string time;

            if (singleDigit)
            {
                int milliseconds = Mathf.FloorToInt(((float)(timeMs % 1000) / 1000) * 10);
                time = DisplayTime(timeMs, includeHours);

                time += $":{milliseconds}";
            }
            else
            {
                int milliseconds = Mathf.FloorToInt(((float)(timeMs % 1000) / 1000) * 100);
                time = DisplayTime(timeMs, includeHours);

                time += $":{(milliseconds < 10 ? $"0{milliseconds}" : $"{milliseconds}")}";
            }

            return time;
        }
    }

}