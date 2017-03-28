/* Script allowing for the player to reload the level is anything breaks
 * and quit the game
 * */
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetButtonDown("Reset"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if(Input.GetButtonDown("Quit"))
        {
            Application.Quit();
        }
    }
}
