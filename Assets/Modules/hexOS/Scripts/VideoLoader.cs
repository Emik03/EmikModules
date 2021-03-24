using HexOSModule;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class VideoLoader : MonoBehaviour
{
    public static VideoClip[] clips;
    private static bool _loaded;

    public void Awake()
    {
        DestroyImmediate(GetComponent<KMService>());
    }

    public void Start()
    {
        StartCoroutine(LoadVideoClips());
    }

    private IEnumerator LoadVideoClips()
    {
        if (_loaded)
            yield break;

        var paths = ReflectionHelper.FindType("ModManager")
            .GetValue<object>("Instance")
            .GetValue<IDictionary>("loadedMods")
            .CastDict()
            .ToDictionary(entry => entry.Key, entry => entry.Value)
            .Values
            .FirstOrDefault(mod => mod.GetValue<string>("ModID") == "EmikModules")
            .CallMethod<List<string>>("GetAssetBundlePaths");

        if (paths == null)
        {
            yield break;
        }

        foreach (string fileName in paths)
        {
            if (Path.GetFileName(fileName) != "hex.bundle")
                continue;

            var request = AssetBundle.LoadFromFileAsync(fileName);
            yield return request;
            AssetBundle mainBundle = request.assetBundle;
            if (mainBundle != null)
            {
                clips = mainBundle.LoadAllAssets<VideoClip>().OrderBy(clip => clip.name).ToArray();
                _loaded = true;
            }
        }
    }
}