namespace PathOfTerraria.Core.UI.SmartUI;

/// <summary>
/// Defines a UI state that cannot be opened alongside another mutually exclusive UI.<br/>
/// This requires that UI define a Toggle method, which is widely used in this codebase but manually and implicitly.<br/>
/// Most non-exclusive UI will also have a Toggle.
/// </summary>
internal interface IMutuallyExclusiveUI
{
	/// <summary>
	/// Toggles the given UI.
	/// </summary>
	public void Toggle();
}
