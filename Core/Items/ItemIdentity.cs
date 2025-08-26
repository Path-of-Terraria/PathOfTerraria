#nullable enable

namespace PathOfTerraria.Core.Items;

/// <summary> A small utility that lets cloned item instances to be linked back to their originals. </summary>
internal sealed class ItemIdentity : GlobalItem
{
	public int Identity { get; private set; }

	public override bool InstancePerEntity => true;

	public override void SetDefaults(Item item)
	{
		Identity = item.GetHashCode();
	}

	public bool IsCloned(Item item)
	{
		return Identity != item.GetHashCode();
	}

	public static bool? AreItemsRelated(Item a, Item b)
	{
		if (!a.TryGetGlobalItem(out ItemIdentity idA) || !b.TryGetGlobalItem(out ItemIdentity idB))
		{
			return null;
		}

		return idA.Identity == idB.Identity;
	}
}
