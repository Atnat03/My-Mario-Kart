using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugConsoleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform logContainer;
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    [Header("Settings")]
    [SerializeField] private int maxEntries = 100;
    [SerializeField] private bool showLogs    = true;
    [SerializeField] private bool showWarnings = true;
    [SerializeField] private bool showErrors   = true;

    private readonly List<GameObject> _entries = new();

    // Couleurs
    private static readonly Color ColorLog     = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color ColorWarning = new Color(1f,    0.85f, 0.3f);
    private static readonly Color ColorError   = new Color(1f,    0.35f, 0.35f);

    void OnEnable()  => Application.logMessageReceived += HandleLog;
    void OnDisable() => Application.logMessageReceived -= HandleLog;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            panel.SetActive(!panel.activeSelf);
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        // Filtres
        if (type == LogType.Log     && !showLogs)     return;
        if (type == LogType.Warning && !showWarnings) return;
        if (type is LogType.Error or LogType.Exception or LogType.Assert && !showErrors) return;

        // Supprimer les vieilles entrées si on dépasse le max
        if (_entries.Count >= maxEntries)
        {
            Destroy(_entries[0]);
            _entries.RemoveAt(0);
        }

        GameObject entry = Instantiate(logEntryPrefab, logContainer);
        TMP_Text text = entry.GetComponent<TMP_Text>();

        string prefix = type switch
        {
            LogType.Warning                                   => "[WARN] ",
            LogType.Error or LogType.Exception or LogType.Assert => "[ERR]  ",
            _                                                 => "[LOG]  "
        };

        Color color = type switch
        {
            LogType.Warning                                   => ColorWarning,
            LogType.Error or LogType.Exception or LogType.Assert => ColorError,
            _                                                 => ColorLog
        };

        text.text  = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{prefix}</color>{message}";
        _entries.Add(entry);

        // Auto-scroll vers le bas
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // Appelable depuis des boutons UI
    public void ToggleLogs()     => showLogs     = !showLogs;
    public void ToggleWarnings() => showWarnings = !showWarnings;
    public void ToggleErrors()   => showErrors   = !showErrors;
    public void ClearAll()
    {
        foreach (var e in _entries) Destroy(e);
        _entries.Clear();
    }
}