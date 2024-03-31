using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using Reversi2024.Model;
using Reversi2024.View.Objects.Board;
using UnityEngine;

namespace Reversi2024.Scene
{
    public class GameScene : MonoBehaviour
    {
        private GameModel gameModel = null;
        [SerializeField]
        private CellHandler cellHandler;
        
        private void Awake()
        {
            gameModel = new GameModel(false,false);
            gameModel.OnChangedBoard.Subscribe(cellHandler.OnChangedBoard).AddTo(this);
            gameModel.OnChangedEnablePut.Subscribe(cellHandler.OnChangedEnablePut).AddTo(this);
            cellHandler.OnClickObservable.Subscribe(gameModel.PutPosition).AddTo(this);
            
            gameModel.StartGame();
        }
    }
}

