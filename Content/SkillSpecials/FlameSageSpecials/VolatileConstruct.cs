using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Skills.Summon;

namespace PathOfTerraria.Content.SkillSpecials.FlameSageSpecials;

public class VolatileConstruct(SkillTree tree) : SkillSpecial(tree)
{
	public class VolatileSentry : FlameSage.FlameSentry
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		public override void AI()
		{
			NPC.Opacity = Math.Min(NPC.Opacity + 0.08f, 1);
		}
	}
}