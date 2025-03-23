using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextDebugger : MonoBehaviour
{
    public TextMeshProUGUI text;
    Queue<string> debugMessages;
    int maxMessages = 15;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        debugMessages = new Queue<string>();

        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string message = $"[{type}] {logString}";
        debugMessages.Enqueue(message);

        if (debugMessages.Count > maxMessages)
            debugMessages.Dequeue();

        DisplayMessages();
    }

    void DisplayMessages()
    {
        string fullText = string.Join("\n", debugMessages.ToArray());
        text.text = fullText;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}