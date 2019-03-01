using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct cardData
{
    public float maxHealth;

    public float attackRange;
    public float attackSpeed;
    public int attackDamage;

    public Texture2D texture;
}

public class main : MonoBehaviour
{
    public GameObject prefab;

    public Stack<cardData> deck = new Stack<cardData>();
    public List<cardData> hand = new List<cardData>();

    public int team;

    // Start is called before the first frame update
    void Start()
    {
        team = 0;

        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/00_fool") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/01_magician") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/02_high_priestess") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/03_empress") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/04_emperor") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/05_pope") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/06_lovers") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/07_chariot") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/08_justice") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/09_hermit") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/10_wheel_of_fortune") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/11_strength") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/12_hanged_man") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/13_death") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/14_temperance") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/15_devil") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/16_tower") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/17_star") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/18_moon") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/19_sun") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/20_judgment") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/21_world") });

        Camera.main.transform.SetPositionAndRotation(GetCameraPosByTeam(), GetCameraRotByTeam());
        NewHand();
    }

    void OnGUI()
    {
        if (Event.current.Equals(Event.KeyboardEvent("space")))
        {
            NewHand();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void NewHand()
    {
        GameObject[] hand_objects = GameObject.FindGameObjectsWithTag("hand_card");
        foreach (var obj in hand_objects)
        {
            Destroy(obj);
        }
        foreach (var card in hand)
        {
            deck.Push(card);
        }
        hand.Clear();

        Shuffle();

        int handSize = deck.Count > 5 ? 5 : deck.Count;
        for (int i = 0; i < handSize; i++)
        {
            Transform t = Instantiate(prefab.transform, GetCardPosByTeam(), GetCardRotationByTeam());
            card c = t.gameObject.GetComponent<card>();

            c.stats = deck.Pop();
            c.targetPos = GetCardTargetByTeam(i);
            c.team = team;

            hand.Add(c.stats);
        }
    }

    void Shuffle()
    {
        var array = deck.ToArray();
        deck.Clear();

        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            cardData t = array[r];
            array[r] = array[i];
            array[i] = t;
        }

        foreach (cardData c in array)
        {
            deck.Push(c);
        }
    }

    Quaternion GetCardRotationByTeam()
    {
        switch (team)
        {
            case 0:
                {
                    return Quaternion.Euler(17.5F, 180, -1);
                }
            case 1:
                {
                    return Quaternion.Euler(17.5F, 0, -1);
                }
            case 2:
                {
                    return Quaternion.Euler(17.5F, 90, -1);
                }
            case 3:
                {
                    return Quaternion.Euler(17.5F, 270, -1);
                }
            default:
                {
                    return Quaternion.identity;
                }
        }
    }

    public Vector3 GetCardPosByTeam()
    {
        switch (team)
        {
            case 0:
                {
                    return new Vector3(Camera.main.transform.position.x - 20.02F, 28, Camera.main.transform.position.z + 0.42F);
                }
            case 1:
                {
                    return new Vector3(Camera.main.transform.position.x + 20.02F, 28, Camera.main.transform.position.z - 0.42F);
                }
            case 2:
                {
                    return new Vector3(Camera.main.transform.position.x - 0.42F, 28, Camera.main.transform.position.z - 20.02F);
                }
            case 3:
                {
                    return new Vector3(Camera.main.transform.position.x + 0.42F, 28, Camera.main.transform.position.z + 20.02F);
                }
            default:
                {
                    return Vector3.zero;
                }
        }
    }

    Vector3 GetCardTargetByTeam(int i)
    {
        switch (team)
        {
            case 0:
                {
                    return new Vector3(Camera.main.transform.position.x - 4.02F + 2 * i, 28, Camera.main.transform.position.z + 0.42F);
                }
            case 1:
                {
                    return new Vector3(Camera.main.transform.position.x - 4.02F + 2 * i, 28, Camera.main.transform.position.z - 0.42F);
                }
            case 2:
                {
                    return new Vector3(Camera.main.transform.position.x - 0.42F, 28, Camera.main.transform.position.z - 4.02F + 2 * i);
                }
            case 3:
                {
                    return new Vector3(Camera.main.transform.position.x + 0.42F, 28, Camera.main.transform.position.z - 4.02F + 2 * i);
                }
            default:
                {
                    return Vector3.zero;
                }
        }
    }

    Vector3 GetCameraPosByTeam()
    {
        switch (Camera.main.GetComponent<main>().team)
        {
            case 0:
                {
                    return new Vector3(0, 38, -50);
                }
            case 1:
                {
                    return new Vector3(0, 38, 50);
                }
            case 2:
                {
                    return new Vector3(50, 38, 0);
                }
            case 3:
                {
                    return new Vector3(-50, 38, 0);
                }
            default:
                {
                    return Vector3.zero;
                }
        }
    }

    Quaternion GetCameraRotByTeam()
    {
        switch (Camera.main.GetComponent<main>().team)
        {
            case 0:
                {
                    return Quaternion.Euler(66, 0, 0);
                }
            case 1:
                {
                    return Quaternion.Euler(66, 180, 0);
                }
            case 2:
                {
                    return Quaternion.Euler(66, 270, 0);
                }
            case 3:
                {
                    return Quaternion.Euler(66, 90, 0);
                }
            default:
                {
                    return Quaternion.identity;
                }
        }
    }
}
