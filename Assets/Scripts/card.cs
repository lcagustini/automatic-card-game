using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card : MonoBehaviour
{
    const float MAX_HEIGHT = 28.5F;
    const float MIN_HEIGHT = 28F;

    public GameObject prefab;

    public cardData stats;
    public Vector3 targetPos;

    public static Rect[] playerArea = new Rect[] {
        new Rect(-16.7F, -50, 33.4F, 15),
        new Rect(-16.7F, 35, 33.4F, 15),
        new Rect(35, -16.7F, 15, 33.4F),
        new Rect(-50, -16.7F, 15, 33.4F),
    };

    private bool mouseOver = false;
    private bool dragging = false;
    private bool casting = false;
    private float castCounter = 2F;

    public int team;

    // Start is called before the first frame update
    void Start()
    {
        transform.gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", stats.texture);

        GameObject.Find("test").transform.position = new Vector3(playerArea[team].center.x, 0.2F, playerArea[team].center.y);
        GameObject.Find("test").transform.localScale = new Vector3(playerArea[team].width/10, 1, playerArea[team].height/10);
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

        if (Physics.Raycast(ray, out hit) && !casting)
        {
            if (hit.transform.gameObject.tag != "hand_card")
            {
                if (playerArea[team].Contains(new Vector2(hit.point.x, hit.point.z)))
                {
                    foreach (ParticleSystem par in GetComponentsInChildren<ParticleSystem>())
                    {
                        if (par.name == "explode" || par.name == "explode2")
                        {
                            par.Play();
                        }
                    }

                    Vector3 point = hit.point - new Vector3(0, 2.5F, 0);
                    Transform t = Instantiate(prefab.transform, point, Quaternion.identity);
                    monster m = t.gameObject.GetComponent<monster>();
                    m.stats = stats;
                    m.team = Camera.main.GetComponent<main>().team;

                    Camera.main.GetComponent<main>().hand.Remove(stats);

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

        if (team == 0 || team == 1)
        {
            transform.position += new Vector3((7 * Time.deltaTime * (targetPos - transform.position)).x, 0, 0);

            Vector3 z = Camera.main.GetComponent<main>().GetCardPosByTeam();
            transform.position = new Vector3(transform.position.x, transform.position.y, z.z);
        }
        else
        {
            transform.position += new Vector3(0, 0, (7 * Time.deltaTime * (targetPos - transform.position)).z);

            Vector3 x = Camera.main.GetComponent<main>().GetCardPosByTeam();
            transform.position = new Vector3(x.x, transform.position.y, transform.position.z);
        }
    }
}
