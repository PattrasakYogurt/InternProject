namespace MinigameTemplate
{
    using Boom;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    public class NewGameManager : MonoBehaviour
    {
        [SerializeField] Button buttonWin;
        [SerializeField] Button buttonLose;

        #region MONO
        private void Awake()
        {
            buttonWin.onClick.AddListener(ButtonWinClickHandler);
            buttonLose.onClick.AddListener(ButtonLoseClickHandler);
        }
        private void OnDestroy()
        {
            buttonWin.onClick.RemoveListener(ButtonWinClickHandler);
            buttonLose.onClick.RemoveListener(ButtonLoseClickHandler);
        }
        #endregion

        #region HANDLERS

        private void ButtonWinClickHandler()
        {
            buttonWin.interactable = false;
            buttonLose.interactable = false;

            UserWonMatch().Forget();
        }

        private void ButtonLoseClickHandler()
        {
            buttonWin.interactable = false;
            buttonLose.interactable = false;

            UserLostMatch().Forget();
        }

        #endregion

        /// <summary>
        /// Method that handles executing the UserWonMatch 
        /// </summary>
        /// <returns></returns>
        private async UniTaskVoid UserWonMatch()
        {

            //Record on your world that the user has won the match.
            //It will increment +1 to the user's played count and +1 to the user's won count.
            var result = await ActionUtil.Guilds.UserWonMatch();

            if (result.IsOk)
            {
                Debug.Log($"{nameof(UserWonMatch)} request has been executed");
            }
            else
            {
                Debug.LogError($"Something went wrong executing {nameof(UserWonMatch)}. More details: {result.AsErr()}");
            }

            buttonWin.interactable = true;
            buttonLose.interactable = true;
        }

        /// <summary>
        /// Method that handles executing the UserLostMatch
        /// </summary>
        /// <returns></returns>
        private async UniTaskVoid UserLostMatch()
        {
            //Record on your world that the user has lost the match.
            //It will increment +1 to the user's played count and +1 to the user's lost count.
            var result = await ActionUtil.Guilds.UserLostMatch();

            if (result.IsOk)
            {
                Debug.Log($"{nameof(UserLostMatch)} request has been executed");
            }
            else
            {
                Debug.LogError($"Something went wrong executing {nameof(UserLostMatch)}. More details: {result.AsErr()}");
            }

            buttonWin.interactable = true;
            buttonLose.interactable = true;
        }
    }
}