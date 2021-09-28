
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalSetting;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
