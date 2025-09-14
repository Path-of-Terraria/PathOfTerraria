using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

public abstract class SentryNPC : ModNPC, ITargetable
{
	public const int DefaultSentryDuration = 60 * 5;

	/// <summary> The degree in which this sentry draws other NPC attention. </summary>
	public int Aggro;
	/// <summary> The time remaining until this NPC automatically expires. </summary>
	public int TimeLeft;

	private bool _justSpawned;

	public int TimeLeftMax { get; private set; }

	/// <summary> The player that this sentry belongs to, indicated by <see cref="NPC.releaseOwner"/>. </summary>
	public Player Owner => Main.player[NPC.releaseOwner];

	#region static helpers
	/// <inheritdoc cref="Spawn(int, Player, Vector2)"/>
	public static NPC Spawn<T>(Player owner, Vector2 position, int timeLeft = DefaultSentryDuration) where T : SentryNPC
	{
		return Spawn(ModContent.NPCType<T>(), owner, position, timeLeft);
	}

	/// <summary> Spawns the given sentry NPC at <paramref name="position"/> and assigns the owner accordingly. </summary>
	public static NPC Spawn(int type, Player owner, Vector2 position, int timeLeft = DefaultSentryDuration)
	{
		NPC npc = Main.npc[NPC.ReleaseNPC((int)position.X, (int)position.Y, type, 0, owner.whoAmI)];

		if (npc.ModNPC is SentryNPC s)
		{
			s.InitializeTimeLeft(timeLeft);
			npc.netUpdate = true;
		}

		return npc;
	}

	/// <summary> Finds a valid spawn position with custom checks. </summary>
	public static bool FindRestingSpot(Player owner, out Vector2 worldCoords, Vector2 offset = default)
	{
		const int maxDistance = 400;

		Vector2 coordinates = Main.MouseWorld;
		if (owner.DistanceSQ(Main.MouseWorld) > maxDistance * maxDistance) //Limit the maximum scan distance
		{
			coordinates = owner.Center + owner.DirectionTo(coordinates) * maxDistance;
		}

		Point tileCoords = coordinates.ToTileCoordinates();
		while (WorldGen.InWorld(tileCoords.X, tileCoords.Y, 20) && !WorldGen.SolidTile2(tileCoords.X, tileCoords.Y))
		{
			tileCoords.Y++;
		}

		worldCoords = tileCoords.ToWorldCoordinates() + offset;
		return !Collision.SolidCollision(new(worldCoords.X - 8, worldCoords.Y - 8), 16, 16);
	}

	/// <summary> Destroys the oldest <see cref="SentryNPC"/> owned by <paramref name="owner"/> over capacity. </summary>
	public static void TryDestroyOldest(Player owner)
	{
		SentryNPCPlayer mp = owner.GetModPlayer<SentryNPCPlayer>();
		HashSet<SentryNPC> sentries = mp.GetSentries();

		if (sentries.Count >= mp.SentrySlots && sentries.OrderBy(x => (float)x.TimeLeft / x.TimeLeftMax).FirstOrDefault() is SentryNPC item && item != default)
		{
			item.NPC.StrikeInstantKill();
		}
	}
	#endregion

	public override void SetStaticDefaults()
	{
		NPCID.Sets.UsesNewTargetting[Type] = true;
		NPCID.Sets.NeverDropsResourcePickups[Type] = true;
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
		InitializeTimeLeft(DefaultSentryDuration);
	}

	/// <summary><inheritdoc cref="ModNPC.PreAI"/><para/>
	/// Additionally handles spawn, immunity and <see cref="TimeLeft"/> expiry logic. </summary>
	public override bool PreAI()
	{
		if (!_justSpawned)
		{
			_justSpawned = true;
			OnSyncedSpawn();
		}

		if (--TimeLeft <= 0 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			NPC.StrikeInstantKill();
		}

		NPC.immune[255] = 0; //Grant no immunity frames from non-player damage
		return true;
	}

	/// <summary> Called on all sides when this NPC just spawns. </summary>
	public virtual void OnSyncedSpawn() { }

	/// <summary><inheritdoc cref="ModNPC.SendExtraAI(BinaryWriter)"/><para/>
	/// Base implementation is used to send additional data. </summary>
	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write((ushort)TimeLeft);
		writer.Write((ushort)TimeLeftMax);
		writer.Write((ushort)NPC.damage);
	}

	/// <summary><inheritdoc cref="ModNPC.ReceiveExtraAI(BinaryReader)"/><para/>
	/// Base implementation is used to receive additional data. </summary>
	public override void ReceiveExtraAI(BinaryReader reader)
	{
		TimeLeft = reader.ReadUInt16();
		TimeLeftMax = reader.ReadUInt16();
		NPC.damage = reader.ReadUInt16();
	}

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
	public int SentrySlots;

	public override void ResetEffects()
	{
		SentrySlots = 2;
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