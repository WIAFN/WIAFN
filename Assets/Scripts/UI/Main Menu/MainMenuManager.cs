using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WIAFN.Constants;

namespace WIAFN.UI.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartGame()
        {
            SceneManager.LoadSceneAsync(WIAFNScenes.Loading);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
