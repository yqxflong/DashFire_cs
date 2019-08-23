﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire {
  class AiLogic_Demo_Boss : AbstractNpcStateLogic {

    protected override void OnInitStateHandlers()
    {
      SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
      SetStateHandler((int)AiStateId.Pursuit, this.PursuitHandler);
      SetStateHandler((int)AiStateId.Combat, this.CombatHandler);
      SetStateHandler((int)AiStateId.GoHome, this.GoHomeHandler);
      SetStateHandler((int)AiStateId.MoveCommand, this.MoveCommandHandler);
      SetStateHandler((int)AiStateId.PatrolCommand, this.PatrolCommandHandler);
    }

    protected override void OnStateLogicInit(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time = 0;
      npc.GetMovementStateInfo().IsMoving = false;
      info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
      info.Target = 0;
      if (!string.IsNullOrEmpty(info.AiParam[0])) {
        if (int.Parse(info.AiParam[0]) == 0) {
          InitPatrolData(npc);
        } else if(int.Parse(info.AiParam[0]) == 1){
          InitIdleAnim(npc);
        }
      }
      npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, GetWalkSpeed(npc));
      npc.GetMovementStateInfo().MovementMode = MovementMode.LowSpeed;
    }

    private void InitPatrolData(NpcInfo npc) {
      AiData_ForPatrolCommand data = new AiData_ForPatrolCommand();
      data.IsLoopPatrol = true;
      List<Vector3> path = new List<Vector3>();
      NpcAiStateInfo info = npc.GetAiStateInfo();
      path = Converter.ConvertVector3DList(info.AiParam[1]);
      data.PatrolPath.SetPathPoints(npc.GetAiStateInfo().HomePos, path);
      npc.GetAiStateInfo().AiDatas.AddData<AiData_ForPatrolCommand>(data);
      AiData_Demo_Melee aiData = GetAiData(npc);
      aiData.HasPatrolData = true;
    }

    private void InitIdleAnim(NpcInfo npc) {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      if(null != info){
        List<int> anims = Converter.ConvertNumericList<int>(info.AiParam[1]);
        if (null != OnSetNpcIdleAnim) {
          OnSetNpcIdleAnim(npc, anims);
        }
        if (!string.IsNullOrEmpty(info.AiParam[2])) {
          AiData_Demo_Melee aiData = GetAiData(npc);
          aiData.MeetEnemyAnim = (Animation_Type)int.Parse(info.AiParam[2]);
        }
      }
    }
    private void IdleHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 100) {
        info.Time = 0;
        if (GetAiData(npc).HasPatrolData) {
          npc.GetMovementStateInfo().IsMoving = false;
          ChangeToState(npc, (int)AiStateId.PatrolCommand);
        } else {
          CharacterInfo target = null;
          if (info.IsExternalTarget) {
            target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
            if (null == target) {
              target = AiLogicUtility.GetNearstTargetHelper(npc, CharacterRelation.RELATION_ENEMY);
              if (null != target)
                info.Target = target.GetId();
            }
          } else {
            target = AiLogicUtility.GetNearstTargetHelper(npc, CharacterRelation.RELATION_ENEMY);
            if (null != target)
              info.Target = target.GetId();
          }
          if (null != target) {
            NotifyNpcTargetChange(npc);
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);
            info.Time = 0;
            ChangeToState(npc, (int)AiStateId.Pursuit);
          }
        }
      }
    }

    private void PursuitHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      bool goHome = false;
      AiData_Demo_Melee data = GetAiData(npc);
      if (null != data) {
        CharacterInfo target = AiLogicUtility.GetLivingCharacterInfoHelper(npc, info.Target);
        if (null != target) {
          if (data.WaitTime <= m_ResponseTime) {
            if (!data.HasMeetEnemy) {
              if (null != OnNpcMeetEnemy) {
                OnNpcMeetEnemy(npc, data.MeetEnemyAnim);
              }
              data.HasMeetEnemy = true;
            }
            float angle = Geometry.GetYAngle(npc.GetMovementStateInfo().GetPosition2D(), target.GetMovementStateInfo().GetPosition2D());
              NotifyNpcFace(npc, angle);
            data.WaitTime += deltaTime;
            return;
          }
          data.WalkTime += deltaTime;
          if(data.WalkTime < 4000){
            npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, GetWalkSpeed(npc));
            npc.GetMovementStateInfo().MovementMode = MovementMode.LowSpeed;
          } else {
            npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, GetRunSpeed(npc));
            npc.GetMovementStateInfo().MovementMode = MovementMode.HighSpeed;
          }
          float dist = (float)npc.GetActualProperty().AttackRange;
          float distGoHome = (float)npc.GohomeRange;
          Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
          float powDist = Geometry.DistanceSquare(srcPos, targetPos);
          if (0 == data.SkillToCast) {
            data.SkillToCast = GetNextSkillInChase();
          }
          if (powDist < (m_TauntDisMax) * (m_TauntDisMax) && powDist > (m_TauntDisMin) * (m_TauntDisMin)) {
            if (m_Taunt == data.SkillToCast || m_HeavyAttack == data.SkillToCast) {
              float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              if (Geometry.IsSameDouble(angle, npc.GetMovementStateInfo().GetFaceDir())){
              OnNpcSkill(npc, data.SkillToCast, target);
              data.SkillToCast = 0;
              }
            }
          }
          float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
          if (powDist < dist * dist) {
            npc.GetMovementStateInfo().IsMoving = false;
            info.Time = 0;
            data.Time = 0;
            npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, GetRunSpeed(npc));
            npc.GetMovementStateInfo().MovementMode = MovementMode.HighSpeed;
            ChangeToState(npc, (int)AiStateId.Combat);
            NotifyNpcMove(npc);
          } else{
            info.Time += deltaTime;
            if (info.Time > m_IntervalTime) {
              info.Time = 0;
              AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, m_IntervalTime, true, this);
            }
          }
        } else {
          goHome = true;
        }
        if (goHome) {
          npc.GetMovementStateInfo().IsMoving = false;
          NotifyNpcMove(npc);
          info.Time = 0;
          data.Time = 0;
          npc.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, 3.5f);
          ChangeToState(npc, (int)AiStateId.Combat);
          ChangeToState(npc, (int)AiStateId.GoHome);
        }
      }
    }

    private float GetWalkSpeed(NpcInfo info) {
      Data_ActionConfig ac = info.GetCurActionConfig();
      if (null != ac) {
        return ac.m_SlowStdSpeed;
      } else {
        LogSystem.Error("AiLogic_Demo_Melee can't find CurActionConfig");
      }
      return 0.0f;
    }

    private float GetRunSpeed(NpcInfo info) {
      Data_ActionConfig ac = info.GetCurActionConfig();
      if (null != ac) {
        return ac.m_FastStdSpeed;
      } else {
        LogSystem.Error("AiLogic_Demo_Melee can't find CurActionConfig");
      }
      return 0.0f;
    }
    private int GetNextSkillInChase() {
      int x = Helper.Random.Next(0, 9);
      if (x < 2) {
        return m_Taunt; 
      } else if (x < 9) {
        return m_HeavyAttack;
      } else {
        return m_NormalAttack;
      }
    }
    private int GetNextSkillInCombat() {
      int x = Helper.Random.Next(0, 9);
      if (x < 2) {
        return m_Taunt; 
      } else if (x < 6) {
        return m_HeavyAttack;
      } else {
        return m_NormalAttack;
      }
    }
    private void CombatHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > m_IntervalTime) {
        AiData_Demo_Melee data = GetAiData(npc);
        if (null != data) {
          data.Time += info.Time;
          info.Time = 0;
          bool goHome = false;
          CharacterInfo target = AiLogicUtility.GetLivingCharacterInfoHelper(npc, info.Target);
          //CharacterInfo target = AiLogicUtility.GetInterestestTargetHelper(npc, CharacterRelation.RELATION_ENEMY, AiTargetType.USER);
          if (null != target) {
            float dist = (float)npc.GetActualProperty().AttackRange;
            float distGoHome = (float)npc.GohomeRange;
            ScriptRuntime.Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
            ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float powDist = Geometry.DistanceSquare(srcPos, targetPos);
            float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
            if (powDist < dist * dist) {
              float rps = npc.GetActualProperty().Rps;
              if (rps > 0.001f && data.Time > 1000 / rps) {
                data.Time = 0;
                float dir = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
                NotifyNpcFace(npc, dir);
                //npc.GetMovementStateInfo().SetMoveDir(dir);
                if (powDist < (npc.ViewRange - 1.0f) * (npc.ViewRange - 1.0f)) {
                  if (0 == data.SkillToCast) {
                    data.SkillToCast = GetNextSkillInCombat();
                  }
                  if (null != OnNpcSkill) {
                    float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
                    if (Geometry.IsSameDouble(angle, npc.GetMovementStateInfo().GetFaceDir())) {
                    OnNpcSkill(npc, data.SkillToCast, target);
                    data.SkillToCast = 0;
                    }
                  }
                }
              }
            } else{
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              info.Time = 0;
              data.FoundPath.Clear();
              data.SkillToCast = 0;
              ChangeToState(npc, (int)AiStateId.Pursuit);
            }
          } else {
            goHome = true;
          }
          if (goHome) {
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);
            info.Time = 0;
            ChangeToState(npc, (int)AiStateId.GoHome);
          }
        } else {
          info.Time = 0;
        }
      }
    }
    private void GoHomeHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      ChangeToState(npc, (int)AiStateId.Idle);
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > m_IntervalTime) {
        info.Time = 0;
        AiData_Demo_Melee data = GetAiData(npc);
        if (null != data) {
          Vector3 targetPos = info.HomePos;
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
          if (powDistToHome <= 1) {
            npc.GetMovementStateInfo().IsMoving = false;
            info.Time = 0;
            ChangeToState(npc, (int)AiStateId.Idle);
          } else {
            AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, 100, true, this);
          }
        }
      }
    }
    private void MoveCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      AiLogicUtility.DoMoveCommandState(npc, aiCmdDispatcher, deltaTime);
    }
    private void PatrolCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      AiLogicUtility.DoPatrolCommandState(npc, aiCmdDispatcher, deltaTime, this);
    }
    private AiData_Demo_Melee GetAiData(NpcInfo npc)
    {
      AiData_Demo_Melee data = npc.GetAiStateInfo().AiDatas.GetData<AiData_Demo_Melee>();
      if (null == data) {
        data = new AiData_Demo_Melee();
        npc.GetAiStateInfo().AiDatas.AddData(data);
      }
      return data;
    }
    private const long m_IntervalTime = 100;
    private const long m_WaltMaxTime = 2000;
    private const long m_ResponseTime = 2000;
    private const int m_NormalAttack = 10001;
    private const int m_HeavyAttack = 10002;
    private const int m_Taunt = 10003;
    private const float m_TauntDisMin = 4.0f;
    private const float m_TauntDisMax = 7.0f;
  }
}
