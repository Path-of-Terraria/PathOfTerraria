using System.Collections.Generic;

namespace FunnyExperience.Core.Systems.TreeSystem.Passives
{
	internal class MeleePassive : Passive
	{
		public MeleePassive() : base()
		{
			name = "Martial Mastery";
			tooltip = "Increases your melee damage by 20% per level";
			maxLevel = 5;
			treePos = new Vector2(250, 300);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Melee) += 0.2f;
		}

		public override void Connect(List<Passive> all)
		{
			Connect<CloseRangePassive>(all);
			Connect<BleedPassive>(all);
		}
	}

	internal class RangedPassive : Passive
	{
		public RangedPassive() : base()
		{
			name = "Marksmanship Mastery";
			tooltip = "Increases your ranged damage by 20% per level";
			maxLevel = 5;
			treePos = new Vector2(350, 250);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Ranged) += 0.2f;
		}

		public override void Connect(List<Passive> all)
		{
			Connect<LongRangePassive>(all);
			Connect<AmmoPassive>(all);
		}
	}

	internal class MagicPassive : Passive
	{
		public MagicPassive() : base()
		{
			name = "Arcane Mastery";
			tooltip = "Increases your magic damage by 20% per level";
			maxLevel = 5;
			treePos = new Vector2(450, 250);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Magic) += 0.2f;
		}

		public override void Connect(List<Passive> all)
		{
			Connect<LongRangePassive>(all);
			Connect<ManaPassive>(all);
		}
	}

	internal class SummonPassive : Passive
	{
		public SummonPassive() : base()
		{
			name = "Summoning Mastery";
			tooltip = "Increases your summon damage by 20% per level";
			maxLevel = 5;
			treePos = new Vector2(550, 300);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Summon) += 0.2f;
		}

		public override void Connect(List<Passive> all)
		{
			Connect<MinionPassive>(all);
			Connect<SentryPassive>(all);
		}
	}
}
