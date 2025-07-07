using MonoMod.Cil;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;
using PathOfTerraria.Common.World.Generation;
using ReLogic.Content;
using SubworldLibrary;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonlordBackgroundDrawing : ModSystem
{
	private const string TexPath = "PathOfTerraria/Assets/Backgrounds/";

	public readonly record struct Planet(Point16 Position, MoonlordPlanetGen.PlanetType Type, float Scale, float Rotation, byte Frame);

	private static readonly List<Planet> Planets = [];
	private static readonly List<Star> Stars = [];
	private static readonly Asset<Texture2D>[] PlanetTextures = [ModContent.Request<Texture2D>(TexPath + "VortexPlanet"), 
		ModContent.Request<Texture2D>(TexPath + "StardustPlanet"), ModContent.Request<Texture2D>(TexPath + "SolarPlanet"), 
		ModContent.Request<Texture2D>(TexPath + "NebulaPlanet")];

	private static float? DrawingSpecialStars = null;

	public override void Load()
	{
		On_Main.DrawStarsInBackground += BgStarDrawingHook;
		IL_Main.DrawStar += ModifyDrawColor;
	}

	private void ModifyDrawColor(ILContext il)
	{
		ILCursor c = new(il);
		MethodInfo drawInfo = typeof(SpriteBatch).GetMethod(nameof(SpriteBatch.Draw), BindingFlags.Public | BindingFlags.Instance, [typeof(Texture2D), typeof(Vector2),
			typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float)]);

		if (!c.TryGotoNext(x => x.MatchCallvirt(drawInfo)))
		{
			return;
		}

		if (!c.TryGotoNext(x => x.MatchCallvirt(drawInfo)))
		{
			return;
		}

		if (!c.TryGotoPrev(MoveType.After, x => x.MatchLdloc1()))
		{
			return;
		}

		c.EmitDelegate((Color color) =>
		{
			if (DrawingSpecialStars is null)
			{
				return color;
			}

			return color * DrawingSpecialStars.Value;
		});
	}

	public static void ReloadPlanets()
	{
		Planets.Clear();
		Stars.Clear();

		for (int i = 0; i < 80; ++i)
		{
			Point16 pos = new(Main.rand.Next(1921), Main.rand.Next(1201));
			var type = (MoonlordPlanetGen.PlanetType)Main.rand.Next(4);
			Planets.Add(new Planet(pos, type, Main.rand.NextFloat(0.2f, 2f), Main.rand.NextFloat(MathHelper.TwoPi), (byte)Main.rand.Next(4)));
		}

		FastNoiseLite starNoise = new(Main.rand.Next());
		starNoise.SetFrequency(0.002f);

		for (int i = 0; i < 1200; ++i)
		{
			Vector2 pos = new(Main.rand.Next(1921), Main.rand.Next(1201));
			Stars.Add(new Star() { falling = false, position = pos, scale = 2 * Math.Abs(starNoise.GetNoise(pos.X, pos.Y)) });
		}
	}

	private void BgStarDrawingHook(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
	{
		orig(self, sceneArea, artificial);

		if (SubworldSystem.Current is MoonLordDomain)
		{
			float opacity;
			float y = Main.LocalPlayer.Center.Y / 16;   
			
			if (y < MoonLordDomain.PlanetTop - 200)
			{
				DrawingSpecialStars = 1 - MoonDomainSystem.EffectStrength;
				opacity = DrawingSpecialStars.Value;

				if (MoonDomainSystem.EffectStrength > 0.99f)
				{
					return;
				}
			}
			else if (y < MoonLordDomain.PlanetTop)
			{
				opacity = MathHelper.Lerp(Utils.GetLerpValue(MoonLordDomain.PlanetTop - 200, MoonLordDomain.PlanetTop, y, true), 1, 1 - MoonDomainSystem.EffectStrength);
				DrawingSpecialStars = opacity;
			}
			else if (y < MoonLordDomain.CloudTop)
			{
				opacity = Utils.GetLerpValue(MoonLordDomain.CloudTop, MoonLordDomain.CloudTop - 150, y, true);
			}
			else
			{
				return;
			}

			foreach (Planet planet in Planets) 
			{
				Vector2 position = planet.Position.ToVector2() / 1200f * new Vector2(sceneArea.totalWidth, sceneArea.totalHeight) 
					+ new Vector2(0f, sceneArea.bgTopY) + sceneArea.SceneLocalScreenPositionOffset;
				Texture2D tex = PlanetTextures[(int)planet.Type].Value;
				var color = Color.Lerp(Color.White, Color.DarkGray, Utils.GetLerpValue(2f, 0.4f, planet.Scale, true));
				Rectangle src = new(0, 17 * planet.Frame, 16, 17);

				Main.spriteBatch.Draw(tex, position, src, color * opacity, planet.Rotation, src.Size() / 2f, planet.Scale, SpriteEffects.None, 0);
			}

			DrawingSpecialStars = opacity;

			for (int i = 0; i < Stars.Count; i++)
			{
				Star star = Stars[i];
				star.Update();
				CallDrawStar(Main.instance, ref sceneArea, 1f, Main.ColorOfTheSkies * opacity, i, star, artificial);
			}

			DrawingSpecialStars = null;

			if (y < MoonLordDomain.PlanetTop)
			{
				opacity = MathHelper.Lerp(Utils.GetLerpValue(MoonLordDomain.PlanetTop - 200, MoonLordDomain.PlanetTop, y, true), 1, 1 - MoonDomainSystem.EffectStrength);
				DrawingSpecialStars = opacity;
			}
		}
		else
		{
			DrawingSpecialStars = null;
		}
	}
	
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawStar")]
	private static extern void CallDrawStar(Main instance, ref Main.SceneArea sceneArea, float starOpacity, Color bgColorForStars, int i, Star theStar, bool artificial,
		bool foreground = false);
}
