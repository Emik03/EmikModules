using KeepCoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneDimensionalChess
{
    internal static class Position
    {
        internal static readonly CGameResult finishedGame = new CGameResult { Piece = new Piece { Type = PieceType.King, Color = PieceColor.White }, Origin = -1, Destination = -1 };

        internal const int Depth = 16;
        internal const string PieceChars = "_bknpqrBKNPQR";
        internal static Random random = new Random();

        internal static string Generate(int length, int pieceCount)
        {
            // Creates as many underscores as the specified length.
            var str = new StringBuilder(Enumerable.Repeat('_', length).Join(""));

            var used = new List<int>();
            int temp;

            // The white king is located in the first square.
            temp = 0;
            str[temp] = 'K';
            used.Add(temp);

            // The black king is located in the last square.
            temp = str.Length - 1;
            str[temp] = 'k';
            used.Add(temp);

            const string pieceChars = "bnpqr";

            for (int i = 0; i < pieceCount; i++)
            {
                // Place a random white chess piece on an unoccupied square in the left half of the string.
                do 
                    temp = random.Next(0, str.Length / 2);
                while (used.Contains(temp));

                used.Add(temp);
                str[temp] = pieceChars[random.Next(0, pieceChars.Length)].ToUpper();

                // Place a random black chess piece on an unoccupied square in the right half of the string.
                do 
                    temp = random.Next(str.Length / 2, str.Length - 1);
                while (used.Contains(temp));

                used.Add(temp);
                str[temp] = pieceChars[random.Next(0, pieceChars.Length)].ToLower();
            }

            return str.ToString();
        }

        internal static string Move(this CGameResult move, string current, ModuleScript moduleScript = null)
        {
            var str = new StringBuilder(current);

            // If the destination is occupied, this implies a capture.
            if (str[move.Destination] != '_' && moduleScript != null)
                moduleScript.PlaySound(SFX._1dch.Capture);

            // Moves origin to destination, leaving origin empty.
            str[move.Destination] = str[move.Origin];
            str[move.Origin] = '_';

            return str.ToString();
        }

        internal static bool IsStalemate(this string state, PieceColor color)
        {
            // Plays out the game 1 move deep, to find at least one move.
            var game = Engine.Calculate(state, 1, color == PieceColor.White);

            // It is stalemate if it cannot make a move, and the evaluation is a draw.
            return game.IsEqual(finishedGame) && game.Evaluation == 0;
        }

        internal static bool IsGameEnd(this string state, PieceColor color)
        {
            // Plays out the game 1 move deep, to find at least one move.
            var game = Engine.Calculate(state, 1, color == PieceColor.White);

            // The game has ended if it cannot make a move.
            return game.IsEqual(finishedGame);
        }

        internal static PieceColor Flip(this PieceColor color)
        {
            return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        }

        internal static bool IsEqual(this CGameResult move, CGameResult other)
        {
            return move.Origin == other.Origin && move.Destination == other.Destination;
        }

        internal static Piece Piece(this char c)
        {
            return new Piece
            {
                Color = c.GetPieceColor(),
                Type = c.GetPieceType()
            };
        }

        internal static PieceColor GetPieceColor(this char c)
        {
            return c == c.ToUpper() ? PieceColor.White : PieceColor.Black;
        }

        internal static PieceType GetPieceType(this char c)
        {
            switch (c.ToLower())
            {
                case 'b':
                    return PieceType.Bishop;
                case 'k':
                    return PieceType.King;
                case 'n':
                    return PieceType.Knight;
                case 'p':
                    return PieceType.Pawn;
                case 'r':
                    return PieceType.Rook;
                case 'q':
                    return PieceType.Queen;
                default:
                    throw new NotImplementedException("char: " + c);
            }
        }

        internal static char Symbol(this Piece piece)
        {
            switch (piece.Type)
            {
                // The knight is notated with N because the king starts with a K.
                case PieceType.Knight: return 'N';
                default: return piece.Type.ToString().First();
            }
        }
    }
}
