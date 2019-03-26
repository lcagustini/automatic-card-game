using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MonsterState
{
    RISING,
    IDLE,
    WALKING,
    ATTACKING,
    DYING
};

public class monster : MonoBehaviour
{
    const float DEATH_ANIMATION_DURATION = 3F;
    public GameObject prefab;
    public monsterData stats;

    public int id;
    public int health;
    public int team;

    public MonsterState state = MonsterState.RISING;
    public float death_countdown = DEATH_ANIMATION_DURATION;

    public Vector3 startingPos;
    public Quaternion startingRot;
#if UNITY_SERVER
    private GameObject target;
    private float attackDelay = 0;
    static public int monsterCount = 0;

    void Start()
    {
        health = (int)stats.maxHealth;

        switch (team)
        {
            case 0:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case 1:
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case 2:
                transform.rotation = Quaternion.Euler(0, 270, 0);
                break;
            case 3:
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
        }
    }

    void Update()
    {
        if (attackDelay > 0)
        {
            attackDelay -= Time.deltaTime;
        }

        if (health <= 0)
        {
            state = MonsterState.DYING;
            Destroy(transform.gameObject.GetComponent<Rigidbody>());
            Destroy(transform.gameObject.GetComponent<BoxCollider>());
        }

        switch (state)
        {
            case MonsterState.RISING:
                {
                    if (transform.position.y < 0.2)
                    {
                        transform.position += new Vector3(0, 2.5F * Time.deltaTime, 0);
                    }
                    else
                    {
                        if (transform.gameObject.GetComponent<Rigidbody>() == null)
                        {
                            transform.gameObject.AddComponent<BoxCollider>();
                            transform.gameObject.AddComponent<Rigidbody>();
                        }
                        transform.gameObject.GetComponent<Rigidbody>().useGravity = true;
                        startingPos = transform.position;
                        startingRot = transform.rotation;
                        state = MonsterState.IDLE;
                    }
                }
                break;
            case MonsterState.IDLE:
                if (server.current_phase == RoundPhase.BATTLE)
                {
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
                }
                break;
            case MonsterState.WALKING:
                {
                    if (target != null && target.GetComponent<monster>().state != MonsterState.DYING && server.current_phase == RoundPhase.BATTLE)
                    {
                        Vector3 dir = target.transform.position - transform.position;
                        transform.rotation = Quaternion.LookRotation(dir);
                        if (dir.magnitude > stats.attackRange)
                        {
                            dir.Normalize();

                            transform.position += dir * Time.deltaTime * stats.speed;
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
                    if (target != null && target.GetComponent<monster>().state != MonsterState.DYING && server.current_phase == RoundPhase.BATTLE)
                    {
                        Vector3 dir = target.transform.position - transform.position;
                        transform.rotation = Quaternion.LookRotation(dir);
                        if (dir.magnitude <= stats.attackRange)
                        {
                            if (attackDelay <= 0)
                            {
                                target.GetComponent<monster>().health -= stats.attackDamage;
                                if (target.GetComponent<monster>().health <= 0)
                                {
                                    foreach (PlayerInfo player in server.players)
                                    {
                                        if (player.team == team)
                                        {
                                            player.money += 1;
                                        }
                                    }
                                }
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
                    //TODO: Better handling of monster death to avoid client from not destroying entity
                    if (death_countdown > -0.8)
                    {
                        death_countdown -= Time.deltaTime;
                        transform.position -= new Vector3(0, Time.deltaTime / 3, 0);
                    }

                    break;
                }
        }
    }
#else
    // could be const but the compiler doesn't think so
    private Color[] team_colors = {Color.cyan, new Color(1, 0.5f, 0), new Color(0.5F, 0, 1), Color.yellow};

    private GameObject lifeBar;

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
        if (health <= 0)
        {
            GetComponentInChildren<Animator>().Play("Armature|Die");
            lifeBar.SetActive(false);
        } else
        {
            lifeBar.SetActive(true);

            RectTransform t = lifeBar.GetComponent<RectTransform>();

            Vector3 pos = Vector3.zero;
            switch (Camera.main.GetComponent<main>().info.team)
            {
                case 0:
                    pos = new Vector3(-2F, 3, 0);
                    break;
                case 1:
                    pos = new Vector3(2F, 3, 0);
                    break;
                case 2:
                    pos = new Vector3(0, 3, -2F);
                    break;
                case 3:
                    pos = new Vector3(0, 3, 2F);
                    break;
            }
            
            t.anchoredPosition = Camera.main.WorldToScreenPoint(transform.position + pos);

            lifeBar.GetComponent<UnityEngine.UI.Slider>().value = health / stats.maxHealth;
            lifeBar.GetComponent<sliderColor>().fill.color = team_colors[team];
        }

        switch (state)
        {
            case MonsterState.RISING:
                break;
            case MonsterState.IDLE:
                GetComponentInChildren<Animator>().Play("Armature|Idle");
                break;
            case MonsterState.WALKING:
                GetComponentInChildren<Animator>().Play("Armature|Walk");
                break;
            case MonsterState.ATTACKING:
                GetComponentInChildren<Animator>().Play("Armature|Attack");
                break;
            case MonsterState.DYING:
                break;
        }
    }
#endif
        }