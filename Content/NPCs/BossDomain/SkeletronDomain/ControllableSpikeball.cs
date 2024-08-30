using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.BrainDomain;

public sealed class ControllableSpikeball : ModNPC
{
	public override string Texture => $"Terraria/Images/NPC_" + NPCID.SpikeBall;

	private ref float Speed => ref NPC.ai[0];

	private Vector2 Anchor
	{
		get => new(NPC.ai[1], NPC.ai[2]);

		set 
		{
			NPC.ai[1] = value.X;
			NPC.ai[2] = value.Y;
		}
	}

	private ref float Length => ref NPC.ai[3];

	private bool IsInitialized
	{
		get => NPC.localAI[0] == 1f;
		set => NPC.localAI[0] = value ? 1f : 0f;
	}

	private ref float Timer => ref NPC.localAI[1];

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.SpikeBall);
		NPC.aiStyle = -1;
	}

	public override void AI()
	{
		if (!IsInitialized)
		{
			Anchor = NPC.Center;
			
			if (Length == 0f)
			{
				Length = Main.rand.NextFloat(80, 120f);
			}

			NPC.netUpdate = true;
			IsInitialized = true;
		}

		Timer++;
		Vector2 offset = new Vector2(Length, 0).RotatedBy(Timer * Speed);
		NPC.rotation = offset.ToRotation();
		NPC.Center = Anchor + offset;
	}

	public override bool CheckActive()
	{
		return false;
	}
}