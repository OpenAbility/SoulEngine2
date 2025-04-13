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

	public static readonly Colour White = new Colour(1, 1, 1);
	public static readonly Colour Black = new Colour(0, 0, 0);
	public static readonly Colour Blank = new Colour(0, 0, 0, 0);
	public static readonly Colour Red = new Colour(1, 0, 0);
	public static readonly Colour Green = new Colour(0, 1, 0);
	public static readonly Colour Blue = new Colour(0, 0, 1);
	public static readonly Colour Yellow = new Colour(1, 1, 0);
	public static readonly Colour Grey  = new Colour(.5f, .5f, .5f);
	public static readonly Colour Pink = new Colour(1, 0, 1, 1);

	public readonly Colour WithAlpha(float alpha)
	{
		return new Colour(R, G, B, alpha);
	}

	public static Colour operator *(Colour a, float b)
	{
		return new Colour(a.R * b, a.G * b, a.B * b, a.A * b);
	}

	public static Colour Lerp(Colour a, Colour b, float d)
	{
		return new Colour(
				Mathf.Lerp(a.R, b.R, d),
				Mathf.Lerp(a.G, b.G, d),
				Mathf.Lerp(a.B, b.B, d),
				Mathf.Lerp(a.A, b.A, d));
	}
	
	public static Colour LerpUnclamped(Colour a, Colour b, float d)
	{
		return new Colour(
			Mathf.LerpUnclamped(a.R, b.R, d),
			Mathf.LerpUnclamped(a.G, b.G, d),
			Mathf.LerpUnclamped(a.B, b.B, d),
			Mathf.LerpUnclamped(a.A, b.A, d));
	}

	public static Colour FromRgba32(byte r, byte g, byte b, byte a)
	{
		return new Colour(r / (float)Byte.MaxValue, g / (float)Byte.MaxValue, b/ (float)Byte.MaxValue, a / (float)Byte.MaxValue);
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

	public void ToRgba32(ref byte[] target, int offset = 0)
	{
		target[offset] = (byte)(R * Byte.MaxValue);
		target[offset + 1] = (byte)(G * Byte.MaxValue);
		target[offset + 2] = (byte)(B * Byte.MaxValue);
		target[offset + 3] = (byte)(A * Byte.MaxValue);
	}
	
	public unsafe uint ToUint32()
	{
		uint targetInt = 0;
		byte* target = (byte*)&targetInt;
		target[0] = (byte)(R * Byte.MaxValue);
		target[1] = (byte)(G * Byte.MaxValue);
		target[2] = (byte)(B * Byte.MaxValue);
		target[3] = (byte)(A * Byte.MaxValue);
		return targetInt;
	}

	public byte[] ToRgba32()
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
	public Colour Brighten(float amount)
	{
		return new Colour(R + amount, G + amount, B + amount, A);
	}
}
