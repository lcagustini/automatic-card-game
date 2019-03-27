using System.Net;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;

using UdpCNetworkDriver = Unity.Networking.Transport.BasicNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket>;

public class testClient : MonoBehaviour
{
    public UdpCNetworkDriver m_Driver;
    public NetworkConnection m_Connection;

#if !UNITY_SERVER
    private float timeout = 0;

    void Start()
    {
        Debug.Log("Starting client...");
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        m_Connection = default(NetworkConnection);

        var endpoint = new IPEndPoint(IPAddress.Loopback, 9000);
        m_Connection = m_Driver.Connect(endpoint);
        Debug.Log("Client started.");
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

    // TODO: make it not work when not connected
    public void SendSpawnMonster(int cardid, int monsterIndex, Vector3 pos)
    {
        card.requestLock = true;

        // TODO: think if there is something like sizeof(float) for better crossplatformness
        using (var writer = new DataStreamWriter(24, Allocator.Temp))
        {
            writer.Write((int)MessageType.REQUEST_SPAWN_MONSTER);
            writer.Write(cardid);
            writer.Write(monsterIndex);
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);
            m_Driver.Send(m_Connection, writer);
        }
    }

    public void AskNewHand()
    {
        card.requestLock = true;

        // TODO: think if there is something like sizeof(float) for better crossplatformness
        using (var writer = new DataStreamWriter(4, Allocator.Temp))
        {
            writer.Write((int)MessageType.REQUEST_NEW_HAND);
            m_Driver.Send(m_Connection, writer);
        }
    }

    public void MoveMonster(int id, float x, float z)
    {
        using (var writer = new DataStreamWriter(16, Allocator.Temp))
        {
            writer.Write((int)MessageType.MOVE_MONSTER);
            writer.Write(id);
            writer.Write(x);
            writer.Write(z);
            m_Driver.Send(m_Connection, writer);
        }
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            Debug.Log("Connection failed.");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        timeout += Time.deltaTime;
        if (timeout >= 25)
        {
            timeout = 0;
            using (var writer = new DataStreamWriter(4, Allocator.Temp))
            {
                Debug.Log("Pinging...");
                writer.Write((int)MessageType.PING);
                m_Driver.Send(m_Connection, writer);
            }
        } 

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) !=
               NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var readerCtx = default(DataStreamReader.Context);
                MessageType messageType = (MessageType) stream.ReadInt(ref readerCtx);
                switch (messageType)
                {
                    case MessageType.TEAM_ASSIGNMENT:
                        {
                            Camera.main.GetComponent<main>().info.team = (int)stream.ReadUInt(ref readerCtx);
                            Debug.Log("I was assigned the team " + Camera.main.GetComponent<main>().info.team);
                            Camera.main.GetComponent<main>().OnClientConnected();
                            break;
                        }
                    case MessageType.SPAWN_MONSTER:
                        {
                            int monsterID = stream.ReadInt(ref readerCtx);
                            int monsterTeam = stream.ReadInt(ref readerCtx);
                            int monsterIndex = stream.ReadInt(ref readerCtx);
                            Vector3 pos;
                            pos.x = stream.ReadFloat(ref readerCtx);
                            pos.y = stream.ReadFloat(ref readerCtx);
                            pos.z = stream.ReadFloat(ref readerCtx);

                            monster m = Camera.main.GetComponent<main>().SpawnMonster(monsterIndex, monsterTeam, pos);
                            m.id = monsterID;

                            Debug.Log("Received message from server: SPAWN_MONSTER " + monsterIndex + " " + monsterTeam + " " + pos + "id: " + monsterID);

                            break;
                        }
                    case MessageType.UPDATE_MONSTER:
                        {
                            GameObject[] monsters = GameObject.FindGameObjectsWithTag("monster");

                            int lenght = stream.ReadInt(ref readerCtx);
                            for (int i = 0; i < lenght; i++)
                            {
                                int id = stream.ReadInt(ref readerCtx);
                                float pos_x = stream.ReadFloat(ref readerCtx);
                                float pos_y = stream.ReadFloat(ref readerCtx);
                                float pos_z = stream.ReadFloat(ref readerCtx);

                                float rot_x = stream.ReadFloat(ref readerCtx);
                                float rot_y = stream.ReadFloat(ref readerCtx);
                                float rot_z = stream.ReadFloat(ref readerCtx);
                                float rot_w = stream.ReadFloat(ref readerCtx);
                                MonsterState state = (MonsterState) stream.ReadInt(ref readerCtx);
                                int health = stream.ReadInt(ref readerCtx);
                                float death = stream.ReadFloat(ref readerCtx);

                                //Debug.Log("id: "+id+" pos:"+pos_x+"/"+pos_y+"/"+pos_z+" state:"+state+" health:"+health+" death:"+death);

                                foreach (GameObject m_obj in monsters)
                                {
                                    monster m = m_obj.GetComponent<monster>();
                                    if (m.id == id)
                                    {
                                        m.transform.position = new Vector3(pos_x, pos_y, pos_z);
                                        m.transform.rotation = new Quaternion(rot_x, rot_y, rot_z, rot_w);
                                        m.state = state;
                                        m.health = health;
                                        m.death_countdown = death;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case MessageType.NEW_HAND:
                        {
                            card.requestLock = false;

                            int handSize = stream.ReadInt(ref readerCtx);
                            main m = Camera.main.GetComponent<main>();

                            GameObject[] hand_objects = GameObject.FindGameObjectsWithTag("hand_card");
                            foreach (var obj in hand_objects)
                            {
                                Destroy(obj);
                            }

                            for (int i = 0; i < handSize; i++)
                            {
                                Transform t = Instantiate(m.cardPrefab.transform, m.GetCardPosByTeam(), m.GetCardRotationByTeam());
                                card c = t.gameObject.GetComponent<card>();

                                c.id = stream.ReadInt(ref readerCtx);
                                c.targetPos = m.GetCardTargetByTeam(i);
                                c.team = m.info.team;
                            }

                            break;
                        }
                    case MessageType.UPDATE_MONEY:
                        {
                            Camera.main.GetComponent<main>().info.money = stream.ReadInt(ref readerCtx);
                            card.requestLock = false;

                            break;
                        }
                    case MessageType.UPDATE_PHASE:
                        {
                            main.current_phase = (RoundPhase)stream.ReadInt(ref readerCtx);
                            if (main.current_phase == RoundPhase.BATTLE)
                            {
                                main.phase_timer = 60F;

                                Camera.main.GetComponent<main>().phaseUI.color = Color.red;
                                Camera.main.GetComponent<main>().phaseUI.text = "Battle";

                                GameObject[] hand_objects = GameObject.FindGameObjectsWithTag("hand_card");
                                foreach (var obj in hand_objects)
                                {
                                    Destroy(obj);
                                }
                            }
                            else
                            {
                                main.phase_timer = 30F;

                                Camera.main.GetComponent<main>().phaseUI.color = Color.green;
                                Camera.main.GetComponent<main>().phaseUI.text = "Preparing";

                                GameObject.Find("Client").GetComponent<testClient>().AskNewHand();
                            }

                            break;
                        }
                    case MessageType.UPDATE_RANKINGS:
                        {
                            main.enable_ranks = true;

                            main.rankings[0].team = stream.ReadInt(ref readerCtx);
                            main.rankings[0].wins = stream.ReadInt(ref readerCtx);
                            main.rankings[1].team = stream.ReadInt(ref readerCtx);
                            main.rankings[1].wins = stream.ReadInt(ref readerCtx);
                            main.rankings[2].team = stream.ReadInt(ref readerCtx);
                            main.rankings[2].wins = stream.ReadInt(ref readerCtx);
                            main.rankings[3].team = stream.ReadInt(ref readerCtx);
                            main.rankings[3].wins = stream.ReadInt(ref readerCtx);

                            break;
                        }
                    default:
                        Debug.Log("Unexpected message received, aborting...");
                        Debug.Assert(false);
                        break;
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }
#endif
}