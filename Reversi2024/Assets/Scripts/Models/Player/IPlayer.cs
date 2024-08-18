using System;
using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Reversi2024.Model.Player
{
    public interface IPlayer : IDisposable
    {
        public Observable<Vector2Int?> OnSelectedPutPosition { get; }

        public Observable<bool> ShouldShowLoading { get; }
        
        public void StartThinking(bool isBlackTurn, BoardModel boardModel);

        public void ClickedCell(Vector2Int position);
    }
}
