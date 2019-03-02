using System.Net;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;

using UdpCNetworkDriver = Unity.Networking.Transport.BasicNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket>;

public class testClient : MonoBehaviour
{
    public UdpCNetworkDriver m_Driver;
    public NetworkConnection m_Connection;

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
                Camera.main.GetComponent<main>().team = (int) stream.ReadUInt(ref readerCtx);
                Debug.Log("I was assigned the team " + Camera.main.GetComponent<main>().team);
                Camera.main.GetComponent<main>().OnClientConnected();
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }
}