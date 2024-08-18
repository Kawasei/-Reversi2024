using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Reversi2024.Model.Player
{
    public class HumanPlayer : IPlayer
    {
        private Subject<Vector2Int?> onSelectPosSubject = new Subject<Vector2Int?>();
        
        public Observable<Vector2Int?> OnSelectedPutPosition => onSelectPosSubject;
        public Observable<bool> ShouldShowLoading { get; } = new Subject<bool>();

        private Vector2Int? selectedCellPos = null;
        
        
        public void StartThinking(bool isBlackTurn, BoardModel boardModel)
        {
            Debug.Log("人間ターン開始");
            ThinkingTask(isBlackTurn,boardModel).Forget();
        }

        public void ClickedCell(Vector2Int position)
        {
            selectedCellPos = position;
        }

        private async UniTask ThinkingTask(bool isBlackTurn, BoardModel boardModel)
        {
            var enablePuts = (await boardModel.CalculateEnablePutAndResultAsync(isBlackTurn)).Keys;
            if (enablePuts.Count == 0)
            {
                onSelectPosSubject.OnNext(null);
                Debug.Log("人間ターン終了(パス)");
                return;
            }
            while (true)
            {
                if (selectedCellPos.HasValue)
                {
                    if (enablePuts.Count(x => x == selectedCellPos) > 0)
                    {
                        onSelectPosSubject.OnNext(selectedCellPos.Value);
                        selectedCellPos = null;
                        Debug.Log("人間ターン終了");
                        break;
                    }

                    selectedCellPos = null;
                }
                
                await UniTask.DelayFrame(1);
            }
        }

        public void Dispose()
        {
            onSelectPosSubject?.Dispose();
        }
    }
}
