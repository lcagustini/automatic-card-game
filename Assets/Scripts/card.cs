using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card : MonoBehaviour
{
    const float MAX_HEIGHT = 28.5F;
    const float MIN_HEIGHT = 28F;

    public GameObject prefab;

    public cardData stats;

    private Rect player1Area = new Rect(-16.7F, -50, 33.4F, 15);

    private bool mouseOver = false;
    private bool dragging = false;
    private bool casting = false;
    private float castCounter = 2F;

    // Start is called before the first frame update
    void Start()
    {
        transform.gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", stats.texture);
    }

    void OnMouseEnter()
    {
        mouseOver = true;
    }

    void OnMouseExit()
    {
        mouseOver = false;
    }

    void OnMouseUp()
    {
        dragging = false;

        foreach (ParticleSystem par in GetComponentsInChildren<ParticleSystem>())
        {
            par.Stop();
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.tag != "hand_card")
            {
                if (player1Area.Contains(new Vector2(hit.point.x, hit.point.z)))
                {
                    foreach (ParticleSystem par in GetComponentsInChildren<ParticleSystem>())
                    {
                        if (par.name == "explode" || par.name == "explode2")
                        {
                            par.Play();
                        }
                    }

                    Transform t = Instantiate(prefab.transform, hit.point, Quaternion.identity);
                    monster m = t.gameObject.GetComponent<monster>();
                    m.stats = stats;
                    m.team = Random.Range(0,4);

                    casting = true;
                }
            }
        }
    }

    void OnMouseDown()
    {
        dragging = true;

        foreach (ParticleSystem par in GetComponentsInChildren<ParticleSystem>())
        {
            if (par.name != "explode" && par.name != "explode2")
            {
                par.Play();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseOver)
        {
            if (transform.position.y < MAX_HEIGHT)
            {
                transform.position += new Vector3(0, 2*Time.deltaTime, 0);
            }
        }
        else if (!dragging)
        {
            if (transform.position.y > MIN_HEIGHT)
            {
                transform.position -= new Vector3(0, 2*Time.deltaTime, 0);
            }
        }

        if (casting)
        {
            if (castCounter < 1.4)
            {
                transform.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (castCounter < 0)
            {
                Destroy(transform.gameObject);
            }
            castCounter -= Time.deltaTime;
        }
    }
}
