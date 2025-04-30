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
    public float timeThresholdForEfficient = 20f;

    private string logFilePath;
    private int trashPickedCount = 0;
    private int helpHintCount = 0;
    private int spawnMoreCount = 0;
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

        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

        File.WriteAllText(logFilePath, "TimeSinceStart,TrashPicked,HelpHints,SpawnMores,PromptType,Label\n");
    }

    public void OnTrashPicked()
    {
        trashPickedCount++;
        LogGameplayData(PromptType.None);
    }

    public void OnPromptTriggered(PromptType promptType)
    {
        if (promptType == PromptType.HelpHint)
            helpHintCount++;
        else if (promptType == PromptType.SpawnMore)
            spawnMoreCount++;

        LogGameplayData(promptType);
    }

    private void LogGameplayData(PromptType promptType)
    {
        float timeSinceStart = Time.time - sessionStartTime;
        string logEntry = $"{timeSinceStart:F2},{trashPickedCount},{helpHintCount},{spawnMoreCount},{promptType},\n";
        File.AppendAllText(logFilePath, logEntry);
    }

    public void EndSessionAndLabel()
    {
        float timeSinceStart = Time.time - sessionStartTime;

        // Labeling logic based on time
        int label = (timeSinceStart <= timeThresholdForEfficient) ? 1 : 0;

        // Log the final entry, including the label
        string finalEntry = $"{timeSinceStart:F2},{trashPickedCount},{helpHintCount},{spawnMoreCount},Final,{label}\n";
        File.AppendAllText(logFilePath, finalEntry);

        Debug.Log($"GameplayLogger: Session ended with label {label} (time: {timeSinceStart:F2}s)");
    }
}