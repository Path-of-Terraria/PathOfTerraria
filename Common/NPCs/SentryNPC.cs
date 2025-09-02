using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

public abstract class SentryNPC : ModNPC, ITargetable
{
	public int Aggro;

	/// <summary> The player that this sentry belongs to, indicated by <see cref="NPC.releaseOwner"/>. </summary>
	public Player Owner => Main.player[NPC.releaseOwner];

	/// <summary> Spawns the given sentry NPC at <paramref name="position"/> and assigns the owner accordingly. </summary>
	public static void Spawn<T>(Player owner, Vector2 position) where T : SentryNPC
	{
		int type = ModContent.NPCType<T>();
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModContent.GetInstance<SpawnNPCOnServerHandler>().Send((short)type, position); //releaseOwner must be set in this context
		}
		else
		{
			var npc = NPC.NewNPCDirect(Terraria.Entity.GetSource_NaturalSpawn(), position, type);
			npc.releaseOwner = (short)owner.whoAmI;
			npc.netUpdate = true;
		}
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 100;
		Aggro = 0;
	}

	public virtual Terraria.Utilities.NPCUtils.TargetSearchResults FindTarget()
	{
		Terraria.Utilities.NPCUtils.TargetSearchResults results = Terraria.Utilities.NPCUtils.SearchForTarget(NPC, Terraria.Utilities.NPCUtils.TargetSearchFlag.NPCs, npcFilter: static (npc) => npc.CanBeChasedBy());
		return results;
	}

	public virtual bool CanBeTargetedBy(NPC npc, out TargetLoader.FocusPoint focus)
	{
		focus = new(NPC.Hitbox, Aggro);
		return true;
	}
}