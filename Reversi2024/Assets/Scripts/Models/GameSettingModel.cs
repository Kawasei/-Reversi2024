namespace Reversi2024.Model
{
    public class GameSettingModel
    {
        public enum PlayerTypes
        {
            Human,
            CPU_weak,
            CPU_middle,
            CPU_strong,
        }

        public PlayerTypes BlackPlayerTypes { get; } = PlayerTypes.Human;
        public PlayerTypes WhitePlayerTypes { get; } = PlayerTypes.Human;

        public GameSettingModel(PlayerTypes black, PlayerTypes white)
        {
            BlackPlayerTypes = black;
            WhitePlayerTypes = white;
        }
    }
}
