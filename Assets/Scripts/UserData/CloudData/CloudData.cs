using System;
using System.Collections.Generic;
using System.Text;
using BackEnd;
using Newtonsoft.Json;
using UnityEngine;

namespace ETD.Scripts.UserData.CloudData
{
    public partial class CloudData
    {
        private static CloudData _instance;

        public static CloudData Instance => _instance ??= new CloudData();

        public BStage[] bStages;
        public BEnemyCombination[] bEnemyCombinations;
        public BDifficulty[] bDifficulties;
        public BElemental[] bElementals;
        public BEnemy[] bEnemies;
        public BElementalCombine[] bElementalCombines;
        public BAttribute[] bAttributes;
        public BRune[] bRunes;
        public BUpgrade[] bUpgrades;
        public BLocalizedText[] bLocalizedTexts;
        public BEnhancement[] bEnhancements;
        public BShop[] bShops;
        public BQuest[] bQuests;
        public BDungeon[] bDungeons;
        public BMission[] bMissions;
        public BGood[] bGoods;
        public BContentUnlock[] bContentUnlocks;
        public BFreeGift[] bFreeGifts;
        public BTutorial[] bTutorials;
        public BOfflineReward[] bOfflineRewards;
        public BBuff[] bBuffs;
        public BAttendance[] bAttendances;
        public BPass[] bPasses;
        public BResearch[] bResearches;
        public BInfo[] bInfos;
        public BRaid[] bRaids;
        public BGrowPass[] bGrowPasses;
        public BGuild[] bGuilds;
        public BGuildReward[] bGuildRewards;

        public void Load()
        {
            var broForChartVersion = Backend.Chart.GetChartListV2();
            if (!broForChartVersion.IsSuccess())
            {
                throw new Exception($"Get Chart List Error:{broForChartVersion}");
            }

            var chartVersionFildId = string.Empty;
            var jsonForChartVersion = broForChartVersion.FlattenRows();
            for (var i = 0; i < jsonForChartVersion.Count; ++i)
            {
                if (jsonForChartVersion[i]["chartName"].ToString() == "bChartVersion")
                {
                    chartVersionFildId = jsonForChartVersion[i]["selectedChartFileId"].ToString();
                    break;
                }
            }

            if (string.IsNullOrEmpty(chartVersionFildId))
            {
                throw new Exception($"Chart Version Error:{chartVersionFildId}");
            }

            var chartVertionBro = Backend.Chart.GetChartContents(chartVersionFildId);
            if (!chartVertionBro.IsSuccess())
            {
                throw new Exception($"Chart Version Error:{chartVertionBro}"); 
            }

            #if UNITY_IOS
            var os = "iOS";
            #else
            var os = "AOS";
            #endif
            var version = Application.version;
            version = version.Length >= 5 ? version[..5] : version;
            
            var chartFolderId = -1;
            
            var chartVersionJson = chartVertionBro.FlattenRows();
            for (var i = 0; i < chartVersionJson.Count; ++i)
            {
                if (chartVersionJson[i]["OS"].ToString().Equals(os)
                    && chartVersionJson[i]["version"].ToString().Equals(version))
                {
                    chartFolderId = int.Parse(chartVersionJson[i]["chartFolderId"].ToString());
                    break;
                }
            }

            if (chartFolderId == -1)
            {
                throw new Exception($"Chart Folder Id Error:{chartVersionJson}"); 
            }
            
            var bro = Backend.Chart.GetChartListByFolderV2(chartFolderId);
            var chartDatas = new List<ChartData>();
            var json = bro.FlattenRows();
            for (var i = 0; i < json.Count; ++i)
            {
                var chartData = new ChartData
                {
                    chartName = json[i]["chartName"].ToString().Replace(version, ""),
                    chartExplain = json[i]["chartExplain"].ToString(),
                    selectedChartFileId = json[i]["selectedChartFileId"].ToString()
                };

                chartDatas.Add(chartData);
            }

            var sb = new StringBuilder();

            var chartIndex = 0;
            foreach (var chart in chartDatas)
            {
                var chartContents = Backend.Chart.GetChartContents(chart.selectedChartFileId);
                if (!chartContents.IsSuccess())
                {
                    throw new Exception($"Get Chart Contents Error:{chartContents}");
                }

                var toJson = chartContents.GetReturnValue().Replace("rows", chart.chartName);

                if (chartIndex > 0)
                {
                    var length = toJson.Length - 1;
                    toJson = "," + toJson[1..length];
                }

                sb.Insert(Mathf.Max(sb.Length - 1, 0), toJson);
#if IS_TEST
                    Debug.Log($"{chart.chartName} is Success");
#endif
                chartIndex++;
            }

            _instance = JsonConvert.DeserializeObject<CloudData>(sb.ToString(),
                BackEndJsonUtility.BackEndJsonConverters);
        }
    }

    public class ChartData
    {
        public string chartName;
        public string chartExplain;
        public string selectedChartFileId;
    }
}