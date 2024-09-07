using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using Reversi2024.Model;
using Reversi2024.Model.Player;
using Reversi2024.View.Objects.Board;
using Reversi2024.View.UI;
using UnityEngine;

namespace Reversi2024.Scene
{
    public class GameScene : MonoBehaviour
    {
        private GameModel gameModel = null;
        private GameSettingModel gameSettingModel = null;
        [SerializeField]
        private CellHandler cellHandler;

        [SerializeField] private StatusView statusView;
        [SerializeField] private StartView startView;
        [SerializeField] private EndView endView;
        [SerializeField] private LoadingLayer loadingLayer;
        
        private void Awake()
        {
            gameModel = new GameModel();
            gameModel.OnChangedBoard.Subscribe(cellHandler.OnChangedBoard).AddTo(this.gameObject);
            gameModel.OnChangedEnablePut.Subscribe(cellHandler.OnChangedEnablePut).AddTo(this.gameObject);
            gameModel.IsBlackTurnObservable.Subscribe(statusView.UpdateTurn).AddTo(this.gameObject);
            gameModel.OnChangedCounter.Subscribe(counter => statusView.UpdateCounter(counter.Item1, counter.Item2))
                .AddTo(this.gameObject);
            gameModel.HistoriesObservable.Subscribe(statusView.UpdateHistories).AddTo(this.gameObject);
            gameModel.OnEndGame.Subscribe(_ =>
            {
                endView.Setup(gameModel.BoardModel.BlackCount, gameModel.BoardModel.WhiteCount);
                endView.Open();
            }).AddTo(this.gameObject);
            gameModel.ShouldShowLoading.Subscribe(show =>
            {
                if (show)
                {
                    loadingLayer.Open();
                }
                else
                {
                    loadingLayer.Close();
                }
            }).AddTo(this.gameObject);

            endView.OnClickReset.Subscribe(_ =>
            {
                endView.Close();
                startView.Open();
            }).AddTo(this.gameObject);
            
            startView.OnClickStart.Subscribe(playerTypes =>
            {
                startView.Close();

                List<IPlayer> players = new List<IPlayer>();
                foreach (var type in playerTypes)
                {
                    IPlayer player = type switch
                    {
                        GameSettingModel.PlayerTypes.Human => new HumanPlayer(),
                        GameSettingModel.PlayerTypes.CPU_weak => new CpuWeakPlayer(),
                        GameSettingModel.PlayerTypes.CPU_middle => new CpuMiddlePlayer(),
                        GameSettingModel.PlayerTypes.CPU_strong => new CpuStrongPlayer(),
                        _ => new HumanPlayer()
                    };
                    cellHandler.OnClickObservable.Subscribe(pos => player.ClickedCell(pos))
                        .AddTo(cellHandler.gameObject);
                    players.Add(player);
                }
                gameModel.StartGame(players);
            }).AddTo(this.gameObject);
            
            
            
            startView.Open();
        }
    }
}

