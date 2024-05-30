namespace MinigameTemplate.Example
{
    using UnityEngine;

    public class HandsRef : MonoBehaviour
    {
        [field: SerializeField] public Transform LHand { get; private set; }
        [field: SerializeField] public Transform RHand { get; private set; }
    }
}