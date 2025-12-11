using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Game/Mission")]
public class MissionData : ScriptableObject
{
    public string missionID;
    public string missionTitle;

    [TextArea(3, 5)]
    public string description;

    public Sprite missionImage;
}
