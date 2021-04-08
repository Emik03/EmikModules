using UnityEngine;

/// <summary>
/// Contains all sounds for every module.
/// </summary>
internal static class Sounds 
{
    /// <summary>
    /// 1D Chess
    /// </summary>
    internal static class _1dch
    {
        internal const string 
            Capture = "1DChessCapture",
            Check = "1DChessCheck",
            Click = "1DChessClick",
            GameEnd = "1DChessGameEnd",
            GameStart = "1DChessGameStart",
            Illegal = "1DChessIllegal",
            Opponent = "1DChessOpponent",
            Self = "1DChessSelf",
            Solve = "1DChessSolve",
            Strike = "1DChessStrike";
    }

    /// <summary>
    /// Coinage
    /// </summary>
    internal static class Coin
    {
        internal static string Flip { get { return "Coinage".Add(Random.Range(1, 4)); } }

        internal const string 
            Solve = "CoinageSolve",
            Strike = "CoinageStrike";
    }

    /// <summary>
    /// Etterna
    /// </summary>
    internal static class Ett
    {
        internal const string
            Clap = "EtternaClap",
            High = "EtternaHigh",
            Low = "EtternaLow",
            Music = "EtternaMusic",
            Solve = "EtternaSolve",
            Start = "EtternaStart",
            Strike = "EtternaStrike";
    }

    /// <summary>
    /// Forget Any Color
    /// </summary>
    internal static class Fac
    {
        internal const string
            FinalStage = "ForgetAnyColorFinalStage",
            Strike = "ForgetAnyColorStrike";

        internal static string Next(int i)
        {
            return "ForgetAnyColorNext".Add(i);
        }
        
        internal static string Stage(int i)
        {
            return "ForgetAnyColorStage".Add(i);
        }
    }

    /// <summary>
    /// Forget The Colors
    /// </summary>
    internal static class Ftc
    {
        internal const string
            Key = "ForgetTheColorsKey",
            KeySuccess = "ForgetTheColorsKeySuccess",
            NextStage = "ForgetTheColorsNextStage",
            Solve = "ForgetTheColorsSolved",
            Start = "ForgetTheColorsStart";
    }

    /// <summary>
    /// hexOS
    /// </summary>
    internal static class Hex
    {
        internal const string 
            Cancel = "hexOScancel",
            Clap = "hexOSclap",
            Click = "hexOSclick",
            Crash = "hexOScrash",
            HiHat = "hexOShihat",
            OctActivate = "hexOSoctActivate",
            OctSolve = "hexOSoctSolve",
            OctStrike = "hexOSoctStrike",
            OctStrikeFast = "hexOSoctStrikeFast",
            Ready = "hexOSready",
            Solve = "hexOSsolve",
            SolveAlt = "hexOSsolveAlt",
            Strike = "hexOSstrike",
            Submit = "hexOSsubmit";

        internal static string Chord(int i)
        {
            return "hexOSchord".Add(i);
        }
    }

    /// <summary>
    /// Linq
    /// </summary>
    internal static class Linq
    {
        internal static string Id(int i)
        {
            return "Linq".Add(i);
        }
    }

    /// <summary>
    /// Logging
    /// </summary>
    internal static class Log
    {
        internal const string Solve = "Logging";
    }

    /// <summary>
    /// Lying Indicators
    /// </summary>
    internal static class Lid
    {
        internal const string
            Solve = "LyingIndicatorsSolve",
            SolveBass = "LyingIndicatorsSolveBass";

        internal static string Press { get { return "LyingIndicatorsPress".Add(Random.Range(1, 3)); } }
    }

    /// <summary>
    /// Naming Conventions
    /// </summary>
    internal static class Nc
    {
        internal const string 
            Solve = "NamingConventionsSolve",
            Strike = "NamingConventionsStrike",
            Tick = "NamingConventionsTick",
            Touch = "NamingConventionsTouch";
    }

    /// <summary>
    /// Netherite
    /// </summary>
    internal static class N
    {
        internal static string Dig { get { return "NetheriteDig".Add(Random.Range(1, 6)); } }
        internal static string Hit { get { return "NetheriteHit".Add(Random.Range(1, 6)); } }

        internal const string 
            Ping = "NetheritePing",
            Solve = "NetheriteSolve";
    }

    /// <summary>
    /// The Octadecayotton
    /// </summary>
    internal static class Oct
    {
        internal const string
            Interact = "TheOctadecayottonInteract",
            InteractInterrupt = "TheOctadecayottonInteractInterrupt",
            Solve = "TheOctadecayottonSolve",
            SolveHard = "TheOctadecayottonSolveHard",
            StartingSphere = "TheOctadecayottonStartingSphere",
            Startup = "TheOctadecayottonStartup",
            StartupHard = "TheOctadecayottonStartupHard",
            Strike = "TheOctadecayottonStrike";
    }

    /// <summary>
    /// Palindromes
    /// </summary>
    internal static class Pld
    {
        internal const string
            Answer = "PalindromesAnswer",
            Calculate = "PalindromesCalculate",
            Cycle = "PalindromesCycle",
            Delete = "PalindromesDelete",
            Submit = "PalindromesSubmit";
    }

    /// <summary>
    /// Phosphorescence
    /// </summary>
    internal static class Pho
    {
        internal static class Voice
        {
            internal const string 
                BombDisarmed = "Phosphorescence_voice_bombdisarmed",
                ChallengeComplete = "Phosphorescence_voice_challengecomplete",
                GameOver = "Phosphorescence_voice_gameover",
                Go = "Phosphorescence_voice_go",
                One = "Phosphorescence_voice_one",
                Two = "Phosphorescence_voice_two",
                Three = "Phosphorescence_voice_three",
                Four = "Phosphorescence_voice_four",
                Five = "Phosphorescence_voice_five",
                Six = "Phosphorescence_voice_six",
                Seven = "Phosphorescence_voice_seven",
                Eight = "Phosphorescence_voice_eight",
                Nine = "Phosphorescence_voice_nine",
                Ten = "Phosphorescence_voice_ten",
                Thirty = "Phosphorescence_voice_thirtyseconds",
                OneMinute = "Phosphorescence_voice_oneminute",
                TwoMinutes = "Phosphorescence_voice_twominutes",
                ThreeMinutes = "Phosphorescence_voice_threeminutes",
                FourMinutes = "Phosphorescence_voice_fourminutes";
        }

        internal const string
            EndSubmit = "PhosphorescenceEndSubmit",
            InvalidButton = "PhosphorescenceInvalidButton",
            MarkerOff = "PhosphorescenceMarkerOff",
            MarkerOn = "PhosphorescenceMarkerOn",
            NotableTimeLeft = "PhosphorescenceNotableTimeLeft",
            Reshuffle = "PhosphorescenceReshuffle",
            ScreenPress = "PhosphorescenceScreenPress",
            ScreenRelease = "PhosphorescenceScreenRelease",
            ShuffleButtons = "PhosphorescenceShuffleButtons",
            Solve = "PhosphorescenceSolve",
            Start = "PhosphorescenceStart",
            StartSubmit = "PhosphorescenceStartSubmit",
            Strike = "PhosphorescenceStrike",
            Success = "PhosphorescenceSuccess",
            TimerTick = "PhosphorescenceTimerTick";

        internal static string Submit(int i)
        {
            return "PhosphorescenceSubmit".Add(i);
        }
    }

    /// <summary>
    /// Placeholder Talk
    /// </summary>
    internal static class Pht
    {
        internal const string 
            Solve = "PlaceholderTalkDisarm",
            Shuffle = "PlaceholderTalkShuffle",
            Strike = "PlaceholderTalkStrike";
    }

    /// <summary>
    /// Quaver
    /// </summary>
    internal static class Q
    {
        internal const string Higher = "QuaverHigher",
            Lower = "QuaverLower",
            Select = "QuaverSelect",
            Solve = "QuaverSolve",
            Start = "QuaverStart",
            Strike = "QuaverStrike";

        internal static string Note(string i)
        {
            return "QuaverNote".Add(i);
        }

        internal static string Submit(bool b)
        {
            return "QuaverSubmit".Add(b);
        }
    }

    /// <summary>
    /// Reformed Role Reversal
    /// </summary>
    internal static class Rrr
    {
        internal const string
            Strike = "ReformedRoleReversalStrike",
            Solve = "ReformedRoleReversalSolve";

        internal static string Button(int i)
        {
            return "ReformedRoleReversalButton".Add(i);
        }
    }

    /// <summary>
    /// Role Reversal
    /// </summary>
    internal static class Rv
    {
        internal const string 
            Solve = "RoleReversalRopesSolved",
            Strike = "RoleReversalRopesStrike";
    }

    /// <summary>
    /// Scale
    /// </summary>
    internal static class Sc
    {

    }

    /// <summary>
    /// Sorting
    /// </summary>
    internal static class Srt
    {
        internal const string
            Bogosort = "SortingBogosort",
            Deselect = "SortingDeselect",
            Solve = "SortingModulePass",
            Strike = "SortingModuleStrike",
            Select = "SortingSelect",
            Swap = "SortingSwap",
            SuccessfulSwap = "SortingSwapSuccess",
            Tick = "SortingTick";

        internal static string Button(int i)
        {
            return "SortingButton".Add(i);
        }
    }

    /// <summary>
    /// Updog
    /// </summary>
    internal static class Ud
    {
        internal const string Solve = "Updog";
    }

    private static string Add(this string s, object i)
    {
        return s + i;
    }
}
