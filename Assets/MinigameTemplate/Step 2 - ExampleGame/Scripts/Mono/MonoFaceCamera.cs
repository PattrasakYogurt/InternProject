namespace MinigameTemplate.Example
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MonoFaceCamera : MonoBehaviour
    {
        Camera mainCamera = null;
        [SerializeField] bool invertLookAt;
        private void Start()
        {
            mainCamera = Camera.main;
        }
        void Update()
        {
            // Find the main camera in the scene

            if (mainCamera != null)
            {


                transform.transform.forward = invertLookAt ? -mainCamera.transform.forward : mainCamera.transform.forward;
            }
            else
            {
                Debug.LogWarning("Main camera not found in the scene!");
            }
        }
    }
}