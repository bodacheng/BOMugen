﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using dataAccess;
using System.IO;
using System.Linq;
using mainMenu;
using NoSuchStudio.Common;
using PlayFab.ClientModels;

public class FightInfo : ScriptableObject
{
    public int battleGroundID;
    public int fightBGM = 0;
    
    // 底下这个记录的是敌人的信息
    [SerializeField] List<UnitInfo> unitsData = new List<UnitInfo>();

    public UnitInfo GetRepresentUnitInfo()
    {
        return FightMembers.EnemySets.GetValues().FirstOrDefault(x => x != null && x.id != null && Units.GetUnitConfig(x.r_id) != null);
    }
    
    public List<UnitInfo> UnitsData
    {
        get => unitsData;
        set => unitsData = value;
    }

    public string GetBGMKey()
    {
        switch (fightBGM)
        {
            case 0:
                return CommonSetting.FightThemeAddressKey1;
            case 1:
                return CommonSetting.FightThemeAddressKey2;
            default:
                return CommonSetting.FightThemeAddressKey1;
        }
    }
    
    public FightEventType EventType
    {
        set;
        get;
    }
    
    [SerializeField] bool evolutionMode;
    public float team1HpRate = 1f;
    public float team2HpRate = 1f;
    public CriticalGaugeMode team1CGMode = CriticalGaugeMode.Normal;
    public CriticalGaugeMode team2CGMode = CriticalGaugeMode.Normal;
    public TeamMode team1Mode = TeamMode.Rotation;
    public TeamMode team2Mode = TeamMode.Rotation;
    public AIMode team1AIMode = AIMode.Aggressive;
    public AIMode team2AIMode = AIMode.Aggressive;
    public int dumbAIDecisionDelay = 20;
    public int dreamComboAIRateNum = 5;

    public float stageRefLevel;

    public void SetUnitLevelByRefLevel()
    {
        if (!EvolutionMode)
        {
            foreach (var data in UnitsData)
            {
                data.level = stageRefLevel;
            }
        }
        else
        {
            // 原本希望让敌人按登场顺序逐渐等级提升。。。
            for (var index = 0; index < UnitsData.Count; index++)
            {
                var unitInfo = UnitsData[index];
                unitInfo.level = stageRefLevel;
            }
        }
    }
    
    // 首发版本我们未必把进化模式的等级分配想的太过复杂。。
    public bool EvolutionMode
    {
        get => evolutionMode;
        set
        {
            if (value)
            {
                team1Mode = TeamMode.Rotation;
                team2Mode = TeamMode.Rotation;
                AutoFillEvolution(FightMembers, "human");
            }
            evolutionMode = value;
        }
    }

    private List<List<int>> EnemyForEvolutionTeamUnitSets = new List<List<int>>
    {
        new List<int>(){8,11,16,1},
        new List<int>(){8,11,16,2},
        new List<int>(){8,11,16,3},
        new List<int>(){9,9,9,2},
        new List<int>(){13,13,13,5},
        new List<int>(){12,12,12,6},
        new List<int>(){10,10,10,14},
        new List<int>(){9,9,9,1},
        new List<int>(){11,11,11,15},
        new List<int>(){16,16,16,3},
        new List<int>(){1,2,3,7},
        new List<int>(){5,5,5,6},
        new List<int>(){3,6,4,14},
        new List<int>(){13,13,13,12},
        new List<int>(){15,15,15,15},
        new List<int>(){9,9,9,10},
    };

    // 我们设想这个玩法下玩家一共进化三次
    private readonly int _evolutionEnemyCount = 4;
    void AutoFillEvolution(FightMembers target, string type)
    {
        var enemyRSet = EnemyForEvolutionTeamUnitSets.Random();
        var recordIds = Units.GetMonsterIDsAndNamesDic(type).Keys.ToList();
        for (var index = 0; index < _evolutionEnemyCount; index++)
        {
            var currentUnit = target.EnemySets.Get(0, index);
            var config = Units.GetUnitConfig(currentUnit?.r_id);
            if (currentUnit != null && config != null) continue;
            
            var unitInfo = new UnitInfo
            {
                id = index.ToString(),
                r_id = enemyRSet.Count > index ? enemyRSet[index].ToString() : recordIds.Random()
            };
            target.EnemySets.Set(0, index, unitInfo);
            SaveDicToData();
        }
        
        for (var index = 0; index < UnitsData.Count; index++)
        {
            var unitInfo = UnitsData[index];
            if (unitInfo.set.CheckEdit() != SkillSet.SkillEditError.Empty)
            {
                continue;
            }
            
            var form = new StoneFilterForm
            {
                Type = type,
                ExType = new[] { 0 }
            };
            var passiveSKillRecordId = UnitPassiveTable.GetUnitPassiveRecordId(unitInfo.r_id);
            switch (index)
            {
                case 0:
                    unitInfo.set =  SkillSet.RandomSkillSet(type, passiveSKillRecordId,  false, form, false);
                    break;
                case 1:
                    form = new StoneFilterForm
                    {
                        Type = type,
                        ExType = new[] { 0 , 1 }
                    };
                    unitInfo.set =  SkillSet.RandomSkillSet(type, passiveSKillRecordId,  false, form, false);
                    break;
                case 2:
                    form = new StoneFilterForm
                    {
                        Type = type,
                        ExType = new[] { 0, 1, 2 }
                    };
                    unitInfo.set =  SkillSet.RandomSkillSet(type, passiveSKillRecordId,  false, form, false);
                    break;
                default:
                    form = new StoneFilterForm
                    {
                        Type = type,
                        ExType = new[] { 0, 1, 2, 3 }
                    };
                    unitInfo.set =  SkillSet.RandomSkillSet(type, passiveSKillRecordId,  false, form, false);
                    break;
            }
        }
    }

    public int ArcadeFightMode
    {
        get;
        set;
    }

    public bool RunTutorial
    {
        set;
        get;
    }

    public string Team1OneWord
    {
        set;
        get;
    }
    
    public string Team2OneWord
    {
        set;
        get;
    }
    
    public void Awake()
    {
        OpenAndSetEnemyDataOnPlace();
    }

    public string ID
    {
        set;
        get;
    }
    public string Team1ID { set; get; }
    public string Team2ID { set; get; }
    
    public PlayerLeaderboardEntry Team1LeaderboardEntry {
        set;
        get;
    }
    
    public PlayerLeaderboardEntry Team2LeaderboardEntry {
        set;
        get;
    }
    
    public FightMembers FightMembers
    {
        set;
        get;
    }
    
    public bool Team1Auto
    {
        get;
        set;
    }
    
    public bool Team2Auto
    {
        get;
        set;
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetTeam"></param>
    /// <param name="path">"Assets/" 开头</param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static FightInfo CreateFightInfoAsset(FightMembers targetTeam, string path, string fileName)
    {
        var fightInfo = CreateInstance<FightInfo>();
        if (!Directory.Exists(path))
        {
            //if it doesn't, create it
            Directory.CreateDirectory(path);
        }
        
        fightInfo.FightMembers = targetTeam;
        fightInfo.SaveDicToData();
        fightInfo.team1Mode = TeamMode.Rotation;
        fightInfo.team2Mode = TeamMode.Rotation;
        
        AssetDatabase.CreateAsset(fightInfo, path + "/" + fileName + ".asset");
        Debug.Log("Generated：" + path + "/" + fileName + ".asset");
        AssetDatabase.Refresh();
        return fightInfo;
    }
    
    public static GangbangInfo CreateGangbangInfoAsset(FightMembers targetTeam, string path, string fileName)
    {
        var gangbangInfo = CreateInstance<GangbangInfo>();
        if (!Directory.Exists(path))
        {
            //if it doesn't, create it
            Directory.CreateDirectory(path);
        }
        
        gangbangInfo.FightMembers = targetTeam;
        gangbangInfo.SaveDicToData();
        gangbangInfo.team1Mode = TeamMode.Rotation;
        gangbangInfo.team2Mode = TeamMode.Rotation;
        
        AssetDatabase.CreateAsset(gangbangInfo, path + "/" + fileName + ".asset");
        Debug.Log("Generated：" + path + "/" + fileName + ".asset");
        AssetDatabase.Refresh();
        return gangbangInfo;
    }
    #endif

    public void OpenAndSetEnemyDataOnPlace()
    {
        ID = this.name;
        FightMembers = new FightMembers();
        for (var i = 0; i < unitsData.Count; i++)
        {
            FightMembers.EnemySets.Set(0,i, unitsData[i]);
        }
    }

    public void SaveDicToData()
    {
        unitsData = new List<UnitInfo>();
        foreach (var info in FightMembers.EnemySets.GetValues())
        {
            if (info != default)
                unitsData.Add(info);
        }
    }
    
    public void LoadMyTeam()
    {
        PosKeySet set;
        switch (EventType)
        {
            case FightEventType.Quest:
                set = TeamSet.Default;
                break;
            case FightEventType.Arena:
                set = TeamSet.Arena3V3;
                break;
            case FightEventType.Gangbang:
                set = TeamSet.Gangbang;
                break;
            case FightEventType.Event:
                set = TeamSet.Origin;
                break;
            default:
                set = TeamSet.Default;
                break;
        }
        
        FightMembers.HeroSets = set.LoadTeamDic();
        Team1ID = PlayerAccountInfo.Me.PlayFabId;
    }
    
    public static FightInfo ArenaStage(FightMembers fightUnits)
    {
        var stage = CreateInstance<FightInfo>();
        stage.FightMembers = fightUnits;
        stage.battleGroundID = 0;
        stage.team1Mode = TeamMode.Rotation;
        stage.team2Mode = TeamMode.Rotation;
        stage.EventType = FightEventType.Arena;
        return stage;
    }

    public static FightInfo Copy(FightInfo source)
    {
        var stage = CreateInstance<FightInfo>();
        
        stage.ID = source.ID;
        stage.ArcadeFightMode = source.ArcadeFightMode;
        stage.FightMembers = source.FightMembers;
        stage.battleGroundID = source.battleGroundID;
        stage.stageRefLevel = source.stageRefLevel;
        stage.fightBGM = source.fightBGM;
        stage.team1Mode = source.team1Mode;
        stage.team2Mode = source.team2Mode;
        stage.Team1Auto = source.Team1Auto;
        stage.Team2Auto = source.Team2Auto;
        stage.team1AIMode = source.team1AIMode;
        stage.team2AIMode = source.team2AIMode;
        stage.Team1ID = source.Team1ID;
        stage.Team2ID = source.Team2ID;
        stage.team1HpRate = source.team1HpRate;
        stage.team2HpRate = source.team2HpRate;
        stage.team1CGMode = source.team1CGMode;
        stage.team2CGMode = source.team2CGMode;
        stage.Team1LeaderboardEntry = source.Team1LeaderboardEntry;
        stage.Team2LeaderboardEntry = source.Team2LeaderboardEntry;
        stage.RunTutorial = source.RunTutorial;
        stage.evolutionMode = source.evolutionMode;
        stage.EventType = source.EventType;
        stage.dreamComboAIRateNum = source.dreamComboAIRateNum;
        return stage;
    }
    
    public static FightInfo RandomSkillTestStage(TeamMode teamMode)
    {
        var stage = CreateInstance<FightInfo>();
        stage.FightMembers = FightMembers.RandomSkillTest(teamMode);
        stage.battleGroundID = 0;
        stage.fightBGM = 0;
        stage.Team1Auto = true;
        stage.Team2Auto = true;
        stage.team1Mode = teamMode;
        stage.team2Mode = teamMode;
        stage.EventType = FightEventType.SkillTest;
        return stage;
    }
    
    public static FightInfo ScreenSaverStage(TeamMode teamMode)
    {
        var stage = CreateInstance<FightInfo>();
        stage.FightMembers = FightMembers.ScreenSaver(teamMode);
        stage.battleGroundID = 0;
        stage.fightBGM = 0;
        stage.Team1Auto = true;
        stage.Team2Auto = true;
        stage.team1Mode = teamMode;
        stage.team2Mode = teamMode;
        stage.EventType = FightEventType.SkillTest;
        return stage;
    }
    
    public static FightInfo RandomStage()
    {
        var stage = CreateInstance<FightInfo>();
        stage.FightMembers = FightMembers.RandomFight();
        stage.battleGroundID = 0;
        stage.fightBGM = 0;
        stage.team1Mode = TeamMode.Rotation;
        stage.team2Mode = TeamMode.Rotation;
        stage.EventType = FightEventType.Arena;
        return stage;
    }
}

public enum CriticalGaugeMode
{
    Normal,
    DoubleGain,
    Unlimited
}

public enum AIMode
{
    Aggressive,
    Dumb
}

// 系统会根据这个量来决定一场战斗结束后应该做什么。
// 比如一个剧情战斗，他结束了后应该是播放某个动画片，
// 再比如是自己打自己的一个战斗，结束后回到的应该是那个自己打自己的选人菜单。
public enum FightEventType
{
    Screensaver = 0,
    Quest = 1,
    Gangbang = 3,
    Arena = 2,
    Self = 4,
    SkillTest = 5,
    Event = 6
}

public enum TeamMode
{
    Keep = 0,
    MultiRaid = 1,
    Rotation = 2
}