using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bad;
using KeepCoding;
using UnityEngine;
using Object = UnityEngine.Object;

public class TemplateScript : ModuleScript, IAwake, IStartCoroutine
{
    private readonly Dictionary<GameObject, object> _dictionary = new Dictionary<GameObject, object>();

    public IEnumerator Start()
    {
        yield return new WaitWhile(() => Modules == null);
    }
}

namespace Bad
{
    public sealed class CoroutineBehaviour : MonoBehaviour { }

    [Serializable]
    public sealed class VineThudException : Exception
    {
        private readonly Application.LogCallback _callback;

        public VineThudException(string message = "BOOM", Action onThrow = null) : base(message) 
        {
            if (onThrow == null)
                onThrow = Default();

            _callback = HandleLog(onThrow);

            Application.logMessageReceived += _callback;
        }

        ~VineThudException()
        {
            Application.logMessageReceived -= _callback;
        }

        private static Action Default()
        {
            return () =>
            {
                var go = new GameObject("VineThudPlayer", typeof(AudioSource), typeof(CoroutineBehaviour));
                go.GetComponent<CoroutineBehaviour>().StartCoroutine(PlaySound(go));
            };
        }

        private static Application.LogCallback HandleLog(Action onThrow)
        {
            return (exception, stackTrace, logType) =>
            {
                if (logType == LogType.Exception &&
                    exception.StartsWith(typeof(VineThudException).Name))
                    onThrow();
            };
        }

        private static IEnumerator PlaySound(GameObject go)
        {
            var audio = go.GetComponent<AudioSource>();

            audio.clip = Resources.Load<AudioClip>("VineThud");
            audio.Play();

            yield return new WaitWhile(() => audio.isPlaying);

            Object.Destroy(go);
        }
    }
}
