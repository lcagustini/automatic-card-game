using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct cardStats
{
    public float maxHealth;

    public float attackRange;
    public float attackSpeed;
    public int attackDamage;
}

public class main : MonoBehaviour
{
    public GameObject prefab;

    public Stack<cardStats> deck = new Stack<cardStats>();

    // Start is called before the first frame update
    void Start()
    {
        deck.Push(new cardStats { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10 });
        deck.Push(new cardStats { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10 });
        deck.Push(new cardStats { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10 });
        deck.Push(new cardStats { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10 });
        deck.Push(new cardStats { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10 });
        deck.Push(new cardStats { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10 });
        deck.Push(new cardStats { maxHealth = 100F, attackRange = 2.5F, attackSpeed = 0.5F, attackDamage = 10 });

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
        if (Event.current.Equals(Event.KeyboardEvent("return")))
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
            Transform t = Instantiate(prefab.transform, new Vector3(-4.2F + 2 * i, 28, -47), Quaternion.Euler(17.5F, 180, -1));
            card c = t.gameObject.GetComponent<card>();

            c.stats = deck.Pop();
        }
    }

    void Shuffle()
    {
        var values = deck.ToArray();
        deck.Clear();

        for (int i = 0; i < values.Length; i++)
        {
            var index = Random.Range(0, i);
            var temp = values[index];
            values[index] = values[values.Length - 1];
            deck.Push(temp);
        }
    }
}
