using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

/// <summary>
/// Adds some custom PlayerLoop phases to allow for more granular control of async routine execution
/// </summary>
public static class CustomUpdatePhases
{
    public static class PostUpdatePhase
    {
        public static event Action OnPostUpdate;

        public static PlayerLoopSystem CreateSystem()
        {
            return new PlayerLoopSystem()
            {
                type = typeof(PostUpdatePhase),
                updateDelegate = () => OnPostUpdate?.Invoke()
            };
        }
    }

    public static class PreRenderPhase
    {
        public static event Action OnPreRender;

        public static PlayerLoopSystem CreateSystem()
        {
            return new PlayerLoopSystem()
            {
                type = typeof(PreRenderPhase),
                updateDelegate = () => OnPreRender?.Invoke()
            };
        }
    }
    
    public static class EndOfFramePhase
    {
        public static event Action OnEndOfFrame;

        public static PlayerLoopSystem CreateSystem()
        {
            return new PlayerLoopSystem()
            {
                type = typeof(EndOfFramePhase),
                updateDelegate = () => OnEndOfFrame?.Invoke()
            };
        }
    }
    
    private static bool playerLoopModified = false;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ModifyPlayerLoop()
    {
        if (playerLoopModified)
        {
            return;
        }
        
        var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        AppendSystem<Update.ScriptRunBehaviourUpdate>(ref playerLoop, PostUpdatePhase.CreateSystem());
        AppendSystem<PostLateUpdate>(ref playerLoop, PreRenderPhase.CreateSystem());
        AppendSystem<PostLateUpdate.TriggerEndOfFrameCallbacks>(ref playerLoop, EndOfFramePhase.CreateSystem());

        PlayerLoop.SetPlayerLoop(playerLoop);
        playerLoopModified = true;

        // Uncomment to lop the modified player loop at startup
        //LogPlayerLoop(playerLoop);
    }
    
    private static bool AppendSystem<T>(ref PlayerLoopSystem system, PlayerLoopSystem newSystem)
    {
        if (system.type == typeof(T))
        {
            var modifiableList = system.subSystemList?.ToList() ?? new List<PlayerLoopSystem>();
            modifiableList.Insert( 0, newSystem );
            system.subSystemList = modifiableList.ToArray();
            
            return true;
        }
        
        if (system.subSystemList != null)
        {
            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (AppendSystem<T>(ref system.subSystemList[i], newSystem))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    private static void LogPlayerLoop(PlayerLoopSystem playerLoop)
    {
        var stringBuilder = new StringBuilder();

        void BuildPlayerLoopString(PlayerLoopSystem system, int depth = 0)
        {
            for (int i = 1; i < depth; i++)
            {
                stringBuilder.Append("    ");
            }
            
            if (system.type != null)
            {
                stringBuilder.AppendLine(system.type.Name);
            }
            
            if (system.subSystemList != null)
            {
                foreach (var subSystem in system.subSystemList)
                {
                    BuildPlayerLoopString(subSystem, depth+1);
                }
            }
        }
        
        BuildPlayerLoopString(playerLoop);
        Debug.Log(stringBuilder);
    }
}
