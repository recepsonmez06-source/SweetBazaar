using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SweetBazaar.Game
{
    // Tiny coroutine-based animation helpers; no tween library needed for the few motions the game has.
    internal static class Tween
    {
        public static float EaseInOut(float t) => t * t * (3f - 2f * t);

        public static float EaseOutCubic(float t)
        {
            float u = 1f - t;
            return 1f - u * u * u;
        }

        // Overshoots slightly before settling; used for pop-in effects.
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        // Moves in world space; arc is the extra height reached halfway (a small hop).
        public static IEnumerator MoveWorld(Transform target, Vector3 to, float duration, float arc = 0f)
        {
            Vector3 from = target.position;
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float k = EaseInOut(time / duration);
                target.position = Vector3.Lerp(from, to, k) + Vector3.up * (arc * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            target.position = to;
        }

        public static IEnumerator MoveLocal(Transform target, Vector3 to, float duration)
        {
            Vector3 from = target.localPosition;
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                target.localPosition = Vector3.Lerp(from, to, EaseInOut(time / duration));
                yield return null;
            }
            target.localPosition = to;
        }

        public static IEnumerator ScaleTo(Transform target, Vector3 to, float duration, Func<float, float> ease = null)
        {
            ease = ease ?? EaseInOut;
            Vector3 from = target.localScale;
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                target.localScale = Vector3.LerpUnclamped(from, to, ease(time / duration));
                yield return null;
            }
            target.localScale = to;
        }

        public static IEnumerator Delay(float seconds, IEnumerator then)
        {
            for (float time = 0f; time < seconds; time += Time.deltaTime)
                yield return null;
            yield return then;
        }

        // Runs the routines at the same time and finishes when all of them are done.
        public static IEnumerator All(MonoBehaviour host, IList<IEnumerator> routines)
        {
            int running = routines.Count;
            foreach (var routine in routines)
                host.StartCoroutine(Run(routine, () => running--));

            while (running > 0)
                yield return null;
        }

        private static IEnumerator Run(IEnumerator routine, Action done)
        {
            yield return routine;
            done();
        }
    }
}
