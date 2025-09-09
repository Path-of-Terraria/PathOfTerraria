using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class FreezeDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		FreezeNPC freeze = npc.GetGlobalNPC<FreezeNPC>();

		// Just frozen effects
		if (!freeze.LastFrozen && !Main.dedServ)
		{
			SpawnEffects(npc, buffIndex);
		}

		// "Break out" effects
		if (npc.buffTime[buffIndex] == 2)
		{
			SpawnEffects(npc, buffIndex);
		}

		freeze.Frozen = true;
	}

	private void SpawnEffects(NPC npc, int buffIndex)
	{
		for (int i = 0; i < 10; ++i)
		{
			Dust.NewDust(npc.position, npc.width, npc.height, DustID.Ice);
		}

		for (int i = 0; i < 8; ++i)
		{
			Vector2 vel = new Vector2(Main.rand.NextFloat(1, 5), 0).RotatedByRandom(MathHelper.TwoPi);
			Gore.NewGore(new EntitySource_Buff(npc, Type, buffIndex), npc.Center, vel, ModContent.GoreType<ColdAir>(), Main.rand.NextFloat(0.8f, 1.5f));
		}
	}
}

internal class FreezeNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public bool Frozen = false;
	public bool LastFrozen = false;
	
	private double lastFrame = -1;

	public override void Load()
	{
		On_NPC.StrikeNPC_HitInfo_bool_bool += HijackSoundEffect;
		On_NPC.checkDead += HijackDeathEffect;
	}

	// Replace the death sound effect with the shatter sound
	private void HijackDeathEffect(On_NPC.orig_checkDead orig, NPC self)
	{
		SoundStyle? hitSound = self.DeathSound;

		if (self.GetGlobalNPC<FreezeNPC>().Frozen)
		{
			self.DeathSound = SoundID.Item27;
		}

		orig(self);

		// I don't think I need to reset this but I will if only for posterity
		self.DeathSound = hitSound;
	}

	// Replace the hit sound effect with the shatter sound
	private static int HijackSoundEffect(On_NPC.orig_StrikeNPC_HitInfo_bool_bool orig, NPC self, NPC.HitInfo hit, bool fromNet, bool noPlayerInteraction)
	{
		SoundStyle? hitSound = self.HitSound;

		if (self.GetGlobalNPC<FreezeNPC>().Frozen)
		{
			self.HitSound = SoundID.Item27;
		}

		int result = orig(self, hit, fromNet, noPlayerInteraction);
		self.HitSound = hitSound;
		return result;
	}

	public override void ResetEffects(NPC npc)
	{
		LastFrozen = Frozen;
		Frozen = false;
	}

	public override void HitEffect(NPC npc, NPC.HitInfo hit)
	{
		if (Frozen)
		{
			int count = npc.life <= 0 ? 12 : 3;

			for (int i = 0; i < count; ++i)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Ice);
			}
		}
	}

	public override bool PreAI(NPC npc)
	{
		if (Frozen)
		{
			// LastFrame stores npc.frameCounter, 'freezing' it
			if (lastFrame != -1)
			{
				npc.frameCounter = lastFrame;
			}

			lastFrame = npc.frameCounter;
			npc.position -= npc.velocity;

			if (Main.rand.NextBool(420))
			{
				Vector2 vel = new Vector2(Main.rand.NextFloat(1, 5), 0).RotatedByRandom(MathHelper.TwoPi);
				Gore.NewGore(npc.GetSource_FromAI(), npc.Center, vel, ModContent.GoreType<ColdAir>(), Main.rand.NextFloat(0.8f, 1.5f));
			}

			return false;
		}

		lastFrame = -1;
		return true;
	}

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (Frozen && !FrozenNPCBatching.Drawing)
		{
			FrozenNPCBatching.CachedNPCs.Enqueue(npc.whoAmI);
			return false;
		}

		return Frozen == FrozenNPCBatching.Drawing;
	}
}

public class ColdAir : ModGore
{
	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		gore.Frame = new SpriteFrame(1, 2, 0, (byte)Main.rand.Next(2));
		gore.alpha = Main.rand.Next(50, 155);
	}

	public override bool Update(Gore gore)
	{
		gore.velocity *= 0.95f;
		gore.position += gore.velocity;
		gore.alpha++;
		gore.rotation += gore.velocity.X / 7f;

		if (gore.alpha >= 255)
		{
			gore.active = false;
		}

		return false;
	}
}