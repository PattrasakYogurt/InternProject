namespace MinigameTemplate.Example
{
    using System.Collections;
    using UnityEngine;

    public class EffectSkinBlink : MonoBehaviour
    {

        GameObject skin;

        [SerializeField, Range(.125f, 2)] float blinkDuration = .5f;
        [SerializeField, Range(.5f, 50f)] float blinkIntensity = 1;
        [SerializeField, Range(1, 10)] int blinkCount = 3;

        [SerializeField, ShowOnly] bool playingEffect;

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void Setup(GameObject skinsHolder)
        {
            skin = skinsHolder;
        }


        public void PlayEffect()
        {
            if (playingEffect) return;

            StartCoroutine(PlayEffectRoutine());
        }

        IEnumerator PlayEffectRoutine()
        {
            playingEffect = true;



            int currentCount = blinkCount;

            while (currentCount > 0)
            {
                skin.SetActive(false);
                var currentDuration = blinkDuration * .5f;

                while (currentDuration > 0)
                {
                    currentDuration -= Time.deltaTime;
                    yield return null;
                }

                currentCount -= 1;

                skin.SetActive(true);

                if (currentCount == 0) break;

                currentDuration = blinkDuration * .5f;

                while (currentDuration > 0)
                {
                    currentDuration -= Time.deltaTime;
                    yield return null;
                }

                yield return null;
            }


            playingEffect = false;

        }
    }
}