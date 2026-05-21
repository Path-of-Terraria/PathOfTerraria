using Terraria.ID;

namespace PathOfTerraria.Common.Items;

[ReinitializeDuringResizeArrays]
internal class CustomItemSets
{
	/// <summary>
	/// Defines an item as a 'visual channel' item - something that channels a visual effect, but isn't actually a channeled weapon.<br/>
	/// Examples would be the Staff weapons or Laser Machinegun.
	/// </summary>
	public static bool[] VisualChannelOnly = ItemID.Sets.Factory.CreateNamedSet(PoTMod.Instance, "VisualChannelOnly")
		.Description("Defines an item as a 'visual channel' item - something that channels a visual effect, but isn't actually a channeled weapon.")
		.RegisterBoolSet(false, [ItemID.LaserMachinegun, ItemID.VortexBeater]);

	/// <summary>
	/// Defines which weapons should scale with Area of Effect modifiers.
	/// </summary>
	public static bool[] AreaOfEffectWeapons = ItemID.Sets.Factory.CreateNamedSet(PoTMod.Instance, "AreaOfEffectWeapons")
		.Description("Defines which weapons should scale with Area of Effect modifiers.")
		.RegisterBoolSet(false);
}
