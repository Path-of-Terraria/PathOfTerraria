using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Mapping;
using PathOfTerraria.Common.Systems.Sigils;
using PathOfTerraria.Content.Tiles.Furniture;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Mapping.Sigils;

#nullable enable

internal abstract class DomainSigil : ModItem
{
	private static readonly Dictionary<(SigilKind Kind, SigilGrade Grade), int> itemTypes = [];

	public abstract SigilKind Kind { get; }
	public abstract SigilGrade Grade { get; }
	protected virtual int UpgradeItemType => 0;

	public SigilFamily Family => SigilCatalog.GetFamily(Kind);
	public SigilEntry Entry => new(Kind, Grade);
	public override string Texture => $"Terraria/Images/Item_{ItemID.CelestialSigil}";

	public override void SetStaticDefaults()
	{
		itemTypes[(Kind, Grade)] = Type;
	}

	public override void SetDefaults()
	{
		Item.width = 28;
		Item.height = 28;
		Item.maxStack = 99;
		Item.rare = Grade switch
		{
			SigilGrade.Carved => ItemRarityID.Blue,
			SigilGrade.Gilded => ItemRarityID.Orange,
			SigilGrade.Prismatic => ItemRarityID.Pink,
			_ => ItemRarityID.Red,
		};
		Item.value = Item.buyPrice(silver: Grade switch
		{
			SigilGrade.Carved => 5,
			SigilGrade.Gilded => 20,
			SigilGrade.Prismatic => 75,
			_ => 150,
		});
		Item.color = SigilCatalog.GetColor(Kind);
	}

	public override bool CanRightClick()
	{
		return SmartUiLoader.TryGetUiState(out MapDeviceState? state) && state.Visible
			&& MapDeviceInterface.Entity is { PortalActive: false } device
			&& !device.SigilSlots.Any(item => item.ModItem is DomainSigil other && other.Family == Family)
			&& device.SigilSlots.Take(SigilSystem.UnlockedSlotCount).Any(item => item.IsAir);
	}

	public override bool ConsumeItem(Player player)
	{
		// RightClick only consumes after it has actually inserted one sigil into the device.
		return false;
	}

	public override void RightClick(Player player)
	{
		if (MapDeviceInterface.Entity is not { PortalActive: false } device)
		{
			return;
		}

		if (device.SigilSlots.Any(item => item.ModItem is DomainSigil other && other.Family == Family))
		{
			return;
		}

		for (int i = 0; i < SigilSystem.UnlockedSlotCount; i++)
		{
			if (!device.SigilSlots[i].IsAir)
			{
				continue;
			}

			Item inserted = Item.Clone();
			inserted.stack = 1;
			device.SigilSlots[i] = inserted;
			Item.stack--;
			if (Item.stack <= 0)
			{
				Item.TurnToAir();
			}

			SoundEngine.PlaySound(SoundID.Grab);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				MapDeviceSync.Send(device.ID, MapDeviceSync.Flags.Sigils, [i]);
			}
			break;
		}
	}

	public override void AddRecipes()
	{
		if (UpgradeItemType > 0)
		{
			Recipe.Create(UpgradeItemType).AddIngredient(Type, 3).AddTile(TileID.WorkBenches).Register();
		}
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		tooltips.Add(new TooltipLine(Mod, "SigilUsage", Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.SigilCommon.Usage"))
		{
			OverrideColor = new Color(150, 190, 230),
		});
	}

	public static int ItemTypeFor(SigilKind kind, SigilGrade grade)
	{
		return itemTypes.TryGetValue((kind, grade), out int type) ? type : 0;
	}
}

internal abstract class CarvedSigil<TGilded> : DomainSigil where TGilded : ModItem
{
	public sealed override SigilGrade Grade => SigilGrade.Carved;
	protected override int UpgradeItemType => ModContent.ItemType<TGilded>();
}

internal abstract class GildedSigil<TPrismatic> : DomainSigil where TPrismatic : ModItem
{
	public sealed override SigilGrade Grade => SigilGrade.Gilded;
	protected override int UpgradeItemType => ModContent.ItemType<TPrismatic>();
}

internal abstract class PrismaticSigil : DomainSigil
{
	public sealed override SigilGrade Grade => SigilGrade.Prismatic;
}

internal abstract class UniqueSigil : DomainSigil
{
	public sealed override SigilGrade Grade => SigilGrade.Unique;
}

[LegacyName("CarvedScarabOfBinding")]
internal sealed class CarvedSigilOfBinding : CarvedSigil<GildedSigilOfBinding> { public override SigilKind Kind => SigilKind.Binding; }
[LegacyName("GildedScarabOfBinding")]
internal sealed class GildedSigilOfBinding : GildedSigil<PrismaticSigilOfBinding> { public override SigilKind Kind => SigilKind.Binding; }
[LegacyName("PrismaticScarabOfBinding")]
internal sealed class PrismaticSigilOfBinding : PrismaticSigil { public override SigilKind Kind => SigilKind.Binding; }

[LegacyName("CarvedScarabOfInfernalConflux")]
internal sealed class CarvedSigilOfInfernalConflux : CarvedSigil<GildedSigilOfInfernalConflux> { public override SigilKind Kind => SigilKind.InfernalConflux; }
[LegacyName("GildedScarabOfInfernalConflux")]
internal sealed class GildedSigilOfInfernalConflux : GildedSigil<PrismaticSigilOfInfernalConflux> { public override SigilKind Kind => SigilKind.InfernalConflux; }
[LegacyName("PrismaticScarabOfInfernalConflux")]
internal sealed class PrismaticSigilOfInfernalConflux : PrismaticSigil { public override SigilKind Kind => SigilKind.InfernalConflux; }

[LegacyName("CarvedScarabOfGlacialConflux")]
internal sealed class CarvedSigilOfGlacialConflux : CarvedSigil<GildedSigilOfGlacialConflux> { public override SigilKind Kind => SigilKind.GlacialConflux; }
[LegacyName("GildedScarabOfGlacialConflux")]
internal sealed class GildedSigilOfGlacialConflux : GildedSigil<PrismaticSigilOfGlacialConflux> { public override SigilKind Kind => SigilKind.GlacialConflux; }
[LegacyName("PrismaticScarabOfGlacialConflux")]
internal sealed class PrismaticSigilOfGlacialConflux : PrismaticSigil { public override SigilKind Kind => SigilKind.GlacialConflux; }

[LegacyName("CarvedScarabOfCelestialConflux")]
internal sealed class CarvedSigilOfCelestialConflux : CarvedSigil<GildedSigilOfCelestialConflux> { public override SigilKind Kind => SigilKind.CelestialConflux; }
[LegacyName("GildedScarabOfCelestialConflux")]
internal sealed class GildedSigilOfCelestialConflux : GildedSigil<PrismaticSigilOfCelestialConflux> { public override SigilKind Kind => SigilKind.CelestialConflux; }
[LegacyName("PrismaticScarabOfCelestialConflux")]
internal sealed class PrismaticSigilOfCelestialConflux : PrismaticSigil { public override SigilKind Kind => SigilKind.CelestialConflux; }

[LegacyName("CarvedScarabOfDevotion")]
internal sealed class CarvedSigilOfDevotion : CarvedSigil<GildedSigilOfDevotion> { public override SigilKind Kind => SigilKind.Devotion; }
[LegacyName("GildedScarabOfDevotion")]
internal sealed class GildedSigilOfDevotion : GildedSigil<PrismaticSigilOfDevotion> { public override SigilKind Kind => SigilKind.Devotion; }
[LegacyName("PrismaticScarabOfDevotion")]
internal sealed class PrismaticSigilOfDevotion : PrismaticSigil { public override SigilKind Kind => SigilKind.Devotion; }

[LegacyName("CarvedScarabOfInfestation")]
internal sealed class CarvedSigilOfInfestation : CarvedSigil<GildedSigilOfInfestation> { public override SigilKind Kind => SigilKind.Infestation; }
[LegacyName("GildedScarabOfInfestation")]
internal sealed class GildedSigilOfInfestation : GildedSigil<PrismaticSigilOfInfestation> { public override SigilKind Kind => SigilKind.Infestation; }
[LegacyName("PrismaticScarabOfInfestation")]
internal sealed class PrismaticSigilOfInfestation : PrismaticSigil { public override SigilKind Kind => SigilKind.Infestation; }

[LegacyName("CarvedScarabOfNemeses")]
internal sealed class CarvedSigilOfNemeses : CarvedSigil<GildedSigilOfNemeses> { public override SigilKind Kind => SigilKind.Nemeses; }
[LegacyName("GildedScarabOfNemeses")]
internal sealed class GildedSigilOfNemeses : GildedSigil<PrismaticSigilOfNemeses> { public override SigilKind Kind => SigilKind.Nemeses; }
[LegacyName("PrismaticScarabOfNemeses")]
internal sealed class PrismaticSigilOfNemeses : PrismaticSigil { public override SigilKind Kind => SigilKind.Nemeses; }

[LegacyName("CarvedScarabOfWardedWealth")]
internal sealed class CarvedSigilOfWardedWealth : CarvedSigil<GildedSigilOfWardedWealth> { public override SigilKind Kind => SigilKind.WardedWealth; }
[LegacyName("GildedScarabOfWardedWealth")]
internal sealed class GildedSigilOfWardedWealth : GildedSigil<PrismaticSigilOfWardedWealth> { public override SigilKind Kind => SigilKind.WardedWealth; }
[LegacyName("PrismaticScarabOfWardedWealth")]
internal sealed class PrismaticSigilOfWardedWealth : PrismaticSigil { public override SigilKind Kind => SigilKind.WardedWealth; }

[LegacyName("CarvedScarabOfCartography")]
internal sealed class CarvedSigilOfCartography : CarvedSigil<GildedSigilOfCartography> { public override SigilKind Kind => SigilKind.Cartography; }
[LegacyName("GildedScarabOfCartography")]
internal sealed class GildedSigilOfCartography : GildedSigil<PrismaticSigilOfCartography> { public override SigilKind Kind => SigilKind.Cartography; }
[LegacyName("PrismaticScarabOfCartography")]
internal sealed class PrismaticSigilOfCartography : PrismaticSigil { public override SigilKind Kind => SigilKind.Cartography; }

[LegacyName("CarvedScarabOfSovereignty")]
internal sealed class CarvedSigilOfSovereignty : CarvedSigil<GildedSigilOfSovereignty> { public override SigilKind Kind => SigilKind.Sovereignty; }
[LegacyName("GildedScarabOfSovereignty")]
internal sealed class GildedSigilOfSovereignty : GildedSigil<PrismaticSigilOfSovereignty> { public override SigilKind Kind => SigilKind.Sovereignty; }
[LegacyName("PrismaticScarabOfSovereignty")]
internal sealed class PrismaticSigilOfSovereignty : PrismaticSigil { public override SigilKind Kind => SigilKind.Sovereignty; }

[LegacyName("CarvedScarabOfPeril")]
internal sealed class CarvedSigilOfPeril : CarvedSigil<GildedSigilOfPeril> { public override SigilKind Kind => SigilKind.Peril; }
[LegacyName("GildedScarabOfPeril")]
internal sealed class GildedSigilOfPeril : GildedSigil<PrismaticSigilOfPeril> { public override SigilKind Kind => SigilKind.Peril; }
[LegacyName("PrismaticScarabOfPeril")]
internal sealed class PrismaticSigilOfPeril : PrismaticSigil { public override SigilKind Kind => SigilKind.Peril; }

[LegacyName("ScarabOfTheRunicMenagerie")]
internal sealed class SigilOfTheRunicMenagerie : UniqueSigil { public override SigilKind Kind => SigilKind.RunicMenagerie; }
[LegacyName("ScarabOfTheTriuneConflux")]
internal sealed class SigilOfTheTriuneConflux : UniqueSigil { public override SigilKind Kind => SigilKind.TriuneConflux; }
[LegacyName("ScarabOfTheCrownedTyrant")]
internal sealed class SigilOfTheCrownedTyrant : UniqueSigil { public override SigilKind Kind => SigilKind.CrownedTyrant; }
