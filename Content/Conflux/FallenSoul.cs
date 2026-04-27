using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal sealed class FallenSoul : ModProjectile
{
	public int NpcType
	{
		get => (int)Projectile.ai[0];
		set => Projectile.ai[0] = value;
	}
	
	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 8;
	}
	public override void SetDefaults()
	{
		Projectile.hostile = false;
		Projectile.friendly = false;
		Projectile.damage = 0;
		Projectile.Size = new(16);
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 60 * 30;
		Projectile.netImportant = true;
	}

	public override void AI()
	{
		if (NpcType == 0)
		{
			NpcType = NPCID.Zombie;
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (!Main.dedServ)
		{
			for (int i = 0; i < 10; i++) { Dust.NewDustPerfect(Projectile.Center, DustID.SomethingRed, Main.rand.NextVector2Circular(10f, 10f)); }
		}
	}

	public bool Respawn()
	{
		if (NpcType <= 0) { return false; }
		if (Main.netMode == NetmodeID.MultiplayerClient) { return false; }

		var npc = NPC.NewNPCDirect(Projectile.GetSource_FromThis(), Projectile.Center, NpcType);
		if (npc.whoAmI >= 0 && npc.whoAmI < Main.maxNPCs)
		{
			// Prevent loot experience gain.
			npc.SpawnedFromStatue = true;
			Projectile.Kill();
			return true;
		}

		return false;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		const int numFrames = 8;
		Projectile.frame = (int)MathF.Floor(((float)Main.timeForVisualEffects + (Projectile.identity * 100f)) / 5f) % numFrames;
		Lighting.AddLight(Projectile.Center, Color.IndianRed.ToVector3() * 0.3f);
		Projectile.gfxOffY = MathF.Sin((float)Main.timeForVisualEffects / 20f) * 2f - 1f;

		Texture2D texture = TextureAssets.Projectile[Type].Value;
		SpriteFrame spriteFrame = new(1, (byte)Main.projFrames[Type]) { PaddingX = 0, PaddingY = 0 };
		Rectangle frame = spriteFrame.With(0, (byte)Projectile.frame).GetSourceRectangle(texture);
		Vector2 pos = Projectile.Center + new Vector2(0f, Projectile.gfxOffY) - Main.screenPosition;
		Main.EntitySpriteDraw(texture, pos, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, 0, 0f);

		return false;
	}
}
