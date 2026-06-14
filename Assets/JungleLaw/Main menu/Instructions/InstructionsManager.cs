using UnityEngine;
using UnityEngine.UI;

public class InstructionsManager : MonoBehaviour
{
    [Header("Elementy interfejsu")]
    public Image displayImage;
    public Button leftButton;
    public Button rightButton;

    [Header("Slajdy z obrazkami")]
    public Sprite[] pages;

    private int currentPage = 0;

    void OnEnable()
    {
        // Za kaødym razem, gdy gracz wchodzi w instrukcje, resetujemy widok do 1. strony
        currentPage = 0;
        UpdatePage();
    }

    public void NextPage()
    {
        // Przejdü dalej, jeúli to nie jest ostatnia strona
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PreviousPage()
    {
        // WrÛÊ, jeúli to nie jest pierwsza strona
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        // 1. Zmiana obrazka na aktualny
        if (pages.Length > 0)
        {
            displayImage.sprite = pages[currentPage];
        }

        // 2. W≥πczanie lub wy≥πczanie strza≥ek na skrajnych stronach
        leftButton.interactable = (currentPage > 0);
        rightButton.interactable = (currentPage < pages.Length - 1);
    }
}