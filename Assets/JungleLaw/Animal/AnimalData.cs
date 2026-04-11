using UnityEngine;

[CreateAssetMenu(fileName = "New Animal", menuName = "JungleLaw/Animal")]
public class AnimalData : ScriptableObject
{
    public string speciesName;
    public int moveRange;
    public int attackRange;
    public int maxHP;
    public int maxAttack;

    [Header("Grafiki Emocji")]
    public Sprite idleSprite;
    public Sprite angrySprite;
    public Sprite surpriseSprite;

    [Header("Animacja Œmierci")]
    public Sprite[] deathFrames;
    public Sprite graveSprite;

}
