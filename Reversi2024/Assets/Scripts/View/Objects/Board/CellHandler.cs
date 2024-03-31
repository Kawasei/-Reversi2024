using System;
using R3;
using UnityEngine;

namespace Reversi2024.View.Objects.Board
{
    public class CellHandler : MonoBehaviour
    {
        [SerializeField] private Cell cellBase;
        
        private Subject<Vector2Int> onClickSubject = new Subject<Vector2Int>();
        public Observable<Vector2Int> OnClickObservable => onClickSubject.AsObservable();

        private Subject<ulong> onChangedEnablePutSubject = new Subject<ulong>();
        private Subject<ValueTuple<ulong, ulong>> onChangedBoardSubject = new Subject<(ulong, ulong)>();
        
        private void Awake()
        {
            for(int x =0;x<8;x++)
            {
                for(int y =0;y<8;y++)
                {
                    var newButton = Instantiate(cellBase.gameObject, cellBase.transform.parent);
                    var button = newButton.GetComponent<Cell>();
                    button.Setup(new Vector2Int(x,y));
                    button.OnClickObservable.Subscribe(val =>
                    {
                        Debug.Log($"OnClicked : {val}");
                        onClickSubject.OnNext(val);
                    }).AddTo(this.gameObject);
                    onChangedEnablePutSubject.Subscribe(val => button.OnChangeEnablePut(val)).AddTo(this.gameObject);
                    onChangedBoardSubject.Subscribe(val => button.OnChangeBoard(val)).AddTo(this.gameObject);
                }
            }
            cellBase.gameObject.SetActive(false);
        }

        public void OnChangedBoard(ValueTuple<ulong, ulong> board)
        {
            onChangedBoardSubject.OnNext(board);
        }

        public void OnChangedEnablePut(ulong enable)
        {
            onChangedEnablePutSubject.OnNext(enable);
        }

        private void OnDestroy()
        {
            onChangedEnablePutSubject.Dispose();
            onChangedBoardSubject.Dispose();
        }
    }
}
