using System;
using System.Collections.Generic;
using System.Threading;
using BackEnd;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasProfile : ControllerCanvas
    {
        private const string ViewSlotCombatName = nameof(ViewSlotCombat);
        private readonly List<ViewSlotCombat> _viewCombatSlots = new();
        
        private ViewCanvasProfile View => ViewCanvas as ViewCanvasProfile;
        private bool _isChangeableNickname;
        private string _changedNickname = "";

        public ControllerCanvasProfile(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasProfile>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.EditNicknameViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            
            View
                .SetNickname(Backend.UserNickName)
                .SetNicknameChangeChance(DataController.Instance.player.usedNicknameChangeChance);
            
            View.EditNicknameButton.onClick.AddListener(() =>
            {
                View.NicknameInputField.text = "";
                View.SetBadParameterText(string.Empty);
                View.EditNicknameViewCanvasPopup.Open();
            });
            
            View.NicknameInputField.onEndEdit.AddListener((value) =>
            {
                if (value.Length < 2)
                {
                    View.SetBadParameterText(LocalizeManager.GetText(LocalizedTextType.Nickname_BadParameter_NameIsToShort));
                    return;
                }
                BackendManager.CheckNicknameDuplication(value, (bro) =>
                {
                    Debug.Log(bro.GetStatusCode());
                    switch (bro.GetStatusCode())
                    {
                        case "204":
                            _isChangeableNickname = true;
                            _changedNickname = value;
                            View.SetBadParameterText(string.Empty);
                            break;
                        case "400":
                            _isChangeableNickname = false;
                            View.SetBadParameterText(
                                LocalizeManager.GetText(LocalizedTextType.Nickname_BadParameter_UnavailableName));
                            break;
                        case "409":
                            _isChangeableNickname = false;
                            View.SetBadParameterText(
                                LocalizeManager.GetText(LocalizedTextType.Nickname_Duplicate));
                            break;
                    }
                });
            });
            View.ChangeNicknameButton.onClick.AddListener(() =>
            {
                if (_isChangeableNickname)
                {
                    var isConsume = true;
                    if (DataController.Instance.player.usedNicknameChangeChance)
                    {
                        isConsume = DataController.Instance.good.TryConsume(GoodType.Dia, 100);
                    }

                    if (isConsume)
                    {
                        var loading = Get<ControllerCanvasToastMessage>();
                        loading.ShowLoading();
                        BackendManager.UpdateNickname(_changedNickname, (bro) =>
                        {
                            switch (bro.GetStatusCode())
                            {
                                case "204":
                                    View.SetNicknameChangeChance(true);
                                    DataController.Instance.player.usedNicknameChangeChance = true;
                                    loading.ShowTransientToastMessage(LocalizedTextType.Nickname_ChangeNicknameSuccess);
                                    DataController.Instance.player.OnChangedNickname?.Invoke(_changedNickname);
                                    View.EditNicknameViewCanvasPopup.Close();
                                    break;
                            }
                            
                            loading.CloseLoading();
                        });
                    }
                }
            });
            View.CopyUUIDButton.onClick.AddListener(() => {CopyTextToClipboard(BackendManager.Uuid);});
            View.UuidTMP.text = $"UUID : {BackendManager.Uuid}";

            View.CombatPowerSlideButton.AddListener(UpdateCombatPower);
            
            DataController.Instance.player.OnChangedNickname += (nickname) => View.SetNickname(nickname);
            DataController.Instance.player.OnBindChangeTotalCombat += UpdateTotalCombatPower;
            
            View.EditNicknameViewCanvasPopup.Close();
            
            View.CombatPowerSlideButton.OnClick(0);
        }
        
        private void UpdateCombatPower(int projectorIndex)
        {
            var viewSlots = _viewCombatSlots.GetViewSlots(ViewSlotCombatName, View.CombatPowerSlotParent, Enum.GetValues(typeof(AttributeType)).Length);
            var i = 0;
            foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
            {
                var slot = viewSlots[i];
                var desc = ((float)DataController.Instance.player.GetTotalAttributeValue(attributeType, projectorIndex))
                    .ToAttributeValueString(attributeType);
                
                slot
                    .SetTitle(LocalizeManager.GetText(attributeType))
                    .SetDesc(desc)
                    .SetActive(true);
                
                if(attributeType is AttributeType.None or AttributeType.RandomTag
                   or AttributeType.AddGoldGain or AttributeType.GameSpeed) slot.SetActive(false);
                
                ++i;
            }
        }

        private void UpdateTotalCombatPower(double value)
        {
            View.SetCombatPower(value.ToDamage());
        }

        private void CopyTextToClipboard(string text)
        {
            GUIUtility.systemCopyBuffer = text;
            Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.CopySuccess);
        }
    }
}