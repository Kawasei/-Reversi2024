using System;
using System.Collections;
using System.Collections.Generic;
using KBS.Module.PoolScrollView;
using Reversi2024.Model;
using TMPro;
using UnityEngine;

namespace Reversi2024.View.UI
{
    public class HistoryCell : MonoBehaviour, IPoolScrollViewItem
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI turnCountText;
        [SerializeField] private GameObject blackIcon;
        [SerializeField] private GameObject whiteIcon;
        [SerializeField] private TextMeshProUGUI putPointText;
        
        public RectTransform RectTransform => rectTransform;
        public GameObject GameObject => this.gameObject;

        private Func<int, HistoryModel> historyModelFunc;

        private readonly Dictionary<int, (string, string)> PosTextDictionary = new Dictionary<int, (string, string)>()
        {
            { 0, ("1", "A" ) },
            { 1, ("2", "B") },
            { 2, ("3", "C") },
            { 3, ("4", "D") },
            { 4, ("5", "E") },
            { 5, ("6", "F") },
            { 6, ("7", "G") },
            { 7, ("8", "H") },
        };

        public void SetUp(Func<int, HistoryModel> historyModelFunc)
        {
            this.historyModelFunc = historyModelFunc;
        }
        
        public void UpdateDisplay(int index)
        {
            var history = historyModelFunc.Invoke(index);

            turnCountText.text = $"Turn:{history.Turn}";
            blackIcon.SetActive(history.IsBlackTurn);
            whiteIcon.SetActive(!history.IsBlackTurn);
            if (history.PutPosition.HasValue)
            {
                putPointText.text =
                    $"({PosTextDictionary[history.PutPosition.Value.y].Item1},{PosTextDictionary[history.PutPosition.Value.x].Item2})";
            }
            else
            {
                putPointText.text = "パス";
            }
        }
    }
}
