using UnityEngine;

[CreateAssetMenu(fileName = "New Animal", menuName = "JungleLaw/Animal")]
public class AnimalData : ScriptableObject
{
    public string speciesName;
    public int moveRange;
    public int attackRange;
    public int maxHP;
}
