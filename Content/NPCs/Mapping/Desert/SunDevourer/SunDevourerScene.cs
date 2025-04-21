using PathOfTerraria.Core.Graphics.Filters;
using PathOfTerraria.Core.Graphics.Particles;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

public sealed class SunDevourerScene : ModSceneEffect
{
	/// <summary>
	///		Gets or sets the strength of the vignette effect.
	/// </summary>
	public float Strength { get; private set; }

	public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

	public override void SpecialVisuals(Player player, bool isActive)
	{
		base.SpecialVisuals(player, isActive);

		Strength = MathHelper.SmoothStep(Strength, isActive ? 1f : 0f, 0.25f);

		Effect shader = VignetteFilter.Vignette.Value;
		shader.Parameters["uOpacity"].SetValue(Strength);

		if (isActive)
		{
			shader.Parameters["strength"].SetValue(0.8f);
			shader.Parameters["curvature"].SetValue(0.5f);
			shader.Parameters["innerRadius"].SetValue(0.5f);
			shader.Parameters["outerRadius"].SetValue(1.2f);

			//Vector2 position = Main.screenPosition + new Vector2(Main.rand.Next(0, Main.screenWidth), Main.rand.Next(0, Main.screenHeight));

			//ParticleSystem.Create(new SunDevourerParticle(position));
		}

		//player.ManageSpecialBiomeVisuals($"{PoTMod.ModName}:Vignette", isActive);
		player.ManageSpecialBiomeVisuals(SunDevourerFilter.FILTER_NAME, isActive);
	}

	public override bool IsSceneEffectActive(Player player)
	{
		return NPC.AnyNPCs(ModContent.NPCType<SunDevourerNPC>());
	}
}