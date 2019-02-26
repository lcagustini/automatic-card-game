using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Instantiate(prefab.transform, new Vector3(-4.2F + 2*i, 28, -47), Quaternion.Euler(17.5F, 180, -1));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
