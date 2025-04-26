using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.CloudData;
using JetBrains.Annotations;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataProbability probability;
    }

    [Serializable]
    public class DataProbability
    {
        private List<ProbabilityCardV2> _probabilityCards;
        
        [CanBeNull]
        public async UniTask<List<ProbabilityItem>> GetRandomProbabilitys(ProbabilityType probabilityType, int count)
        {
            if (count == 0) return null;
            
            var fieldId = await GetFieldId(probabilityType);
            if (string.IsNullOrEmpty(fieldId)) return null;

            var itemList = new List<ProbabilityItem>();
            var isRun = true;
            Backend.Probability.GetProbabilitys(fieldId, count, bro =>
            {
                if (!bro.IsSuccess())
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                    isRun = false;
                    return;
                }

                var json = bro.GetFlattenJSON()["elements"];
                for (var i = 0; i < json.Count; i++)
                {
                    var item = new ProbabilityItem
                    {
                        index = int.Parse(json[i]["index"].ToString()),
                        probabilityType =
                            (ProbabilityType)Enum.Parse(typeof(ProbabilityType), json[i]["probabilityType"].ToString()),
                        gradeType = (GradeType)Enum.Parse(typeof(GradeType), json[i]["gradeType"].ToString()),
                        percent = json[i]["percent"].ToString()
                    };

                    itemList.Add(item);
                }

                isRun = false;
            });
            
            while (isRun) await UniTask.Yield();
            
            return itemList.Count > 0 ? itemList : null;
        }

        public async void GetProbability(ProbabilityType type, UnityAction<List<float>> callback)
        {
            var fieldId = await GetFieldId(type);
            Backend.Probability.GetProbabilityContents(fieldId, bro =>
            {
                if (!bro.IsSuccess())
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                    callback?.Invoke(null);
                }

                var json = bro.FlattenRows();
                var percents = new List<float>();
                for (var i = 0; i < json.Count; ++i)
                {
                    percents.Add(float.Parse(json[i]["percent"].ToString()));
                }
                
                callback?.Invoke(percents);
            });
        }

        private async UniTask<string> GetFieldId(ProbabilityType probabilityType)
        {
            if (_probabilityCards == null)
                await GetProbabilityCardList();

            if (_probabilityCards == null || _probabilityCards.Count == 0) return string.Empty;
            return _probabilityCards.Find(x => x.ProbabilityType == probabilityType).SelectedProbabilityFileId;
        }

        private async UniTask GetProbabilityCardList()
        {
            var isRun = true;
            Backend.Probability.GetProbabilityCardListV2(bro =>
            {
                if (!bro.IsSuccess()) return;

                _probabilityCards = new List<ProbabilityCardV2>();

                var json = bro.FlattenRows();
                for (var i = 0; i < json.Count; i++)
                {
                    var probabilityCard = new ProbabilityCardV2
                    {
                        ProbabilityName =  json[i]["probabilityName"].ToString(),
                        ProbabilityExplain = json[i]["probabilityExplain"].ToString(),
                        SelectedProbabilityFileId = json[i]["selectedProbabilityFileId"].ToString()
                    };

                    probabilityCard.ProbabilityType = Enum.TryParse<ProbabilityType>(probabilityCard.ProbabilityName, out var type) ? type : ProbabilityType.SummonElemental;
                    _probabilityCards.Add(probabilityCard);
                }

                isRun = false;
            });
            while (isRun) await UniTask.Yield();
        }
    }
}