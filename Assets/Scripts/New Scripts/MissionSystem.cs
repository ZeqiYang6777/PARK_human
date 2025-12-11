using UnityEngine;
using System;

public class MissionSystem : MonoBehaviour
{
    public static MissionSystem Instance { get; private set; }

    private MissionData currentMission;

    public event Action<MissionData> OnMissionChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCurrentMission(MissionData mission)
    {
        currentMission = mission;
        Debug.Log($"任务更新: {mission.missionTitle}");

        OnMissionChanged?.Invoke(mission);
    }

    public MissionData GetCurrentMission()
    {
        return currentMission;
    }
}
