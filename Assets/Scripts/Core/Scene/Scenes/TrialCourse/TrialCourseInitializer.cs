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
    private bool spawnCPUs;

    [SerializeField]
    private List<CourseRunner> computerPlayerPrefabs;

    [SerializeField]
    private AttachableInputSource inputPrefab;

    [SerializeField]
    private TrainingCourse course;

    private void Awake()
    {
        GetComponent<SceneInitializer>().RegisterCallback(InitializeScene);
        GetComponent<SceneInitializer>().RegisterEditorSceneSeeder(SeedSceneWithMockData);
    }

    private void InitializeScene(ISceneLoader loader, System.Action callback)
    {
        var layout = loader.GetContext<TrialCourseLayout>();
        var roster = loader.GetContext<CourseRoster>();

        var parameters = new TrialParameters(roster, layout);
        mainController.BootGame(parameters);
    }

    private void SeedSceneWithMockData(IDebugSceneSeeder seeder)
    {
        var inputSourceInstance = Instantiate(inputPrefab);
        var players = new List<PlayerRegistration>();
        players.Add(new PlayerRegistration(inputSourceInstance, playerPrefab));

        if (spawnCPUs)
            foreach (var cpu in computerPlayerPrefabs)
                players.Add(new PlayerRegistration(null, cpu));

        seeder.AddContext(new CourseRoster(players));
        seeder.AddContext(new TrialCourseLayout(course.chunks));
    }
}
