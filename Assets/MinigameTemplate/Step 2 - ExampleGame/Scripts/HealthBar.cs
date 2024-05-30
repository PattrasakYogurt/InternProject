namespace MinigameTemplate.Example
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public class HealthBar : MonoBehaviour
    {
        [SerializeField] HealthComponent healthComponent;
        [SerializeField] Image progressBar;
        [SerializeField] float progressChangeSpeed = 500;
        [SerializeField, ShowOnly] float currentProgressChangeSpeed;
        [SerializeField] Gradient gradient;
        Coroutine progressCoroutine;

        private void Awake()
        {
            healthComponent.OnHealthChange.AddListener(OnHealthChangeHandler);
        }

        private void Start()
        {
            progressBar.fillAmount = healthComponent.currentHealth / healthComponent.maxHealth;

            if(HealthBarCanvas.Instance)
            {
                transform.SetParent(HealthBarCanvas.Instance.transform);
                transform.localScale = Vector3.one;
            }
        }

        private void OnDestroy()
        {
            healthComponent.OnHealthChange.RemoveListener(OnHealthChangeHandler);
        }

        private void OnHealthChangeHandler(HealthComponent.HealthChangeInfo arg0)
        {
            float progress = arg0.newHealthValue / arg0.maxHealth;
            float diff = Mathf.Abs(progressBar.fillAmount - progress);

            currentProgressChangeSpeed = progressChangeSpeed * (diff * .1f);
            if (progressCoroutine != null)
            {
                StopCoroutine(progressCoroutine);
            }
            progressCoroutine = StartCoroutine(AnimateProgress(progress, currentProgressChangeSpeed));

            if (progress == 0) Destroy(gameObject, .5f);
        }

        IEnumerator AnimateProgress(float progress, float speed)
        {
            float time = 0;
            float initialProgress = progressBar.fillAmount;

            while (time < 1)
            {
                progressBar.fillAmount = Mathf.Lerp(initialProgress, progress, time);
                time += Time.deltaTime * speed;

                progressBar.color = gradient.Evaluate(1 - progress);

                yield return null;
            }

            progressBar.fillAmount = progress;
            progressBar.color = gradient.Evaluate(1 - progress);
        }
    }
}