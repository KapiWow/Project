using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Server : MonoBehaviour
{

    public GameObject PlayerPrefab; // Персонаж игрока
    public string ip = "127.0.0.1"; // ip для создания или подключения к серверу
    public string port = "5300";    // Порт
    public bool connected;          // Статус подключения
    private GameObject _go;         // Объект для ссылки на игрока
    public bool _visible = false;   // Статус показа меню
    private bool upgrade = false;

    private float score = 0;
    private float maxHp = 10;
    private float damage = 1;
    private float maxSpeed = 10f;

    void Start()
    {

        List<float> saveIt = new List<float>();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
        saveIt = (List<float>)bf.Deserialize(file);
        score = saveIt[0];
        if (saveIt[1] > 10)
            maxHp = saveIt[1];
        if (saveIt[2] > 10)
            damage = saveIt[2];
        if (saveIt[1] > 10)
            maxSpeed = saveIt[3];
        file.Close();
    }
    // На каждый кадр
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            _visible = !_visible;
    }

    // На каждый кадр для прорисовки кнопок
    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 30, Screen.height - 30, 20, 20), "s"))
        {
            maxHp = 10;
            damage = 1;
            maxSpeed = 10;
            Save();
        }
        // Если мы на сервере
        if (upgrade)
        {
            GUI.Label(new Rect((Screen.width - 200) / 2, Screen.height / 2 - 80, 200, 30), "Меню улучшения персонажа");
            GUI.Label(new Rect((Screen.width - 200) / 2, Screen.height / 2 - 60, 200, 20), "Score:" + score.ToString());
            GUI.Label(new Rect((Screen.width - 200) / 2, Screen.height / 2 - 40, 200, 20), "MaxHP:" + maxHp.ToString());
            if (GUI.Button(new Rect(2 * (Screen.width - 150) / 3, Screen.height / 2 - 40, 20, 20), "+"))
            {
                if (score >= maxHp)
                {
                    score -= maxHp;
                    maxHp += 10;
                    Save();
                }
            }
            GUI.Label(new Rect((Screen.width - 200) / 2, Screen.height / 2 - 20, 200, 20), "Damage:" + damage.ToString());
            if (GUI.Button(new Rect(2 * (Screen.width - 150) / 3, Screen.height / 2 - 20, 20, 20), "+"))
            {
                if (score >= damage * 10)
                {
                    score -= damage * 10;
                    damage += 1;
                    Save();
                }
            }
            GUI.Label(new Rect((Screen.width - 200) / 2, Screen.height / 2, 200, 20), "MaxSpeed:" + maxSpeed.ToString());
            if (GUI.Button(new Rect(2 * (Screen.width - 150) / 3, Screen.height / 2, 20, 20), "+"))
            {
                if (score >= (maxSpeed - 10) * 100 + 10)
                {
                    score -= (maxSpeed - 10) * 100 + 10;
                    maxSpeed += (float)0.1;
                    Save();
                }
            }

            if (GUI.Button(new Rect((Screen.width - 150) / 2, Screen.height / 2 + 35, 150, 30), "Назад"))
                upgrade = false;
        }
        else if (connected)
        {
            if (_visible)
            {
                GUI.Label(new Rect((Screen.width - 120) / 2, Screen.height / 2 - 35, 120, 30), "Присоединились: " + Network.connections.Length);
                if (GUI.Button(new Rect((Screen.width - 100) / 2, Screen.height / 2, 100, 30), "Отключиться"))
                    Network.Disconnect(200);

                if (GUI.Button(new Rect((Screen.width - 100) / 2, Screen.height / 2 + 35, 100, 30), "Выход"))
                    Application.Quit();
            }
            //Если мы в главном меню
        }
        else
        {
            GUI.Label(new Rect((Screen.width - 100) / 2, Screen.height / 2 - 60, 100, 20), "Ip");
            GUI.Label(new Rect((Screen.width - 100) / 2, Screen.height / 2 - 30, 100, 20), "Порт");
            ip = GUI.TextField(new Rect((Screen.width - 100) / 2 + 35, Screen.height / 2 - 60, 100, 20), ip);
            port = GUI.TextField(new Rect((Screen.width - 100) / 2 + 35, Screen.height / 2 - 30, 50, 20), port);

            if (GUI.Button(new Rect((Screen.width - 150) / 2, Screen.height / 2, 150, 30), "Присоединиться"))
                Network.Connect(ip, Convert.ToInt32(port));

            if (GUI.Button(new Rect((Screen.width - 150) / 2, Screen.height / 2 + 35, 150, 30), "Создать сервер"))
                Network.InitializeServer(10, Convert.ToInt32(port), false);

            if (GUI.Button(new Rect((Screen.width - 150) / 2, Screen.height / 2 + 70, 150, 30), "Улучшение персонажа"))
                upgrade = true;

            if (GUI.Button(new Rect((Screen.width - 150) / 2, Screen.height / 2 + 105, 150, 30), "Выход"))
                Application.Quit();
        }
    }

    // Вызывается когда мы подключились к серверу
    void OnConnectedToServer()
    {
        CreatePlayer();
    }
    // Когда мы создали сервер
    void OnServerInitialized()
    {
        CreatePlayer();
    }

    // Создание игрока
    void CreatePlayer()
    {
        connected = true;
        GetComponent<Camera>().enabled = false;
        GetComponent<Camera>().gameObject.GetComponent<AudioListener>().enabled = false;
        _go = (GameObject)Network.Instantiate(PlayerPrefab, transform.position, transform.rotation, 1);
        _go.transform.GetComponentInChildren<Camera>().GetComponent<Camera>().enabled = true;
        _go.transform.GetComponentInChildren<AudioListener>().enabled = true;
    }

    // При отключении от сервера
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        connected = false;
        GetComponent<Camera>().enabled = true;
        GetComponent<Camera>().gameObject.GetComponent<AudioListener>().enabled = true;
        Application.LoadLevel(Application.loadedLevel);
    }

    // Вызывается каждый раз когда игрок отсоеденяется от сервера
    void OnPlayerDisconnected(NetworkPlayer pl)
    {
        Network.RemoveRPCs(pl);
        Network.DestroyPlayerObjects(pl);
    }

    void Save()
    {
        List<float> saveIt = new List<float>();
        saveIt.Add(score);
        saveIt.Add(maxHp);
        saveIt.Add(damage);
        saveIt.Add(maxSpeed);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd");
        bf.Serialize(file, saveIt);
        file.Close();
    }
}
