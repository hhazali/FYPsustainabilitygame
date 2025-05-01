using System.IO;
using UnityEngine;
using Unity.Barracuda;

public enum PromptType
{
    None,
    HelpHint,
    SpawnMore
}

public class GameplayLogger : MonoBehaviour
{
    public static GameplayLogger Instance;

    [Header("AI Model Settings")]
    public NNModel onnxModelAsset;

    [Header("Threshold Settings")]
    public float timeThresholdForEfficient = 20f;

    private string logFilePath;
    private int trashPickedCount = 0;
    private int helpHintCount = 0;
    private int spawnMoreCount = 0;
    private float sessionStartTime;

    // Barracuda model fields
    private Model runtimeModel;
    private IWorker worker;
    private float[] means = { 14.76f, 3.12f, 2.42f, 54.1498f };
    private float[] stds  = { 9.37456132f, 2.7541968f, 1.35779233f, 23.60896635f };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartNewSession();

            runtimeModel = ModelLoader.Load(onnxModelAsset);
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
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

        float[] inputRaw = new float[]
        {
            trashPickedCount,
            helpHintCount,
            spawnMoreCount,
            timeSinceStart
        };

        int label = PredictClass(inputRaw); // AI-predicted label (0 = efficient, 1 = inefficient)

        string finalEntry = $"{timeSinceStart:F2},{trashPickedCount},{helpHintCount},{spawnMoreCount},Final,{label}\n";
        File.AppendAllText(logFilePath, finalEntry);

        Debug.Log($"GameplayLogger: Session ended with AI label {label} (time: {timeSinceStart:F2}s, trash picked: {trashPickedCount}, help hints: {helpHintCount}, difficulty increases: {spawnMoreCount})");
    }

    private float[] NormalizeInput(float[] raw)
    {
        float[] norm = new float[raw.Length];
        for (int i = 0; i < raw.Length; i++)
            norm[i] = (raw[i] - means[i]) / stds[i];
        return norm;
    }

    private int PredictClass(float[] inputRaw)
    {
        float[] normalized = NormalizeInput(inputRaw);
        Tensor inputTensor = new Tensor(1, 4, normalized);

        worker.Execute(inputTensor);
        Tensor output = worker.PeekOutput();
        int predictedClass = output.ArgMax()[0];

        inputTensor.Dispose();
        output.Dispose();

        return predictedClass;
    }

    private void OnDestroy()
    {
        if (worker != null)
            worker.Dispose();
    }
}