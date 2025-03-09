// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HexaGen.Runtime;
using System.Numerics;
using Hexa.NET.ImGui;

namespace Hexa.NET.ImGuizmo
{
	public unsafe partial class ImGuizmo
	{
		internal static FunctionTable funcTable;

		/// <summary>
		/// Initializes the function table, automatically called. Do not call manually, only after <see cref="FreeApi"/>.
		/// </summary>
		public static void InitApi(INativeContext context)
		{
			funcTable = new FunctionTable(context, 38);
			funcTable.Load(0, "ImGuizmo_SetDrawlist");
			funcTable.Load(1, "ImGuizmo_BeginFrame");
			funcTable.Load(2, "ImGuizmo_SetImGuiContext");
			funcTable.Load(3, "ImGuizmo_IsOver_Nil");
			funcTable.Load(4, "ImGuizmo_IsUsing");
			funcTable.Load(5, "ImGuizmo_IsUsingViewManipulate");
			funcTable.Load(6, "ImGuizmo_IsViewManipulateHovered");
			funcTable.Load(7, "ImGuizmo_IsUsingAny");
			funcTable.Load(8, "ImGuizmo_Enable");
			funcTable.Load(9, "ImGuizmo_DecomposeMatrixToComponents");
			funcTable.Load(10, "ImGuizmo_RecomposeMatrixFromComponents");
			funcTable.Load(11, "ImGuizmo_SetRect");
			funcTable.Load(12, "ImGuizmo_SetOrthographic");
			funcTable.Load(13, "ImGuizmo_DrawCubes");
			funcTable.Load(14, "ImGuizmo_DrawGrid");
			funcTable.Load(15, "ImGuizmo_Manipulate");
			funcTable.Load(16, "ImGuizmo_ViewManipulate_Float");
			funcTable.Load(17, "ImGuizmo_ViewManipulate_FloatPtr");
			funcTable.Load(18, "ImGuizmo_SetAlternativeWindow");
			funcTable.Load(19, "ImGuizmo_SetID");
			funcTable.Load(20, "ImGuizmo_PushID_Str");
			funcTable.Load(21, "ImGuizmo_PushID_StrStr");
			funcTable.Load(22, "ImGuizmo_PushID_Ptr");
			funcTable.Load(23, "ImGuizmo_PushID_Int");
			funcTable.Load(24, "ImGuizmo_PopID");
			funcTable.Load(25, "ImGuizmo_GetID_Str");
			funcTable.Load(26, "ImGuizmo_GetID_StrStr");
			funcTable.Load(27, "ImGuizmo_GetID_Ptr");
			funcTable.Load(28, "ImGuizmo_IsOver_OPERATION");
			funcTable.Load(29, "ImGuizmo_SetGizmoSizeClipSpace");
			funcTable.Load(30, "ImGuizmo_AllowAxisFlip");
			funcTable.Load(31, "ImGuizmo_SetAxisLimit");
			funcTable.Load(32, "ImGuizmo_SetAxisMask");
			funcTable.Load(33, "ImGuizmo_SetPlaneLimit");
			funcTable.Load(34, "ImGuizmo_IsOver_FloatPtr");
			funcTable.Load(35, "Style_Style");
			funcTable.Load(36, "Style_destroy");
			funcTable.Load(37, "ImGuizmo_GetStyle");
		}

		public static void FreeApi()
		{
			funcTable.Free();
		}
	}
}
