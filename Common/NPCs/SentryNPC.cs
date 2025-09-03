using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

public abstract class SentryNPC : ModNPC, ITargetable
{
	/// <summary> The degree in which this sentry draws other NPC attention. </summary>
	public int Aggro;
	/// <summary> The time remaining until this NPC automatically expires. </summary>
	public int TimeLeft;

	private bool _justSpawned;

	public int TimeLeftMax { get; private set; }

	/// <summary> The player that this sentry belongs to, indicated by <see cref="NPC.releaseOwner"/>. </summary>
	public Player Owner => Main.player[NPC.releaseOwner];

	/// <summary> Spawns the given sentry NPC at <paramref name="position"/> and assigns the owner accordingly. </summary>
	public static void Spawn<T>(Player owner, Vector2 position) where T : SentryNPC
	{
		int type = ModContent.NPCType<T>();
		NPC.ReleaseNPC((int)position.X, (int)position.Y, type, 0, owner.whoAmI);
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() { Hide = true });
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.value = 0;
		NPC.dontCountMe = true;
		NPC.friendly = true;
		NPC.lifeMax = 100;
		NPC.knockBackResist = 0;

		Aggro = 0;
	}

	/// <summary><inheritdoc cref="ModNPC.PreAI"/><para/>
	/// Additionally handles <see cref="TimeLeft"/> expiry logic. </summary>
	public override bool PreAI()
	{
		if (!_justSpawned)
		{
			InitializeTimeLeft(Owner.GetModPlayer<SentryNPCPlayer>().SentryDuration);
			_justSpawned = true;

			OnSpawn();
		}

		if (--TimeLeft <= 0 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			NPC.StrikeInstantKill();
		}

		return true;
	}

	/// <summary> Called on all sides when this NPC just spawns. </summary>
	public virtual void OnSpawn() { }

	public virtual Terraria.Utilities.NPCUtils.TargetSearchResults FindTarget()
	{
		Terraria.Utilities.NPCUtils.TargetSearchResults results = Terraria.Utilities.NPCUtils.SearchForTarget(NPC, Terraria.Utilities.NPCUtils.TargetSearchFlag.NPCs, npcFilter: static (npc) => npc.CanBeChasedBy());
		return results;
	}

	public virtual bool CanBeTargetedBy(NPC npc, out TargetLoader.FocusPoint focus)
	{
		const int maxDistance = 500;

		focus = new(NPC.Hitbox, Aggro);
		return npc.DistanceSQ(NPC.Center) < maxDistance * maxDistance;
	}

	public void InitializeTimeLeft(int value)
	{
		TimeLeftMax = TimeLeft = value;
	}
}

internal class SentryNPCPlayer : ModPlayer
{
	public const int DefaultSentryDuration = 60 * 5;

	public int SentrySlots;
	public int SentryDuration;

	public override void ResetEffects()
	{
		SentrySlots = 2;
		SentryDuration = DefaultSentryDuration;
	}

	public HashSet<SentryNPC> GetSentries()
	{
		HashSet<SentryNPC> value = [];
		foreach (NPC n in Main.ActiveNPCs)
		{
			if (n.ModNPC is SentryNPC sentry && sentry.Owner == Player)
			{
				value.Add(sentry);
			}
		}

		return value;
	}
}