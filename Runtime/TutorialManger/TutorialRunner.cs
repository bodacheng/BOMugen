using System.Collections.Generic;
using dataAccess;
using mainMenu;
using UnityEngine;

public class TutorialRunner
{
    static TutorialRunner _instance;
    public static TutorialRunner Main
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TutorialRunner();
            }
            return _instance;
        }
    }

    public void Shutdown()
    {
        currentProcess = null;
        _tutorialProcesses.Clear();
    }
    
    TutorialProcess currentProcess;
    private readonly List<TutorialProcess> _tutorialProcesses = new List<TutorialProcess>();
    
    void GenerateStep1Tutorial()
    {
        var skillEditTry = new SkillEditTry("openInstruction1");
        _tutorialProcesses.Clear();
        _tutorialProcesses.Add(skillEditTry);
    }
    
    void GenerateStep2Tutorial()
    {
        bool StageOneFinished()
        {
            return PlayerAccountInfo.Me.tutorialProgress == "StageOneFinished";
        }
        
        var waitFighting = new WaitProcess(StageOneFinished);
        _tutorialProcesses.Clear();
        _tutorialProcesses.Add(waitFighting);
        ArcadeModeManager.Instance.DirectToArcadeStage(PlayerAccountInfo.Me.arcadeProcess + 1, false);
    }

    void GenerateStep3Tutorial()
    {
        if (ProcessesRunner.Main.currentProcess == null || ProcessesRunner.Main.currentProcess.Step != MainSceneStep.FrontPage)
        {
            ReturnLayer.Stack(MainSceneStep.FrontPage, (x)=> PreScene.target.trySwitchToStep(x, false));
        }
        
        var tryGotcha = new TryGotcha();
        var forceBack = new ForceBack(() => ProcessesRunner.Main.currentProcess.Step == MainSceneStep.FrontPage);
        
        _tutorialProcesses.Clear();
        _tutorialProcesses.Add(tryGotcha);
        _tutorialProcesses.Add(forceBack);
    }

    void GenerateStep4Tutorial()
    {
        PreScene.target.trySwitchToStep(MainSceneStep.FrontPage, false);
        var goTo = new GoTo(MainSceneStep.UnitList);
        var openSkillEdit = new OpenSkillEdit("1");
        var skillEditTry = new SkillEditTry("openInstruction2");
        _tutorialProcesses.Clear();
        _tutorialProcesses.Add(goTo);
        _tutorialProcesses.Add(openSkillEdit);
        _tutorialProcesses.Add(skillEditTry);
    }
    
    void GenerateStep5Tutorial()
    {
        ArcadeModeManager.Instance.DirectToArcadeStage(PlayerAccountInfo.Me.arcadeProcess + 1, false);
        
        bool StageTwoFinished()
        {
            return PlayerAccountInfo.Me.tutorialProgress == "Finished";
        }
        
        
        var goTo = new GoTo(MainSceneStep.ArcadeFront);
        var waitFighting = new WaitProcess(StageTwoFinished);
        _tutorialProcesses.Clear();
        _tutorialProcesses.Add(goTo);
        _tutorialProcesses.Add(waitFighting);
    }
    
    public void Process()
    {
        if (currentProcess != null)
        {
            currentProcess.LocalUpdate();
            if (currentProcess.CanEnterOtherProcess()) // && currentProcess.nextProcessStep != MainSceneStep.None
            {
                MoveToNext();
            }
        }
    }
    
    public void MoveToNext()
    {
        var index = 0;
        TutorialProcess toBeRemove = currentProcess;
        if (currentProcess != null)
        {
            index = _tutorialProcesses.IndexOf(currentProcess)+ 1;
            toBeRemove = currentProcess;
        }
        var toRunProcess = _tutorialProcesses.Count > index ? _tutorialProcesses[index] : null;
        ChangeProcess(toRunProcess);
        if (toBeRemove != null)
            _tutorialProcesses.Remove(toBeRemove);
    }
    
    void ChangeProcess(TutorialProcess nextProcess)
    {
        currentProcess?.ProcessEnd();
        currentProcess = nextProcess;
        if (currentProcess != null)
        {
            currentProcess.ProcessEnter();
        }
        else
            TutorialCheck();
    }
    
    // 所有的教程链都是以FrontPage为起点
    public void TutorialCheck()
    {
        // 在以下的分歧之前，账户信息必须是最新，否则反应不到账户真实进度。
        switch (PlayerAccountInfo.Me.tutorialProgress)
        {
            case "Started":
                Main.GenerateStep1Tutorial();
                Main.MoveToNext();
                PlayFabReadClient.DontShowFrontFight = "false";
                break;
            case "SkillEditFinished": // 技能编辑教程结束 
                Main.GenerateStep2Tutorial();
                Main.MoveToNext();
                break;
            case "StageOneFinished": // 第一关结束
                GenerateStep3Tutorial();
                Main.MoveToNext();
                break;
            case "GotchaFinished":
                GenerateStep4Tutorial();
                Main.MoveToNext();
                break;
            case "SkillEditFinished2":
                
                var adam = dataAccess.Units.GetByRId("1");
                TeamSet.GetTargetSet("arcade").SetPosUnitByInstanceID(0, adam.id);
                TeamSet.SaveTeamSet("arcade", (x) =>
                {
                    if (x)
                    {
                        GenerateStep5Tutorial();
                        Main.MoveToNext();
                    }
                });
                
                break;
            case "Finished":
                PlayFabReadClient.DontShowFrontFight = "true";
                break;
            default:
                break;
        }
    }
}
