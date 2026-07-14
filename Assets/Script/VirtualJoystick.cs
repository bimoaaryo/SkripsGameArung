using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static VirtualJoystick Instance { get; private set; }

    [Header("Referensi")]
    [Tooltip("Image transparan full-screen yang menangkap input tap/drag")]
    public RectTransform touchArea;

    [Tooltip("RectTransform lingkaran background joystick — muncul di posisi tap")]
    public RectTransform joystickBackground;

    [Tooltip("RectTransform handle/knob joystick — child dari joystickBackground")]
    public RectTransform handle;

    [Header("Pengaturan")]
    [Tooltip("Radius maksimum handle bisa bergerak dari pusat (dalam pixel)")]
    [Min(10f)] public float handleRange = 60f;

    [Tooltip("Deadzone — magnitude di bawah ini dianggap 0 (mencegah drift kecil)")]
    [Range(0f, 0.5f)] public float deadzone = 0.1f;

    [Header("Visual")]
    [Range(0f, 1f)] public float activeAlpha = 0.8f;

    public Vector2 Direction { get; private set; } = Vector2.zero;

    public float Magnitude { get; private set; } = 0f;

    public bool IsActive { get; private set; } = false;

    private CanvasGroup joystickCanvasGroup;
    private Canvas      parentCanvas;
    private Vector2     inputVector = Vector2.zero;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogWarning("[VirtualJoystick] Lebih dari satu instance ditemukan!");

        parentCanvas = GetComponentInParent<Canvas>();

        if (joystickBackground != null)
        {
            joystickCanvasGroup = joystickBackground.GetComponent<CanvasGroup>();
            if (joystickCanvasGroup == null)
                joystickCanvasGroup = joystickBackground.gameObject.AddComponent<CanvasGroup>();
        }

        SetJoystickVisible(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        IsActive = true;

        if (joystickBackground != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                touchArea, eventData.position, eventData.pressEventCamera, out localPoint);

            joystickBackground.anchoredPosition = localPoint;
        }

        SetJoystickVisible(true);

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (joystickBackground == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground, eventData.position, eventData.pressEventCamera, out localPoint);

        Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);

        if (handle != null)
            handle.anchoredPosition = clamped;

        inputVector = clamped / handleRange;
        Magnitude   = inputVector.magnitude;

        if (Magnitude < deadzone)
        {
            Direction = Vector2.zero;
            Magnitude = 0f;
        }
        else
        {
            Direction = inputVector.normalized;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsActive    = false;
        inputVector = Vector2.zero;
        Direction   = Vector2.zero;
        Magnitude   = 0f;

        SetJoystickVisible(false);
    }


    void SetJoystickVisible(bool visible)
    {
        if (joystickCanvasGroup == null) return;
        joystickCanvasGroup.alpha = visible ? activeAlpha : 0f;
    }
}