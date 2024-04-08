// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections;
using KeepCoding;
using UnityEngine;
using Object = UnityEngine.Object;

public class TemplateScript : ModuleScript
{
    [SerializeField]
    TextMesh _textMesh;

    public IEnumerator Start()
    {
        Debug.unityLogger.LogFormat(LogType.Log, this, "foobar {0}", "hi");
    	Log(Wawa.Modules.Stringifier.Stringify((Enum)System.Reflection.BindingFlags.IgnoreCase));

        //Debug.Log(_textMesh.text = "Example Example Text");

        yield return null;
    }

    Func<bool> A(object o)
    {
        var del = (Func<bool>)Delegate.CreateDelegate(
            typeof(Func<bool>),
            ((KMBombModule)o).OnPass.Target,
            ((KMBombModule)o).OnPass.Method
        );

        Log(((KMBombModule)o).OnPass.Target);
        Log(((KMBombModule)o).OnPass.Method);
        Log(del);
        Log(del());

        return del;
    }
}

namespace Bad
{
    public sealed class CoroutineBehaviour : MonoBehaviour { }

    [Serializable]
    public sealed class VineThudException : Exception
    {
        readonly Application.LogCallback _callback;

        public VineThudException(string message = "BOOM", Action onThrow = null)
            : base(message)
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

        static Action Default()
        {
            return () =>
            {
                var go = new GameObject("VineThudPlayer", typeof(AudioSource), typeof(CoroutineBehaviour));
                go.GetComponent<CoroutineBehaviour>().StartCoroutine(PlaySound(go));
            };
        }

        static Application.LogCallback HandleLog(Action onThrow)
        {
            return (exception, stackTrace, logType) =>
            {
                if (logType == LogType.Exception &&
                    exception.StartsWith(typeof(VineThudException).Name))
                    onThrow();
            };
        }

        static IEnumerator PlaySound(GameObject go)
        {
            var audio = go.GetComponent<AudioSource>();

            audio.clip = Resources.Load<AudioClip>("VineThud");
            audio.Play();

            yield return new WaitWhile(() => audio.isPlaying);

            Object.Destroy(go);
        }
    }
}
