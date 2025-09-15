using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Core.UI;

internal abstract class UIComponent
{
	// Avoid keeping attached elements alive.
	private WeakReference<UIElement> reference = null!;

	/// <summary> The element that this UI component is attached to. When possible, it best to instead use the element parameter provided by events. </summary>
	public UIElement Element
	{
		get
		{
			reference.TryGetTarget(out UIElement? element);
			return element!;
		}
	}

	protected abstract void OnAttach(UIElement element);

	protected abstract void OnDetach(UIElement element);

	public void AttachTo(UIElement element)
	{
		if (reference != null)
		{
			throw new InvalidOperationException("UI component already attached to an element.");
		}

		reference = new WeakReference<UIElement>(element);

		OnAttach(element);
	}

	public void DetachFrom(UIElement element)
	{
		if (reference == null)
		{
			throw new InvalidOperationException("UI component not attached to an element.");
		}

		if (element != Element)
		{
			throw new InvalidOperationException("Attempted to detach a UI component from an element it is not attached to.");
		}

		OnDetach(element);

		reference = null!;
	}
}

internal static class UIComponentExtensions
{
	/// <summary> Links a given component with this UI element, returns the component. </summary>
	public static TComponent AddComponent<TComponent>(this UIElement element, TComponent component) where TComponent : UIComponent
	{
		component.AttachTo(element);
		return component;
	}
}
