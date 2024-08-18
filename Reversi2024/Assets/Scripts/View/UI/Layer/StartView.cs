using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using R3;
using Reversi2024.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reversi2024.View.UI
{
    public class StartView : AbstractLayer
    {
        [SerializeField] private TMP_Dropdown blackTypesDropdown;
        [SerializeField] private TMP_Dropdown whiteTypesDropdown;
        [SerializeField] private Button startButton;
        
        private GameSettingModel.PlayerTypes blackSelectedType = GameSettingModel.PlayerTypes.Human;
        private GameSettingModel.PlayerTypes whiteSelectedType = GameSettingModel.PlayerTypes.Human;

        public Observable<List<GameSettingModel.PlayerTypes>> OnClickStart => startButton.OnClickAsObservable().Select(_ => new List<GameSettingModel.PlayerTypes>(){blackSelectedType, whiteSelectedType});

        private void Awake()
        {
            var options = Enum.GetNames(typeof(GameSettingModel.PlayerTypes)).Select(x => new TMP_Dropdown.OptionData(x)).ToList();

            blackTypesDropdown.options = options;
            whiteTypesDropdown.options = options;

            GameSettingModel.PlayerTypes GetTypeByIndex(int index)
            {
                var option = options[index];
                var values = Enum.GetValues(typeof(GameSettingModel.PlayerTypes)).Cast<GameSettingModel.PlayerTypes>();
                foreach (var value in values)
                {
                    if (value.ToString() == option.text)
                    {
                        return value;
                    }
                }

                return GameSettingModel.PlayerTypes.Human;
            }

            blackTypesDropdown.onValueChanged.AsObservable()
                .Subscribe(_ => blackSelectedType = GetTypeByIndex(blackTypesDropdown.value)).AddTo(this.gameObject);
            whiteTypesDropdown.onValueChanged.AsObservable()
                .Subscribe(_ => whiteSelectedType = GetTypeByIndex(whiteTypesDropdown.value)).AddTo(this.gameObject);

        }
    }
}
