using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Reversi2024.View.UI
{
    public class TurnView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void UpdateTurn(bool isBlack)
        {
            text.text = $"{(isBlack ? "黒" : "白")}の番です";
        }
    }
}
