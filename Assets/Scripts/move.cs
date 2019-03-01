using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float verticalOffset = Input.GetAxis("Vertical");
        float horizontalOffset = Input.GetAxis("Horizontal");

        Vector3 newPosition = transform.position;
        newPosition.z += verticalOffset;
        newPosition.x += horizontalOffset;

        transform.SetPositionAndRotation(newPosition, transform.rotation);

        GameObject[] handCards = GameObject.FindGameObjectsWithTag("hand_card");
        foreach (GameObject card in handCards)
        {
            card.GetComponent<card>().targetPos += new Vector3(horizontalOffset, 0, verticalOffset);
        }
    }
}
