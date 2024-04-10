// SPDX-License-Identifier: MPL-2.0
namespace Canvas;

static class CanvasApi
{
    const int Attempts = 3, Ok = 200;

    const float TimeoutInSeconds = 1;

    static readonly TimeSpan s_getDelay = TimeSpan.FromSeconds(5), s_postDelay = TimeSpan.FromSeconds(300);

#pragma warning disable CA1802, IDE0044, RCS1187 // ReSharper disable once ConvertToConstant.Local FieldCanBeMadeReadOnly.Local
    // Do not turn this into a constant:
    // If migration ever happens but the mod can't update the endpoint, reflection can be used as a last resort.
    // As a constant it would still be possible to change, but this would require Harmony, which is far more invasive.
    static string s_endpoint = "https://canvas.emik.dev/v1";
#pragma warning restore CA1802, IDE0044, RCS1187

    static DateTime s_lastGetRequest, s_lastPostRequest;

    public static event Action<Sprite> OnBoardReceived = Noop;

    public static IEnumerator Get(Action<string> logger, Action onPanic)
    {
        if (DateTime.Now is var now && now - s_lastGetRequest < s_getDelay)
            yield break;

        s_lastGetRequest = now;
        using var web = UnityWebRequest.Get($"{s_endpoint}/board");
        logger($"Sending GET request to: {web.url}");

        for (var i = 0; i < Attempts; i++)
        {
            yield return web.SendWebRequest();

            if (web.responseCode is Ok)
            {
                logger("GET request successful.");
                OnBoardReceived(ToSprite(web.downloadHandler.text));
                yield break;
            }

            logger($"Response code {web.responseCode} due to: {web.downloadHandler.text}");
            yield return new WaitForSeconds(TimeoutInSeconds);
        }

        onPanic();
    }

    public static IEnumerator Post(Pixels pixel, int index, Action<string> logger, Action onPanic)
    {
        if (DateTime.Now is var now && now - s_lastPostRequest < s_postDelay)
            yield break;

        s_lastPostRequest = now;
        var json = JsonConvert.SerializeObject(new { pixel, index });
        using UploadHandlerRaw upload = new(Encoding.UTF8.GetBytes(json));
        using DownloadHandlerBuffer download = new();
        using UnityWebRequest web = new($"{s_endpoint}/submit", UnityWebRequest.kHttpVerbPOST, download, upload);
        web.SetRequestHeader("Content-Type", "application/json");
        logger($"Sending POST request to: {web.url}");

        for (var i = 0; i < Attempts; i++)
        {
            yield return web.SendWebRequest();

            if (web.responseCode is Ok)
            {
                logger("POST request successful.");
                yield break;
            }

            logger($"Response code {web.responseCode} due to: {web.downloadHandler.text}");
            yield return new WaitForSeconds(TimeoutInSeconds);
        }

        onPanic();
    }

    static Sprite ToSprite(string board)
    {
        var length = (int)Math.Sqrt(board.Length);
        Texture2D texture = new(length, length);

        for (var y = 0; y < length; y++)
            for (var x = 0; x < length; x++)
                texture.SetPixel(x, y, board[y * length + x].ToPixel().ToColor());

        texture.Apply();
        var sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f));
        sprite.texture.filterMode = FilterMode.Point;
        return sprite;
    }
}
