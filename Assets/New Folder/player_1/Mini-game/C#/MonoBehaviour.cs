using UnityEngine;
using UnityEditor; // 需要添加这个命名空间

public class 模型地面设置工具 : MonoBehaviour
{
    [Header("地面设置")]
    public bool 自动设置碰撞体 = true;
    public bool 使用简化碰撞体 = false;
    public PhysicMaterial 物理材质;

#if UNITY_EDITOR
    [ContextMenu("设置选中模型为地面")]
    void 设置选中模型为地面()
    {
        GameObject[] 选中物体 = Selection.gameObjects;

        foreach (GameObject 物体 in 选中物体)
        {
            设置单个模型为地面(物体);
        }

        Debug.Log($"已设置 {选中物体.Length} 个模型为地面");
    }
#endif

    void 设置单个模型为地面(GameObject 模型)
    {
        // 设置层级
        int 地面层级 = LayerMask.NameToLayer("Ground");
        if (地面层级 == -1)
        {
            Debug.LogError("请先创建 'Ground' 层级！");
            return;
        }
        模型.layer = 地面层级;

        // 添加碰撞体
        if (自动设置碰撞体)
        {
            Collider 现有碰撞体 = 模型.GetComponent<Collider>();
            if (现有碰撞体 == null)
            {
                if (使用简化碰撞体)
                {
                    // 使用盒子碰撞体简化
                    BoxCollider 盒子碰撞体 = 模型.AddComponent<BoxCollider>();
                    if (物理材质 != null)
                        盒子碰撞体.material = 物理材质;
                }
                else
                {
                    // 使用网格碰撞体
                    MeshCollider 网格碰撞体 = 模型.AddComponent<MeshCollider>();
                    网格碰撞体.convex = false;
                    if (物理材质 != null)
                        网格碰撞体.material = 物理材质;
                }
            }
        }

        Debug.Log($"已设置 {模型.name} 为地面");
    }
}