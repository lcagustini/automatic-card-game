using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

#if !UNITY_SERVER
    // Update is called once per frame
    void Update()
    {
        float verticalOffset = Input.GetAxis("Vertical");
        float horizontalOffset = Input.GetAxis("Horizontal");

        Vector3 newPosition = transform.position;

        switch (Camera.main.GetComponent<main>().team)
        {
            case 0:
                {
                    newPosition.z += verticalOffset;
                    newPosition.x += horizontalOffset;


                    GameObject[] handCards = GameObject.FindGameObjectsWithTag("hand_card");
                    foreach (GameObject card in handCards)
                    {
                        card.GetComponent<card>().transform.position += new Vector3(horizontalOffset, 0, verticalOffset);
                        card.GetComponent<card>().targetPos += new Vector3(horizontalOffset, 0, verticalOffset);
                    }
                }
                break;
            case 1:
                {
                    newPosition.z -= verticalOffset;
                    newPosition.x -= horizontalOffset;


                    GameObject[] handCards = GameObject.FindGameObjectsWithTag("hand_card");
                    foreach (GameObject card in handCards)
                    {
                        card.GetComponent<card>().transform.position += new Vector3(-horizontalOffset, 0, -verticalOffset);
                        card.GetComponent<card>().targetPos += new Vector3(-horizontalOffset, 0, -verticalOffset);
                    }
                }
                break;
            case 2:
                {
                    newPosition.z += horizontalOffset;
                    newPosition.x -= verticalOffset;


                    GameObject[] handCards = GameObject.FindGameObjectsWithTag("hand_card");
                    foreach (GameObject card in handCards)
                    {
                        card.GetComponent<card>().transform.position += new Vector3(-verticalOffset, 0, horizontalOffset);
                        card.GetComponent<card>().targetPos += new Vector3(-verticalOffset, 0, horizontalOffset);
                    }
                }
                break;
            case 3:
                {
                    newPosition.z -= horizontalOffset;
                    newPosition.x += verticalOffset;


                    GameObject[] handCards = GameObject.FindGameObjectsWithTag("hand_card");
                    foreach (GameObject card in handCards)
                    {
                        card.GetComponent<card>().transform.position += new Vector3(verticalOffset, 0, -horizontalOffset);
                        card.GetComponent<card>().targetPos += new Vector3(verticalOffset, 0, -horizontalOffset);
                    }
                }
                break;
        }
        
        transform.SetPositionAndRotation(newPosition, transform.rotation);
    }
#endif
}
