using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SceneInitializer))]
public class TrialCourseInitializer : MonoBehaviour
{
    [SerializeField]
    private TrialRunController mainController;

    [Header("Test Data")]

    [SerializeField]
    private CourseRunner playerPrefab;

    [SerializeField]
    private AttachableInputSource inputPrefab;

    [SerializeField]
    private List<CourseChunk> chunkList;

    private void Awake()
    {
        GetComponent<SceneInitializer>().RegisterCallback(InitializeScene);
        GetComponent<SceneInitializer>().RegisterEditorSceneSeeder(SeedSceneWithMockData);
    }

    private void InitializeScene(ISceneLoader loader, System.Action callback)
    {
        var layout = loader.GetContext<TrialCourseLayout>();
        var playerRegistration = loader.GetContext<PlayerRegistration>();

        var parameters = new TrialParameters(playerRegistration, layout);
        mainController.BootGame(parameters);
    }

    private void SeedSceneWithMockData(IDebugSceneSeeder seeder)
    {
        var inputSourceInstance = Instantiate(inputPrefab);
        seeder.AddContext(new PlayerRegistration(inputSourceInstance, playerPrefab));
        seeder.AddContext(new TrialCourseLayout(chunkList));
    }
}
