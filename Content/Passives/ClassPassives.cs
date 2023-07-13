using FunnyExperience.Core.Systems.TreeSystem;
using System.Collections.Generic;

namespace FunnyExperience.Content.Passives
{
	internal class MeleePassive : Passive
	{
		public MeleePassive() : base()
		{
			name = "Martial Mastery";
			tooltip = "Increases your melee damage by 5% per level";
			maxLevel = 5;
			treePos = new Vector2(250, 300);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Melee) += 0.05f * level;
		}

		public override void Connect(List<Passive> all, Player player)
		{
			Connect<CloseRangePassive>(all, player);
			Connect<BleedPassive>(all, player);
		}
	}

	internal class RangedPassive : Passive
	{
		public RangedPassive() : base()
		{
			name = "Marksmanship Mastery";
			tooltip = "Increases your ranged damage by 5% per level";
			maxLevel = 5;
			treePos = new Vector2(350, 250);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Ranged) += 0.05f * level;
		}

		public override void Connect(List<Passive> all, Player player)
		{
			Connect<LongRangePassive>(all, player);
			Connect<AmmoPassive>(all, player);
		}
	}

	internal class MagicPassive : Passive
	{
		public MagicPassive() : base()
		{
			name = "Arcane Mastery";
			tooltip = "Increases your magic damage by 5% per level";
			maxLevel = 5;
			treePos = new Vector2(450, 250);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Magic) += 0.05f * level;
		}

		public override void Connect(List<Passive> all, Player player)
		{
			Connect<LongRangePassive>(all, player);
			Connect<ManaPassive>(all, player);
		}
	}

	internal class SummonPassive : Passive
	{
		public SummonPassive() : base()
		{
			name = "Summoning Mastery";
			tooltip = "Increases your summon damage by 5% per level";
			maxLevel = 5;
			treePos = new Vector2(550, 300);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Summon) += 0.05f * level;
		}

		public override void Connect(List<Passive> all, Player player)
		{
			Connect<MinionPassive>(all, player);
			Connect<SentryPassive>(all, player);
		}
	}
}
