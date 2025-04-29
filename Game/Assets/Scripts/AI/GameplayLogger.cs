using System.IO;
using UnityEngine;

public enum PromptType
{
    None,
    HelpHint,
    SpawnMore
}

public class GameplayLogger : MonoBehaviour
{
    public static GameplayLogger Instance;

    private string logFilePath;
    private int trashPickedCount = 0;
    private int promptsTriggered = 0;
    private float sessionStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartNewSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StartNewSession()
    {
        sessionStartTime = Time.time;
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        logFilePath = Application.dataPath + "/Logs/GameplayLog_" + timestamp + ".csv";

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

        // Write CSV header
        File.WriteAllText(logFilePath, "TimeSinceStart,TrashPicked,PromptsTriggered,PromptType\n");
    }

    public void OnTrashPicked()
    {
        trashPickedCount++;
        Debug.Log("Trash picked: " + trashPickedCount);

        // ONLY log a trash pickup. Don't trigger HelpHint here.
        LogGameplayData(PromptType.None);
    }

    public void OnPromptTriggered(PromptType promptType)
    {
        promptsTriggered++;
        Debug.Log("Prompt triggered: " + promptType);

        LogGameplayData(promptType);
    }

    private void LogGameplayData(PromptType promptType)
    {
        float timeSinceStart = Time.time - sessionStartTime;
        string logEntry = $"{timeSinceStart:F2},{trashPickedCount},{promptsTriggered},{promptType}\n";
        File.AppendAllText(logFilePath, logEntry);
    }
}