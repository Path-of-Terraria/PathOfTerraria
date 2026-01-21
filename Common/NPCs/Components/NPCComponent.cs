#nullable enable
#pragma warning disable IDE0032 // Use auto property

namespace PathOfTerraria.Common.NPCs.Components;

public abstract class NPCComponent : GlobalNPC
{
	public sealed override bool InstancePerEntity { get; } = true;
	
	/// <summary>
	///		Whether the component is enabled or not.
	/// </summary>
	public bool Enabled { get; protected set; }

	protected internal virtual void SetEnabled(bool value)
	{
		Enabled = value;
	}
}

public abstract class NPCComponent<TData> : NPCComponent where TData : class, new()
{
	private TData? data = null;

	public TData Data
	{
		get
		{
#if DEBUG
			if (data == null) { throw new InvalidOperationException("NPC component data accessed without being set!"); }
#endif

			return data;
		}
		set => data = value;
	}

	protected internal sealed override void SetEnabled(bool value)
	{
		if (!Enabled && value) { Data = new(); }

		Enabled = value;
	}
}