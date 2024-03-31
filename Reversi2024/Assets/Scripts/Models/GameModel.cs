using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Reversi2024.Model
{
    public class GameModel : IDisposable
    {
        private BoardModel boardModel = new BoardModel();
        
        private bool isBlackTurn = true;
        private int turn = 1;
        private bool[] cpuPlayer = new bool[2];
        private bool isCpuTurn = false;
        private ReactiveProperty<Dictionary<Vector2Int,ulong>> currentEnablePutAndResult = new ReactiveProperty<Dictionary<Vector2Int, ulong>>();
        private ReactiveProperty<ulong> currentEnablePutsBit = new ReactiveProperty<ulong>();

        private Vector2Int? putPosition;
        
        private CancellationTokenSource cancellationTokenSource = null;
        private CompositeDisposable compositeDisposable = new CompositeDisposable();
        
        public BoardModel BoardModel => boardModel;
        public Observable<ValueTuple<ulong, ulong>> OnChangedBoard => boardModel.OnChangedBoard;
        public Observable<ulong> OnChangedEnablePut => currentEnablePutsBit;
        
        public GameModel(bool isBlackCpu,bool isWhiteCpu)
        {
            cpuPlayer[0] = isBlackCpu;
            cpuPlayer[1] = isWhiteCpu;
            currentEnablePutAndResult.Subscribe(val =>
            {
                ulong bit = 0;
                if (val != null)
                {
                    foreach (var put in val.Keys)
                    {
                        bit |= Utility.ConvertPosition(put);
                    }
                }

                currentEnablePutsBit.Value = bit;
            }).AddTo(compositeDisposable);
        }

        public void StartGame()
        {
            Reset();
            cancellationTokenSource = new CancellationTokenSource();
            MainLoop(cancellationTokenSource.Token).Forget();
        }

        public void EndGame()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        public void Reset()
        {
            boardModel.Reset();
            isBlackTurn = true;
            currentEnablePutAndResult.Value = boardModel.CalculateEnablePutAndResult(isBlackTurn);
            turn = 1;
        }

        public void PutPositionByUI(Vector2Int pos)
        {
            if (isCpuTurn)
            {
                return;
            }
            
            PutPosition(pos);
        }

        public void PutPosition(Vector2Int pos)
        {
            putPosition = pos;
        }

        private async UniTask MainLoop(CancellationToken cancellationToken)
        {
            bool isPassed = false;
            try
            {
                Debug.Log("Start Game");
                while (true)
                {
                    putPosition = null;
                    isCpuTurn = cpuPlayer[isBlackTurn ? 0 : 1];


                    currentEnablePutAndResult.Value =
                        await boardModel.CalculateEnablePutAndResultAsync(isBlackTurn, cancellationToken);
                    if (currentEnablePutAndResult.Value == null)
                    {
                        if (isPassed)
                        {
                            //両方パスなので終了処理
                            break;
                        }

                        isPassed = true;
                        //TODO 履歴保存
                        isBlackTurn = !isBlackTurn;
                        turn++;
                        continue;
                    }


                    await UniTask.WaitWhile(() => !putPosition.HasValue, PlayerLoopTiming.Update,
                        cancellationTokenSource.Token);
                    if (!putPosition.HasValue ||
                        (currentEnablePutsBit.Value & Utility.ConvertPosition(putPosition.Value)) == 0)
                    {
                        //不正なポジションにおかれたのでやり直し
                        continue;
                    }

                    boardModel.PutStone(putPosition.Value, isBlackTurn);
                    //TODO 履歴の保存
                    isBlackTurn = !isBlackTurn;
                    turn++;
                }

                //TODO 終了処理
                Debug.Log("End Game");
            }catch (OperationCanceledException)
            {
                Debug.Log("ゲームはキャンセルされました");
            }
        }

        public void Dispose()
        {
            boardModel?.Dispose();
            currentEnablePutAndResult?.Dispose();
            currentEnablePutsBit?.Dispose();
            cancellationTokenSource?.Dispose();
        }
    }
}

