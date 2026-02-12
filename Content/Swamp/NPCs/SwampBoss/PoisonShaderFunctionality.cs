using ReLogic.Content;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

internal class PoisonShaderFunctionality : ModSystem
{
	private const string EffectKey = "PathOfTerraria:Poison";

	internal bool PoisonActive = false;

	internal static float Intensity { get; private set; }

	public override void Load()
	{
		Asset<Effect> filterShader = Mod.Assets.Request<Effect>("Assets/Effects/PurpleSmog");
		Filters.Scene[EffectKey] = new Filter(new ScreenShaderData(filterShader, "Pass0"), EffectPriority.VeryHigh);
	}

	public override void PreUpdateEntities()
	{
		if (Main.mouseRight && Main.mouseMiddle && Main.mouseMiddleRelease)
		{
			PoisonActive = true;
		}

		if (Main.netMode != NetmodeID.Server)// && SubworldSystem.Current is SwampArea) // This all needs to happen client-side!
		{
			Intensity = MathHelper.Lerp(Intensity, PoisonActive ? 0.4f : 0, 0.1f);

			if (Intensity > 0 && !Filters.Scene[EffectKey].Active)
			{
				Filters.Scene.Activate(EffectKey);
			}
			else if (Intensity <= 0.01f && Filters.Scene[EffectKey].Active)
			{
				Intensity = 0;
				Filters.Scene[EffectKey].Deactivate();
			}

			if (Filters.Scene[EffectKey].Active)
			{
				Filters.Scene[EffectKey].GetShader().UseProgress(Main.GameUpdateCount * 0.002f);
				Vector2 direction = Main.screenPosition / Main.ScreenSize.ToVector2();
				direction.X %= 1;
				direction.Y %= 1;
				Filters.Scene[EffectKey].GetShader().UseDirection(direction);
				Filters.Scene[EffectKey].GetShader().UseImage(ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/PerlinNoise"));
				Filters.Scene[EffectKey].GetShader().UseIntensity(Intensity);

				int mothers = 0;
				Main.NewText((Main.MouseWorld - Main.screenPosition) / Main.ScreenSize.ToVector2());
				Main.NewText(((Main.MouseWorld - Main.screenPosition) / Main.ScreenSize.ToVector2()).Distance(new Vector2(0.5f)));

				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.ModNPC is Mossmother)
					{
						Vector2 scale = (Main.MouseWorld - Main.screenPosition) / Main.ScreenSize.ToVector2();// (npc.Center - Main.screenPosition) / Main.ScreenSize.ToVector2();
						Filters.Scene[EffectKey].GetShader().UseImageScale(scale, mothers);
						
						if (mothers++ >= 3)
						{
							break;
						}
					}
				}
			}
		}
	}
}
