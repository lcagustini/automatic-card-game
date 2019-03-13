using System.Net;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;

using UdpCNetworkDriver = Unity.Networking.Transport.BasicNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket>;

public class testClient : MonoBehaviour
{
    public UdpCNetworkDriver m_Driver;
    public NetworkConnection m_Connection;

#if UNITY_SERVER
#else
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
    public void SendSpawnMonster(int monsterIndex, Vector3 pos)
    {
        // TODO: think if there is something like sizeof(float) for better crossplatformness
        using (var writer = new DataStreamWriter(20, Allocator.Temp))
        {
            writer.Write((int)MessageType.REQUEST_SPAWN_MONSTER);
            // TODO: this probably should not use the allCards array, maybe we should create an allMonsters array and cardData should refer to It?
            writer.Write(monsterIndex);
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);
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
                            Camera.main.GetComponent<main>().team = (int)stream.ReadUInt(ref readerCtx);
                            Debug.Log("I was assigned the team " + Camera.main.GetComponent<main>().team);
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
                                MonsterState state = (MonsterState) stream.ReadInt(ref readerCtx);
                                int health = stream.ReadInt(ref readerCtx);
                                float death = stream.ReadFloat(ref readerCtx);

                                Debug.Log("id: "+id+" pos:"+pos_x+"/"+pos_y+"/"+pos_z+" state:"+state+" health:"+health+" death:"+death);

                                foreach (GameObject m_obj in monsters)
                                {
                                    monster m = m_obj.GetComponent<monster>();
                                    if (m.id == id)
                                    {
                                        m.transform.position = new Vector3(pos_x, pos_y, pos_z);
                                        m.state = state;
                                        m.health = health;
                                        m.death_countdown = death;
                                        break;
                                    }
                                }
                            }
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