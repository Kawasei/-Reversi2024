using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reversi2024.View.UI.CounterView
{
    public class CounterView : MonoBehaviour
    {
        [SerializeField] private CounterCell blackCounterCell;
        [SerializeField] private CounterCell whiteCounterCell;

        public void UpdateCount(int black, int white)
        {
            blackCounterCell.UpdateCount(black);
            whiteCounterCell.UpdateCount(white);
        }
    }
}
