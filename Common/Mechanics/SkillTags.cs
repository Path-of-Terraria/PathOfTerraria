namespace PathOfTerraria.Common.Mechanics;

[Flags]
public enum SkillTags : ulong
{
	None            = 0,
	Fire            = 1 << 0,
	Cold            = 1 << 1,
	Lightning       = 1 << 2,
	Chaos           = 1 << 3,
	Melee           = 1 << 4,
	Ranged          = 1 << 5,
	Magic           = 1 << 6,
	Summon          = 1 << 7,
	Projectile      = 1 << 8,
	Channeling      = 1 << 9,
	AreaOfEffect    = 1 << 10,
	Sentry          = 1 << 11,
	Minion          = 1 << 12,
	Debuff          = 1 << 13,
	Buff            = 1 << 14,
	Movement        = 1 << 15,
	Critical        = 1 << 16,
	Healing         = 1 << 17,

	Elemental       = Fire | Cold | Lightning,
}
