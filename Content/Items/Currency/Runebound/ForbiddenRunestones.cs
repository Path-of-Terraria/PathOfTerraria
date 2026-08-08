using System.Diagnostics.CodeAnalysis;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.Runebound;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency.Runebound;

internal abstract class ForbiddenRunestone : CurrencyShard
{
	protected abstract Type ChaseAffixType { get; }

	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Currency/AscendantShard";

	protected override void SetStaticData()
	{
		// Chase runestones are exclusive to high-level Runebound encounters.
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Purple;
	}

	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		PoTInstanceItemData data = slotItem.GetInstanceData();
		if (data.Rarity != ItemRarity.Rare || !PoTItemHelper.HasNonImplicitAffixes(slotItem))
		{
			failKey = "NotRare";
			return false;
		}

		if (!PoTItemHelper.TryCreateSpecificAffix(slotItem, ChaseAffixType, out _))
		{
			failKey = "RuneboundInvalidType";
			return false;
		}

		failKey = null;
		return true;
	}

	public override void RightClick(Player player)
	{
		base.RightClick(player);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}

	public override void ApplyToItem(Item slotItem)
	{
		if (CanUseInPouch(slotItem, out _) && PoTItemHelper.TryReplaceRandomExplicitAffix(slotItem, ChaseAffixType, chaseAffix: true))
		{
			RuneboundPlayer progress = Main.LocalPlayer.GetModPlayer<RuneboundPlayer>();
			progress.HasCraftedRunestone = true;
			progress.SyncState();
		}
	}
}

internal sealed class ForbiddenRunestoneOfThePrism : ForbiddenRunestone
{
	protected override Type ChaseAffixType => typeof(RuneboundPrismaticAffix);
}

internal sealed class ForbiddenRunestoneOfTheTitan : ForbiddenRunestone
{
	protected override Type ChaseAffixType => typeof(RuneboundTitanAffix);
}

internal sealed class ForbiddenRunestoneOfAnnihilation : ForbiddenRunestone
{
	protected override Type ChaseAffixType => typeof(RuneboundAnnihilationAffix);
}

internal sealed class ForbiddenRunestoneOfTrinity : ForbiddenRunestone
{
	protected override Type ChaseAffixType => typeof(RuneboundTrinityAffix);
}
