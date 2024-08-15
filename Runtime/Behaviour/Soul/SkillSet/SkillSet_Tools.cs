﻿using System;
using System.Collections.Generic;
using System.Linq;
using dataAccess;
using Skill;

public partial class SkillSet
{
    /// <summary>
    /// 这个仅仅是一个辅助于技能编辑画面的工具，并不能用来判断角色实际装备中技能
    /// 实际装备中技能是用Stones.GetEquippingStones
    /// </summary>
    /// <returns></returns>
    public List<string> SkillIDList()
    {
        var ids = new List<string>();
        
        if (a1 != null)
            ids.Add(a1);
        if (a2 != null)
            ids.Add(a2);
        if (a3 != null)
            ids.Add(a3);
            
        if (b1 != null)
            ids.Add(b1);
        if (b2 != null)
            ids.Add(b2);
        if (b3 != null)
            ids.Add(b3);
            
        if (c1 != null)
            ids.Add(c1);
        if (c2 != null)
            ids.Add(c2);
        if (c3 != null)
            ids.Add(c3);
        
        return ids;
    }
    
    public List<string> GetAllInstanceIdsThatRelatesToCurrentSet()
    {
        List<string> instanceIds = new List<string>();
        foreach (var skillId in SkillIDList())
        {
            instanceIds.AddRange(Stones.GetMyStonesBySkillID(skillId));
        }

        return instanceIds;
    }
    
    // 获取平均技能等级
    public float GetAerLevel(List<float> levels)
    {
        float aver = 0;
        foreach (var t in levels)
        {
            aver += t;
        }
        return (float)Math.Round(aver / levels.Count, 1);
    }
    
    public static float INI_Hp(List<string> skillIds, float lv)
    {
        float wholeHp = 0;
        foreach (var skillId in skillIds)
        {
            var skillConfig = SkillConfigTable.GetSkillConfigByRecordId(skillId);
            wholeHp += FightGlobalSetting.StoneHpCal(skillConfig.HP_WEIGHT, lv);
        }
        return wholeHp;
    }
    
    // 获取技能实体列表，调用必须在SortNineAndTwo之后
    public List<SkillEntity> SkillEntityList()
    {
        var behaviorTransitionSets = new List<SkillEntity>();
        
        if (A1 != null)
            behaviorTransitionSets.Add(A1);
        if (A2 != null)
            behaviorTransitionSets.Add(A2);
        if (A3 != null)
            behaviorTransitionSets.Add(A3);
        
        if (B1 != null)
            behaviorTransitionSets.Add(B1);
        if (B2 != null)
            behaviorTransitionSets.Add(B2);
        if (B3 != null)
            behaviorTransitionSets.Add(B3);
        
        if (C1 != null)
            behaviorTransitionSets.Add(C1);
        if (C2 != null)
            behaviorTransitionSets.Add(C2);
        if (C3 != null)
            behaviorTransitionSets.Add(C3);
            
        if (D != null)
            behaviorTransitionSets.Add(D);
        if (M != null)
            behaviorTransitionSets.Add(M);
        if (R != null)
            behaviorTransitionSets.Add(R);
        if (_empty != null)
            behaviorTransitionSets.Add(_empty);
        if (_victory != null)
            behaviorTransitionSets.Add(_victory);    
        if (_death != null)
            behaviorTransitionSets.Add(_death);
        if (_hit != null)
            behaviorTransitionSets.Add(_hit);
        if (_getUp != null)
            behaviorTransitionSets.Add(_getUp);
        if (_knockOff != null)
            behaviorTransitionSets.Add(_knockOff);
            
        return behaviorTransitionSets;
    }
    
    //下面的环节纯粹是针对SkillPrintOut的一些处理
    public IDictionary<int, SkillEntity> GetAttack1Chan()
    {
        IDictionary<int, SkillEntity> chain = new Dictionary<int, SkillEntity>
        {
            { 1, A1 },
            { 2, A2 },
            { 3, A3 }
        };
        return chain;
    }
    public IDictionary<int, SkillEntity> GetAttack2Chan()
    {
        IDictionary<int, SkillEntity> chain = new Dictionary<int, SkillEntity>
        {
            { 1, B1 },
            { 2, B2 },
            { 3, B3 }
        };
        return chain;
    }
    public IDictionary<int, SkillEntity> GetAttack3Chan()
    {
        IDictionary<int, SkillEntity> chain = new Dictionary<int, SkillEntity>
        {
            { 1, C1 },
            { 2, C2 },
            { 3, C3 }
        };
        return chain;
    }
    
    public SkillConfig GetA1Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(a1);
    }
    public SkillConfig GetA2Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(a2);
    }
    public SkillConfig GetA3Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(a3);
    }
    public SkillConfig GetB1Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(b1);
    }
    public SkillConfig GetB2Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(b2);
    }
    public SkillConfig GetB3Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(b3);
    }
    public SkillConfig GetC1Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(c1);
    }
    public SkillConfig GetC2Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(c2);
    }
    public SkillConfig GetC3Config()
    {
        return SkillConfigTable.GetSkillConfigByRecordId(c3);
    }
        
    public SkillEntity GetM_STS()
    {
        return M;
    }
    
    public int GetLowestSpLevel()
    {
        var sp = new List<int>()
        {
            GetA1Config() != null ? GetA1Config().SP_LEVEL : 0, 
            GetA2Config() != null ? GetA2Config().SP_LEVEL : 0,
            GetA3Config() != null ? GetA3Config().SP_LEVEL : 0,
            
            GetB1Config() != null ? GetB1Config().SP_LEVEL : 0, 
            GetB2Config() != null ? GetB2Config().SP_LEVEL : 0,
            GetB3Config() != null ? GetB3Config().SP_LEVEL : 0,
            
            GetC1Config() != null ? GetC1Config().SP_LEVEL : 0, 
            GetC2Config() != null ? GetC2Config().SP_LEVEL : 0,
            GetC3Config() != null ? GetC3Config().SP_LEVEL : 0,
        };
        
        return sp.Min();
    }
    
    // 这个的运行是建立在九宫格满的前提上
    public int RecommendedTargetReplaceSlot(bool mugen)
    {
        var list = SkillIDList();
        var A1Config = SkillConfigTable.GetSkillConfigByRecordId(list[0]);
        var B1Config = SkillConfigTable.GetSkillConfigByRecordId(list[3]);
        var C1Config = SkillConfigTable.GetSkillConfigByRecordId(list[6]);

        int normalSkillCountAtFirstRow = 0;

        void temp(int sp)
        {
            if (sp == 0)
            {
                normalSkillCountAtFirstRow += 1;
            }
        }
        
        temp(A1Config.SP_LEVEL);
        temp(B1Config.SP_LEVEL);
        temp(C1Config.SP_LEVEL);
        
        List<int> hopeSearchOrder = new List<int>()
        {
            0,3,6,1,4,7,2,5,8
        };
        for (int order = 0; order <= hopeSearchOrder.Count; order++)
        {
            var index = hopeSearchOrder[order];
            var config = SkillConfigTable.GetSkillConfigByRecordId(list[index]);
            if (config.SP_LEVEL == 0 && (mugen || ((index != 0 && index != 3 && index != 6))||
                                         normalSkillCountAtFirstRow > 1))
            {
                return index + 1;
            }
        }
        return 0;
    }
}
