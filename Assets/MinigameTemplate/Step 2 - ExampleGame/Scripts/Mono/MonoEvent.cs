namespace MinigameTemplate.Mono
{
    using UnityEngine;
    using UnityEngine.Events;

    public class MonoEvent : MonoBehaviour
    {
        public enum MonoType
        {
            Awake, Start, OnDestroy, OnEnable, OnDisable
        }

        [SerializeField] UnityEvent uEvent;
        [SerializeField] MonoType triggerOn;

        private void Awake()
        {
            if (triggerOn == MonoType.Awake) uEvent.Invoke();
        }
        private void Start()
        {
            if (triggerOn == MonoType.Start) uEvent.Invoke();
        }

        private void OnDestroy()
        {
            if (triggerOn == MonoType.OnDestroy) uEvent.Invoke();
        }

        private void OnEnable()
        {
            if (triggerOn == MonoType.OnEnable) uEvent.Invoke();
        }

        private void OnDisable()
        {
            if (triggerOn == MonoType.OnDisable) uEvent.Invoke();
        }
    }
}