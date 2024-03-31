using System;
using System.Threading;
using System.Threading.Tasks;
using R3;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Reversi2024.View.Objects.Board
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image enableImage;
        [SerializeField] private Image stoneImage;

        private CancellationTokenSource cancellationTokenSource = null;

        private Subject<Vector2Int> onClickSubject = new Subject<Vector2Int>();
        
        internal Observable<Vector2Int> OnClickObservable => onClickSubject.AsObservable();

        private Vector2Int pos;
        public void Setup(Vector2Int position)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickSubject.OnNext(position));
            pos = position;
        }

        internal void OnChangeEnablePut(ulong enableData)
        {
            enableImage.gameObject.SetActive((enableData & Utility.ConvertPosition(pos)) != 0);
        }

        internal void OnChangeBoard(ValueTuple<ulong, ulong> boardData)
        {
            bool? isBlack = null;
            ulong posulong = Utility.ConvertPosition(pos);
            if ((boardData.Item1 & posulong) != 0)
            {
                isBlack = true;
            }else if((boardData.Item2 & posulong) != 0)
            {
                isBlack = false;
            }

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            cancellationTokenSource = new CancellationTokenSource();
            animateStone(isBlack, cancellationTokenSource.Token).Forget();
        }

        private async UniTask animateStone(bool? isBlack,CancellationToken cancellationToken)
        {
            try
            {
                stoneImage.gameObject.SetActive(isBlack.HasValue);
                stoneImage.color = (isBlack.HasValue && isBlack.Value) ? Color.black : Color.white;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("animate stone canceled");
            }
        }

        private void OnDestroy()
        {
            this.onClickSubject.Dispose();
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Dispose();
            }
        }
    }
}

