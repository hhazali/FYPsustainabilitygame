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

        // Criteria for efficient gameplay (Label = 1)
        bool isEfficient = trashPickedCount >= 4 && helpHintCount <= 2 && spawnMoreCount <= 2;

        // Criteria for struggling gameplay (Label = 0)
        bool isStruggling = trashPickedCount < 4 || helpHintCount >= 3;

        int label;
        if (isEfficient)
        {
            label = 1; // Efficient gameplay
        }
        else if (isStruggling)
        {
            label = 0; // Struggling gameplay
        }
        else
        {
            // This could be a "balanced" session where neither efficiency nor struggle is dominant
            label = 1; // Default to 1 (could adjust this based on further insights)
        }

        // Log the final entry with the label
        string finalEntry = $"{timeSinceStart:F2},{trashPickedCount},{helpHintCount},{spawnMoreCount},Final,{label}\n";
        File.AppendAllText(logFilePath, finalEntry);

        Debug.Log($"GameplayLogger: Session ended with label {label} (time: {timeSinceStart:F2}s, trash picked: {trashPickedCount}, help hints: {helpHintCount}, difficulty increases: {spawnMoreCount})");
    }
}