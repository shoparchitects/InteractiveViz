using M2MqttUnity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using shop;
using Newtonsoft.Json;

public class mqttReceiver : M2MqttUnityClient
{
    public static mqttReceiver _mqttReceiver;
    [Header("MQTT topics")]
    [Tooltip("Set the topic to subscribe. !!!ATTENTION!!! multi-level wildcard # subscribes to all topics")]
    // topic to subscribe. !!! The multi-level wildcard # is used to subscribe to all the topics. Attention i if #, subscribe to all topics. Attention if MQTT is on data plan
    string[] topicSubscribe = { "M2MQTT_Unity/test/#", "Test/#", "Test/test"};  //"Moetsii/Bodies", 
    byte[] qosLevelsSubscribe = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };

    
     
    [Tooltip("Set the topic to publish (optional)")]
    public string topicPublish = "SHoPortal"; // topic to publish
    public string messagePublish = "test"; // message to publish

    [Tooltip("Set this to true to perform a testing cycle automatically on startup")]
    public bool autoTest = false;

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
    private Dictionary<string, string> MQTTMessages = new Dictionary<string, string>();


    public void Publish(string _topic, string msg)
    {
        if (client == null)
            return;
        client.Publish(
            _topic, System.Text.Encoding.UTF8.GetBytes(msg),
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        Debug.Log("message published: " + _topic + "--" + msg);
    }

    public void ToggleSwitch()
    {
        if (isConnected)
        {
            Publish("cmnd/plug2/POWER", "toggle");
        }
        else
        {

        }

    }

    //public void PublishLWT(string _topic, string msg)
    //{
    //    if (client == null)
    //        return;
    //    client.Publish(
    //        _topic, System.Text.Encoding.UTF8.GetBytes(msg),
    //        MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    //}
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
        //PublishLWT("SHoPortal/" + clientId, "Online");
        //Task.Run(() => PersistConnectionAsync());

        if (autoTest)
        {
            Publish(topicPublish, messagePublish);
        }
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
        
    }
    
    static bool _tryReconnectMQTT = true;
    private async Task PersistConnectionAsync()
    {
        var connected = client.IsConnected;
        while (_tryReconnectMQTT)
        {
            //Debug.Log("Persisting MQTT connection...");
            if (!connected)
            {
                try
                {
                    Debug.Log("Reconnecting...");
                    base.Connect();
                }
                catch
                {
                    Debug.Log("failed reconnect");
                }
            }
            await Task.Delay(5000);
            connected = client.IsConnected;
        }
    }

    protected override void SubscribeTopics()
    {
        if (client == null)
            return;
        client.Subscribe(topicSubscribe, qosLevelsSubscribe);
    }

    protected override void UnsubscribeTopics()
    {
        if (client == null)
            return;
        client.Unsubscribe(topicSubscribe);
    }

    protected override void Start()
    {
        base.Start();
    }


    protected override void DecodeMessage(string topic, byte[] message)
    {
        //The message is decoded and messages are relayed to respective functions
        switch (topic)
        {
            case "M2MQTT_Unity/test/Attitude":
                Debug.Log("Recived" + "M2MQTT_Unity/test/Attitude");
                msg = System.Text.Encoding.UTF8.GetString(message);
                Debug.Log("Recived" + msg);
                Movemodel._Movemodel.moveModel(msg);

                break;
            case "Moetsii/Colabs":

                break;
            default:
                msg = System.Text.Encoding.UTF8.GetString(message);
                break;
        }

        //Debug.Log("Received: " + msg);
        //Debug.Log("from topic: " + topic);

        //StoreMessage(topic,msg);
    }




    private void StoreMessage(string eventTopic, string eventMsg)
    {
        if (eventMessages.Count > 50)
        {
            eventMessages.Clear();
        }
        eventMessages.Add(eventMsg);
        if (MQTTMessages.Count > 50)
        {
            MQTTMessages.Clear();
        }
        MQTTMessages.Add(eventTopic,eventMsg);
    }

    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()

    }

    private void OnDestroy()
    {
        //PublishLWT("SHoPortal/" + clientId, "Offline(stopped)");
        //Disconnect();
    }

    private void OnValidate()
    {
        if (autoTest)
        {
            autoConnect = true;
        }
    }


    protected override void Awake()
    {
        base.Awake();
        if (_mqttReceiver == null)
        {
            //DontDestroyOnLoad(gameObject);
            _mqttReceiver = this;
        }
        else
        {
            Destroy(this);
        }
        

    }
}