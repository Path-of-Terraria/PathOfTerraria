using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria.GameContent;

namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// This IL edit stops an esoteric error from happening, where <see cref="PlayerContainerNPC"/>s throw an exception when the npc housing page is open.
/// That error occurs even though player container NPCs don't have housing, and even though the banners wouldn't be visible anyway. Weird!
/// </summary>
internal class HouseBannerHook : ILoadable
{
	public void Load(Mod mod)
	{
		IL_Main.DrawNPCHousesInWorld += IL_Main_DrawNPCHousesInWorld;
	}

	private void IL_Main_DrawNPCHousesInWorld(ILContext il)
	{
		ILCursor c = new(il);

		for (int i = 0; i < 2; ++i)
		{
			if (!c.TryGotoNext(x => x.MatchLdsfld<Main>(nameof(Main.spriteBatch))))
			{
				return;
			}
		}

		MethodInfo method = typeof(SpriteBatch).GetMethod
		(
			nameof(SpriteBatch.Draw),
			BindingFlags.Public | BindingFlags.Instance,
			[typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float)]
		);

		if (!c.TryGotoNext(MoveType.After, x => x.MatchCallvirt(method)))
		{
			return;
		}

		ILLabel label = c.MarkLabel();

		if (!c.TryGotoPrev(x => x.MatchCall<TownNPCProfiles>(nameof(TownNPCProfiles.GetHeadIndexSafe))))
		{
			return;
		}

		c.Index--;

		c.Emit(OpCodes.Ldloc_S, (byte)3);
		c.Emit(OpCodes.Ldloc_S, (byte)14);
		c.Emit(OpCodes.Ldloc_S, (byte)7);
		c.Emit(OpCodes.Ldloc_S, (byte)19);
		c.Emit(OpCodes.Ldloca_S, (byte)18);
		c.EmitDelegate(PreDrawNPCHeadIcon);
		c.Emit(OpCodes.Brfalse, label);
	}

	public static bool PreDrawNPCHeadIcon(NPC npc, float baseY, int homeTileY, float drawScale, ref int headIndex)
	{
		const int XOffset = 8;

		int yOffset = 18;

		if (Main.tile[npc.homeTileX, homeTileY].TileType == 19)
		{
			yOffset -= 8;
		}

		Vector2 position = new Vector2(npc.homeTileX * 16 + XOffset, baseY + yOffset + 2f) - Main.screenPosition;
		Color color = Lighting.GetColor(npc.homeTileX, homeTileY);

		if (npc.ModNPC is PlayerContainerNPC container) // Stops player container NPCs from having their head drawn at all
		{
			return false;
		}

		return true;
	}

	public void Unload()
	{
	}
}
