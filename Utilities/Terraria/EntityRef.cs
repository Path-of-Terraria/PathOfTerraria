using System.Diagnostics;
using System.Runtime.InteropServices;

#nullable enable

namespace PathOfTerraria.Utilities.Terraria;

/// <summary> Provides a reference to a world instance (slot) of a Player, NPC, Projectile, or Item. </summary>
[StructLayout(LayoutKind.Sequential, Size = 8)]
internal struct EntityRef
{
    public enum EntityType : ushort { None, Player, NPC, Projectile, Item }

    //TODO: Implement an actual generation counter?
    public ushort Index;
    public EntityType Kind;
    public int Identifier;

    public readonly bool IsValid => Entity != null;
    public readonly Player? Player => Kind == EntityType.Player && Main.player[Index] is { active: true } e && e.name.GetHashCode() == Identifier ? e : null;
    public readonly NPC? NPC => Kind == EntityType.NPC && Main.npc[Index] is { active: true } e && e.type == Identifier ? e : null;
    public readonly Projectile? Projectile => Kind == EntityType.Projectile && Main.projectile[Index] is { active: true } e && e.identity == Identifier ? e : null;
    public readonly Item? Item => Kind == EntityType.Item && Main.item[Index] is { active: true } e && e.type == Identifier ? e : null;
    public readonly Entity? Entity => Kind switch
	{
		EntityType.None => null,
		EntityType.Player => Player,
		EntityType.NPC => NPC,
		EntityType.Projectile => Projectile,
		EntityType.Item => Item,
		_ => null,
	};

#if DEBUG
    static EntityRef()
    {
        Debug.Assert(!default(EntityRef).IsValid);
    }
#endif

    public static explicit operator EntityRef(Player player)
    {
        return new() { Kind = EntityType.Player, Index = (ushort)player.whoAmI, Identifier = player.name.GetHashCode() };
    }
    public static explicit operator EntityRef(NPC npc)
    {
        return new() { Kind = EntityType.NPC, Index = (ushort)npc.whoAmI, Identifier = npc.type };
    }
    public static explicit operator EntityRef(Projectile proj)
    {
        return new() { Kind = EntityType.Projectile, Index = (ushort)proj.whoAmI, Identifier = proj.identity };
    }
    public static explicit operator EntityRef(Item item)
    {
        int index = Array.IndexOf(Main.item, item);
        if (index < 0) { throw new InvalidOperationException("Cannot create a reference to a non-instanced item."); }
        return new() { Kind = EntityType.Item, Index = (ushort)index, Identifier = item.type };
    }
}