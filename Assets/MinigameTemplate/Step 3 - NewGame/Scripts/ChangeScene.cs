using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    void Awake()
    {
        SceneManager.LoadScene(3);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
    private void OnEnable()
    {
        SceneManager.LoadScene(3);
    }
    */
    
}
