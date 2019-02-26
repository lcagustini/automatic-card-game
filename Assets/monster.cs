using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterState
{
    IDLE,
    WALKING,
    ATTACKING
};

public class monster : MonoBehaviour
{
    const float STATE_THRESHOLD = 1.3F;
    const float MAX_HEALTH = 100F;

    public GameObject prefab;

    public int health = (int) MAX_HEALTH;
    public int attack = 10;

    private MonsterState state = MonsterState.IDLE;
    private GameObject target;
    private GameObject lifeBar;
    private float attackDelay = 0;

    // Start is called before the first frame update
    void Start()
    {
        lifeBar = Instantiate(prefab.transform).gameObject;

        GameObject screen = GameObject.Find("Canvas");

        lifeBar.transform.SetParent(screen.transform);
    }

    // Update is called once per frame
    void Update()
    {
        {
            RectTransform t = lifeBar.GetComponent<RectTransform>();

            Vector3 pos = new Vector3(-2.5F, 3, 0);
            t.anchoredPosition = Camera.main.WorldToScreenPoint(transform.position + pos);
            lifeBar.GetComponent<UnityEngine.UI.Slider>().value = health / MAX_HEALTH;
        }

        if (attackDelay > 0)
        {
            attackDelay -= Time.deltaTime;
        }

        if (health <= 0)
        {
            Destroy(transform.gameObject);
            Destroy(lifeBar);
        }

        switch (state)
        {
            case MonsterState.IDLE:
                GameObject[] monsters = GameObject.FindGameObjectsWithTag("monster");

                foreach (GameObject m in monsters)
                {
                    if (m != transform.gameObject)
                    {
                        target = m;
                        state = MonsterState.WALKING;
                        break;
                    }
                }

                break;
            case MonsterState.WALKING:
                {
                    if (target != null)
                    {
                        Vector3 dir = target.transform.position - transform.position;
                        if (dir.magnitude > STATE_THRESHOLD)
                        {
                            dir.Normalize();

                            transform.position += dir * Time.deltaTime;
                        }
                        else
                        {
                            state = MonsterState.ATTACKING;
                        }
                    }
                    else
                    {
                        state = MonsterState.IDLE;
                    }
                    
                    break;
                }
            case MonsterState.ATTACKING:
                {
                    if (target != null)
                    {
                        Vector3 dir = target.transform.position - transform.position;
                        if (dir.magnitude <= STATE_THRESHOLD)
                        {
                            if (attackDelay <= 0) {
                                target.GetComponent<monster>().health -= attack;
                                attackDelay = 0.5F;
                            }
                        }
                        else
                        {
                            state = MonsterState.WALKING;
                        }
                    }
                    else
                    {
                        state = MonsterState.IDLE;
                    }
                    break;
                }
        }
    }
}
