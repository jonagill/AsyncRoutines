#if DOTWEEN_ENABLED

using AsyncRoutines;
using DG.Tweening;
using UnityEngine.Assertions;

namespace AsyncRoutines
{
    public static class DOTweenExtensions
    {
        public class YieldForTween : IYieldInstruction
        {
            private Tween _tween;

            public UpdatePhase UpdatePhase { get; }

            public bool Poll() => _tween.active && !_tween.IsComplete();

            public float DeferPollUntilTime => 0.0f;
            public float DeferPollUntilRealTime => 0.0f;


            public YieldForTween(Tween tween, UpdatePhase updatePhase = UpdatePhase.Update)
            {
                Assert.IsNotNull( tween );

                _tween = tween;
                UpdatePhase = updatePhase;
            }
        }

        public static IYieldInstruction ToYield(this Tween tween, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new YieldForTween( tween, updatePhase );
        }
    }
}
#endif
