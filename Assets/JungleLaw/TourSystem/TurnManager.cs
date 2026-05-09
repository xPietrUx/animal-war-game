using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState { Player1, Player2 }
    public TurnState currentTurn;

    [Header("Player 1 Stats")]
    public int p1_HP = 100;
    public int p1_Gold = 50;

    [Header("Player 2 Stats")]
    public int p2_HP = 100;
    public int p2_Gold = 50;

    [Header("Mana")]
    public int p1_Mana = 0;
    public int p2_Mana = 0;
    public int baseManaPerTurn = 1;

    [Header("Economy")]
    public int baseGoldPerTurn = 30; // Podstawowe złoto, które dostaje się zawsze
    [HideInInspector] public int currentGoldIncome;
    [HideInInspector] public int currentManaIncome;

    private void Awake() => Instance = this;

    void Start()
    {
        currentTurn = TurnState.Player1;
        StartTurn();

    }

    public void EndTurn()
    {
        // Jeśli HP zeszło poniżej zera (gra skończona), blokujemy zmianę tury!
        if (p1_HP <= 0 || p2_HP <= 0) return;

        // Zmiana gracza
        currentTurn = (currentTurn == TurnState.Player1) ? TurnState.Player2 : TurnState.Player1;

        Debug.Log("Teraz tura: " + currentTurn);
        StartTurn();
    }



    private void StartTurn()
    {
        // Losowanie pogody tylko gdy zaczyna Gracz 1 (Początek nowego "Dnia")
        if (currentTurn == TurnState.Player1)
        {
            if (Random.Range(0, 100) < 20) WeatherManager.Instance.ChangeWeather(WeatherManager.WeatherCondition.Rain);
            else WeatherManager.Instance.ChangeWeather(WeatherManager.WeatherCondition.Clear);
        }

        // 1. ZRESETUJ JEDNOSTKI I ZAKTUALIZUJ ICH KOLORY
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            a.ResetTurn();
            a.UpdateAllegianceColor();
        }

        // 2. ZBIERZ ZŁOTO, MANĘ I ZADAJ OBRAŻENIA BAZOM
        currentGoldIncome = baseGoldPerTurn; 
        currentManaIncome = baseManaPerTurn; 

        CalculateBaseDamage(); // Tu już wszystko liczymy poprawnie dzięki poprawce poniżej

        // 3. DODAJ ZASOBY I ZAKTUALIZUJ UI
        if (currentTurn == TurnState.Player1)
        {
            p1_Gold += currentGoldIncome;
            p1_Mana += currentManaIncome;
            UIManager.Instance.UpdateTurnInfo("PLAYER 1", p1_Gold, currentGoldIncome, p1_Mana, currentManaIncome);
        }
        else
        {
            p2_Gold += currentGoldIncome;
            p2_Mana += currentManaIncome;
            UIManager.Instance.UpdateTurnInfo("PLAYER 2", p2_Gold, currentGoldIncome, p2_Mana, currentManaIncome);
        }

        UIManager.Instance.UpdateBaseHP(p1_HP, p2_HP);
        
        // 4. SPRAWDZENIE WARUNKU ZWYCIĘSTWA PO ODJĘCIU PUNKTÓW ZDROWIA(HP) BAzy
        CheckWinCondition();

        int currentTurnTeam = (currentTurn == TurnState.Player1) ? 1 : 2;

        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.UpdateFog(currentTurnTeam);
        }
    }

    private void CalculateBaseDamage()
    {
        CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);

        foreach (CapturePoint point in allPoints)
        {
            if (point.isContested) continue;

            if (point.ownerTeam == 1 && currentTurn == TurnState.Player1)
            {
                currentGoldIncome += point.goldPerTurn;
                currentManaIncome += point.manaPerTurn;
                if (point.isAttackPoint) p2_HP -= point.damagePerTurn;
            }
            else if (point.ownerTeam == 2 && currentTurn == TurnState.Player2)
            {
                currentGoldIncome += point.goldPerTurn;
                currentManaIncome += point.manaPerTurn;
                if (point.isAttackPoint) p1_HP -= point.damagePerTurn;
            }
        }
    }

    private void CheckWinCondition()
    {
        Debug.Log($"Sprawdzanie stanu bazy: P1 HP = {p1_HP}, P2 HP = {p2_HP}");

        if (GameOverManager.Instance == null)
        {
            Debug.LogError("BRAK INSTANCJI GameOverManager! Upewnij się, że obiekt z tym skryptem jest aktywny na scenie.");
            return;
        }

        // Sprawdzamy czy którykolwiek gracz ma 0 lub less punktów zdrowia
        if (p1_HP <= 0 && p2_HP <= 0)
        {
            GameOverManager.Instance.ShowGameOver("REMIS");
            this.enabled = false; // <<< TO BLOKUJE DALSZĄ GRĘ
        }
        else if (p1_HP <= 0)
        {
            Debug.Log("Zniszczono bazę Gracza 1!");
            GameOverManager.Instance.ShowGameOver("Gracz 2");
            this.enabled = false; // <<< TO BLOKUJE DALSZĄ GRĘ
        }
        else if (p2_HP <= 0)
        {
            Debug.Log("Zniszczono bazę Gracza 2!");
            GameOverManager.Instance.ShowGameOver("Gracz 1");
            this.enabled = false; // <<< TO BLOKUJE DALSZĄ GRĘ
        }
    }
}