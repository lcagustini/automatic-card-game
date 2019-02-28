using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MonsterState
{
    IDLE,
    WALKING,
    ATTACKING,
    DYING
};

public class monster : MonoBehaviour
{
    const float DEATH_ANIMATION_DURATION = 3F;

    // could be const but the compiler doesn't think so
    private Color[] team_colors = {Color.cyan, new Color(1, 0.5f, 0), new Color(0.5F, 0, 1), Color.yellow};

    public GameObject prefab;

    public cardData stats;
    public int health;
    public int team;

    private MonsterState state = MonsterState.IDLE;
    private GameObject target;
    private GameObject lifeBar;
    private float attackDelay = 0;
    private float death_countdown = DEATH_ANIMATION_DURATION;

    // Start is called before the first frame update
    void Start()
    {
        lifeBar = Instantiate(prefab.transform).gameObject;

        GameObject screen = GameObject.Find("Canvas");

        lifeBar.transform.SetParent(screen.transform);

        health = (int) stats.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (attackDelay > 0)
        {
            attackDelay -= Time.deltaTime;
        }

        if (health <= 0)
        {
            state = MonsterState.DYING;
            GetComponentInChildren<Animator>().Play("Armature|Die");
            Destroy(lifeBar);
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<BoxCollider>());
        } else
        {
            RectTransform t = lifeBar.GetComponent<RectTransform>();

            Vector3 pos = new Vector3(-2.5F, 3, 0);
            t.anchoredPosition = Camera.main.WorldToScreenPoint(transform.position + pos);
            lifeBar.GetComponent<UnityEngine.UI.Slider>().value = health / stats.maxHealth;
            lifeBar.GetComponent<sliderColor>().fill.color = team_colors[team];
        }

        switch (state)
        {
            case MonsterState.IDLE:
                GetComponentInChildren<Animator>().Play("Armature|Idle");

                GameObject[] monsters = GameObject.FindGameObjectsWithTag("monster");

                foreach (GameObject m in monsters)
                {
                    monster m_class = m.GetComponent<monster>();
                    if (m != transform.gameObject && m_class.state != MonsterState.DYING && m_class.team != team)
                    {
                        target = m;
                        state = MonsterState.WALKING;
                        break;
                    }
                }

                break;
            case MonsterState.WALKING:
                {
                    GetComponentInChildren<Animator>().Play("Armature|Walk");

                    if (target != null && target.GetComponent<monster>().state != MonsterState.DYING)
                    {
                        Vector3 dir = target.transform.position - transform.position;
                        transform.rotation = Quaternion.LookRotation(dir);
                        if (dir.magnitude > stats.attackRange)
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
                    GetComponentInChildren<Animator>().Play("Armature|Attack");

                    if (target != null && target.GetComponent<monster>().state != MonsterState.DYING)
                    {
                        Vector3 dir = target.transform.position - transform.position;
                        transform.rotation = Quaternion.LookRotation(dir);
                        if (dir.magnitude <= stats.attackRange)
                        {
                            if (attackDelay <= 0) {
                                target.GetComponent<monster>().health -= stats.attackDamage;
                                attackDelay = stats.attackSpeed;
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
            case MonsterState.DYING:
                {
                    if (death_countdown < 0) {
                        Destroy(transform.gameObject);
                    }
                    death_countdown -= Time.deltaTime;
                    transform.position -= new Vector3(0, Time.deltaTime/5, 0);

                    break;
                }
        }
    }
}
