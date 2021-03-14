using System.Linq;
using EmikBaseModules;
using System.Text;
using System.Collections.Generic;
using System;

namespace OneDimensionalChess
{
    internal static class Position
    {
        internal static readonly CGameResult finishedGame = new CGameResult { Piece = new Piece { Type = PieceType.King, Color = PieceColor.White }, Origin = -1, Destination = -1 };

        internal const int Depth = 18;
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
                do temp = random.Next(0, str.Length / 2);
                while (used.Contains(temp));
                
                used.Add(temp);
                str[temp] = pieceChars[random.Next(0, pieceChars.Length)].ToUpper();

                // Place a random black chess piece on an unoccupied square in the right half of the string.
                do temp = random.Next(str.Length / 2, str.Length - 1);
                while (used.Contains(temp));

                used.Add(temp);
                str[temp] = pieceChars[random.Next(0, pieceChars.Length)].ToLower();
            }

            return str.ToString();
        }

        internal static string Move(this CGameResult move, string current, KMAudio audio = null)
        {
            var str = new StringBuilder(current);

            // If the destination is occupied, this implies a capture.
            if (str[move.Destination] != '_' && audio != null)
                audio.Play(audio.transform, Sounds.Capture);

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

        internal static string Symbol(this Piece piece)
        {
            char c;

            switch (piece.Type)
            {
                case PieceType.Bishop:
                    c = 'b';
                    break;
                case PieceType.King:
                    c = 'k';
                    break;
                case PieceType.Knight:
                    c = 'n';
                    break;
                case PieceType.Pawn:
                    c = 'p';
                    break;
                case PieceType.Rook:
                    c = 'r';
                    break;
                case PieceType.Queen:
                    c = 'q';
                    break;
                default:
                    throw new NotImplementedException("char: " + piece.Type);
            }

            return piece.Color == PieceColor.White ? c.ToUpper().ToString() : c.ToLower().ToString();
        }
    }
}
