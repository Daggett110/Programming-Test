using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour {

    public string levelName;

    private void OnTriggerEnter(Collider other)
    {
        // If we come into contact with the player ..
        if (other.tag == "Player")
        {
            // Load the intended scene
            SceneManager.LoadScene(levelName);
        }
    }
}
