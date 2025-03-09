// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using HexaGen.Runtime;
using System.Numerics;

namespace Hexa.NET.ImGui
{
	/// <summary>
	/// To be documented.
	/// </summary>
	[Flags]
	public enum ImGuiInputTextFlags : int
	{
		/// <summary>
		/// To be documented.
		/// </summary>
		None = unchecked(0),

		/// <summary>
		/// Allow 0123456789.+-*<br/>
		/// </summary>
		CharsDecimal = unchecked(1),

		/// <summary>
		/// Allow 0123456789ABCDEFabcdef<br/>
		/// </summary>
		CharsHexadecimal = unchecked(2),

		/// <summary>
		/// Allow 0123456789.+-*eE (Scientific notation input)<br/>
		/// </summary>
		CharsScientific = unchecked(4),

		/// <summary>
		/// Turn a..z into A..Z<br/>
		/// </summary>
		CharsUppercase = unchecked(8),

		/// <summary>
		/// Filter out spaces, tabs<br/>
		/// </summary>
		CharsNoBlank = unchecked(16),

		/// <summary>
		/// Pressing TAB input a '\t' character into the text field<br/>
		/// </summary>
		AllowTabInput = unchecked(32),

		/// <summary>
		/// Return 'true' when Enter is pressed (as opposed to every time the value was modified). Consider using IsItemDeactivatedAfterEdit() instead!<br/>
		/// </summary>
		EnterReturnsTrue = unchecked(64),

		/// <summary>
		/// Escape key clears content if not empty, and deactivate otherwise (contrast to default behavior of Escape to revert)<br/>
		/// </summary>
		EscapeClearsAll = unchecked(128),

		/// <summary>
		/// In multi-line mode, validate with Enter, add new line with Ctrl+Enter (default is opposite: validate with Ctrl+Enter, add line with Enter).<br/>
		/// </summary>
		CtrlEnterForNewLine = unchecked(256),

		/// <summary>
		/// Read-only mode<br/>
		/// </summary>
		ReadOnly = unchecked(512),

		/// <summary>
		/// Password mode, display all characters as '*', disable copy<br/>
		/// </summary>
		Password = unchecked(1024),

		/// <summary>
		/// Overwrite mode<br/>
		/// </summary>
		AlwaysOverwrite = unchecked(2048),

		/// <summary>
		/// Select entire text when first taking mouse focus<br/>
		/// </summary>
		AutoSelectAll = unchecked(4096),

		/// <summary>
		/// InputFloat(), InputInt(), InputScalar() etc. only: parse empty string as zero value.<br/>
		/// </summary>
		ParseEmptyRefVal = unchecked(8192),

		/// <summary>
		/// InputFloat(), InputInt(), InputScalar() etc. only: when value is zero, do not display it. Generally used with ImGuiInputTextFlags_ParseEmptyRefVal.<br/>
		/// </summary>
		DisplayEmptyRefVal = unchecked(16384),

		/// <summary>
		/// Disable following the cursor horizontally<br/>
		/// </summary>
		NoHorizontalScroll = unchecked(32768),

		/// <summary>
		/// Disable undoredo. Note that input text owns the text data while active, if you want to provide your own undoredo stack you need e.g. to call ClearActiveID().<br/>
		/// </summary>
		NoUndoRedo = unchecked(65536),

		/// <summary>
		/// When text doesn't fit, elide left side to ensure right side stays visible. Useful for pathfilenames. Single-line only!<br/>
		/// </summary>
		ElideLeft = unchecked(131072),

		/// <summary>
		/// Callback on pressing TAB (for completion handling)<br/>
		/// </summary>
		CallbackCompletion = unchecked(262144),

		/// <summary>
		/// Callback on pressing UpDown arrows (for history handling)<br/>
		/// </summary>
		CallbackHistory = unchecked(524288),

		/// <summary>
		/// Callback on each iteration. User code may query cursor position, modify text buffer.<br/>
		/// </summary>
		CallbackAlways = unchecked(1048576),

		/// <summary>
		/// Callback on character inputs to replace or discard them. Modify 'EventChar' to replace or discard, or return 1 in callback to discard.<br/>
		/// </summary>
		CallbackCharFilter = unchecked(2097152),

		/// <summary>
		/// Callback on buffer capacity changes request (beyond 'buf_size' parameter value), allowing the string to grow. Notify when the string wants to be resized (for string types which hold a cache of their Size). You will be provided a new BufSize in the callback and NEED to honor it. (see misccppimgui_stdlib.h for an example of using this)<br/>
		/// </summary>
		CallbackResize = unchecked(4194304),

		/// <summary>
		/// Callback on any edit. Note that InputText() already returns true on edit + you can always use IsItemEdited(). The callback is useful to manipulate the underlying buffer while focus is active.<br/>
		/// </summary>
		CallbackEdit = unchecked(8388608),
	}
}
