using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Reversi2024.Model.Player
{
    public class CpuStrongPlayer : IPlayer
    {
        
        private Subject<Vector2Int?> onSelectPosSubject = new Subject<Vector2Int?>();
        private Subject<bool> shouldShowLoadingSubject = new Subject<bool>();

        public Observable<Vector2Int?> OnSelectedPutPosition => onSelectPosSubject;
        public Observable<bool> ShouldShowLoading => shouldShowLoadingSubject;

        private CpuModel cpuModel = new CpuModel();
        
        public void StartThinking(bool isBlackTurn, BoardModel boardModel)
        {
            Debug.Log("CPUMiddleターン開始");
            shouldShowLoadingSubject.OnNext(true);
            StartCalculate(isBlackTurn, boardModel).Forget();
        }

        private async UniTask StartCalculate(bool isBlackTurn, BoardModel boardModel)
        {
            var res = await cpuModel.Calculate(isBlackTurn, boardModel, 3);
            shouldShowLoadingSubject.OnNext(false);
            onSelectPosSubject.OnNext(res);
        }
        
        public void ClickedCell(Vector2Int position)
        {
            // なにもしない
        }
        
        public void Dispose()
        {
            cpuModel.Dispose();
        }
    }
}