using System.Drawing;
using System.Globalization;
using SoulEngine.Mathematics;

namespace SoulEngine;

public struct Colour
{
	public float R, G, B, A = 0.0f;

	public Colour(float r, float g, float b, float a = 1f)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public Colour(Color system) : this(system.R / 255f, system.G / 255f, system.B / 255f, system.A / 255f)
	{

	}
	

	public readonly Colour WithAlpha(float alpha)
	{
		return new Colour(R, G, B, alpha);
	}

	public static  Colour operator *(Colour a, float b)
	{
		return new Colour(a.R * b, a.G * b, a.B * b, a.A * b);
	}

	public static Colour Lerp(Colour a, Colour b, float d)
	{
		return new Colour(
			Mathx.Lerp(a.R, b.R, d),
			Mathx.Lerp(a.G, b.G, d),
			Mathx.Lerp(a.B, b.B, d),
			Mathx.Lerp(a.A, b.A, d));
	}

	public static Colour LerpUnclamped(Colour a, Colour b, float d)
	{
		return new Colour(
			Mathx.LerpUnclamped(a.R, b.R, d),
			Mathx.LerpUnclamped(a.G, b.G, d),
			Mathx.LerpUnclamped(a.B, b.B, d),
			Mathx.LerpUnclamped(a.A, b.A, d));
	}

	public static  Colour FromRgba32(byte r, byte g, byte b, byte a)
	{
		return new Colour(r / (float)Byte.MaxValue, g / (float)Byte.MaxValue, b / (float)Byte.MaxValue,
			a / (float)Byte.MaxValue);
	}

	public static Colour FromRgba32(uint encoded)
	{
		byte r = (byte)(encoded & 0xFF);
		byte g = (byte)(encoded >> 8 & 0xFF);
		byte b = (byte)(encoded >> 16 & 0xFF);
		byte a = (byte)(encoded >> 24 & 0xFF);
		return FromRgba32(r, g, b, a);
	}

	public static Colour FromAbgr32(uint encoded)
	{
		byte a = (byte)(encoded & 0xFF);
		byte b = (byte)(encoded >> 8 & 0xFF);
		byte g = (byte)(encoded >> 16 & 0xFF);
		byte r = (byte)(encoded >> 24 & 0xFF);
		return FromRgba32(r, g, b, a);
	}

	public static Colour FromBgra32(uint encoded)
	{
		byte b = (byte)(encoded & 0xFF);
		byte g = (byte)(encoded >> 8 & 0xFF);
		byte r = (byte)(encoded >> 16 & 0xFF);
		byte a = (byte)(encoded >> 24 & 0xFF);
		return FromRgba32(r, g, b, a);
	}

	public static Colour operator *(Colour a, Colour b)
	{
		a.R *= b.R;
		a.G *= b.G;
		a.B *= b.B;
		a.A *= b.A;
		return a;
	}

	public static Colour FromRgba32(byte[] data)
	{
		return FromRgba32(data[0], data[1], data[2], data[3]);
	}

	public static Colour FromRgba32(byte[] data, int offset)
	{
		return FromRgba32(data[0 + offset], data[1 + offset], data[2], data[3 + offset]);
	}

	public readonly void ToRgba32(ref byte[] target, int offset = 0)
	{
		target[offset] = (byte)(R * Byte.MaxValue);
		target[offset + 1] = (byte)(G * Byte.MaxValue);
		target[offset + 2] = (byte)(B * Byte.MaxValue);
		target[offset + 3] = (byte)(A * Byte.MaxValue);
	}

	public readonly unsafe uint ToUint32()
	{
		uint targetInt = 0;
		byte* target = (byte*)&targetInt;
		target[0] = (byte)(R * Byte.MaxValue);
		target[1] = (byte)(G * Byte.MaxValue);
		target[2] = (byte)(B * Byte.MaxValue);
		target[3] = (byte)(A * Byte.MaxValue);
		return targetInt;
	}

	public readonly byte[] ToRgba32()
	{
		return new byte[4]
		{
			(byte)(R * Byte.MaxValue), (byte)(G * Byte.MaxValue), (byte)(B * Byte.MaxValue), (byte)(A * Byte.MaxValue)
		};
	}

	public static Colour FromHex(string hex)
	{
		if (hex.StartsWith("#"))
			hex = hex[1..];
		if (UInt32.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out uint parsed))
		{
			return FromRgba32(parsed);
		}
		else
		{
			return Blank;
		}

	}

	public readonly Colour Brighten(float amount)
	{
		return new Colour(R + amount, G + amount, B + amount, A);
	}
	
	// Colours, from System.Drawing
	
	public static readonly Colour Blank = new Colour(0, 0, 0, 0);
	public static readonly Colour Grey = new Colour(.5f, .5f, .5f);
	public static readonly Colour Transparent = new Colour(Color.Transparent);
	public static readonly Colour AliceBlue = new Colour(Color.AliceBlue);
	public static readonly Colour AntiqueWhite = new Colour(Color.AntiqueWhite);
	public static readonly Colour Aqua = new Colour(Color.Aqua);
	public static readonly Colour Aquamarine = new Colour(Color.Aquamarine);
	public static readonly Colour Azure = new Colour(Color.Azure);
	public static readonly Colour Beige = new Colour(Color.Beige);
	public static readonly Colour Bisque = new Colour(Color.Bisque);
	public static readonly Colour Black = new Colour(Color.Black);
	public static readonly Colour BlanchedAlmond = new Colour(Color.BlanchedAlmond);
	public static readonly Colour Blue = new Colour(Color.Blue);
	public static readonly Colour BlueViolet = new Colour(Color.BlueViolet);
	public static readonly Colour Brown = new Colour(Color.Brown);
	public static readonly Colour BurlyWood = new Colour(Color.BurlyWood);
	public static readonly Colour CadetBlue = new Colour(Color.CadetBlue);
	public static readonly Colour Chartreuse = new Colour(Color.Chartreuse);
	public static readonly Colour Chocolate = new Colour(Color.Chocolate);
	public static readonly Colour Coral = new Colour(Color.Coral);
	public static readonly Colour CornflowerBlue = new Colour(Color.CornflowerBlue);
	public static readonly Colour Cornsilk = new Colour(Color.Cornsilk);
	public static readonly Colour Crimson = new Colour(Color.Crimson);
	public static readonly Colour Cyan = new Colour(Color.Cyan);
	public static readonly Colour DarkBlue = new Colour(Color.DarkBlue);
	public static readonly Colour DarkCyan = new Colour(Color.DarkCyan);
	public static readonly Colour DarkGoldenrod = new Colour(Color.DarkGoldenrod);
	public static readonly Colour DarkGray = new Colour(Color.DarkGray);
	public static readonly Colour DarkGreen = new Colour(Color.DarkGreen);
	public static readonly Colour DarkKhaki = new Colour(Color.DarkKhaki);
	public static readonly Colour DarkMagenta = new Colour(Color.DarkMagenta);
	public static readonly Colour DarkOliveGreen = new Colour(Color.DarkOliveGreen);
	public static readonly Colour DarkOrange = new Colour(Color.DarkOrange);
	public static readonly Colour DarkOrchid = new Colour(Color.DarkOrchid);
	public static readonly Colour DarkRed = new Colour(Color.DarkRed);
	public static readonly Colour DarkSalmon = new Colour(Color.DarkSalmon);
	public static readonly Colour DarkSeaGreen = new Colour(Color.DarkSeaGreen);
	public static readonly Colour DarkSlateBlue = new Colour(Color.DarkSlateBlue);
	public static readonly Colour DarkSlateGray = new Colour(Color.DarkSlateGray);
	public static readonly Colour DarkTurquoise = new Colour(Color.DarkTurquoise);
	public static readonly Colour DarkViolet = new Colour(Color.DarkViolet);
	public static readonly Colour DeepPink = new Colour(Color.DeepPink);
	public static readonly Colour DeepSkyBlue = new Colour(Color.DeepSkyBlue);
	public static readonly Colour DimGray = new Colour(Color.DimGray);
	public static readonly Colour DodgerBlue = new Colour(Color.DodgerBlue);
	public static readonly Colour Firebrick = new Colour(Color.Firebrick);
	public static readonly Colour FloralWhite = new Colour(Color.FloralWhite);
	public static readonly Colour ForestGreen = new Colour(Color.ForestGreen);
	public static readonly Colour Fuchsia = new Colour(Color.Fuchsia);
	public static readonly Colour Gainsboro = new Colour(Color.Gainsboro);
	public static readonly Colour GhostWhite = new Colour(Color.GhostWhite);
	public static readonly Colour Gold = new Colour(Color.Gold);
	public static readonly Colour Goldenrod = new Colour(Color.Goldenrod);
	public static readonly Colour Gray = new Colour(Color.Gray);
	public static readonly Colour Green = new Colour(Color.Green);
	public static readonly Colour GreenYellow = new Colour(Color.GreenYellow);
	public static readonly Colour Honeydew = new Colour(Color.Honeydew);
	public static readonly Colour HotPink = new Colour(Color.HotPink);
	public static readonly Colour IndianRed = new Colour(Color.IndianRed);
	public static readonly Colour Indigo = new Colour(Color.Indigo);
	public static readonly Colour Ivory = new Colour(Color.Ivory);
	public static readonly Colour Khaki = new Colour(Color.Khaki);
	public static readonly Colour Lavender = new Colour(Color.Lavender);
	public static readonly Colour LavenderBlush = new Colour(Color.LavenderBlush);
	public static readonly Colour LawnGreen = new Colour(Color.LawnGreen);
	public static readonly Colour LemonChiffon = new Colour(Color.LemonChiffon);
	public static readonly Colour LightBlue = new Colour(Color.LightBlue);
	public static readonly Colour LightCoral = new Colour(Color.LightCoral);
	public static readonly Colour LightCyan = new Colour(Color.LightCyan);
	public static readonly Colour LightGoldenrodYellow = new Colour(Color.LightGoldenrodYellow);
	public static readonly Colour LightGreen = new Colour(Color.LightGreen);
	public static readonly Colour LightGray = new Colour(Color.LightGray);
	public static readonly Colour LightPink = new Colour(Color.LightPink);
	public static readonly Colour LightSalmon = new Colour(Color.LightSalmon);
	public static readonly Colour LightSeaGreen = new Colour(Color.LightSeaGreen);
	public static readonly Colour LightSkyBlue = new Colour(Color.LightSkyBlue);
	public static readonly Colour LightSlateGray = new Colour(Color.LightSlateGray);
	public static readonly Colour LightSteelBlue = new Colour(Color.LightSteelBlue);
	public static readonly Colour LightYellow = new Colour(Color.LightYellow);
	public static readonly Colour Lime = new Colour(Color.Lime);
	public static readonly Colour LimeGreen = new Colour(Color.LimeGreen);
	public static readonly Colour Linen = new Colour(Color.Linen);
	public static readonly Colour Magenta = new Colour(Color.Magenta);
	public static readonly Colour Maroon = new Colour(Color.Maroon);
	public static readonly Colour MediumAquamarine = new Colour(Color.MediumAquamarine);
	public static readonly Colour MediumBlue = new Colour(Color.MediumBlue);
	public static readonly Colour MediumOrchid = new Colour(Color.MediumOrchid);
	public static readonly Colour MediumPurple = new Colour(Color.MediumPurple);
	public static readonly Colour MediumSeaGreen = new Colour(Color.MediumSeaGreen);
	public static readonly Colour MediumSlateBlue = new Colour(Color.MediumSlateBlue);
	public static readonly Colour MediumSpringGreen = new Colour(Color.MediumSpringGreen);
	public static readonly Colour MediumTurquoise = new Colour(Color.MediumTurquoise);
	public static readonly Colour MediumVioletRed = new Colour(Color.MediumVioletRed);
	public static readonly Colour MidnightBlue = new Colour(Color.MidnightBlue);
	public static readonly Colour MintCream = new Colour(Color.MintCream);
	public static readonly Colour MistyRose = new Colour(Color.MistyRose);
	public static readonly Colour Moccasin = new Colour(Color.Moccasin);
	public static readonly Colour NavajoWhite = new Colour(Color.NavajoWhite);
	public static readonly Colour Navy = new Colour(Color.Navy);
	public static readonly Colour OldLace = new Colour(Color.OldLace);
	public static readonly Colour Olive = new Colour(Color.Olive);
	public static readonly Colour OliveDrab = new Colour(Color.OliveDrab);
	public static readonly Colour Orange = new Colour(Color.Orange);
	public static readonly Colour OrangeRed = new Colour(Color.OrangeRed);
	public static readonly Colour Orchid = new Colour(Color.Orchid);
	public static readonly Colour PaleGoldenrod = new Colour(Color.PaleGoldenrod);
	public static readonly Colour PaleGreen = new Colour(Color.PaleGreen);
	public static readonly Colour PaleTurquoise = new Colour(Color.PaleTurquoise);
	public static readonly Colour PaleVioletRed = new Colour(Color.PaleVioletRed);
	public static readonly Colour PapayaWhip = new Colour(Color.PapayaWhip);
	public static readonly Colour PeachPuff = new Colour(Color.PeachPuff);
	public static readonly Colour Peru = new Colour(Color.Peru);
	public static readonly Colour Pink = new Colour(Color.Pink);
	public static readonly Colour Plum = new Colour(Color.Plum);
	public static readonly Colour PowderBlue = new Colour(Color.PowderBlue);
	public static readonly Colour Purple = new Colour(Color.Purple);
	public static readonly Colour RebeccaPurple = new Colour(Color.RebeccaPurple);
	public static readonly Colour Red = new Colour(Color.Red);
	public static readonly Colour RosyBrown = new Colour(Color.RosyBrown);
	public static readonly Colour RoyalBlue = new Colour(Color.RoyalBlue);
	public static readonly Colour SaddleBrown = new Colour(Color.SaddleBrown);
	public static readonly Colour Salmon = new Colour(Color.Salmon);
	public static readonly Colour SandyBrown = new Colour(Color.SandyBrown);
	public static readonly Colour SeaGreen = new Colour(Color.SeaGreen);
	public static readonly Colour SeaShell = new Colour(Color.SeaShell);
	public static readonly Colour Sienna = new Colour(Color.Sienna);
	public static readonly Colour Silver = new Colour(Color.Silver);
	public static readonly Colour SkyBlue = new Colour(Color.SkyBlue);
	public static readonly Colour SlateBlue = new Colour(Color.SlateBlue);
	public static readonly Colour SlateGray = new Colour(Color.SlateGray);
	public static readonly Colour Snow = new Colour(Color.Snow);
	public static readonly Colour SpringGreen = new Colour(Color.SpringGreen);
	public static readonly Colour SteelBlue = new Colour(Color.SteelBlue);
	public static readonly Colour Tan = new Colour(Color.Tan);
	public static readonly Colour Teal = new Colour(Color.Teal);
	public static readonly Colour Thistle = new Colour(Color.Thistle);
	public static readonly Colour Tomato = new Colour(Color.Tomato);
	public static readonly Colour Turquoise = new Colour(Color.Turquoise);
	public static readonly Colour Violet = new Colour(Color.Violet);
	public static readonly Colour Wheat = new Colour(Color.Wheat);
	public static readonly Colour White = new Colour(Color.White);
	public static readonly Colour WhiteSmoke = new Colour(Color.WhiteSmoke);
	public static readonly Colour Yellow = new Colour(Color.Yellow);
	public static readonly Colour YellowGreen = new Colour(Color.YellowGreen);
}
