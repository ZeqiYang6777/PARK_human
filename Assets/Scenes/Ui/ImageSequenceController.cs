using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSequenceController : MonoBehaviour
{
    [Header("图片序列设置")]
    public List<GameObject> imagePrefabList = new List<GameObject>();
    public Transform parentTransform;
    [Header("高级设置")]
    public bool playOnStart = true;
    public float initialDelay = 1.0f;

    private bool isPlaying = false;
    private GameObject currentImageInstance;

    void Start()
    {
        if (parentTransform == null) parentTransform = this.transform;
        if (playOnStart && imagePrefabList.Count > 0) StartCoroutine(DelayedStart());
    }
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(initialDelay);
        StartSequence();
    }
    public void StartSequence()
    {
        if (!isPlaying && imagePrefabList.Count > 0) StartCoroutine(PlaySequenceCoroutine());
    }
    private IEnumerator PlaySequenceCoroutine()
    {
        isPlaying = true;
        Debug.Log($"开始播放图片序列，共 {imagePrefabList.Count} 张");
        bool currentImageFinished = false;
        for (int i = 0; i < imagePrefabList.Count; i++)
        {
            GameObject prefab = imagePrefabList[i];
            if (prefab == null)
            {
                Debug.LogWarning($"序列中第 {i + 1} 项为空，已跳过。");
                continue;
            }
            Debug.Log($"正在播放第 {i + 1} 张: {prefab.name}");
            currentImageInstance = Instantiate(prefab, parentTransform);
            currentImageInstance.SetActive(true);
            ImageNarratorController narratorCtrl = currentImageInstance.GetComponentInChildren<ImageNarratorController>();
            if (narratorCtrl != null)
            {
                currentImageFinished = false;
                narratorCtrl.OnNarrationComplete.AddListener(() => { currentImageFinished = true; });
                narratorCtrl.PlayNarration();
                yield return new WaitUntil(() => currentImageFinished);
                narratorCtrl.OnNarrationComplete.RemoveAllListeners();
            }
            else
            {
                Debug.LogError($"预制体 {prefab.name} 上未找到脚本，将使用固定延迟。");
                yield return new WaitForSeconds(5f);
            }
            if (currentImageInstance != null) Destroy(currentImageInstance);
        }
        Debug.Log("所有图片序列播放完毕。");
        isPlaying = false;
        OnSequenceComplete();
    }
    public void SkipSequence()
    {
        if (isPlaying)
        {
            StopAllCoroutines();
            if (currentImageInstance != null)
            {
                ImageNarratorController narratorCtrl = currentImageInstance.GetComponentInChildren<ImageNarratorController>();
                if (narratorCtrl != null) narratorCtrl.SkipNarration();
                Destroy(currentImageInstance);
            }
            isPlaying = false;
            Debug.Log("序列已被跳过。");
        }
    }
    private void OnSequenceComplete()
    {
        Debug.Log("SequenceController: 所有旁白播放完成！");
        // 在此触发游戏主逻辑
    }
}