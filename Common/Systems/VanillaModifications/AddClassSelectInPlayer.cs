using PathOfTerraria.Common.Classing;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Items.Gear.Weapons.Staff;
using PathOfTerraria.Content.Items.Gear.Weapons.Wand;
using PathOfTerraria.Content.Skills.Magic;
using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Content.Skills.Ranged;
using PathOfTerraria.Content.Skills.Summon;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

internal class AddClassSelectInPlayer : ModSystem
{
	public override void Load()
	{
		On_UICharacterCreation.FinishCreatingCharacter += FinishCreatingChar;
		On_UICharacterCreation.SetupPlayerStatsAndInventoryBasedOnDifficulty += ReplaceWeaponWithClassItem;
	}

	private void ReplaceWeaponWithClassItem(On_UICharacterCreation.orig_SetupPlayerStatsAndInventoryBasedOnDifficulty orig, UICharacterCreation self)
	{
		orig(self);

		Player player = GetPlayer(self);
		StarterClasses classType = player.GetModPlayer<ClassingPlayer>().Class;

		player.GetModPlayer<SkillCombatPlayer>().TryAddSkill(classType switch
		{
			StarterClasses.Melee => new Berserk(),
			StarterClasses.Ranged => new RainOfArrows(),
			StarterClasses.Summon => new FlameSage(),
			StarterClasses.Magic or _ => new Fireball(),
		}, true);

		if (classType != StarterClasses.Melee)
		{
			player.inventory[0].SetDefaults(classType switch
			{
				StarterClasses.Ranged => ModContent.ItemType<WoodenShortBow>(),
				StarterClasses.Summon => ItemID.SlimeStaff,
				StarterClasses.Magic or _ => ModContent.ItemType<EbonwoodWand>()
			});
		}
	}

	private void FinishCreatingChar(On_UICharacterCreation.orig_FinishCreatingCharacter orig, UICharacterCreation self)
	{
		Player player = GetPlayer(self);

		if (player.GetModPlayer<ClassingPlayer>().Class is { } classType && classType == StarterClasses.None)
		{
			void Reset()
			{
				Main.MenuUI.SetState(self);
			}

			Main.MenuUI.SetState(new ClassUIState(Reset, GetPlayer(self)));
			return;
		}

		orig(self);
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_player")]
	private static extern ref Player GetPlayer(UICharacterCreation ui);
}
