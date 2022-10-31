///
/// Read mobile devices sensors and publish to MQTT broker

///

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using UnityEngine.InputSystem.Android;

public class SenseSomething : MonoBehaviour
{

    public static SenseSomething _senseSomething;
    mqttHandle _MqttHandle;
    

    //MQTT topics
    string topicLight = "M2MQTT_Unity/test/Light";
    string topicProximity = "M2MQTT_Unity/test/Proximity";
    string topicAccel = "M2MQTT_Unity/test/Accel";
    string topicGravity = "M2MQTT_Unity/test/Gravity";
    string topicAttitude = "M2MQTT_Unity/test/Attitude";
    string topicMagnetic = "M2MQTT_Unity/test/Magnetic";
    string topicStepCounter = "M2MQTT_Unity/test/StepCounter";
    string topicHumidity = "M2MQTT_Unity/test/Humidity";
    string topicPressure = "M2MQTT_Unity/test/Pressure";
    string topicTemperature = "M2MQTT_Unity/test/Temperature";



    //For MQTT messages
    string messageLight;
    string messageAccel;
    string messageMagnetic;
    string messageProximity;
    string messageStepCounter;
    string messageHumidity;
    string messagePressure;
    string messageTemperature;
    string messageGravity;
    string messageAttitude;

    public bool toggleAccelerometer = false;
    public bool toggleProximity = true;
    public bool toggleLight = true;
    public bool toggleMagnetometer = true;



    //interval to publish MQTT messages
    float lastPublish = 0;
    public float PublishInterval = 2f;

    //interval to read sensors
    float lastRead = 0;
    public float ReadInterval = 0.5f;

    private void Awake()
    {
        if (_senseSomething == null)
        {
            //DontDestroyOnLoad(gameObject);
            _senseSomething = this;
        }
        else
        {
            Destroy(gameObject);
        }



    }


    // Start is called before the first frame update
    void Start()
    {
        //Lock orientation to portrait
        Screen.orientation = ScreenOrientation.Portrait;
        //Keep screen on
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        _MqttHandle = GameObject.FindGameObjectWithTag("MQTT").GetComponent<mqttHandle>();

        //if android device
#if UNITY_ANDROID
        InputSystem.EnableDevice(LightSensor.current);
        InputSystem.EnableDevice(Accelerometer.current);
        InputSystem.EnableDevice(AndroidGyroscope.current);
        InputSystem.EnableDevice(GravitySensor.current);
        InputSystem.EnableDevice(AttitudeSensor.current);
        InputSystem.EnableDevice(MagneticFieldSensor.current);
        InputSystem.EnableDevice(ProximitySensor.current);
        InputSystem.EnableDevice(StepCounter.current);
        InputSystem.EnableDevice(HumiditySensor.current);
        InputSystem.EnableDevice(AmbientTemperatureSensor.current);
        InputSystem.EnableDevice(PressureSensor.current);

#endif
#if UNITY_IOS
        InputSystem.EnableDevice(Accelerometer.current);
        InputSystem.EnableDevice(GravitySensor.current);
        InputSystem.EnableDevice(AttitudeSensor.current);
#endif

    }

    // Update is called once per frame
    void Update()
    {
        if ((Time.time - lastRead) >= ReadInterval)
        {
            if (LightSensor.current != null)
            {
                Debug.Log("Light: "+LightSensor.current.lightLevel.ReadValue());
                messageLight = LightSensor.current.lightLevel.ReadValue().ToString();
            }
            else
            {
                messageLight = "0";
            }

            if (Accelerometer.current != null)
            {
                Debug.Log("Accel: " + Accelerometer.current.acceleration.ReadValue());
                messageAccel = Accelerometer.current.acceleration.ReadValue().ToString();
            }
            else
            {
                messageAccel = "0";
            }

            if (MagneticFieldSensor.current != null)
            {
                Debug.Log("Mag: " + MagneticFieldSensor.current.magneticField.ReadValue());
                messageMagnetic = MagneticFieldSensor.current.magneticField.ReadValue().ToString();
            }
            else
            {
                messageMagnetic = "0";
            }

            if (ProximitySensor.current != null)
            {
                Debug.Log("Prox: " + ProximitySensor.current.distance.ReadValue());
                messageProximity = ProximitySensor.current.distance.ReadValue().ToString();
            }
            else
            {
                messageProximity = "0";
            }

            if (HumiditySensor.current != null)
            {
                Debug.Log("Hum: " + HumiditySensor.current.relativeHumidity.ReadValueAsObject());
                messageHumidity = HumiditySensor.current.relativeHumidity.ReadValueAsObject().ToString();
            }
            else
            {
                messageHumidity = "0";
            }

            if (AmbientTemperatureSensor.current != null)
            {
                Debug.Log("Temp: " + AmbientTemperatureSensor.current.ambientTemperature.ReadValueAsObject());
                messageTemperature = AmbientTemperatureSensor.current.ambientTemperature.ReadValueAsObject().ToString();
            }
            else
            {
                messageTemperature = "0";
            }

            if (PressureSensor.current != null)
            {
                Debug.Log("Pres: " + PressureSensor.current.atmosphericPressure.ReadValueAsObject());
                messagePressure = (PressureSensor.current.atmosphericPressure.ReadValueAsObject().ToString());
            }
            else
            {
                messagePressure = "0";
            }

            if (GravitySensor.current != null)
            {
                Debug.Log("Grav: " + GravitySensor.current.gravity.ReadValueAsObject());
                messageGravity = GravitySensor.current.gravity.ReadValueAsObject().ToString();
            }
            else
            {
                messageGravity = "0";
            }

            if (AttitudeSensor.current != null)
            {
                Debug.Log("Att: " + AttitudeSensor.current.attitude.ReadValueAsObject());
                messageAttitude = AttitudeSensor.current.attitude.ReadValueAsObject().ToString();
            }
            else
            {
                messageAttitude = "0";
            }

            if (StepCounter.current != null)
            {
                Debug.Log("Step: " + StepCounter.current.stepCounter.ReadValueAsObject());
                messageStepCounter = StepCounter.current.stepCounter.ReadValueAsObject().ToString();
            }
            else
            {
                messageStepCounter = "0";
            }







        }
                
        if ((Time.time - lastPublish) >= PublishInterval)
        {

            //mqttHandle._MqttHandle.publishData(topicHumidity, messageHumidity);
            //Debug.Log("Published: " + messageHumidity);
            //mqttHandle._MqttHandle.publishData(topicTemperature, messageTemperature);
            //Debug.Log("Published: " + messageTemperature);
            //mqttHandle._MqttHandle.publishData(topicPressure, messagePressure);
            //Debug.Log("Published: " + messagePressure);
            //mqttHandle._MqttHandle.publishData(topicStepCounter, messageStepCounter);
            //Debug.Log("Published: " + messageStepCounter);
            if (toggleAccelerometer)
            {
                mqttHandle._MqttHandle.publishData(topicGravity, messageGravity);
                Debug.Log("Published: " + messageGravity);
                mqttHandle._MqttHandle.publishData(topicAttitude, messageAttitude);
                Debug.Log("Published: " + messageAttitude);
                mqttHandle._MqttHandle.publishData(topicAccel, messageAccel);
                Debug.Log("Published: " + messageAccel);
            }
            if (toggleProximity)
            {
                mqttHandle._MqttHandle.publishData(topicProximity, messageProximity);
                Debug.Log("Published: " + messageProximity);
            }
            if (toggleLight)
            {
                mqttHandle._MqttHandle.publishData(topicLight, messageLight);
                Debug.Log("Published: " + messageLight);
            }
            if (toggleMagnetometer)
            {
                mqttHandle._MqttHandle.publishData(topicMagnetic, messageMagnetic);
                Debug.Log("Published: " + messageMagnetic);
            }

            lastPublish = Time.time;
        }
    }

    public void changePublishInterval(float newInterval)
    {
        PublishInterval = newInterval;
    }
    
    public void publishFast()
    {
        PublishInterval = 1f;
        ReadInterval = 0.5f;
    }

    public void publishSlow()
    {
        PublishInterval = 3f;
        ReadInterval = 1f;
    }

    public void sendToggle()
    {

        mqttHandle._MqttHandle.publishData("M2MQTT_Unity/test/Toggle", "toggle");
    }

    public void toggleAccelerometerSensor()
    {
        if (toggleAccelerometer)
        {
            toggleAccelerometer = false;
            InputSystem.DisableDevice(Accelerometer.current);
            InputSystem.DisableDevice(GravitySensor.current);
            InputSystem.DisableDevice(AttitudeSensor.current);
            //InputSystem.DisableDevice(MagneticFieldSensor.current);
        }
        else
        {
            toggleAccelerometer = true;
            InputSystem.EnableDevice(Accelerometer.current);
            InputSystem.EnableDevice(GravitySensor.current);
            InputSystem.EnableDevice(AttitudeSensor.current);
            //InputSystem.EnableDevice(MagneticFieldSensor.current);
        }
    }

    public void toggleProximitySensor()
    {
        if (toggleProximity)
        {
            toggleProximity = false;
            InputSystem.DisableDevice(ProximitySensor.current);
        }
        else
        {
            toggleProximity = true;
            InputSystem.EnableDevice(ProximitySensor.current);
        }
    }

    public void toggleLightSensor()
    {
        if (toggleLight)
        {
            toggleLight = false;
            InputSystem.DisableDevice(LightSensor.current);
        }
        else
        {
            toggleLight = true;
            InputSystem.EnableDevice(LightSensor.current);
        }
    }

    public void togglemagnetometerSensor()
    {
        if (toggleMagnetometer)
        {
            toggleMagnetometer = false;
            InputSystem.DisableDevice(MagneticFieldSensor.current);
        }
        else
        {
            toggleMagnetometer = true;
            InputSystem.EnableDevice(MagneticFieldSensor.current);
        }
    }


}
