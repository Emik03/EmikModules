// SPDX-License-Identifier: MPL-2.0
namespace Canvas;

enum Pixels : byte
{
    White,
    LightGray,
    DarkGray,
    Black,
    Pink,
    Red,
    Orange,
    Brown,
    Yellow,
    Lime,
    Green,
    Cyan,
    Teal,
    Blue,
    Magenta,
    Purple,
}

#pragma warning disable MA0048
static class PixelExtensions
#pragma warning restore MA0048
{
    public static char AsChar(this Pixels p) => (char)((byte)p + '0');

    public static Pixels ToPixel(this char c) => (Pixels)(c - '0');

    public static Color ToColor(this Pixels pixel) =>
        pixel switch
        {
            Pixels.White => Color.white,
            Pixels.LightGray => From(0xd4d7d9),
            Pixels.DarkGray => From(0x898d90),
            Pixels.Black => Color.black,
            Pixels.Pink => From(0xff99aa),
            Pixels.Red => From(0xff4500),
            Pixels.Orange => From(0xffa800),
            Pixels.Brown => From(0x9c6926),
            Pixels.Yellow => From(0xffd623),
            Pixels.Lime => From(0x7eed56),
            Pixels.Green => From(0x00a368),
            Pixels.Cyan => From(0x51e9f4),
            Pixels.Teal => From(0x365aea),
            Pixels.Blue => From(0x2432a4),
            Pixels.Magenta => From(0xff99aa),
            Pixels.Purple => From(0xb44ac0),
            _ => throw new ArgumentOutOfRangeException(nameof(pixel), pixel, null),
        };

    static Color32 From(uint rgb) =>
        new((byte)(rgb >> 16 & 255), (byte)(rgb >> 8 & 255), (byte)(rgb & 255), byte.MaxValue);
}
