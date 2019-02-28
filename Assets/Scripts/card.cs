using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card : MonoBehaviour
{
    public GameObject prefab;
    private bool dragging = false;

    public cardData stats;

    // Start is called before the first frame update
    void Start()
    {
        transform.gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", stats.texture);
    }

    void OnMouseDown()
    {
        dragging = true;
    }

    void OnMouseUp()
    {
        dragging = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.tag != "hand_card")
            {
                Transform t = Instantiate(prefab.transform, hit.point, Quaternion.identity);
                monster m = t.gameObject.GetComponent<monster>();
                m.stats = stats;

                Destroy(transform.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
