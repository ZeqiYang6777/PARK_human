using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("旋转设置")]
    public float rotationSpeed = 100f;
    public Vector3 rotationAxis = Vector3.up;

    [Header("浮动设置")]
    public bool enableFloat = true;
    public float floatSpeed = 2f;
    public float floatAmount = 0.3f;

    [Header("视觉效果")]
    public bool addGlow = true;
    public Color glowColor = Color.yellow;
    public float glowIntensity = 1.5f;

    [Header("音效")]
    public AudioClip coinCollectSound; // 供玩家脚本读取

    private Vector3 startPosition;
    private Light coinLight;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.position;
        randomOffset = Random.Range(0f, 100f);

        // 确保有碰撞器
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }
        col.isTrigger = false; // ← 改为 false，使用物理碰撞

        // 添加光效
        if (addGlow)
        {
            AddLight();
        }

        // 确保 Tag
        if (!gameObject.CompareTag("Coin"))
        {
            gameObject.tag = "Coin";
        }
    }

    void Update()
    {
        // 旋转
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        // 上下浮动
        if (enableFloat)
        {
            float newY = startPosition.y +
                Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatAmount;
            transform.position = new Vector3(
                transform.position.x,
                newY,
                transform.position.z
            );
        }
    }

    void AddLight()
    {
        GameObject lightObj = new GameObject("CoinLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        coinLight = lightObj.AddComponent<Light>();
        coinLight.type = LightType.Point;
        coinLight.color = glowColor;
        coinLight.range = 3f;
        coinLight.intensity = glowIntensity;
    }

    

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
