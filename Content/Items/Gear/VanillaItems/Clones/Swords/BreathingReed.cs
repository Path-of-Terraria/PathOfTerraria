using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class BreathingReed : VanillaClone
{
	protected override short VanillaItemId => ItemID.BreathingReed;

	public override void Load()
	{
		On_Player.CheckDrowning += HijackHeldItemType;
	}

	private void HijackHeldItemType(On_Player.orig_CheckDrowning orig, Player self)
	{
		bool wasModReed = self.HeldItem.type == ModContent.ItemType<BreathingReed>();

		if (wasModReed)
		{
			self.HeldItem.type = ItemID.BreathingReed;
		}

		orig(self);

		if (wasModReed)
		{
			self.HeldItem.type = ModContent.ItemType<BreathingReed>();
		}
	}

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}
}
