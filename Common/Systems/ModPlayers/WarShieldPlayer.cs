using PathOfTerraria.Content.Items.Gear.Weapons.WarShields;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class WarShieldPlayer : ModPlayer
{
	public bool CanBash => _bashCooldown <= 0;
	public bool Bashing => _bashTime > 0;

	public Vector2 StoredVelocity = Vector2.Zero;

	private int _bashCooldown = 0;
	private int _bashTime = 0;

	public override void PreUpdateMovement()
	{
		_bashCooldown--;
		_bashTime--;

		if (!Bashing)
		{
			return;
		}

		var shield = Player.HeldItem.ModItem as WarShield;
		Player.SetDummyItemTime(2);
		Player.SetImmuneTimeForAllTypes(2);
		Player.velocity = StoredVelocity;
		Player.direction = Math.Sign(StoredVelocity.X);

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.Hitbox.Intersects(Player.Hitbox) && !npc.friendly && !npc.townNPC)
			{
				bool isCrit = Main.rand.NextFloat() < (Player.HeldItem.crit + 4) * 0.01f;

				if (NPCLoader.CanBeHitByItem(npc, Player, Player.HeldItem) is null or true)
				{
					int dir = Math.Sign(StoredVelocity.X);
					NPC.HitModifiers modifiers = npc.GetIncomingStrikeModifiers(Player.HeldItem.DamageType, dir);

					CombinedHooks.ModifyPlayerHitNPCWithItem(Player, Player.HeldItem, npc, ref modifiers);
					var strike = modifiers.ToHitInfo(Player.HeldItem.damage, isCrit, 6f, damageVariation: true, Player.luck);
					npc.StrikeNPC(strike);
					CombinedHooks.OnPlayerHitNPCWithItem(Player, Player.HeldItem, npc, in strike, strike.Damage);
				}

				for (int i = 0; i < 5; ++i)
				{
					Vector2 vel = -StoredVelocity.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.6f, 0.9f);
					Dust.NewDust(Vector2.Lerp(npc.Center, Player.Center, 0.5f), 1, 1, shield.Data.BashDust, vel.X, vel.Y);
				}

				_bashTime = 0;
				Player.velocity = -StoredVelocity * 0.9f;
				break;
			}
		}
	}

	public override void PostUpdateRunSpeeds()
	{
		if (Player.HeldItem.ModItem is WarShield shield)
		{
			Player.gravity *= 1.2f;
			Player.maxFallSpeed *= 1.2f;
		}
	}

	public override void FrameEffects()
	{
		if (Player.HeldItem.ModItem is WarShield shield)
		{
			Player.shield = EquipLoader.GetEquipSlot(Mod, shield.GetType().Name + "_Shield", EquipType.Shield);
		}
	}

	public void StartBash(int bashTime, int bashCooldown, float velocityMagnitude)
	{
		StartBash(bashTime, bashCooldown, Player.DirectionTo(Main.MouseWorld) * velocityMagnitude);
	}

	public void StartBash(int bashTime, int bashCooldown, Vector2 velocity)
	{
		_bashTime = bashTime;
		_bashCooldown = bashCooldown;

		if (_bashTime > _bashCooldown)
		{
			_bashCooldown = _bashTime;
		}

		if (Main.myPlayer == Player.whoAmI)
		{
			StoredVelocity = velocity;

			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 4; ++i)
				{
					Gore.NewGore(Player.GetSource_FromThis(), Player.Center, -velocity.RotatedByRandom(0.2f) * 0.2f, GoreID.Smoke1 + Main.rand.Next(3));
				}
			}
		}
	}
}
