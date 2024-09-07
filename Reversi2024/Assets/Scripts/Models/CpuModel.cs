using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Reversi2024.Model.Player
{
    public class CpuModel : IDisposable
    {
        private const int ThreadCount = 20;
        
        private class LookAheadData
        {
            public LookAheadData Parent { get; }
            public ValueTuple<int, int>? EvaluateValue = null;
            public Vector2Int PutPosition { get; }
            private int HierarchyCount { get; }

            private readonly BoardModel boardModel;
            private readonly bool isBlackTurn;
            private readonly int maxHierarchy;


            public LookAheadData( int hierarchyCount, int maxHierarchy, LookAheadData parent, BoardModel boardModel, bool isBlackTurn,
                Vector2Int putPosition)
            {
                this.HierarchyCount = hierarchyCount;
                this.maxHierarchy = maxHierarchy;
                this.Parent = parent;
                this.boardModel = new BoardModel();
                this.boardModel.Copy(boardModel);
                this.isBlackTurn = isBlackTurn;
                this.PutPosition = putPosition;
            }

            public async UniTask<List<LookAheadData>> StartCalculate()
            {
                boardModel.PutStone(PutPosition, isBlackTurn);

                if (HierarchyCount == maxHierarchy && maxHierarchy >= 0)
                {
                    EvaluateValue = boardModel.GetEvaluationValue();
                    return new List<LookAheadData>(){this};
                }

                bool nextTurnBlack = !isBlackTurn;
                var enables = (await boardModel.CalculateEnablePutAndResultAsync(nextTurnBlack)).Keys;
                var children = new List<LookAheadData>();

                if (enables.Count == 0)
                {
                    //相手パスパターン
                    nextTurnBlack = !nextTurnBlack;
                    enables = (await boardModel.CalculateEnablePutAndResultAsync(nextTurnBlack)).Keys;
                    if (enables.Count == 0)
                    {
                        // 両方パスパターン
                        EvaluateValue = (boardModel.Counter.Item1 > boardModel.Counter.Item2)
                            ? (100,0)
                            : ((boardModel.Counter.Item1 < boardModel.Counter.Item2)
                                ? (0,100)
                                : (0,0));
                        return new List<LookAheadData>() { this };
                    }
                }

                foreach (var putPos in enables)
                {
                    children.Add(new LookAheadData( HierarchyCount + 1, maxHierarchy, this, boardModel, nextTurnBlack, putPos));
                }

                return children;
            }

        }
        
        private UniTask?[] threadPools = new UniTask?[ThreadCount];

        public async UniTask<Vector2Int?> Calculate(bool isBlackTurn, BoardModel boardModel, int maxHierarchy)
        {
            var enables = (await boardModel.CalculateEnablePutAndResultAsync(isBlackTurn)).Keys;
            if (enables.Count == 0)
            {
                // おけるとこないのでパス
                return null;
            }

            Queue<LookAheadData> calculateQueue = new Queue<LookAheadData>();
            List<LookAheadData> resultDatas = new List<LookAheadData>();

            foreach (var putPos in enables)
            {
                calculateQueue.Enqueue(new LookAheadData(0, maxHierarchy, null, boardModel, isBlackTurn, putPos));
            }
            
            
            while (calculateQueue.Count > 0 || threadPools.Any(x => x.HasValue))
            {
                for (int i = 0; i < threadPools.Length; i++)
                {
                    if (threadPools[i].HasValue && threadPools[i].Value.Status != UniTaskStatus.Pending)
                    {
                        threadPools[i] = null;
                    }
                    
                    if (!threadPools[i].HasValue && calculateQueue.Count > 0)
                    {
                        threadPools[i] = CalculateOnOtherThread(calculateQueue.Dequeue(), res =>
                        {
                            res.ForEach(x =>
                            {
                                if (x.EvaluateValue.HasValue)
                                {
                                    resultDatas.Add(x);
                                }
                                else
                                {
                                    calculateQueue.Enqueue(x);
                                }
                            });
                        }).Preserve();
                        await threadPools[i].Value;
                    }
                }
            }

            if (resultDatas.Count == 0)
            {
                return enables.First();
            }


            LookAheadData mostValuableData = null;
            foreach (var data in resultDatas)
            {
                if (mostValuableData == null)
                {
                    mostValuableData = data;
                    continue;
                }

                var currentValue = (mostValuableData.EvaluateValue.Value.Item1 -
                                   mostValuableData.EvaluateValue.Value.Item2) * (isBlackTurn ? 1 : -1);
                var newValue = (data.EvaluateValue.Value.Item1 -
                                data.EvaluateValue.Value.Item2) * (isBlackTurn ? 1 : -1);
                if (newValue > currentValue)
                {
                    mostValuableData = data;
                }
            }
            
            while (true)
            {
                if (mostValuableData.Parent == null)
                {
                    break;
                }

                mostValuableData = mostValuableData.Parent;
            }

            return mostValuableData.PutPosition;
        }

        private async UniTask CalculateOnOtherThread(LookAheadData lookAheadData, Action<List<LookAheadData>> onFinished)
        {
            try
            {
                UniTask.SwitchToThreadPool();
                var res = await lookAheadData.StartCalculate();
                onFinished?.Invoke(res);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                // 例外が発生しようがしまいが、最後に確実にメインスレッドに戻す
                await UniTask.SwitchToMainThread();
            }
        }

        public void Dispose()
        {
           
        }
    }
}
