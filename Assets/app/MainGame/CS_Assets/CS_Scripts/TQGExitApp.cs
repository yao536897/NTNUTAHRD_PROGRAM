using UnityEngine;
using System.Collections;

public class TQGExitApp : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		// Make sure user is on Android platform
		if (Application.platform == RuntimePlatform.Android )
		{
			// Check if Back was pressed this frame
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ExitApp();
			}
		}
	}

    public void ExitApp()
    {
        Application.Quit();
    }
}
