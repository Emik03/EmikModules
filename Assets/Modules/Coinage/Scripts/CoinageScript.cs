using Coinage;
using EmikBaseModules;
using KModkit;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// On the Subject of Coinage - A modded "Keep Talking and Nobody Explodes" module created by Emik.
/// </summary>
public class CoinageScript : ModuleScript
{
    public override ModuleConfig ModuleConfig { get { return new ModuleConfig(kmBombModule: Module); } }

    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public KMSelectable[] Coins;
    public Transform[] CoinHighlightableTransforms;

    internal int[] CoinValues
    {
        get
        {
            return new[]
            {
                    CountCoins(2),
                    CountCoins(4),
                    CountCoins(8),
                    CountCoins(16),
                    CountCoins(32),
                    CountCoins(64)
                };
        }
    }

    internal bool IsCorrect
    {
        get
        {
            return CoinValues.Select((n, i) => n % 2 == 0 ^ hammingCodes[i]).All(b => b);
        }
    }

    internal string souvenirCoin;

    private bool[] coinStates, hammingCodes;
    private const int CoinCount = 64;
    private const string Chars = "56789NOPQRSTUVWXYZ";
    private Routine<int, bool> _rotateCoin;

    private void Start()
    {
        _rotateCoin = new Routine<int, bool>(RotateCoin, this);

        Coins.Assign(onInteract: i =>
        {
            _rotateCoin.StartIfStopped(i, true);
            return false;
        });

        coinStates = Helper.RandomBooleans(CoinCount);

        hammingCodes = Info.GetSerialNumber()
            .Select(c => Chars.Contains(c))
            .ToArray();

        // This flips each coin if needed.
        for (int i = 0; i < CoinCount; i++)
            ApplyRotation(i, 0, 0.6f);

        this.Log("The hamming code is {0}.".Form(hammingCodes.Select(b => b ? "1" : "0").Join("")));
        this.Log("The coins are arranged as {0}, making one of the answers {1}. (chess-coordinates)".Form(CoinValues.Select(n => n % 2 == 1 ? "1" : "0").Join(""), ToCoordinate(GetExampleAnswer())));
    }

    private int CountCoins(int i)
    {
        return coinStates.Where((b, n) => b && (n % i) >= i / 2).Count();
    }

    private void ApplyRotation(int i, float angle, float distance)
    {
        Coins[i].transform.localPosition = new Vector3(0, distance, 0);

        // We need a ternary operation here so that the coin goes into the correct side by the end.
        Coins[i].transform.localRotation = Quaternion.Euler(coinStates[i] ? angle + 90 : angle + 270, 0, 0);

        // We need to rotate the highlight backwards to cancel out the rotation of the coin.
        // This is because upside-down KMHighlights cannot be displayed correctly.
        CoinHighlightableTransforms[i].localRotation = Quaternion.Euler(coinStates[i] ? angle + 180 : angle, 0, 0);
    }

    internal int GetExampleAnswer()
    {
        int answer = 0;

        for (int i = 0; i < 6; i++)
            if (CoinValues[i] % 2 == 1 ^ hammingCodes[i])
                answer += (int)Mathf.Pow(2, i);

        return answer;
    }

    private IEnumerator RotateCoin(int arg, bool playSound)
    {
        if (playSound)
            Audio.Play(Coins[arg].transform, customSound: Sounds.Flip);

        float f = 0;

        // Makes highlight invisible.
        CoinHighlightableTransforms[arg].localScale = new Vector3(0, 0, 0);

        while (f <= 1)
        {
            float angle = Easing.OutQuart(f, 0, 1, 1) * 180,
                distance = (Mathf.Sin(f * Mathf.PI) * Mathf.PI) + 0.55f;

            ApplyRotation(arg, angle, distance);

            f += 1 / 64f;

            yield return new WaitForSecondsRealtime(0.01f);
        }

        // Makes highlight visible again.
        CoinHighlightableTransforms[arg].localScale = new Vector3(1.1f, 1.1f, 0.1f);

        // Since we flip a coin, we naturally need to flip the boolean.
        coinStates[arg] = !coinStates[arg];

        if (IsSolve)
            yield break;

        if (IsCorrect)
        {
            Audio.Play(Coins[arg].transform, customSound: Sounds.Solve);

            this.Solve("The correct coin was flipped. Module solved!");

            souvenirCoin = ToCoordinate(arg);

            for (int i = 0; i < 64; i++)
            {
                int[] coinFlips = Enumerable.Range(0, 64).Where(j => (j % 8) + (j / 8) == i).ToArray();
                foreach (var flip in coinFlips)
                { 
                    StartCoroutine(RotateCoin(flip, false));
                    yield return new WaitForSecondsRealtime(1 / 32f);
                }
            }
        }

        else
        {
            Audio.Play(Coins[arg].transform, customSound: Sounds.Strike);
            this.Strike("Coin {0} was flipped, making the arrangement {1}, strike!".Form(ToCoordinate(arg), CoinValues.Select(n => n % 2 == 1 ? "1" : "0").Join("")),
                "One of the answers is now {1}. (chess-coordinates)".Form(CoinValues.Select(n => n % 2 == 1 ? "1" : "0").Join(""), ToCoordinate(GetExampleAnswer())));
        }
    }

    private string ToCoordinate(int i)
    {
        return "abcdefgh"[i % 8].ToString() + "87654321"[i / 8 % 8];
    }
}
