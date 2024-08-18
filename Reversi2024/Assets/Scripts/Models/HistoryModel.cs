using System;
using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Reversi2024.Model
{
    public class HistoryModel
    {
        public readonly int Turn;
        public readonly bool IsBlackTurn;
        public readonly ValueTuple<ulong, ulong> BeforePutBoardData = new ValueTuple<ulong, ulong>();
        public readonly Vector2Int? PutPosition;
        
        public int BlackCount => CountStone(true);
        public int WhiteCount => CountStone(false);

        public HistoryModel(int turn, bool isBlackTurn, Vector2Int? putPosition, ValueTuple<ulong, ulong> beforePutBoardData)
        {
            Turn = turn;
            IsBlackTurn = isBlackTurn;
            BeforePutBoardData = beforePutBoardData;
            PutPosition = putPosition;
        }

        private int CountStone(bool isBlack)
        {
            var board = isBlack ? BeforePutBoardData.Item1 : BeforePutBoardData.Item2;
            int counter = 0;
            for(int i=0;i<64;i++)
            {
                if ((board & (ulong)1) > 0)
                {
                    counter++;
                }
            }

            return counter;
        }
    }
}
