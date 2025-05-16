using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public float timeframe;
    public int populationSize;//создадим численность популяции
    public GameObject prefab;//holds bot prefab

    public int[] layers = new int[3] { 5, 3, 2 };//инициализация сети до нужного размера

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 10f)] public float Gamespeed = 1f;

    //public List<Bot> Bots;
    public List<NeuralNetwork> networks;
    private List<Bot> cars;
    public GameObject[] checkPoints;

    void Start()// вызывается перед обновлением первого кадра
    {
        File.Create("Assets/Save1.txt").Close();
        StreamWriter writer = new StreamWriter("Assets/Save1.txt", true);
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("CheckPoint");//Получить массив всех объектов по тегу
        checkPoints = taggedObjects.OrderBy(go => go.name).ToArray();
        //for (int i = 0; i < checkPoints.Length; i++)
        //{
        //    writer.WriteLine(checkPoints[i].name);
        //}
        //writer.Close();

            //Debug.Log("Start");
            if (populationSize % 2 != 0)
            populationSize = 50;//если размер популяции нечетный, устанавливает его равным пятидесяти

        InitNetworks();
        InvokeRepeating("CreateBots", 0.1f, timeframe);//повторяющаяся функция
        //вызывается через  0.1f секунд, повторяется через каждые timeframe секунд
    }

    public void InitNetworks()
    {
        //Debug.Log("InitNetworks");
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Pre-trained.txt");//при запуске загрузить сохраненную сеть
            networks.Add(net);
        }
    }

    public void CreateBots()
    {
        //Debug.Log("CreateBots");
        Time.timeScale = Gamespeed;//устанавливает скорость игры, которая будет увеличиваться для ускорения обучения
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                if (cars[i] != null)
                {
                    GameObject.Destroy(cars[i].gameObject);//если есть префабы в сцене то нужно избавиться от них
                }
            }

            SortNetworks();//сортирует сети и мутирует их
        }

        cars = new List<Bot>();
        for (int i = 0; i < populationSize; i++)
        {
            Bot car = (Instantiate(prefab, new Vector3(0, 1.6f, -45), new Quaternion(0, 0, 1, 0))).GetComponent<Bot>();//создаем ботов
            car.network = networks[i];//развертывает сеть для каждого учащегося
            car.tag = "Learner";
            cars.Add(car);
        }
        
    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            cars[i].UpdateFitness();//заставляет ботов настраивать свои соответствующие сети
        }
        networks.Sort();
        networks[populationSize - 1].Save("Assets/Save.txt");//сохраняет веса и смещения сетей в файл, чтобы сохранить производительность сети
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1/MutationChance), MutationStrength);
        }
    }
}
