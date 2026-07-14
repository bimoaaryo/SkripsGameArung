using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RempahHUD : MonoBehaviour
{

    [Header("Slot Rempah")]
    public SlotRempah slotPala;
    public SlotRempah slotCengkeh;
    public SlotRempah slotBeras;
    public SlotRempah slotKain;
    public SlotRempah slotKeramik;


    void Start()
    {
        if (RempahState.Instance == null)
        {
            Debug.LogWarning("[RempahHUD] RempahState.Instance tidak ditemukan!");
            return;
        }

        RempahState.Instance.OnRempahChanged.AddListener(RefreshHUD);
        RefreshHUD();
    }

    void OnDestroy()
    {
        if (RempahState.Instance == null) return;
        RempahState.Instance.OnRempahChanged.RemoveListener(RefreshHUD);
    }

    void RefreshHUD()
    {
        if (RempahState.Instance == null) return;

        UpdateSlot(slotPala,    RempahState.Instance.PunyaRempah("pala"));
        UpdateSlot(slotCengkeh, RempahState.Instance.PunyaRempah("cengkeh"));
        UpdateSlot(slotBeras,   RempahState.Instance.PunyaRempah("beras"));
        UpdateSlot(slotKain,    RempahState.Instance.PunyaRempah("kain"));
        UpdateSlot(slotKeramik, RempahState.Instance.PunyaRempah("keramik"));
    }

    void UpdateSlot(SlotRempah slot, bool punya)
    {
        if (slot == null || slot.containerObject == null) return;

        slot.containerObject.SetActive(punya);
    }
}


[System.Serializable]
public class SlotRempah
{
    [Tooltip("GameObject container slot ini — diaktifkan/nonaktifkan berdasarkan state rempah")]
    public GameObject containerObject;

    [Tooltip("Ikon rempah")]
    public Image ikonRempah;

    [Tooltip("Nama rempah (opsional)")]
    public TMP_Text namaText;
}