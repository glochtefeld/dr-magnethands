using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Magnethands.Menus
{
    /// <summary>
    /// Finds all canvases in a scene and allows buttons/functions to 
    /// switch between them, either with an index or a direct reference.
    /// </summary>
    public class CanvasSwitcher : MonoBehaviour
    {
        private List<Canvas> canvasOptions = new List<Canvas>();
        [Header("BUTTONS")]
        public Button quitButton;
        private Canvas _activeCanvas;

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                canvasOptions.Add(transform.GetChild(i).GetComponent<Canvas>());
            }
            _activeCanvas = canvasOptions[0];
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
                quitButton.onClick.AddListener(() => Application.Quit());
        }

        public void SwitchCanvas(Canvas to)
        {
            _activeCanvas.gameObject.SetActive(false);
            to.gameObject.SetActive(true);
            _activeCanvas = to;
        }

        // allows direct indexing into canvasOptions, makes syntax nicer to look at
        public Canvas this[int i]
        {
            get { return canvasOptions[i]; }
        }
    }
}

