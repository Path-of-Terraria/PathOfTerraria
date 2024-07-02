using System.Collections.Generic;
using PathOfTerraria.Content.Projectiles.Melee;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;
using ReLogic.Content;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class GuardianAngel : SteelBattleaxe
{
	public override float DropChance => 1f;
	public override bool IsUnique => true;
	public override string AltUseDescription => Language.GetTextValue("Mods.PathOfTerraria.Items.GuardianAngel.AltUseDescription");
	public override string Description => Language.GetTextValue("Mods.PathOfTerraria.Items.GuardianAngel.Description");
	public override int MinDropItemLevel => 25;

	public override void Defaults()
	{
		base.Defaults();
		Item.width = 94;
		Item.height = 108;
	}
	
	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		if (!modPlayer.AltFunctionAvailable)
		{
			return false;
		}

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.CanBeChasedBy() && npc.DistanceSQ(player.Center) < 400 * 400)
			{
				npc.GetGlobalNPC<AngelRingNPC>().ApplyRing(player.whoAmI);
			}
		}

		modPlayer.SetAltCooldown(180, 0);
		return true;
	}
	
	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		target.GetGlobalNPC<AngelRingNPC>().ApplyRing(player.whoAmI);
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<AddedDamageAffix>();
		addedDamageAffix.MinValue = 1;
		addedDamageAffix.MaxValue = 4;
		
		var increasedDamageAffix = (ItemAffix)Affix.CreateAffix<BaseKnockbackItemAffix>();
		addedDamageAffix.MinValue = 0.1f;
		addedDamageAffix.MaxValue = 0.1f;

		var attackSpeedAffix = (ItemAffix)Affix.CreateAffix<IncreasedAttackSpeedAffix>();
		addedDamageAffix.MinValue = 0.1f;
		addedDamageAffix.MaxValue = 0.1f;

		var armorShredAffix = (ItemAffix)Affix.CreateAffix<AddedKnockbackItemAffix>();
		addedDamageAffix.MinValue = 0.1f;
		addedDamageAffix.MaxValue = 0.1f;
		return [increasedDamageAffix, increasedDamageAffix, attackSpeedAffix, armorShredAffix];
	}

	internal class AngelRingNPC : GlobalNPC
	{
		internal static Asset<Texture2D> Ring = null;

		public override bool InstancePerEntity => true;

		private readonly float[] _ringFadeIn = new float[3];

		private int _ringCount = 0;
		private int _ringTime = 0;
		private int _lastPlayerWhoAmI = 0;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return !entity.friendly && !entity.townNPC;
		}

		public override void SetStaticDefaults()
		{
			Ring = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/AngelRing");
		}

		public override void PostAI(NPC npc)
		{
			if (_ringCount >= 3)
			{
				DoLightBeam(npc);
			}

			if (_ringTime-- < 0)
			{
				_ringCount = 0;
				_ringTime = 0;
			}
		}

		public void ApplyRing(int playerWho)
		{
			_ringCount++;
			_ringTime = 5 * 60;
			_lastPlayerWhoAmI = playerWho;
		}

		private void DoLightBeam(NPC npc)
		{
			_ringTime = 0;
			_ringCount = 0;

			if (Main.myPlayer == _lastPlayerWhoAmI)
			{
				int type = ModContent.ProjectileType<GuardianAngelRay>();
				Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, type, 30, 1f, _lastPlayerWhoAmI, npc.whoAmI);
			}
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			// Draw rings 

			for (int i = 0; i < 3; i++)
			{
				if (_ringCount > i)
				{
					_ringFadeIn[i] = MathHelper.Lerp(_ringFadeIn[i], 1f, 0.05f + i * 0.01f);
				}
				else
				{
					_ringFadeIn[i] = MathHelper.Lerp(_ringFadeIn[i], 0f, 0.1f + i * 0.02f);
				}

				// Skip drawing ring if too transparent

				if (_ringFadeIn[i] < 0.01f)
				{
					continue;
				}

				var color = Color.Lerp(Lighting.GetColor(npc.Center.ToTileCoordinates()), Color.White, 0.3f);
				float scale = (1f + i * 0.4f) * _ringFadeIn[i];
				float rotation = (Main.GameUpdateCount * 0.02f + i * MathHelper.PiOver2) * (i % 2 == 0 ? -1 : 1);

				spriteBatch.Draw(Ring.Value, npc.Center - screenPos, null, color * _ringFadeIn[i] * (0.4f + i * 0.3f), rotation, Ring.Size() / 2f, scale, SpriteEffects.None, 0);
			}
		}
	}
}
