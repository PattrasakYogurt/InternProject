namespace MinigameTemplate.Example
{
    using Boom;
    using Boom.Utility;
    using Candid.World;
    using Cysharp.Threading.Tasks;
    using EdjCase.ICP.Candid.Models;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ExamplePausePanel : MonoBehaviour
    {
        [SerializeField] Button buttonOpenGuilds;
        [SerializeField] Button buttonResetQuests;

        [SerializeField] private List<TransformTweening> closeAnimation;


        private void Awake()
        {
            buttonOpenGuilds.onClick.AddListener(ButtonOpenGuildsClickHandler);
            buttonResetQuests.onClick.AddListener(ButtonResetQuestsClickHandler);
        }

        private void OnDestroy()
        {
            buttonOpenGuilds.onClick.RemoveListener(ButtonOpenGuildsClickHandler);
            buttonResetQuests.onClick.RemoveListener(ButtonResetQuestsClickHandler);
        }


        public void PlayCloseAnimation()
        {
            closeAnimation.Iterate(e => e.PlayBackward());
        }

        #region EDITOR ONLY

        private void ButtonOpenGuildsClickHandler()
        {
            Application.OpenURL($"https://t2qzd-6qaaa-aaaak-qdbdq-cai.icp0.io/");
        }

        private void ButtonResetQuestsClickHandler()
        {
            ResetQuests().Forget();
        }

        private async UniTaskVoid ResetQuests()
        {
            if (BoomManager.Instance.BoomSettings.DeploymentEnv_ == BoomSettings.DeploymentEnv.Production)
            {
                Debug.LogError("You can only reset quests from staging enviroment!");

                return;
            }


            if (UserUtil.IsLoggedIn(out var loginData) == false)
            {
                Debug.LogError("You must log in!");
                return;
            }

            WorldApiClient guildCanister = new(loginData.agent, Principal.FromText(Env.CanisterIds.GAMING_GUILDS.STAGING));
            WorldApiClient gamingWorldCanister = new(loginData.agent, Principal.FromText(BoomManager.Instance.WORLD_CANISTER_ID));

            Debug.Log("Quest reset requested ");

            await guildCanister.DeleteTestQuestActionStateForUser(new(loginData.principal));
            await gamingWorldCanister.DeleteActionHistoryForUser(new(loginData.principal));

            Debug.Log("Quest reset completed");
        }

        #endregion
    }
}