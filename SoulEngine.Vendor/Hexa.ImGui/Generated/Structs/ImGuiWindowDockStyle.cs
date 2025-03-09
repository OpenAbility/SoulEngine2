// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HexaGen.Runtime;
using System.Numerics;

namespace Hexa.NET.ImGui
{
	/// <summary>
	/// We don't store style.Alpha: dock_node-&gt;LastBgColor embeds it and otherwise it would only affect the docking tab, which intuitively I would say we don't want to.<br/>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct ImGuiWindowDockStyle
	{
		/// <summary>
		/// To be documented.
		/// </summary>
		public uint Colors_0;
		public uint Colors_1;
		public uint Colors_2;
		public uint Colors_3;
		public uint Colors_4;
		public uint Colors_5;
		public uint Colors_6;
		public uint Colors_7;


		/// <summary>
		/// To be documented.
		/// </summary>
		public unsafe ImGuiWindowDockStyle(uint* colors = default)
		{
			if (colors != default(uint*))
			{
				Colors_0 = colors[0];
				Colors_1 = colors[1];
				Colors_2 = colors[2];
				Colors_3 = colors[3];
				Colors_4 = colors[4];
				Colors_5 = colors[5];
				Colors_6 = colors[6];
				Colors_7 = colors[7];
			}
		}

		/// <summary>
		/// To be documented.
		/// </summary>
		public unsafe ImGuiWindowDockStyle(Span<uint> colors = default)
		{
			if (colors != default(Span<uint>))
			{
				Colors_0 = colors[0];
				Colors_1 = colors[1];
				Colors_2 = colors[2];
				Colors_3 = colors[3];
				Colors_4 = colors[4];
				Colors_5 = colors[5];
				Colors_6 = colors[6];
				Colors_7 = colors[7];
			}
		}


	}

}
