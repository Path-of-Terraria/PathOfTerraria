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
	}

	public override void AI()
	{
		if (NpcType == 0)
		{
			NpcType = NPCID.Zombie;
		}

		Projectile.frame = (int)MathF.Floor(((float)Projectile.timeLeft + (Projectile.identity * 100f)) / 5f) % 8;
		Lighting.AddLight(Projectile.Center, Color.IndianRed.ToVector3() * 0.1f);
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
			Projectile.Kill();
			return true;
		}

		return false;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		SpriteFrame spriteFrame = new(1, (byte)Main.projFrames[Type]) { PaddingX = 0, PaddingY = 0 };
		Rectangle frame = spriteFrame.With(0, (byte)Projectile.frame).GetSourceRectangle(texture);
		Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, 0, 0f);

		return false;
	}
}
