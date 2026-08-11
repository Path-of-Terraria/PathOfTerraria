using System.Diagnostics.CodeAnalysis;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Runebound;
using PathOfTerraria.Core.Hooks;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency.Runebound;

internal interface IRunestoneItem
{
}

internal abstract class Runestone : CurrencyShard, IRunestoneItem
{
	public abstract RuneboundFamily Family { get; }
	public abstract RunestoneGrade Grade { get; }

	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Currency/Runebound/{GetType().Name}";

	protected override void SetStaticData()
	{
		// Runestones are encounter rewards and must not enter the ordinary currency drop pool.
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = Grade switch
		{
			RunestoneGrade.Faint => ItemRarityID.Green,
			RunestoneGrade.Greater => ItemRarityID.Orange,
			_ => ItemRarityID.Pink,
		};
	}

	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		Type affixType = RuneboundCrafting.GetAffixType(Family, slotItem);

		if (affixType is null || !PoTItemHelper.TryCreateSpecificAffix(slotItem, affixType, out _))
		{
			failKey = "RuneboundInvalidType";
			return false;
		}

		PoTInstanceItemData data = slotItem.GetInstanceData();
		bool correctRarity = Grade switch
		{
			RunestoneGrade.Faint => data.Rarity == ItemRarity.Normal,
			RunestoneGrade.Greater => data.Rarity == ItemRarity.Magic,
			RunestoneGrade.Perfect => data.Rarity == ItemRarity.Rare && PoTItemHelper.HasNonImplicitAffixes(slotItem),
			_ => false,
		};

		if (!correctRarity)
		{
			failKey = Grade switch
			{
				RunestoneGrade.Faint => "NotNormal",
				RunestoneGrade.Greater => "NotMagic",
				_ => "NotRare",
			};
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
		if (!CanUseInPouch(slotItem, out _))
		{
			return;
		}

		Type affixType = RuneboundCrafting.GetAffixType(Family, slotItem);
		PoTInstanceItemData data = slotItem.GetInstanceData();
		bool applied;

		if (Grade == RunestoneGrade.Perfect)
		{
			applied = PoTItemHelper.TryReplaceRandomExplicitAffix(slotItem, affixType);
		}
		else if (PoTItemHelper.TryCreateSpecificAffix(slotItem, affixType, out var affix))
		{
			data.Rarity = Grade == RunestoneGrade.Faint ? ItemRarity.Magic : ItemRarity.Rare;
			data.Affixes.Add(affix);
			data.NameAffix = GenerateNameAffixes.Invoke(slotItem);
			applied = true;
		}
		else
		{
			applied = false;
		}

		if (applied && Main.LocalPlayer.TryGetModPlayer(out RuneboundPlayer player))
		{
			player.HasCraftedRunestone = true;
			player.SyncState();
		}
	}
}

internal abstract class VigorRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Vigor; }
internal abstract class BastionRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Bastion; }
internal abstract class EmbersRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Embers; }
internal abstract class RimeRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Rime; }
internal abstract class TempestsRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Tempests; }
internal abstract class VoidRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Void; }
internal abstract class MightRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Might; }
internal abstract class PrecisionRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Precision; }
internal abstract class HasteRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Haste; }
internal abstract class SpiritRunestone : Runestone { public override RuneboundFamily Family => RuneboundFamily.Spirit; }

internal sealed class FaintRunestoneOfVigor : VigorRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfVigor : VigorRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfVigor : VigorRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfTheBastion : BastionRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfTheBastion : BastionRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfTheBastion : BastionRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfEmbers : EmbersRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfEmbers : EmbersRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfEmbers : EmbersRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfRime : RimeRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfRime : RimeRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfRime : RimeRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfTempests : TempestsRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfTempests : TempestsRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfTempests : TempestsRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfTheVoid : VoidRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfTheVoid : VoidRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfTheVoid : VoidRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfMight : MightRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfMight : MightRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfMight : MightRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfPrecision : PrecisionRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfPrecision : PrecisionRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfPrecision : PrecisionRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfHaste : HasteRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfHaste : HasteRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfHaste : HasteRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
internal sealed class FaintRunestoneOfSpirit : SpiritRunestone { public override RunestoneGrade Grade => RunestoneGrade.Faint; }
internal sealed class GreaterRunestoneOfSpirit : SpiritRunestone { public override RunestoneGrade Grade => RunestoneGrade.Greater; }
internal sealed class PerfectRunestoneOfSpirit : SpiritRunestone { public override RunestoneGrade Grade => RunestoneGrade.Perfect; }
