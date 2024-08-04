using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria.UI;

namespace PathOfTerraria.Core.UI.SmartUI;

internal sealed partial class SmartUiLoader
{
	/// <summary>
	///		Attempts to retrieve a singleton instance of smart UI state
	///		<typeparamref name="T"/>.
	/// </summary>
	/// <param name="state">The resolved state.</param>
	/// <typeparam name="T">The smart UI state type.</typeparam>
	/// <returns>
	///		<see langword="true"/> if the state was found; otherwise,
	///		<see langword="false"/>.
	/// </returns>
	public static bool TryGetUiState<T>([NotNullWhen(returnValue: true)] out T? state) where T : SmartUiState
	{
		state = ModContent.GetInstance<SmartUiLoader>().uiStates.FirstOrDefault(n => n is T) as T;
		return state is not null;
	}

	/// <summary>
	///		Retrieves a singleton instance of smart UI state
	///		<typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The smart UI state type.</typeparam>
	/// <returns>The resolved state.</returns>
	public static T GetUiState<T>() where T : SmartUiState
	{
		if (!TryGetUiState(out T? state))
		{
			throw new InvalidOperationException($"No instance of {typeof(T).Name} found.");
		}

		return state;
	}

	/// <summary>
	///		Derives the most-ancestral <see cref="SmartUiElement"/> from the
	///		given <paramref name="element"/>.
	/// </summary>
	/// <remarks>
	///		Traverses up the element hierarchy until a
	///		<see cref="SmartUiElement"/> ancestor is found.
	///		<br />
	///		Returns once the parent is <see langword="null"/> or there is a gap
	///		in smart elements in the hierarchy.
	///		<br />
	///		Returns <see langword="null"/> if no smart UI element is found.
	/// </remarks>
	private static SmartUiElement? DeriveSmartUiElement(UIElement? element)
	{
		return element switch
		{
			null => null,
			SmartUiElement e when element.Parent is not SmartUiElement => e,
			_ => DeriveSmartUiElement(element.Parent),
		};
	}
}