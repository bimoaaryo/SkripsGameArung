using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntryUI : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text namaText;
    public TMP_Text skorText;
    public TMP_Text gelarText;

    [Header("Highlight Pemain")]
    [Tooltip("Background highlight saat ini adalah pemain aktif")]
    public Image    backgroundImage;
    public Color    colorNormal   = new Color(1f, 1f, 1f, 0f);
    public Color    colorHighlight = new Color(1f, 0.85f, 0.2f, 0.3f);

    public void SetData(int rank, string nama, int skor, string gelar,
                        bool isPemain = false)
    {
        if (rankText  != null) rankText.text  = $"#{rank}";
        if (namaText  != null) namaText.text  = isPemain ? $"▶ {nama}" : nama;
        if (skorText  != null) skorText.text  = $"{skor}";
        if (gelarText != null) gelarText.text = gelar;

        if (backgroundImage != null)
            backgroundImage.color = isPemain ? colorHighlight : colorNormal;
    }

    public void SetEntry(int rank, string nama, int skor, string gelar)
        => SetData(rank, nama, skor, gelar);

    public void SetEntry(int rank, string nama, int skor)
        => SetData(rank, nama, skor, "");
}