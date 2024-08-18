using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Reversi2024.View.UI.CounterView
{
    internal class CounterCell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void UpdateCount(int count)
        {
            text.text = $"●({count})";
        }
    }
}
