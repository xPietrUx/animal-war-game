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
            // 20% szans na deszcz w tej rundzie
            if (Random.Range(0, 100) < 20)
            {
                WeatherManager.Instance.ChangeWeather(WeatherManager.WeatherCondition.Rain);
            }
            else
            {
                WeatherManager.Instance.ChangeWeather(WeatherManager.WeatherCondition.Clear);
            }
        }
        // 1. ZRESETUJ JEDNOSTKI I ZAKTUALIZUJ ICH KOLORY
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            a.ResetTurn();
            a.UpdateAllegianceColor(); // NOWOŚĆ: Każdej jednostce każemy przemyśleć swój kolor!
        }

        // 2. ZBIERZ ZŁOTO, MANĘ I ODPAL ARTYLERIĘ
        currentGoldIncome = baseGoldPerTurn; // Zamiast: int goldIncome = ...
        currentManaIncome = baseManaPerTurn; // Zamiast: int manaIncome = ...

        CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);

        foreach (CapturePoint point in allPoints)
        {
            // Jeśli budynek jest sporny, pomijamy go (nie generuje zysków!)
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

        // 3. DODAJ ZASOBY I ZAKTUALIZUJ UI
        if (currentTurn == TurnState.Player1)
        {
            p1_Gold += currentGoldIncome;
            p1_Mana += currentManaIncome;
            // Zmieniamy wywołanie - wysyłamy też DOCHÓD!
            UIManager.Instance.UpdateTurnInfo("PLAYER 1", p1_Gold, currentGoldIncome, p1_Mana, currentManaIncome);
        }
        else
        {
            p2_Gold += currentGoldIncome;
            p2_Mana += currentManaIncome;
            UIManager.Instance.UpdateTurnInfo("PLAYER 2", p2_Gold, currentGoldIncome, p2_Mana, currentManaIncome);
        }


        UIManager.Instance.UpdateBaseHP(p1_HP, p2_HP);
        Debug.Log($"UI zaktualizowane dla: {currentTurn}. Złoto: {currentGoldIncome}, Mana: {currentManaIncome}"); CalculateBaseDamage();
        UIManager.Instance.UpdateBaseHP(p1_HP, p2_HP);

        // Obliczamy numer drużyny (Player1 = 1, Player2 = 2)
        int currentTurnTeam = (currentTurn == TurnState.Player1) ? 1 : 2;

        // Zaktualizuj mgłę dla nowego gracza!
        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.UpdateFog(currentTurnTeam);
        }


        // Zaktualizuj mgłę dla nowego gracza!
        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.UpdateFog(currentTurnTeam);
        }
    }

    private void CalculateBaseDamage()
    {
        // Znajdź wszystkie punkty na mapie
        CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);

        foreach (CapturePoint point in allPoints)
        {
            // Jeśli budynek jest sporny, pomijamy go (nie generuje zysków!)
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
}