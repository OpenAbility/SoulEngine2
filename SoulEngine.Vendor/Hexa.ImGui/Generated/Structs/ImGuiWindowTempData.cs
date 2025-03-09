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
	/// Transient per-window data, reset at the beginning of the frame. This used to be called ImGuiDrawContext, hence the DC variable name in ImGuiWindow.<br/>
	/// (That's theory, in practice the delimitation between ImGuiWindow and ImGuiWindowTempData is quite tenuous and could be reconsidered..)<br/>
	/// (This doesn't need a constructor because we zero-clear it as part of ImGuiWindow and all frame-temporary data are setup on Begin)<br/>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct ImGuiWindowTempData
	{
		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 CursorPos;

		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 CursorPosPrevLine;

		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 CursorStartPos;

		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 CursorMaxPos;

		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 IdealMaxPos;

		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 CurrLineSize;

		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 PrevLineSize;

		/// <summary>
		/// To be documented.
		/// </summary>
		public float CurrLineTextBaseOffset;

		/// <summary>
		/// To be documented.
		/// </summary>
		public float PrevLineTextBaseOffset;

		/// <summary>
		/// To be documented.
		/// </summary>
		public byte IsSameLine;

		/// <summary>
		/// To be documented.
		/// </summary>
		public byte IsSetPos;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImVec1 Indent;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImVec1 ColumnsOffset;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImVec1 GroupOffset;

		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 CursorStartPosLossyness;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImGuiNavLayer NavLayerCurrent;

		/// <summary>
		/// To be documented.
		/// </summary>
		public short NavLayersActiveMask;

		/// <summary>
		/// To be documented.
		/// </summary>
		public short NavLayersActiveMaskNext;

		/// <summary>
		/// To be documented.
		/// </summary>
		public byte NavIsScrollPushableX;

		/// <summary>
		/// To be documented.
		/// </summary>
		public byte NavHideHighlightOneFrame;

		/// <summary>
		/// To be documented.
		/// </summary>
		public byte NavWindowHasScrollY;

		/// <summary>
		/// To be documented.
		/// </summary>
		public byte MenuBarAppending;

		/// <summary>
		/// To be documented.
		/// </summary>
		public Vector2 MenuBarOffset;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImGuiMenuColumns MenuColumns;

		/// <summary>
		/// To be documented.
		/// </summary>
		public int TreeDepth;

		/// <summary>
		/// To be documented.
		/// </summary>
		public uint TreeHasStackDataDepthMask;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImVector<ImGuiWindowPtr> ChildWindows;

		/// <summary>
		/// To be documented.
		/// </summary>
		public unsafe ImGuiStorage* StateStorage;

		/// <summary>
		/// To be documented.
		/// </summary>
		public unsafe ImGuiOldColumns* CurrentColumns;

		/// <summary>
		/// To be documented.
		/// </summary>
		public int CurrentTableIdx;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImGuiLayoutType LayoutType;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImGuiLayoutType ParentLayoutType;

		/// <summary>
		/// To be documented.
		/// </summary>
		public uint ModalDimBgColor;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImGuiItemStatusFlags WindowItemStatusFlags;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImGuiItemStatusFlags ChildItemStatusFlags;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImGuiItemStatusFlags DockTabItemStatusFlags;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImRect DockTabItemRect;

		/// <summary>
		/// To be documented.
		/// </summary>
		public float ItemWidth;

		/// <summary>
		/// To be documented.
		/// </summary>
		public float TextWrapPos;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImVector<float> ItemWidthStack;

		/// <summary>
		/// To be documented.
		/// </summary>
		public ImVector<float> TextWrapPosStack;


		/// <summary>
		/// To be documented.
		/// </summary>
		public unsafe ImGuiWindowTempData(Vector2 cursorPos = default, Vector2 cursorPosPrevLine = default, Vector2 cursorStartPos = default, Vector2 cursorMaxPos = default, Vector2 idealMaxPos = default, Vector2 currLineSize = default, Vector2 prevLineSize = default, float currLineTextBaseOffset = default, float prevLineTextBaseOffset = default, bool isSameLine = default, bool isSetPos = default, ImVec1 indent = default, ImVec1 columnsOffset = default, ImVec1 groupOffset = default, Vector2 cursorStartPosLossyness = default, ImGuiNavLayer navLayerCurrent = default, short navLayersActiveMask = default, short navLayersActiveMaskNext = default, bool navIsScrollPushableX = default, bool navHideHighlightOneFrame = default, bool navWindowHasScrollY = default, bool menuBarAppending = default, Vector2 menuBarOffset = default, ImGuiMenuColumns menuColumns = default, int treeDepth = default, uint treeHasStackDataDepthMask = default, ImVector<ImGuiWindowPtr> childWindows = default, ImGuiStorage* stateStorage = default, ImGuiOldColumns* currentColumns = default, int currentTableIdx = default, ImGuiLayoutType layoutType = default, ImGuiLayoutType parentLayoutType = default, uint modalDimBgColor = default, ImGuiItemStatusFlags windowItemStatusFlags = default, ImGuiItemStatusFlags childItemStatusFlags = default, ImGuiItemStatusFlags dockTabItemStatusFlags = default, ImRect dockTabItemRect = default, float itemWidth = default, float textWrapPos = default, ImVector<float> itemWidthStack = default, ImVector<float> textWrapPosStack = default)
		{
			CursorPos = cursorPos;
			CursorPosPrevLine = cursorPosPrevLine;
			CursorStartPos = cursorStartPos;
			CursorMaxPos = cursorMaxPos;
			IdealMaxPos = idealMaxPos;
			CurrLineSize = currLineSize;
			PrevLineSize = prevLineSize;
			CurrLineTextBaseOffset = currLineTextBaseOffset;
			PrevLineTextBaseOffset = prevLineTextBaseOffset;
			IsSameLine = isSameLine ? (byte)1 : (byte)0;
			IsSetPos = isSetPos ? (byte)1 : (byte)0;
			Indent = indent;
			ColumnsOffset = columnsOffset;
			GroupOffset = groupOffset;
			CursorStartPosLossyness = cursorStartPosLossyness;
			NavLayerCurrent = navLayerCurrent;
			NavLayersActiveMask = navLayersActiveMask;
			NavLayersActiveMaskNext = navLayersActiveMaskNext;
			NavIsScrollPushableX = navIsScrollPushableX ? (byte)1 : (byte)0;
			NavHideHighlightOneFrame = navHideHighlightOneFrame ? (byte)1 : (byte)0;
			NavWindowHasScrollY = navWindowHasScrollY ? (byte)1 : (byte)0;
			MenuBarAppending = menuBarAppending ? (byte)1 : (byte)0;
			MenuBarOffset = menuBarOffset;
			MenuColumns = menuColumns;
			TreeDepth = treeDepth;
			TreeHasStackDataDepthMask = treeHasStackDataDepthMask;
			ChildWindows = childWindows;
			StateStorage = stateStorage;
			CurrentColumns = currentColumns;
			CurrentTableIdx = currentTableIdx;
			LayoutType = layoutType;
			ParentLayoutType = parentLayoutType;
			ModalDimBgColor = modalDimBgColor;
			WindowItemStatusFlags = windowItemStatusFlags;
			ChildItemStatusFlags = childItemStatusFlags;
			DockTabItemStatusFlags = dockTabItemStatusFlags;
			DockTabItemRect = dockTabItemRect;
			ItemWidth = itemWidth;
			TextWrapPos = textWrapPos;
			ItemWidthStack = itemWidthStack;
			TextWrapPosStack = textWrapPosStack;
		}


	}

}
