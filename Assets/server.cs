using System.Net;
using UnityEngine;

using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine.Assertions;

using UdpCNetworkDriver = Unity.Networking.Transport.BasicNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket>;
using System.Collections.Generic;

public enum MessageType
{
    TEAM_ASSIGNMENT = 100,
    REQUEST_SPAWN_MONSTER,
    SPAWN_MONSTER,
    UPDATE_MONSTER,
    PING,
    REQUEST_NEW_HAND,
    NEW_HAND,
    UPDATE_MONEY
}

public class PlayerInfo
{
    public int team;
    public int money;
}

public class server : MonoBehaviour
{
#if UNITY_SERVER
    public UdpCNetworkDriver m_Driver;

    // NOTE: currently these two lists must be always in sync
    private NativeList<NetworkConnection> m_Connections;
    private List<PlayerInfo> players = new List<PlayerInfo>();
    private int nextTeamId = 0;

    private float update_timer = 0F;

    void Start()
    {
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind(new IPEndPoint(IPAddress.Any, 9000)) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(4, Allocator.Persistent);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void Update()
    {
        update_timer += Time.deltaTime;

        if (update_timer > 0.033F)
        {
            m_Driver.ScheduleUpdate().Complete();

            // CleanUpConnections
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                {
                    m_Connections.RemoveAtSwapBack(i);
                    players.RemoveAtSwapBack(i);
                    --i;
                }
            }

            // AcceptNewConnections
            NetworkConnection c;
            while ((c = m_Driver.Accept()) != default(NetworkConnection))
            {
                m_Connections.Add(c);
                players.Add(new PlayerInfo());
                Debug.Log("Accepted a connection.");

                players[players.Count - 1].money = 5;
                players[players.Count - 1].team = nextTeamId;
                Debug.Log("Assigned team " + nextTeamId + " to it.");

                using (var writer = new DataStreamWriter(8, Allocator.Temp))
                {
                    writer.Write((int)MessageType.TEAM_ASSIGNMENT);
                    writer.Write(nextTeamId);
                    m_Driver.Send(c, writer);
                }

                using (var writer = new DataStreamWriter(8, Allocator.Temp))
                {
                    writer.Write((int)MessageType.UPDATE_MONEY);
                    writer.Write(players[players.Count - 1].money);
                    m_Driver.Send(c, writer);
                }
                nextTeamId++;
            }

            DataStreamReader stream;
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                    Assert.IsTrue(true); // ???????

                NetworkEvent.Type cmd;
                while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) !=
                       NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        var readerCtx = default(DataStreamReader.Context);
                        MessageType messageType = (MessageType)stream.ReadInt(ref readerCtx);
                        switch (messageType)
                        {
                            case MessageType.REQUEST_SPAWN_MONSTER:
                                {
                                    int cardid = stream.ReadInt(ref readerCtx);
                                    Camera.main.GetComponent<main>().hands[players[i].team].Remove(cardid);

                                    int monsterIndex = stream.ReadInt(ref readerCtx);
                                    Vector3 pos;
                                    pos.x = stream.ReadFloat(ref readerCtx);
                                    pos.y = stream.ReadFloat(ref readerCtx);
                                    pos.z = stream.ReadFloat(ref readerCtx);

                                    if (card.playerArea[players[i].team].Contains(new Vector2(pos.x, pos.z)) && players[i].money >= main.allCards[cardid].cost)
                                    {
                                        monster m = Camera.main.GetComponent<main>().SpawnMonster(monsterIndex, players[i].team, pos);
                                        players[i].money -= main.allCards[cardid].cost;

                                        Debug.Log("Received message from " + i + ": SEND_SPAWN_MONSTER " + monsterIndex + " " + pos);

                                        for (int j = 0; j < m_Connections.Length; j++)
                                        {
                                            // TODO: think if there is something like sizeof(float) for better crossplatformness
                                            using (var writer = new DataStreamWriter(28, Allocator.Temp))
                                            {
                                                writer.Write((int)MessageType.SPAWN_MONSTER);
                                                writer.Write(m.id);
                                                writer.Write(players[i].team);
                                                writer.Write(monsterIndex);
                                                writer.Write(pos.x);
                                                writer.Write(pos.y);
                                                writer.Write(pos.z);
                                                m_Driver.Send(m_Connections[j], writer);
                                            }
                                        }
                                    }

                                    using (var writer = new DataStreamWriter(8, Allocator.Temp))
                                    {
                                        writer.Write((int)MessageType.UPDATE_MONEY);
                                        writer.Write(players[i].money);
                                        m_Driver.Send(m_Connections[i], writer);
                                    }

                                    break;
                                }
                            case MessageType.REQUEST_NEW_HAND:
                                {
                                    Debug.Log("Received message from "+i+": REQUEST_NEW_HAND");

                                    main m = Camera.main.GetComponent<main>();

                                    foreach (var card in m.hands[players[i].team])
                                    {
                                        m.deck.Push(card);
                                    }
                                    m.hands[players[i].team].Clear();

                                    m.Shuffle();

                                    int handSize = m.deck.Count > 5 ? 5 : m.deck.Count;
                                    for (int j = 0; j < handSize; j++)
                                    {
                                        m.hands[players[i].team].Add(m.deck.Pop());
                                    }

                                    using (var writer = new DataStreamWriter((handSize * 4) + 4, Allocator.Temp))
                                    {
                                        writer.Write((int)MessageType.NEW_HAND);
                                        writer.Write(handSize);
                                        for (int j = 0; j < handSize; j++)
                                        {
                                            writer.Write(m.hands[players[i].team][j]);
                                        }
                                        m_Driver.Send(m_Connections[i], writer);
                                    }

                                    break;
                                }
                            case MessageType.PING:
                                {
                                    //Debug.Log("Pong!");
                                    break;
                                }
                            default:
                                // TODO: think about fake packets
                                Debug.Log("Unexpected message received, ignoring...");
                                break;
                        }
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconnected from server");
                        m_Connections[i] = default(NetworkConnection);
                    }
                }
            }

            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (!m_Connections[i].IsCreated)
                    Assert.IsTrue(true); // ???????

                GameObject[] monsters = GameObject.FindGameObjectsWithTag("monster");

                using (var writer = new DataStreamWriter(28 * monsters.Length + 8, Allocator.Temp))
                {
                    writer.Write((int)MessageType.UPDATE_MONSTER);
                    writer.Write(monsters.Length);

                    foreach (GameObject m_obj in monsters)
                    {
                        monster m = m_obj.GetComponent<monster>();
                        writer.Write(m.id);
                        writer.Write(m.transform.position.x);
                        writer.Write(m.transform.position.y);
                        writer.Write(m.transform.position.z);
                        writer.Write(m.transform.rotation.x);
                        writer.Write(m.transform.rotation.y);
                        writer.Write(m.transform.rotation.z);
                        writer.Write(m.transform.rotation.w);
                        writer.Write((int)m.state);
                        writer.Write(m.health);
                        writer.Write(m.death_countdown);
                    }

                    m_Driver.Send(m_Connections[i], writer);
                }
            }

            update_timer = 0;
        }
    }
#endif
}