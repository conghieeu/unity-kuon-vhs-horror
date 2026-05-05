using System.Collections;
using UnityEngine;

namespace UHFPS.Tools
{
    /// <summary>
    /// Công cụ hỗ trợ chạy Coroutine từ các lớp không kế thừa MonoBehaviour.
    /// </summary>
    [ThunderWire.Attributes.Summary("Cho phép chạy Coroutine từ các script không phải MonoBehaviour.")]
    public class CoroutineRunner : MonoBehaviour
    {
        private IEnumerator coroutine;
        private CoroutineRunner self;

        public static Coroutine RunGet(GameObject owner, IEnumerator coroutine)
        {
            CoroutineRunner runner = owner.AddComponent<CoroutineRunner>();
            runner.coroutine = coroutine;
            runner.self = runner;

            return runner.StartCoroutine(runner.RunCoroutine());
        }

        public static CoroutineRunner Run(GameObject owner, IEnumerator coroutine)
        {
            CoroutineRunner runner = owner.AddComponent<CoroutineRunner>();
            runner.coroutine = coroutine;
            runner.self = runner;

            runner.StartCoroutine(runner.RunCoroutine());
            return runner;
        }

        public void Stop()
        {
            StopAllCoroutines();
            Destroy(self);
        }

        public IEnumerator RunCoroutine()
        {
            yield return coroutine;
            Destroy(self);
        }
    }
}