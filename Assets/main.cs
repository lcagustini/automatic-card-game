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
    // TODO: maybe rethink if cards really need all this information
    public List<cardData> allCards = new List<cardData>();

    public GameObject cardPrefab;
    public GameObject monsterPrefab;

    public Stack<int> deck = new Stack<int>();
    public List<int> hand = new List<int>();

    public int team;

    // Start is called before the first frame update
    void Start()
    {
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/00_fool") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/01_magician") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/02_high_priestess") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/03_empress") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/04_emperor") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/05_pope") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/06_lovers") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/07_chariot") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/08_justice") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/09_hermit") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/10_wheel_of_fortune") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/11_strength") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/12_hanged_man") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/13_death") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/14_temperance") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/15_devil") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/16_tower") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/17_star") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/18_moon") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/19_sun") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/20_judgment") });
        allCards.Add(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/21_world") });

        // TODO: rethink if this is the best we can do
        // TODO: verify that this passes by copy and not by reference
        for (int i = 0; i < allCards.Count; i++)
        {
            deck.Push(i);
        }
    }

    public void OnClientConnected()
    {
        GameObject.Find("test").transform.position = new Vector3(card.playerArea[team].center.x, 0.2F, card.playerArea[team].center.y);
        GameObject.Find("test").transform.localScale = new Vector3(card.playerArea[team].width / 10, 1, card.playerArea[team].height / 10);

        Camera.main.transform.SetPositionAndRotation(GetCameraPosByTeam(), GetCameraRotByTeam());
        NewHand();
    }

    public monster SpawnMonster(int monsterIndex, int team, Vector3 point)
    {
        //monsterPrefab.transform.gameObject.GetComponent<monster>().team = team;
        Transform t = Instantiate(monsterPrefab.transform, point, Quaternion.identity);
        monster m = t.gameObject.GetComponent<monster>();
        m.stats = allCards[monsterIndex];
        m.team = team;
#if UNITY_SERVER
        m.id = monster.monsterCount;
        monster.monsterCount++;
#endif
        return m;
    }

    // TODO: fix this making it not possible before client is connected
    void OnGUI()
    {
        if (Event.current.Equals(Event.KeyboardEvent("space")))
        {
            NewHand();
        }
    }

    // Update is called once per frame
    // TODO: check if we're connected
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
            Transform t = Instantiate(cardPrefab.transform, GetCardPosByTeam(), GetCardRotationByTeam());
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
            int t = array[r];
            array[r] = array[i];
            array[i] = t;
        }

        foreach (int c in array)
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
