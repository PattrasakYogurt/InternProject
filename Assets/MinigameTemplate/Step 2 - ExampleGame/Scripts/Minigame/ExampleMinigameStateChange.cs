namespace MinigameTemplate.Example
{
    using Boom.Patterns.Broadcasts;
    using UnityEngine;
    using UnityEngine.Events;

    public class ExampleMinigameStateChange : MonoBehaviour
    {

        [SerializeField] ExampleGameManager.GameState gameState = ExampleGameManager.GameState.GameOverFailure;
        [SerializeField] UnityEvent onConditionMet;

        private void Awake()
        {
            BroadcastState.Register<ExampleGameManager.MinigameState>(StateChangeHandler);
        }
        private void OnDestroy()
        {
            BroadcastState.Unregister<ExampleGameManager.MinigameState>(StateChangeHandler);
        }
        private void StateChangeHandler(ExampleGameManager.MinigameState state)
        {
            if(state.gameState == gameState)
            {
                onConditionMet.Invoke();
            }
        }
    }
}