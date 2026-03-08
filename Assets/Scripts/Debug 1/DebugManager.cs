using UnityEngine;

public class DebugManager : MonoBehaviour
{
    private void Start()
    {
        //GameManager.WaveManager.SetWaveConfig(GameManager.ConfigManager.ListLevelConfig.LevelConfigs[0].WaveConfig);
        //GameManager.WaveManager.StartWaves();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UnityLogger.Log("Space key was pressed.");
            Time.timeScale = Time.timeScale == 1f ? 0f : 1f;
        }
        //if(Input.GetKeyDown(KeyCode.S))
        //{
        //    pathCreator.SpawnCarriages();
        //}
    }
}
