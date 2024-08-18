using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reversi2024.View.UI
{
    public class EndView : AbstractLayer
    {
        [SerializeField] private GameObject blackWinObject;
        [SerializeField] private GameObject whiteWinObject;
        [SerializeField] private GameObject drawObject;
        [SerializeField] private TextMeshProUGUI blackCounter;
        [SerializeField] private TextMeshProUGUI whiteCounter;
        [SerializeField] private Button resetButton;

        public Observable<Unit> OnClickReset => resetButton.OnClickAsObservable();

        public void Setup(int black, int white)
        {
            blackWinObject.SetActive(black > white);
            whiteWinObject.SetActive(black < white);
            drawObject.SetActive(black == white);
            blackCounter.text = $"●({black})";
            whiteCounter.text = $"●({white})";
        }
    }
}
