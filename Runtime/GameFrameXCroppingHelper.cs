using System;
using GameFrameX.Timer.Runtime;
using UnityEngine;

namespace GameFrameX.Timer.Runtime
{
    public class GameFrameXCroppingHelper : MonoBehaviour
    {
        private void Start()
        {
            _ = typeof(TimerComponent);
            _ = typeof(ITimerManager);
            _ = typeof(TimerManager);
        }
    }
}