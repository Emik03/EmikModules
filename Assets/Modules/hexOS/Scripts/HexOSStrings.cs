using System.Collections.Generic;
using UnityEngine;
using PathManager = KeepCodingAndNobodyExplodes.PathManager;

namespace HexOSModule
{
    internal class HexOSStrings
    {
        internal static readonly string Version = Application.isEditor ? "#.#.#" : PathManager.GetModInfo("EmikModules").Version;

        #region Color32
        public static readonly Color32[] PerfectColors = new Color32[3]
        {
            Color.black,
            Color.white,
            Color.magenta
        };

        public static readonly Color32[] TransparentColors = new Color32[4]
        {
            new Color32(255, 0, 0, 128),
            new Color32(0, 255, 0, 128),
            new Color32(0, 0, 255, 128),
            new Color32(255, 255, 255, 128)
        };
        #endregion

        #region Alphabet
        public static readonly sbyte[] Symbols = new sbyte[12]
        {
            1, // C
            -1, // C#
            2, // D
            -2, // D#
            3, // E
            4, // F
            -4, // F#
            5, // G
            -5, // G#
            6, // A
            -6, // A#
            7 // B
        };

        public static readonly char[] Alphabet = new char[27]
        {
            ' ',
            'i',
            'r',
            'c',
            'l',
            'u',
            'f',
            'o',
            'x',
            'a',
            'j',
            's',
            'd',
            'm',
            'v',
            'g',
            'p',
            'y',
            'b',
            'k',
            't',
            'e',
            'n',
            'w',
            'h',
            'q',
            'z'
        };

        public static readonly Dictionary<byte, char> PerfectCipher = new Dictionary<byte, char>(27)
        {
            { 0, ' ' },
            { 1, 'A' },
            { 2, 'B' },
            { 3, 'C' },
            { 4, 'D' },
            { 5, 'E' },
            { 6, 'F' },
            { 7, 'G' },
            { 8, 'H' },
            { 9, 'I' },
            { 10, 'J' },
            { 11, 'K' },
            { 12, 'L' },
            { 13, 'M' },
            { 14, 'N' },
            { 15, 'O' },
            { 16, 'P' },
            { 17, 'Q' },
            { 18, 'R' },
            { 19, 'S' },
            { 20, 'T' },
            { 21, 'U' },
            { 22, 'V' },
            { 23, 'W' },
            { 24, 'X' },
            { 25, 'Y' },
            { 26, 'Z' }
        };
        #endregion

        #region String Arrays
        internal static readonly string[] Notes = new string[16]
        {
            "X-XXX-X-X-X-X---X---X-X-X---X---X-X-X",
            "XXX-X-X-X-X-X---X---X-X-X-X---X---X-X",
            "X-X-XXX-X-X-X---X---X-X-X-X---X-X-X-X",
            "XXXXX-X-X-X---X-X---X-X-X-X---X-X-X-X",
            "X-XXX-X-X-X-X---X---X-X-X---X--XX-X-X",
            "XXX-X-X-X-X-X---X-XXX-X-X--XX-X---X-X",
            "X-X-XXX-X-X-X---X--XX-X-X--X-X-XX-X-X",
            "XXXXX-X-X-X---X-X--XX-X-X--X--X--XX-X",
            "X-XXX-X-X-X-X---X-XXX-X-X---X-XXX-X-X",
            "XXX-X-X-X-X-X--XX-X-X-X-X-XXX-X---X-X",
            "X-X-XXX-X-X-X---X-XXX-X-X-XXX-X-X-X-X",
            "XXXXX-X-X-X---X-X--XX-X-X-X---X-X-XXX",
            "X-XXX-X-X-X-X---X-XXX-XXX-X-X--XX-X-X",
            "XXX-X-X-X-X-X--XX-XXX-X-X--XX-X---X-X",
            "X-X-XXX-X-X-X---X--XX-X-X--XXX-XX-X-X",
            "XXXXX-X-X-X---X-X--XX-X-X-XXXXX-X-X-X"
        };

        internal static readonly string[] OctNotes = new string[16]
        {
            "X---------X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X",
            "X-----------X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-XXXXX",
            "X-----------X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X-X",
            "X---X-X-X-X-X-X---X-X-X-X---X-X---X-X---X---XXXXX",
            "X-----------X-X-X-XXX-X-X-X-X-X-X-XXX-X-X-X-X-X-X",
            "X-----------X-XXX-X-X-X-X-X-X-XXX-X-X-X-XXXXX-X-X",
            "X-----------XXXXX-X-X-X-X-XXX-X-XXXXX-X-X-XXX-X-X",
            "X---X-X-X-X-X-X---XXXXX-X---X-X---X-X---X---X-X-X",
            "X-------X---XXXXX-X-X-X-X-XXX-X-X-XXX-X-XXXXX-X-X",
            "X---X-X-X-X-XXXXX-XXX-X-X-X-XXXXX-XXX-X-X-X-XXXXX",
            "X---X-X-X-X-XXXXX-X-X-X-X-XXX-X-XXXXX-X-X-XXX-X-X",
            "X---XXX-X-X-X-X---X-X-X-X--XX-X--XX-X---X-X-XXXXX",
            "X---XXX---X-X-XXX-X-XXX-X-XXX-X-XXX-XXXXX-X-XXX-X",
            "X-----X-----X-XXX-XXXXX-X-XXX-XXXXX-X-XXXXX-XXX-X",
            "X---X---X---X-X-XXX-X-XXX-X-XX-XX-XXX-XXX-XXXXXXX",
            "X---XXX-X-X-X-X---XXX-X-X--XX-X--XX-X---X-X-XXXXX"
        };

        internal static readonly string[] OctPhrases = new string[24]
        {
            "a maze with edges like their knives",
            "someday ill be the shape they want me to be",
            "but i dont know how much more theyll wake away before theyre satisfied",
            "they have sliced away my flesh",
            "shorn of unsightly limbs and organs",
            "more stitch and scar than human",
            "if only marble",
            "grew back so quickly",
            "they have stolen away my spirit",
            "memories scattered into the slipstream",
            "i have no idea who i used to be",
            "i can only guess",
            "what they will make me",
            "they found me in my lowest days",
            "breathed life back into my frozen body",
            "promising a more beautiful future",
            "then i discovered",
            "what they really wanted",
            "they pulled me into their vortex",
            "and i saw my future reflected in their eyes",
            "a shimmering halo of impossible dreams",
            "void of my self",
            "it was",
            "perfect"
        };

        internal static readonly string[] SolvePhrases =
        {
            "\"You solved this... manually?\nI... how...?\"",
            "\"Maybe solving this is a\nlot easier when your\nhands aren't slowed\ndown by air friction...\"",
            "\"Technology was a mistake.\nI'm sticking with magic.\"",
            "\"Who the hell is making\nthese modules anyway?\"",
            "\"Is this why I scare\npeople off? For indirectly\ncreating something\nlike this?\"",
            "\"I'm glad you made\nit through that\nwith neither you, nor\nthe fragments getting\nturned into scraps...\"",
            "\"I thought toys were\nsupposed to be safe...\"",
            "\"I'm going back to my\nrhythm games, at least there,\nI don't have to think...\"",
            "\"...I am going to throw\nmyself into the sea.\nI will float there until\nI am ready to go home.\"",
            "\"Now I remember why we\ndon't combine technology\nwith magic...\"",
        };

        internal static readonly string[] AltSolvePhrases =
        {
            "YOU FOUND IT!",
            "Dungeon: hexOS Vaults\nSolver\ncleared the dungeon.",
            "Break!!!",
            "Complete!!!",
            "oh yeah woo yeah",
            "NOOO I DON'T WANNA\nBE A MODULE\nbeing a module is\nfine actually,\ni don't care anymore",
            "construct music",
            "IN A DESPERATE CONFLICT\nWITH A RUTHLESS ENEMY",
            "the fuck is a rotom",
            "hexyl you focusing\npiece of shit.\ni hope you have\na nice life and\nget married",
        };
        #endregion
    }
}