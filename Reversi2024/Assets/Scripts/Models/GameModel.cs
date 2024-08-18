using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Reversi2024.Model.Player;
using UnityEngine;

namespace Reversi2024.Model
{
    public class GameModel : IDisposable
    {
        private BoardModel boardModel = new BoardModel();

        private ReactiveProperty<bool> isBlackTurn = new ReactiveProperty<bool>(true);
        private int turn = 1;
        private List<IPlayer> players = new List<IPlayer>();
        private Vector2Int? selectedPosition;
        private bool isSelectedPosition = false;
        private ReactiveProperty<Dictionary<Vector2Int,ulong>> currentEnablePutAndResult = new ReactiveProperty<Dictionary<Vector2Int, ulong>>();
        private ReactiveProperty<ulong> currentEnablePutsBit = new ReactiveProperty<ulong>();
        private List<HistoryModel> histories = new List<HistoryModel>();
        private Subject<List<HistoryModel>> historiesModifiedSubject = new Subject<List<HistoryModel>>();
        private Subject<Unit> onEndGameSubject = new Subject<Unit>();
        private Subject<bool> shouldShowLoadingSubject = new Subject<bool>();
        
        private CancellationTokenSource cancellationTokenSource = null;
        private CompositeDisposable compositeDisposable = new CompositeDisposable();
        
        public BoardModel BoardModel => boardModel;
        public Observable<ValueTuple<ulong, ulong>> OnChangedBoard => boardModel.OnChangedBoard;
        public Observable<ValueTuple<int, int>> OnChangedCounter => boardModel.OnChangedCounter;
        public Observable<ulong> OnChangedEnablePut => currentEnablePutsBit;
        public Observable<bool> IsBlackTurnObservable => isBlackTurn;
        public Observable<List<HistoryModel>> HistoriesObservable => historiesModifiedSubject;
        public Observable<Unit> OnEndGame => onEndGameSubject;
        public Observable<bool> ShouldShowLoading => shouldShowLoadingSubject;
        
        public GameModel()
        {
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
        
        public void StartGame(List<IPlayer> players)
        {
            foreach (var player in this.players)
            {
                player.Dispose();
            }
            this.players.Clear();
            
            this.players = players;
            foreach (var player in this.players)
            {
                player.OnSelectedPutPosition.Subscribe(pos =>
                {
                    selectedPosition = pos;
                    isSelectedPosition = true;
                }).AddTo(compositeDisposable);
                player.ShouldShowLoading.Subscribe(x => shouldShowLoadingSubject.OnNext(x)).AddTo(compositeDisposable);
            }
            
            Reset();
            cancellationTokenSource = new CancellationTokenSource();
            MainLoop(cancellationTokenSource.Token).Forget();
        }

        public void EndGame()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            onEndGameSubject.OnNext(Unit.Default);
        }

        public void Reset()
        {
            boardModel.Reset();
            isBlackTurn.Value = true;
            currentEnablePutAndResult.Value = boardModel.CalculateEnablePutAndResult(isBlackTurn.Value);
            histories = new List<HistoryModel>();
            historiesModifiedSubject.OnNext(histories);
            turn = 1;
        }

        private async UniTask MainLoop(CancellationToken cancellationToken)
        {
            bool isPassed = false;
            try
            {
                Debug.Log("Start Game");
                while (true)
                {
                    if (boardModel.BlackCount + boardModel.WhiteCount >= 64)
                    {
                        //全部置かれてたら終わり
                        EndGame();
                        return;
                    }
                    isSelectedPosition = false;
                    var putBeforeBoard = new ValueTuple<ulong,ulong>(boardModel.BoardData.Item1, boardModel.BoardData.Item2);
                    
                    currentEnablePutAndResult.Value =
                        await boardModel.CalculateEnablePutAndResultAsync(isBlackTurn.Value, cancellationToken);
                   
                    this.players[isBlackTurn.Value ? 0 : 1 ].StartThinking(isBlackTurn.Value, boardModel);
                    await UniTask.WaitWhile(() => !isSelectedPosition, PlayerLoopTiming.Update,cancellationTokenSource.Token);

                    if (!selectedPosition.HasValue)
                    {
                        //パスの時
                        var lastHistory = histories.LastOrDefault();
                        if (lastHistory is { PutPosition: null })
                        {
                            // 両方パスの時は終わり
                            EndGame();
                            return;
                        }
                        ChangeTurn(null, putBeforeBoard);
                        continue;
                    }

                    //通常通り石が置けたとき
                    boardModel.PutStone(selectedPosition.Value, isBlackTurn.Value);
                    ChangeTurn(selectedPosition.Value, putBeforeBoard);
                }

                onEndGameSubject.OnNext(Unit.Default);
                Debug.Log("End Game");
            }catch (OperationCanceledException)
            {
                Debug.Log("ゲームはキャンセルされました");
                Reset();
            }
        }

        private void ChangeTurn(Vector2Int? putPosition, ValueTuple<ulong, ulong> beforePutBoardData)
        {
            AddHistory(turn, isBlackTurn.Value, putPosition, beforePutBoardData);
            isBlackTurn.Value = !isBlackTurn.Value;
            turn++;
        }

        private void AddHistory(int turn, bool isBlackTurn, Vector2Int? putPosition, ValueTuple<ulong, ulong> beforePutBoardData)
        {
            histories.Insert(0,new HistoryModel(turn, isBlackTurn, putPosition, beforePutBoardData));
            historiesModifiedSubject.OnNext(histories);
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

