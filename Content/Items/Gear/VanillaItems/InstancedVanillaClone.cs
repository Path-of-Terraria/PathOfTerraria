﻿using PathOfTerraria.Core;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems;

[ManuallyLoadPoTItem]
internal class InstancedVanillaClone(short itemId, ItemType itemType, string name) : VanillaClone
{
	protected override short VanillaItemId => ItemId;
	protected override bool CloneNewInstances => true;
	public override string Name => InstanceName;
	public override float DropChance => 0f;
	public override bool IsUnique => true;

	protected string InstanceName = name;
	protected short ItemId = itemId;
	protected ItemType InstancedItemType = itemType;

	public override ModItem Clone(Item newEntity)
	{
		ModItem clone = base.Clone(newEntity);
		var inst = (InstancedVanillaClone)clone;
		inst.ItemId = ItemId;
		inst.ItemType = ItemType;
		inst.InstanceName = InstanceName;
		return clone;
	}

	public override void Defaults()
	{
		ItemType = InstancedItemType;
		base.Defaults();
	}
}