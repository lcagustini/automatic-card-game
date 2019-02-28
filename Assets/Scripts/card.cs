using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card : MonoBehaviour
{
    public GameObject prefab;
    private bool dragging = false;

    public cardData stats;

    private Rect player1Area = new Rect(-16.7F, -50, 33.4F, 15);

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
                if (player1Area.Contains(new Vector2(hit.point.x, hit.point.z)))
                {
                    Transform t = Instantiate(prefab.transform, hit.point, Quaternion.identity);
                    monster m = t.gameObject.GetComponent<monster>();
                    m.stats = stats;
                    m.team = Random.Range(0,4);

                    Destroy(transform.gameObject);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
