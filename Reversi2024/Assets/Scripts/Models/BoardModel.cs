using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Reversi2024.Model
{
    public class BoardModel : IDisposable
    {
        private ReactiveProperty<ValueTuple<ulong, ulong>> boardData = new ReactiveProperty<(ulong, ulong)>();
        public ValueTuple<ulong, ulong> BoardData => boardData.Value;
        public Observable<ValueTuple<ulong, ulong>> OnChangedBoard => boardData;

        private ReactiveProperty<ValueTuple<int, int>> count = new ReactiveProperty<(int, int)>();
        public ValueTuple<int, int> Count => count.Value;
        public Observable<ValueTuple<int, int>> OnChangedCount => count;

        private CompositeDisposable compositeDisposable = new CompositeDisposable();
        
        public BoardModel()
        {
            OnChangedBoard.Subscribe(val => UpdateCount()).AddTo(compositeDisposable);
        }

        public void Reset()
        {
            ValueTuple<ulong, ulong> initializeBoard = new ValueTuple<ulong, ulong>();
            initializeBoard.Item1 = 0b_00000000_000000000_0000000_00010000_00001000_000000000_0000000_00000000;
            initializeBoard.Item2 = 0b_00000000_000000000_0000000_00001000_00010000_000000000_0000000_00000000;
            boardData.Value = initializeBoard;
        }

        public void PutStone(Vector2Int pos, bool isBlack)
        {
            var reverseBit = CalculatePutResult(pos, isBlack);
            ValueTuple<ulong, ulong> result = new ValueTuple<ulong, ulong>();
            if (isBlack)
            {
                result.Item1 = boardData.Value.Item1 | reverseBit | Utility.ConvertPosition(pos);
                result.Item2 = boardData.Value.Item2 & ~reverseBit;
            }
            else
            {
                result.Item1 = boardData.Value.Item1 & ~reverseBit;
                result.Item2 = boardData.Value.Item2 | reverseBit | Utility.ConvertPosition(pos);
            }

            boardData.Value = result;
        }

        public async UniTask<Dictionary<Vector2Int, ulong>> CalculateEnablePutAndResultAsync(bool isBlack, CancellationToken token = new CancellationToken())
        {
            Dictionary<Vector2Int, ulong> result = null;
            try
            {
                await UniTask.SwitchToThreadPool();
                try
                {
                    await UniTask.SwitchToThreadPool();
                    result = CalculateEnablePutAndResult(isBlack);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }finally
                {
                    // 例外が発生しようがしまいが、最後に確実にメインスレッドに戻す
                    await UniTask.SwitchToMainThread();
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            
            return result;
        }
        
        /// <summary>
        /// おける場所とその結果ひっくり返る場所をbitで返す
        /// </summary>
        /// <param name="isBlack"></param>
        /// <returns>keyはおける場所のbit,valueはそこに置いたひっくり返る場所のbit</returns>
        public Dictionary<Vector2Int, ulong> CalculateEnablePutAndResult(bool isBlack)
        {
            var res = new Dictionary<Vector2Int, ulong>();

            for (int originX = 0; originX < 8; originX++)
            {
                for (int originY = 0; originY < 8; originY++)
                {
                    ulong bit = Utility.ConvertPosition(originX, originY);
                    if ((bit & boardData.Value.Item1) != 0　|| (bit & boardData.Value.Item2) != 0)
                    {
                        // 埋まっている箇所はスキップ
                        continue;
                    }

                    var resultForPos =
                        CalculatePutResult(new Vector2Int(originX, originY), isBlack);
                    if (resultForPos == 0)
                    {
                        // ひっくり返るものがなければスキップ
                        continue;
                    }
                    res.Add(new Vector2Int(originX,originY),resultForPos);
                }
            }

            return res;
        }

        /// <summary>
        /// おいたときにひっくり返るマスをbitで返す
        /// </summary>
        /// <returns></returns>
        private ulong CalculatePutResult(Vector2Int origin, bool isBlack)
        {
            ulong res = 0;
            var source = isBlack ? boardData.Value.Item1 : boardData.Value.Item2;
            var target = isBlack ? boardData.Value.Item2 : boardData.Value.Item1;
            
            for (int dir = 0; dir < 9; dir++)
            {
                Vector2Int dirVec = Vector2Int.zero;
                dirVec.x = dir / 3 - 1;
                dirVec.y = dir % 3 - 1;
                if (dirVec.x == 0 && dirVec.y == 0)
                {
                    continue;
                }

                var checkResult = calculateEnablePutAndResultRecursive(origin, dirVec, 1, source, target,0);
                res |= checkResult;
            }

            return res;
        }

        private ulong calculateEnablePutAndResultRecursive(Vector2Int origin,Vector2Int dir, int offset, ulong source ,ulong target, ulong result)
        {
            var checkPos = Vector2Int.zero;
            checkPos.x = origin.x + dir.x * offset;
            checkPos.y = origin.y + dir.y * offset;
            if(checkPos.x < 0 || checkPos.x >= 8 || checkPos.y < 0 || checkPos.y >= 8)
            {
                //枠外なのでなしで判定
                result = 0;
                return result;
            }

            var checkBit = Utility.ConvertPosition(checkPos);
            if ((target & checkBit) > 0)
            {
                //チェック対象が相手色ならoffsetを増やしてチェック追加
                result |= checkBit;
                offset++;
                return calculateEnablePutAndResultRecursive(origin, dir, offset, source, target, result);
            }else if ((source & checkBit) > 0)
            {
                return result;
            }

            result = 0;
            return result;
        }

        private void UpdateCount()
        {
            ValueTuple<int, int> counter = new ValueTuple<int, int>(0, 0);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    ulong bit = Utility.ConvertPosition(x, y);
                    counter.Item1 += (boardData.Value.Item1 & bit) != 0 ? 1 : 0;
                    counter.Item2 += (boardData.Value.Item2 & bit) != 0 ? 1 : 0;
                }
            }

            Debug.Log($"Update Count Black : {counter.Item1} White: {counter.Item2}");
            count.Value = counter;
        }

        public void Dispose()
        {
            boardData?.Dispose();
            count?.Dispose();
            compositeDisposable?.Dispose();
        }
    }
}

