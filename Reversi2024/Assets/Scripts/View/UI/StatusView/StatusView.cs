using System;
using System.Collections;
using System.Collections.Generic;
using KBS.Module.PoolScrollView;
using Reversi2024.Model;
using UnityEngine;

namespace Reversi2024.View.UI
{
    public class StatusView : MonoBehaviour
    {
        [SerializeField] private TurnView turnView;
        [SerializeField] private CounterView.CounterView counterView;
        [SerializeField] private PoolScrollRect historiesView;
        [SerializeField] private HistoryCell historyCell;

        private List<HistoryModel> historyModels = new List<HistoryModel>();

        private void Awake()
        {
            historiesView.Setup(historyModels.Count, CreateItem, historyCell.RectTransform.sizeDelta);
        }

        public void UpdateTurn(bool isBlack)
        {
            turnView.UpdateTurn(isBlack);
        }

        public void UpdateCounter(int black, int white)
        {
            counterView.UpdateCount(black, white);
        }

        public void UpdateHistories(List<HistoryModel> historyModels)
        {
            this.historyModels = historyModels;
            historiesView.UpdateItemCount(historyModels.Count);
        }

        private HistoryCell CreateItem()
        {
            HistoryCell res = Instantiate(historyCell, historyCell.transform.parent);
            res.SetUp(GetHistoryModel);
            return res;
        }

        private HistoryModel GetHistoryModel(int index)
        {
            return historyModels[index];
        }
    }
}
