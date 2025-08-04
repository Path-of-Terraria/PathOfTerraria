using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.GrimoireSelection;
using PathOfTerraria.Content.Projectiles.Summoner;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;

internal class GrimoireItem : Gear
{
	protected override string GearLocalizationCategory => "Grimoire";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0f;
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
		staticData.Description = this.GetLocalization("Description");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 10;
		Item.width = 30;
		Item.height = 34;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.useTime = 40;
		Item.useAnimation = 40;
		Item.autoReuse = false;
		Item.DamageType = DamageClass.Summon;
		Item.knockBack = 8;
		Item.crit = 12;
		Item.UseSound = SoundID.Item1;
		Item.shoot = ProjectileID.PurificationPowder; // The value here is irrelevant
		Item.channel = true;
		Item.noMelee = true;

		Item.shopCustomPrice = Item.buyPrice(silver: 10);

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Grimoire;
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override bool CanUseItem(Player player)
	{
		if (player.altFunctionUse == 2)
		{
			SmartUiLoader.GetUiState<GrimoireSelectionUIState>().Toggle();
			return false;
		}

		return GrimoirePlayer.Get(player).CurrentSummonId != -1;
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		type = GrimoirePlayer.Get(player).CurrentSummonId;
		damage = (ContentSamples.ProjectilesByType[type].ModProjectile as GrimoireSummon).BaseDamage;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
		Main.projectile[proj].damage = damage;
		Main.projectile[proj].originalDamage = damage;

		for (int i = 0; i < 10; i++)
		{
			float strength = Main.rand.NextFloat();
			Vector2 unit = Main.rand.NextVector2Unit() * strength;

			var dust = Dust.NewDustPerfect(position, DustID.GreenTorch, unit * 3, newColor: Color.White with { A = 0 }, Scale: strength * 3f);
			dust.noGravity = true;
			dust.noLightEmittence = true;

			Dust.NewDustPerfect(position, DustID.Smoke, unit, Alpha: 150, newColor: Color.Black, Scale: strength * 3f);
		}

		ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.TerraBlade, new() { PositionInWorld = position });

		SoundEngine.PlaySound(SoundID.AbigailSummon with { Volume = 0.3f, Pitch = -0.2f, PitchVariance = 0.2f }, player.Center);
		SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f, Pitch = 0.5f }, player.Center);

		return false;
	}

	public override bool OnPickup(Player player)
	{
		GrimoirePlayer.Get(player).HasObtainedGrimoire = true;
		return true;
	}

	public override bool ModifyNewTooltipLine(TooltipLine line)
	{
		if (line.Name == "Description")
		{
			int id = GrimoirePlayer.Get().CurrentSummonId;
			bool hasSummon = id != -1;
			string control = hasSummon ? this.GetLocalization("Control").Format(Lang.GetProjectileName(id).Value) : this.GetLocalization("ControlNone").Value;
			line.Text = control + "\n" + this.GetLocalization("Description");
		}

		return true;
	}
}