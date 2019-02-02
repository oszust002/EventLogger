using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoad;
	}

	private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
	{
		if ("Start Menu".Equals(scene.name) || "Win Screen".Equals(scene.name) || "Loose Screen".Equals(scene.name))
		{
			EventManager.Disable();
		}
		else
		{
			EventManager.Enable();
		}
		
		EventManager.LogEvent(Time.time, "LevelManager", "LoadLevel", scene.name);
	}

	public void LoadLevel(string name){
		Debug.Log ("New Level load: " + name);
		EventManager.LogEvent(Time.time, "LevelManager", "LevelChange", name);
		Brick.breakableCount = 0;
		Application.LoadLevel (name);
	}

	public void QuitRequest(){
		EventManager.LogEvent(Time.time, "LevelManager", "LevelChange", "GameQuit");
		Debug.Log ("Quit requested");
		Application.Quit ();
	}
	
	public void LoadNextLevel() {
		Brick.breakableCount = 0;
		var nextLevel = Application.loadedLevel + 1;
		EventManager.LogEvent(Time.time, "LevelManager", "LevelChange", "NextLevel", "LevelNumber", nextLevel);
		Application.LoadLevel(nextLevel);
		
	}
	
	public void BrickDestoyed() {
		EventManager.LogEvent(Time.time, "LevelManager", "BrickDestroyed", "BrickDestroyed", "BricksLeft", Brick.breakableCount);
		if (Brick.breakableCount <= 0) {
			LoadNextLevel();
		}
		
	}

	private void OnDisable()
	{
		EventManager.Disable(true);
		SceneManager.sceneLoaded -= OnSceneLoad;
	}
}
