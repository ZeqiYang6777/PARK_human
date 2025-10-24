using UnityEngine;

public class PipeCameraController : MonoBehaviour
{
    [Header("相机设置")]
    public Transform target;
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -5f);
    public float lookAhead = 2f;

    private Transform currentPipe;
    private Vector3 smoothedPosition;

    void LateUpdate()
    {
        if (target == null) return;

        // 找到目标所在的管道
        FindTargetPipe();

        if (currentPipe != null)
        {
            // 计算相机位置（在管道内部，稍微在玩家后面）
            Vector3 pipeForward = currentPipe.forward;
            Vector3 targetForwardPosition = target.position + pipeForward * lookAhead;

            // 计算相机偏移（保持在管道中心）
            Vector3 cameraPosition = targetForwardPosition + offset;

            // 平滑移动相机
            smoothedPosition = Vector3.Lerp(transform.position, cameraPosition, followSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // 相机看向玩家前方一点的位置
            transform.LookAt(target.position + pipeForward * 2f);
        }
    }

    void FindTargetPipe()
    {
        // 简单的射线检测找到玩家所在的管道
        RaycastHit hit;
        if (Physics.Raycast(target.position, Vector3.down, out hit, 3f))
        {
            if (hit.collider.CompareTag("Pipe"))
            {
                currentPipe = hit.collider.transform;
            }
        }
    }
}