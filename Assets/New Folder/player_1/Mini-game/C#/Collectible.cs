using AmazingAssets.CurvedWorld.Examples;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private ChunkSpawner spawner;

    public void Initialize(ChunkSpawner spawner)
    {
        this.spawner = spawner;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spawner?.CollectItem();
            GetComponent<Collider>().enabled = false;
            GetComponent<Renderer>().enabled = false;
            Destroy(gameObject, 1f);
        }
    }
}
