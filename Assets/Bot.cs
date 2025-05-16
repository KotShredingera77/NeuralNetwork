using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bot : MonoBehaviour
{
    public float speed;//Speed Multiplier
    public float rotation;//Rotation multiplier
    public LayerMask raycastMask;//Mask for the sensors

    private float[] input = new float[5];//input to the neural network
    public NeuralNetwork network;
    private Manager manager;
    public int position;//Номер контрольной точки на трассе
    public bool collided;//сообщить если автомобиль разбился

    private void Start()
    {
        raycastMask = LayerMask.GetMask("Wall");
        manager = GameObject.Find("Plane").GetComponent<Manager>();
    }
    void FixedUpdate()//FixedUpdate вызывается с постоянным интервалом
    {
        if (!collided)//если машина не столкнулась со стеной, она использует нейронную сеть для получения вывода
        {
            for (int i = 0; i < 5; i++)//рисует пять отладочных лучей в качестве входных данных
            {   //вычисление угла raycast
                Vector3 newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;
                RaycastHit hit;//сюда запишется инфо о пересечении луча, если это пересечение будет
                Ray Ray = new Ray(transform.position, newVector); //Новый луч из точки в направлении

                if (Physics.Raycast(Ray, out hit, 100, raycastMask))//пускаем луч
                {
                    input[i] = (10 - hit.distance) / 10;//возврщаем дистанцию , 1 значит  закрыть
                    //Debug.Log("дистанция "+hit.distance);
                }
                else
                {
                    input[i] = 0;//если ничего не обнаружено, вернет 0 в сеть
                }
                //просто для наглядности рисуем луч в окне Scene
                Debug.DrawLine(Ray.origin, transform.position+ (newVector*100), Color.red);//hit.point
            }

            float[] output = network.FeedForward(input);//Звонок в сеть для переадресации
        
            transform.Rotate(0, output[0] * rotation, 0, Space.World);//контролирует поворот автомобилей
            transform.position += this.transform.right * output[1] * speed;//контролирует движение автомобилей
        }   //позиция = позиция + значение
    }
private void OnTriggerEnter(Collider other)
{

 

    if (other.gameObject.tag == "CheckPoint")
       {
            //GameObject[] checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");//Получить массив всех объектов по тегу
            for (int i=0; i < manager.checkPoints.Length; i++)
            {   
                if(other.gameObject == manager.checkPoints[i] && i == (position + 1 + manager.checkPoints.Length) % manager.checkPoints.Length)
                          //&& - логическое и
                          //% - остаток от деления   например 13%10 = 3
                //Стандартная проверка накопительного счетчика при прохождения ворот для движении по кругу          
                {
                    //Debug.Log("checkPoints.Length = "+checkPoints[i].name);
                    position++;//если ворота на один впереди, они увеличивают позицию, 
                    //которая используется для фитнеса(производительности) сети.
                    break;
                }
            }
        }
    else if(other.gameObject.tag != "Learner")
        {
            //Debug.Log("other.gameObject.tag2 = "+other.gameObject.tag);
            collided = true;//остановить работу, если автомобиль столкнулся
        }
    //else
    //    Debug.Log("other.gameObject.tag3 = "+other.gameObject.tag);
}
void OnCollisionEnter(Collision collision)
    {
        
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))//проверить, проезжает ли машина ворота
        {
            //GameObject[] checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");//Получить массив всех объектов по тегу
            for (int i=0; i < manager.checkPoints.Length; i++)
            {
                if(collision.collider.gameObject == manager.checkPoints[i] && i == (position + 1 + manager.checkPoints.Length) % manager.checkPoints.Length)
                          //&& - логическое и
                          //% - остаток от деления   например 13%10 = 3
                //Стандартная проверка накопительного счетчика при прохождения ворот для движении по кругу          
                {
                    position++;//если ворота на один впереди, они увеличивают позицию, 
                    //которая используется для фитнеса(производительности) сети.
                    break;
                }
            }
        }
        else if(collision.collider.gameObject.layer != LayerMask.NameToLayer("Learner"))
        {
            collided = true;//остановить работу, если автомобиль столкнулся
            
        }
        //else {Debug.Log(collision.collider.gameObject.layer);}
    }


    public void UpdateFitness()
    {
        network.fitness = position;//обновляет фитнес сети для сортировки
    }
}
