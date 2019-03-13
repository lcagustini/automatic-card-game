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
    SEND_SPAWN_MONSTER,
    RECV_SPAWN_MONSTER
}

public class PlayerInfo
{
    public int team;
}

public class server : MonoBehaviour
{
    public UdpCNetworkDriver m_Driver;

    // NOTE: currently these two lists must be always in sync
    private NativeList<NetworkConnection> m_Connections;
    private List<PlayerInfo> players = new List<PlayerInfo>();
    private int nextTeamId = 0;

#if UNITY_SERVER
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

            players[players.Count - 1].team = nextTeamId;
            Debug.Log("Assigned team " + nextTeamId + " to it.");

            using (var writer = new DataStreamWriter(8, Allocator.Temp))
            {
                writer.Write((int)MessageType.TEAM_ASSIGNMENT);
                writer.Write(nextTeamId);
                m_Driver.Send(c, writer);
                nextTeamId++;
            }
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
                        case MessageType.SEND_SPAWN_MONSTER:
                            {
                                int monsterIndex = stream.ReadInt(ref readerCtx);
                                Vector3 pos;
                                pos.x = stream.ReadFloat(ref readerCtx);
                                pos.y = stream.ReadFloat(ref readerCtx);
                                pos.z = stream.ReadFloat(ref readerCtx);

                                Debug.Log("Received message from " + i + ": SEND_SPAWN_MONSTER " + monsterIndex + " " + pos);

                                for (int j = 0; j < m_Connections.Length; j++)
                                {
                                    // TODO: think if there is something like sizeof(float) for better crossplatformness
                                    using (var writer = new DataStreamWriter(24, Allocator.Temp))
                                    {
                                        writer.Write((int)MessageType.RECV_SPAWN_MONSTER);
                                        writer.Write(players[i].team);
                                        writer.Write(monsterIndex);
                                        writer.Write(pos.x);
                                        writer.Write(pos.y);
                                        writer.Write(pos.z);
                                        m_Driver.Send(m_Connections[j], writer);
                                    }
                                }
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
    }
#endif
}