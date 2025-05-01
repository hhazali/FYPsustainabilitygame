using UnityEngine;
using Unity.Barracuda;

public class DifficultyPredictor : MonoBehaviour
{
    public NNModel modelAsset;
    private IWorker worker;

    void Start()
    {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
    }

    public int PredictDifficulty(float[] inputData)
    {
        Tensor inputTensor = new Tensor(1, 4, inputData); // 1 sample, 4 features
        worker.Execute(inputTensor);
        Tensor output = worker.PeekOutput();
        int prediction = output.ArgMax()[0];

        inputTensor.Dispose();
        output.Dispose();
        return prediction; // 0 or 1
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}