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

    // Start is called before the first frame update
    void Start()
    {
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/00_fool") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/01_magician") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/02_high_priestess") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/03_empress") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/04_emperor") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/05_pope") });
        deck.Push(new cardData { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10, texture = (Texture2D)Resources.Load("Cards/06_lovers") });


        int handSize = deck.Count > 5 ? 5 : deck.Count;
        for (int i = 0; i < handSize; i++)
        {
            Transform t = Instantiate(prefab.transform, new Vector3(-4.2F + 2 * i, 28, -47), Quaternion.Euler(17.5F, 180, -1));
            card c = t.gameObject.GetComponent<card>();

            c.stats = deck.Pop();
        }
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
        GameObject[] hand = GameObject.FindGameObjectsWithTag("hand_card");
        foreach (var card in hand)
        {
            deck.Push(card.GetComponent<card>().stats);
            Destroy(card);
        }

        Shuffle();

        int handSize = deck.Count > 5 ? 5 : deck.Count;
        for (int i = 0; i < handSize; i++)
        {
            Transform t = Instantiate(prefab.transform, new Vector3(Camera.main.transform.position.x + -4.02F + 2 * i, 28, Camera.main.transform.position.z + 0.42F), Quaternion.Euler(17.5F, 180, -1));
            card c = t.gameObject.GetComponent<card>();

            c.stats = deck.Pop();
        }
    }

    private static Random rng = new Random();

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
}
