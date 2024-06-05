using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/menuParams", order = 1)]
public class menuParams : ScriptableObject
{
    public bool useProofTraps = true;
    public bool useFloorTraps = true;
    public int numFloors = 3;
}