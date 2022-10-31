using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class mqttHandle : M2MqttUnityClient
{

    [Header("MQTT topics")]
    [Tooltip("Set the topic(s) to subscribe. !!!ATTENTION!!! multi-level wildcard # subscribes to all topics")]
    public string[] topics2Subscribe = { "M2MQTT_Unity/test/#", "cmnd/UnityTest", "Test/test" };
    /// set the QoS level for each topic
    byte[] qosLevelsSubscribe = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
    
    
    [Tooltip("Set the topic to publish (optional)")]
    public string topicPublish = "IViz/Test"; // topic to publish
    public string messagePublish = "test"; // message to publish
    private string topicLWT = "IViz/Test/LWT"; // topic to  LWT
    public static mqttHandle _MqttHandle;

    //read in light object from scene to modify
    public Light sceneLight;
    //read in material from scene to modify
    public Material objectMaterial;
    public float maxlightVal = 400;



    //UI elements
    private bool updateUI = false;
    [Header("User Interface")]
    public TMP_InputField consoleInputField;
    public InputField addressInputField;
    public InputField portInputField;
    public Toggle encryptedToggle;
    public Button connectButton;
    public Button disconnectButton;
    public Button testPublishButton;
    public Button clearButton;


    private void Awake()
    {
        base.Awake();

        if (_MqttHandle == null)
        {
            DontDestroyOnLoad(gameObject);
            _MqttHandle = this;
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
            AddUiMessage(DateTime.Now+"published: " + _topic + "--" + msg);
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

    protected override void OnConnecting()
    {
        base.OnConnecting();
        AddUiMessage("Connecting to broker on "+brokerAddress+":"+brokerPort.ToString()+"...\n");
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        isConnected = true;
        //publishLWT("Online");
        //publishData("UnityTest/LWT", "Online");
        

    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        Debug.Log("CONNECTION FAILED! " + errorMessage);
        AddUiMessage("CONNECTION FAILED! " + errorMessage);
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

    public void SetEncrypted(bool isEncrypted)
    {
        this.isEncrypted = isEncrypted;
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(topics2Subscribe, qosLevelsSubscribe);
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(topics2Subscribe);
    }

    protected override void Start()
    {
        string clientId = Guid.NewGuid().ToString();
        base.Start();
        //base.client.Connect(clientId, "", "", true, 1, true, "Ingestion/Unity", "Offline(DC)", true, 60);
        base.Connect();
    }

    public float floatMap(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
    

    protected override void DecodeMessage(string topic, byte[] message)
    {
        //The message is decoded and messages are relayed to respective functions
        switch (topic)
        {
            case "M2MQTT_Unity/test/Attitude":
                //Debug.Log("Recived" + "M2MQTT_Unity/test/Attitude");
                msg = System.Text.Encoding.UTF8.GetString(message);
                //Debug.Log("Recived" + msg);
                //Movemodel._Movemodel.moveModel(msg);

                break;
            case "M2MQTT_Unity/test/Light":
                msg = System.Text.Encoding.UTF8.GetString(message);
                //convert msg to float
                float lightValue = float.Parse(msg);
                Debug.Log("Light Value: " + lightValue);
                //map lightValue from 0 to 1
                float lightIntensity = floatMap(lightValue, 0, maxlightVal, 0, 1);
                //set DirectionalLight value
                sceneLight.intensity = lightIntensity;
                break;
            case "M2MQTT_Unity/test/Proximity":
                msg = System.Text.Encoding.UTF8.GetString(message);
                //convert msg to float
                float proximity = float.Parse(msg);
                //map proximity to boolean
                bool inProx = proximity < 0.5f;
                Debug.Log("Proximity: " + inProx);
                //if in Proximity use a blue color, otherwise use the regular gray
                if (inProx)
                {
                    objectMaterial.color = new Color(0.30588f, 0.92941f, 0.89804f);
                }
                else
                {
                    objectMaterial.color = new Color(0.8f, 0.8f, 0.8f);
                }

                //add in code to change materials diffuse color for example

                break;
            default:
                msg = System.Text.Encoding.UTF8.GetString(message);
                break;
        }

        //Debug.Log("Received: " + msg);
        //Debug.Log("from topic: " + m_msg);

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

        //sceneLight.intensity = Mathf.PingPong(Time.time, 1);

        base.Update(); // call ProcessMqttEvents()
        if (eventMessages.Count > 0)
        {
            foreach (string msg in eventMessages)
            {
                ProcessMessage(msg);
            }
            eventMessages.Clear();
        }

        if (updateUI)
        {
            UpdateUI();
        }

    }

    public void SetBrokerAddress(string brokerAddress)
    {
        if (addressInputField && !updateUI)
        {
            this.brokerAddress = brokerAddress;
        }
    }

    public void SetBrokerPort(string brokerPort)
    {
        if (portInputField && !updateUI)
        {
            int.TryParse(brokerPort, out this.brokerPort);
        }
    }
    private void ProcessMessage(string msg)
    {
        AddUiMessage("Received: " + msg);
    }
    

    public void SetUiMessage(string msg)
    {
        if (consoleInputField != null)
        {
            consoleInputField.text = msg;
            updateUI = true;
        }
    }
    
    public void AddUiMessage(string msg)
    {
        if (consoleInputField != null)
        {
            consoleInputField.text += msg + "\n";
            updateUI = true;
        }
    }

    private void UpdateUI()
    {
        //if scene name is Sensor2MQTT
        if (SceneManager.GetActiveScene().name == "Sensor2MQTT")
        {

            if (client == null)
            {
                if (connectButton != null)
                {
                    connectButton.interactable = true;
                    disconnectButton.interactable = false;
                    testPublishButton.interactable = false;
                }
            }
            else
            {
                if (testPublishButton != null)
                {
                    testPublishButton.interactable = client.IsConnected;
                }
                if (disconnectButton != null)
                {
                    disconnectButton.interactable = client.IsConnected;
                }
                if (connectButton != null)
                {
                    connectButton.interactable = !client.IsConnected;
                }
            }
            if (addressInputField != null && connectButton != null)
            {
                addressInputField.interactable = connectButton.interactable;
                addressInputField.text = brokerAddress;
            }
            if (portInputField != null && connectButton != null)
            {
                portInputField.interactable = connectButton.interactable;
                portInputField.text = brokerPort.ToString();
            }
            if (clearButton != null && connectButton != null)
            {
                clearButton.interactable = connectButton.interactable;
            }
            updateUI = false;
        }
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
