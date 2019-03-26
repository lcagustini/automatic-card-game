using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoundPhase
{
    PREPARE,
    BATTLE
}

public enum cardType
{
    MONSTER,
    MAGIC
}

public struct cardData
{
    public Texture2D texture;

    public cardType type;
    public int cost;

    public int monsterID; //NOTE: only used on monster type cards
}

public struct monsterData
{
    public float maxHealth;

    public float attackRange;
    public float attackSpeed;
    public int attackDamage;

    public float speed;

    public GameObject prefab;
}

public class main : MonoBehaviour
{
    public static List<cardData> allCards = new List<cardData>();
    public static List<monsterData> allMonster = new List<monsterData>();

    public UnityEngine.UI.Text moneyUI; //Client only, but unity wants it available on server too;
    public UnityEngine.UI.Text timeUI;
    public UnityEngine.UI.Text phaseUI;

    public static bool enable_ranks = false;
    public UnityEngine.UI.Text[] rankingsUI;

#if UNITY_SERVER
    public Stack<int> deck = new Stack<int>();
    public List<int>[] hands = { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
#else
    public PlayerInfo info = new PlayerInfo();
    public GameObject cardPrefab;

    public static RoundPhase current_phase;
    public static float phase_timer;
    public static List<PlayerInfo> rankings = new List<PlayerInfo>();
#endif

    // Start is called before the first frame update
    void Start()
    {
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/00_fool"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/01_magician"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/02_high_priestess"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/03_empress"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/04_emperor"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/05_pope"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/06_lovers"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/07_chariot"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/08_justice"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/09_hermit"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/10_wheel_of_fortune"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/11_strength"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/12_hanged_man"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/13_death"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/14_temperance"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/15_devil"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/16_tower"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/17_star"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/18_moon"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/19_sun"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/20_judgment"), type = cardType.MONSTER, cost = 2, monsterID = 0 });
        allCards.Add(new cardData { texture = (Texture2D)Resources.Load("Cards/21_world"), type = cardType.MONSTER, cost = 2, monsterID = 0 });

        allMonster.Add(new monsterData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, speed = 5, prefab = (GameObject)Resources.Load("Prefabs/Knight") });

#if UNITY_SERVER
        // TODO: rethink if this is the best we can do
        // TODO: verify that this passes by copy and not by reference
        for (int i = 0; i < allCards.Count; i++)
        {
            deck.Push(i);
        }
#else
        rankings.Add(new PlayerInfo());
        rankings.Add(new PlayerInfo());
        rankings.Add(new PlayerInfo());
        rankings.Add(new PlayerInfo());
#endif
    }

#if !UNITY_SERVER
    public void OnClientConnected()
    {
        GameObject.Find("test").transform.position = new Vector3(card.playerArea[info.team].center.x, 0.2F, card.playerArea[info.team].center.y);
        GameObject.Find("test").transform.localScale = new Vector3(card.playerArea[info.team].width / 10, 1, card.playerArea[info.team].height / 10);

        Camera.main.transform.SetPositionAndRotation(GetCameraPosByTeam(), GetCameraRotByTeam());
        GameObject.Find("Client").GetComponent<testClient>().AskNewHand();
    }

    // TODO: fix this making it not possible before client is connected
    void OnGUI()
    {
        if (Event.current.Equals(Event.KeyboardEvent("space")) && current_phase == RoundPhase.PREPARE)
        {
            GameObject.Find("Client").GetComponent<testClient>().AskNewHand();
        }
    }
#endif

    public monster SpawnMonster(int monsterIndex, int team, Vector3 point)
    {
        Transform t = Instantiate(allMonster[monsterIndex].prefab.transform, point, Quaternion.identity);
        monster m = t.gameObject.GetComponent<monster>();
        m.stats = allMonster[monsterIndex];
        m.team = team;
#if UNITY_SERVER
        m.id = monster.monsterCount;
        monster.monsterCount++;
#endif
        return m;
    }

#if !UNITY_SERVER
    void Update()
    {
        phase_timer -= Time.deltaTime;

        if (phase_timer < 0)
        {
            phase_timer = 0;
        }
        timeUI.text = ((int)phase_timer).ToString();

        moneyUI.text = info.money.ToString();

        for (int i = 0; i < 4; i++)
        {
            if (enable_ranks)
            {
                rankingsUI[i].transform.gameObject.SetActive(true);
            }
            rankingsUI[i].text = rankings[i].team.ToString() + " -> " + rankings[i].wins.ToString();
        }
    }
#endif

#if UNITY_SERVER
    public void Shuffle()
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
#endif

#if !UNITY_SERVER
    public Quaternion GetCardRotationByTeam()
    {
        switch (info.team)
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
        switch (info.team)
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

    public Vector3 GetCardTargetByTeam(int i)
    {
        switch (info.team)
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

    public Vector3 GetCameraPosByTeam()
    {
        switch (Camera.main.GetComponent<main>().info.team)
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

    public Quaternion GetCameraRotByTeam()
    {
        switch (Camera.main.GetComponent<main>().info.team)
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
#endif
}
