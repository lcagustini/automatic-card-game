using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterState
{
    IDLE,
    WALKING,
    ATTACKING,
    DYING
};

public class monster : MonoBehaviour
{
    const float ATTACK_RANGE = 2.5F;
    const float MAX_HEALTH = 100F;
    const float DEATH_ANIMATION_DURATION = 3F;

    public GameObject prefab;

    public int health = (int) MAX_HEALTH;
    public int attack = 10;

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
            lifeBar.GetComponent<UnityEngine.UI.Slider>().value = health / MAX_HEALTH;
        }

        switch (state)
        {
            case MonsterState.IDLE:
                GetComponentInChildren<Animator>().Play("Armature|Idle");

                GameObject[] monsters = GameObject.FindGameObjectsWithTag("monster");

                foreach (GameObject m in monsters)
                {
                    if (m != transform.gameObject && m.GetComponent<monster>().state != MonsterState.DYING)
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
                        if (dir.magnitude > ATTACK_RANGE)
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
                        if (dir.magnitude <= ATTACK_RANGE)
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
            case MonsterState.DYING:
                {
                    if (death_countdown < 0) {
                        Destroy(transform.gameObject);
                    }
                    death_countdown -= Time.deltaTime;

                    break;
                }
        }
    }
}
