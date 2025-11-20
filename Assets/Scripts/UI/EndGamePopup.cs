using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class EndGamePopup : MonoBehaviour
    {
        private const string WIN_TEXT = "YOU WON";
        private const string LOSE_TEXT = "YOU LOSE";

        [SerializeField] private TMP_Text txtResult;
        [SerializeField] private Button btnRestart;

        private GameManager gameManager;
        
        [Inject]
        private void Construct(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }
        private void Awake()
        {
            btnRestart.onClick.AddListener(Restart);
        }

        public void Show(bool isWin)
        {
            txtResult.SetText(isWin ? WIN_TEXT : LOSE_TEXT);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Restart()
        {
            gameManager.RestartGame();
        }
    }
}