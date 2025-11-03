using UnityEngine;

public class Collectible : MonoBehaviour
{
    public static System.Action OnItemCollected;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 触发收集事件
            OnItemCollected?.Invoke();

            // 播放收集效果
            GetComponent<Collider>().enabled = false;
            GetComponent<Renderer>().enabled = false;

            Destroy(gameObject, 1f);
        }
    }
}