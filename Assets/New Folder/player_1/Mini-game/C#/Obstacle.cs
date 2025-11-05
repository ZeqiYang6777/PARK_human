using AmazingAssets.CurvedWorld.Examples;
using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private ChunkSpawner spawner;

    public void Initialize(ChunkSpawner spawner)
    {
        this.spawner = spawner;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            spawner?.HitObstacle();
            StartCoroutine(FlashColor());
        }
    }

    System.Collections.IEnumerator FlashColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }
}