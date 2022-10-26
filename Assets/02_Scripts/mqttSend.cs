using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Threading.Tasks;

public class mqttSend : M2MqttUnityClient
{

    [Header("MQTT topics")]
    [Tooltip("Set the topic to subscribe. !!!ATTENTION!!! multi-level wildcard # subscribes to all topics")]
    public string topicSubscribe = "cmnd/UnityTest"; // topic to subscribe. 
    [Tooltip("Set the topic to publish (optional)")]
    //public string topicPublish = "Ingestion/UnityManager"; // topic to publish
    public string messagePublish = "test"; // message to publish
    private string topicLWT = "Ingestion/UnityTest"; // topic to  LWT


    public static mqttSend _mqttSend;

    private void Awake()
    {
        base.Awake();
        if (_mqttSend == null)
        {
            DontDestroyOnLoad(gameObject);
            _mqttSend = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //using C# Property GET/SET and event listener to reduce Update overhead in the controlled objects
    private string m_msg;

    public string msg
    {
        get
        {
            return m_msg;
        }
        set
        {
            if (m_msg == value) return;
            m_msg = value;
            if (OnMessageArrived != null)
            {
                OnMessageArrived(m_msg);
            }
        }
    }

    public event OnMessageArrivedDelegate OnMessageArrived;
    public delegate void OnMessageArrivedDelegate(string newMsg);

    //using C# Property GET/SET and event listener to expose the connection status
    private bool m_isConnected;

    public bool isConnected
    {
        get
        {
            return m_isConnected;
        }
        set
        {
            if (m_isConnected == value) return;
            m_isConnected = value;
            if (OnConnectionSucceeded != null)
            {
                OnConnectionSucceeded(isConnected);
            }
        }
    }
    public event OnConnectionSucceededDelegate OnConnectionSucceeded;
    public delegate void OnConnectionSucceededDelegate(bool isConnected);

    // a list to store the messages
    private List<string> eventMessages = new List<string>();


    public void publishData(string _topic, string msg)
    {
        if (isConnected)
        {
            client.Publish(
            _topic, System.Text.Encoding.UTF8.GetBytes(msg),
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            Debug.Log("message published: " + _topic + "--" + msg);
        }

    }
    public void publishLWT(string msg)
    {
        if (isConnected)
        {
            client.Publish(
            topicLWT, System.Text.Encoding.UTF8.GetBytes(msg),
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true);
            Debug.Log("LWT published: " + topicLWT + "--" + msg);
        }
    }
    public void SetEncrypted(bool isEncrypted)
    {
        this.isEncrypted = isEncrypted;
    }

    protected override void OnConnecting()
    {
        base.OnConnecting();
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        isConnected = true;
        //publishLWT("Online");
        //publishData("UnityTest/LWT", "Online");
        //Task.Run(() => PersistConnectionAsync());

    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        Debug.Log("CONNECTION FAILED! " + errorMessage);
    }

    protected override void OnDisconnected()
    {
        Debug.Log("Disconnected.");
        isConnected = false;
    }

    protected override void OnConnectionLost()
    {
        Debug.Log("CONNECTION LOST!");
        //base.Connect();
    }



    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { topicSubscribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { topicSubscribe });
    }

    protected override void Start()
    {
        string clientId = Guid.NewGuid().ToString();
        base.Start();
        //base.client.Connect(clientId, "", "", true, 1, true, "Ingestion/Unity", "Offline(DC)", true, 60);
        base.Connect();
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        //The message is decoded
        msg = System.Text.Encoding.UTF8.GetString(message);

        Debug.Log("Received: " + msg);
        Debug.Log("from topic: " + m_msg);

        StoreMessage(msg);

    }

    private void StoreMessage(string eventMsg)
    {
        if (eventMessages.Count > 50)
        {
            eventMessages.Clear();
        }
        eventMessages.Add(eventMsg);
    }

    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()

    }

    protected override void OnApplicationQuit()
    {
        //publishLWT("Offline(Quit)");
        base.OnApplicationQuit();
        //_tryReconnectMQTT = false;
    }
    private void OnDestroy()
    {
        base.OnDestroy();
        //Disconnect();
    }


}
