using UnityEngine;
using UnityEngine.Events;


public class RempahState : MonoBehaviour
{
    public static RempahState Instance { get; private set; }

    public UnityEvent OnRempahChanged;


    private bool punyaPala    = true;   // dibawa dari Maluku
    private bool punyaCengkeh = true;   // dibawa dari Maluku
    private bool punyaBeras   = false;  // dibeli di Malaka (Level 3)
    private bool punyaKain    = false;  // dibeli di Malaka (Level 3)
    private bool punyaKeramik = false;  // dibeli di Malaka (Level 3)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool PunyaRempah(string namaRempah)
    {
        return namaRempah.ToLower() switch
        {
            "pala"           => punyaPala,
            "cengkeh"        => punyaCengkeh,
            "beras"          => punyaBeras,
            "kain"           => punyaKain,
            "kain patola"    => punyaKain,
            "keramik"        => punyaKeramik,
            "keramik tiongkok" => punyaKeramik,
            _ => false
        };
    }



    public void JualRempah(string namaRempah)
    {
        switch (namaRempah.ToLower())
        {
            case "pala":             punyaPala    = false; break;
            case "cengkeh":          punyaCengkeh = false; break;
            case "beras":            punyaBeras   = false; break;
            case "kain":
            case "kain patola":      punyaKain    = false; break;
            case "keramik":
            case "keramik tiongkok": punyaKeramik = false; break;
        }
        OnRempahChanged?.Invoke();
        Debug.Log($"[RempahState] {namaRempah} terjual.");
    }

    public void BeliRempah(string namaRempah)
    {
        switch (namaRempah.ToLower())
        {
            case "pala":             punyaPala    = true; break;
            case "cengkeh":          punyaCengkeh = true; break;
            case "beras":            punyaBeras   = true; break;
            case "kain":
            case "kain patola":      punyaKain    = true; break;
            case "keramik":
            case "keramik tiongkok": punyaKeramik = true; break;
        }
        OnRempahChanged?.Invoke();
        Debug.Log($"[RempahState] {namaRempah} dibeli.");
    }

    public void ResetAll()
    {
        punyaPala    = true;  
        punyaCengkeh = true; 
        punyaBeras   = false;
        punyaKain    = false;
        punyaKeramik = false;
        OnRempahChanged?.Invoke();
        Debug.Log("[RempahState] Reset semua rempah ke kondisi awal.");
    }
}