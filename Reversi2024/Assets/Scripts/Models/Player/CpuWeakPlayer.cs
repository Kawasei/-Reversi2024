using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Reversi2024.Model.Player
{
    public class CpuWeakPlayer :  IPlayer
    {
        private Subject<Vector2Int?> onSelectPosSubject = new Subject<Vector2Int?>();
        private Subject<bool> shouldShowLoadingSubject = new Subject<bool>();

        public Observable<Vector2Int?> OnSelectedPutPosition => onSelectPosSubject;
        public Observable<bool> ShouldShowLoading => shouldShowLoadingSubject;
        
        public void StartThinking(bool isBlackTurn, BoardModel boardModel)
        {
            Debug.Log("CPUWeakターン開始");
            shouldShowLoadingSubject.OnNext(true);
            ThinkingTask(isBlackTurn,boardModel).Forget();
        }

        public void ClickedCell(Vector2Int position)
        {
            // 何もしない
        }
        
        private async UniTask ThinkingTask(bool isBlackTurn, BoardModel boardModel)
        {
            var enablePuts = (await boardModel.CalculateEnablePutAndResultAsync(isBlackTurn)).Keys.ToList();
            Vector2Int? pos = null;
            if (enablePuts.Count != 0)
            {
                pos = enablePuts[Random.Range(0, enablePuts.Count)];
            }
            onSelectPosSubject.OnNext(pos);
            shouldShowLoadingSubject.OnNext(false);
            Debug.Log("CPUWeakターン終了");
        }

        public void Dispose()
        {
            onSelectPosSubject?.Dispose();
            shouldShowLoadingSubject?.Dispose();
        }
    }
}
