using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSingleton : MonoBehaviour
{
    private static CanvasSingleton _instance;
    // Start is called before the first frame update
    void Start()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            if(_instance != this)
            {
                Destroy (gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);
    }
}
