﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire {
  public enum MessageDefine
  {
    Msg_Invalid = 0,
    Msg_Ping,
    Msg_Pong,
    Msg_CR_ShakeHands,
    Msg_RC_ShakeHands_Ret,
    Msg_CR_Observer,
    Msg_CRC_Create,
    Msg_RC_Enter,
    Msg_RC_Disappear,
    Msg_RC_Dead,
    Msg_RC_Revive,
    Msg_CRC_Exit,
    Msg_CRC_MoveStart,
    Msg_CRC_MoveStop,
    Msg_RC_MoveMeetObstacle,
    Msg_CRC_Face,
    Msg_CRC_Skill,
    Msg_CRC_Effect,                   //10
    Msg_CRC_Action,
    Msg_RC_CreateNpc,
    Msg_RC_DestroyNpc,
    Msg_RC_NpcEnter,
    Msg_RC_NpcMove,
    Msg_RC_NpcFace,
    Msg_RC_NpcTarget,
    Msg_RC_NpcSkill,
    Msg_RC_NpcDead,
    Msg_RC_NpcDisappear,
    Msg_RC_Levelup,
    Msg_RC_SyncLevel,
    Msg_RC_SyncExpMoney,
    Msg_RC_SyncProperty,
    Msg_RC_SyncImpactEffect,
    Msg_RC_SyncItem,
    Msg_RC_ItemChanged,
    Msg_RC_CreateSceneLogic,
    Msg_RC_DestroySceneLogic,
    Msg_RC_SceneLogicImpact,
    Msg_CR_KeyPress,
    Msg_RC_NpcFaceDir,
    Msg_RC_NpcProperty,
    Msg_RC_Move,
    Msg_RC_DebugSpaceInfo,
    Msg_CR_SwitchDebug,
    Msg_CRC_DebugCommand,
    Msg_CR_BuyItem,
    Msg_CR_SellItem,
    Msg_CR_SwapItem,
    Msg_CR_DiscardItem,
    Msg_RC_PvpFinish,
    Msg_RC_PveFinish,
    Msg_RC_SyncCombatStatisticInfo,
    Msg_RC_PvpCombatInfo,
    Msg_CRC_UpdateSkill,
    Msg_RC_PlayAnimation,
    Msg_RC_SendImpactToEntity,
    Msg_RC_PlayEffect,
    Msg_CRC_MousePos,
    Msg_RC_RemoveImpactFromEntity,
    Msg_CRC_InteractObject,
    Msg_RC_ControlObject,
    Msg_RC_RefreshItemSkills,
    Msg_RC_HighlightPrompt,
    Msg_CRC_DescriptorSync,
    Msg_CR_Execute,
    Msg_RC_ExecuteResult,
    Msg_RC_NotifyEarnMoney,
    Msg_CR_QuitClient,
    Msg_RC_UserMove,
    Msg_RC_UserFace,
    Msg_CRC_UserFindPath,
    Msg_Max,
  }
}