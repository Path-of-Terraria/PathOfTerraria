using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Mapping;
using PathOfTerraria.Common.Systems.Scarabs;
using PathOfTerraria.Content.Tiles.Furniture;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Mapping.Scarabs;

#nullable enable

internal abstract class DomainScarab : ModItem
{
	private static readonly Dictionary<(ScarabKind Kind, ScarabGrade Grade), int> itemTypes = [];

	public abstract ScarabKind Kind { get; }
	public abstract ScarabGrade Grade { get; }
	protected virtual int UpgradeItemType => 0;

	public ScarabFamily Family => ScarabCatalog.GetFamily(Kind);
	public ScarabEntry Entry => new(Kind, Grade);
	public override string Texture => $"Terraria/Images/Item_{ItemID.ScarabBomb}";

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
			ScarabGrade.Carved => ItemRarityID.Blue,
			ScarabGrade.Gilded => ItemRarityID.Orange,
			ScarabGrade.Prismatic => ItemRarityID.Pink,
			_ => ItemRarityID.Red,
		};
		Item.value = Item.buyPrice(silver: Grade switch
		{
			ScarabGrade.Carved => 5,
			ScarabGrade.Gilded => 20,
			ScarabGrade.Prismatic => 75,
			_ => 150,
		});
		Item.color = ScarabCatalog.GetColor(Kind);
	}

	public override bool CanRightClick()
	{
		return SmartUiLoader.TryGetUiState(out MapDeviceState? state) && state.Visible
			&& MapDeviceInterface.Entity is { PortalActive: false } device
			&& !device.ScarabSlots.Any(item => item.ModItem is DomainScarab other && other.Family == Family)
			&& device.ScarabSlots.Take(ScarabSystem.UnlockedSlotCount).Any(item => item.IsAir);
	}

	public override bool ConsumeItem(Player player)
	{
		// RightClick only consumes after it has actually inserted one scarab into the device.
		return false;
	}

	public override void RightClick(Player player)
	{
		if (MapDeviceInterface.Entity is not { PortalActive: false } device)
		{
			return;
		}

		if (device.ScarabSlots.Any(item => item.ModItem is DomainScarab other && other.Family == Family))
		{
			return;
		}

		for (int i = 0; i < ScarabSystem.UnlockedSlotCount; i++)
		{
			if (!device.ScarabSlots[i].IsAir)
			{
				continue;
			}

			Item inserted = Item.Clone();
			inserted.stack = 1;
			device.ScarabSlots[i] = inserted;
			Item.stack--;
			if (Item.stack <= 0)
			{
				Item.TurnToAir();
			}

			SoundEngine.PlaySound(SoundID.Grab);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				MapDeviceSync.Send(device.ID, MapDeviceSync.Flags.Scarabs, [i]);
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
		tooltips.Add(new TooltipLine(Mod, "ScarabUsage", Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.ScarabCommon.Usage"))
		{
			OverrideColor = new Color(150, 190, 230),
		});
	}

	public static int ItemTypeFor(ScarabKind kind, ScarabGrade grade)
	{
		return itemTypes.TryGetValue((kind, grade), out int type) ? type : 0;
	}
}

internal abstract class CarvedScarab<TGilded> : DomainScarab where TGilded : ModItem
{
	public sealed override ScarabGrade Grade => ScarabGrade.Carved;
	protected override int UpgradeItemType => ModContent.ItemType<TGilded>();
}

internal abstract class GildedScarab<TPrismatic> : DomainScarab where TPrismatic : ModItem
{
	public sealed override ScarabGrade Grade => ScarabGrade.Gilded;
	protected override int UpgradeItemType => ModContent.ItemType<TPrismatic>();
}

internal abstract class PrismaticScarab : DomainScarab
{
	public sealed override ScarabGrade Grade => ScarabGrade.Prismatic;
}

internal abstract class UniqueScarab : DomainScarab
{
	public sealed override ScarabGrade Grade => ScarabGrade.Unique;
}

internal sealed class CarvedScarabOfBinding : CarvedScarab<GildedScarabOfBinding> { public override ScarabKind Kind => ScarabKind.Binding; }
internal sealed class GildedScarabOfBinding : GildedScarab<PrismaticScarabOfBinding> { public override ScarabKind Kind => ScarabKind.Binding; }
internal sealed class PrismaticScarabOfBinding : PrismaticScarab { public override ScarabKind Kind => ScarabKind.Binding; }

internal sealed class CarvedScarabOfInfernalConflux : CarvedScarab<GildedScarabOfInfernalConflux> { public override ScarabKind Kind => ScarabKind.InfernalConflux; }
internal sealed class GildedScarabOfInfernalConflux : GildedScarab<PrismaticScarabOfInfernalConflux> { public override ScarabKind Kind => ScarabKind.InfernalConflux; }
internal sealed class PrismaticScarabOfInfernalConflux : PrismaticScarab { public override ScarabKind Kind => ScarabKind.InfernalConflux; }

internal sealed class CarvedScarabOfGlacialConflux : CarvedScarab<GildedScarabOfGlacialConflux> { public override ScarabKind Kind => ScarabKind.GlacialConflux; }
internal sealed class GildedScarabOfGlacialConflux : GildedScarab<PrismaticScarabOfGlacialConflux> { public override ScarabKind Kind => ScarabKind.GlacialConflux; }
internal sealed class PrismaticScarabOfGlacialConflux : PrismaticScarab { public override ScarabKind Kind => ScarabKind.GlacialConflux; }

internal sealed class CarvedScarabOfCelestialConflux : CarvedScarab<GildedScarabOfCelestialConflux> { public override ScarabKind Kind => ScarabKind.CelestialConflux; }
internal sealed class GildedScarabOfCelestialConflux : GildedScarab<PrismaticScarabOfCelestialConflux> { public override ScarabKind Kind => ScarabKind.CelestialConflux; }
internal sealed class PrismaticScarabOfCelestialConflux : PrismaticScarab { public override ScarabKind Kind => ScarabKind.CelestialConflux; }

internal sealed class CarvedScarabOfDevotion : CarvedScarab<GildedScarabOfDevotion> { public override ScarabKind Kind => ScarabKind.Devotion; }
internal sealed class GildedScarabOfDevotion : GildedScarab<PrismaticScarabOfDevotion> { public override ScarabKind Kind => ScarabKind.Devotion; }
internal sealed class PrismaticScarabOfDevotion : PrismaticScarab { public override ScarabKind Kind => ScarabKind.Devotion; }

internal sealed class CarvedScarabOfInfestation : CarvedScarab<GildedScarabOfInfestation> { public override ScarabKind Kind => ScarabKind.Infestation; }
internal sealed class GildedScarabOfInfestation : GildedScarab<PrismaticScarabOfInfestation> { public override ScarabKind Kind => ScarabKind.Infestation; }
internal sealed class PrismaticScarabOfInfestation : PrismaticScarab { public override ScarabKind Kind => ScarabKind.Infestation; }

internal sealed class CarvedScarabOfNemeses : CarvedScarab<GildedScarabOfNemeses> { public override ScarabKind Kind => ScarabKind.Nemeses; }
internal sealed class GildedScarabOfNemeses : GildedScarab<PrismaticScarabOfNemeses> { public override ScarabKind Kind => ScarabKind.Nemeses; }
internal sealed class PrismaticScarabOfNemeses : PrismaticScarab { public override ScarabKind Kind => ScarabKind.Nemeses; }

internal sealed class CarvedScarabOfWardedWealth : CarvedScarab<GildedScarabOfWardedWealth> { public override ScarabKind Kind => ScarabKind.WardedWealth; }
internal sealed class GildedScarabOfWardedWealth : GildedScarab<PrismaticScarabOfWardedWealth> { public override ScarabKind Kind => ScarabKind.WardedWealth; }
internal sealed class PrismaticScarabOfWardedWealth : PrismaticScarab { public override ScarabKind Kind => ScarabKind.WardedWealth; }

internal sealed class CarvedScarabOfCartography : CarvedScarab<GildedScarabOfCartography> { public override ScarabKind Kind => ScarabKind.Cartography; }
internal sealed class GildedScarabOfCartography : GildedScarab<PrismaticScarabOfCartography> { public override ScarabKind Kind => ScarabKind.Cartography; }
internal sealed class PrismaticScarabOfCartography : PrismaticScarab { public override ScarabKind Kind => ScarabKind.Cartography; }

internal sealed class CarvedScarabOfSovereignty : CarvedScarab<GildedScarabOfSovereignty> { public override ScarabKind Kind => ScarabKind.Sovereignty; }
internal sealed class GildedScarabOfSovereignty : GildedScarab<PrismaticScarabOfSovereignty> { public override ScarabKind Kind => ScarabKind.Sovereignty; }
internal sealed class PrismaticScarabOfSovereignty : PrismaticScarab { public override ScarabKind Kind => ScarabKind.Sovereignty; }

internal sealed class CarvedScarabOfPeril : CarvedScarab<GildedScarabOfPeril> { public override ScarabKind Kind => ScarabKind.Peril; }
internal sealed class GildedScarabOfPeril : GildedScarab<PrismaticScarabOfPeril> { public override ScarabKind Kind => ScarabKind.Peril; }
internal sealed class PrismaticScarabOfPeril : PrismaticScarab { public override ScarabKind Kind => ScarabKind.Peril; }

internal sealed class ScarabOfTheRunicMenagerie : UniqueScarab { public override ScarabKind Kind => ScarabKind.RunicMenagerie; }
internal sealed class ScarabOfTheTriuneConflux : UniqueScarab { public override ScarabKind Kind => ScarabKind.TriuneConflux; }
internal sealed class ScarabOfTheCrownedTyrant : UniqueScarab { public override ScarabKind Kind => ScarabKind.CrownedTyrant; }
