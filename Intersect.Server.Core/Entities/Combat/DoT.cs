using Intersect.Enums;
using Intersect.Framework.Core;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Server.Entities.Combat;


public partial class DoT
{
    public Guid Id = Guid.NewGuid();

    public Entity Attacker;

    public int Count;

    private long mInterval;

    public SpellBase SpellBase;

    public DoT(Entity attacker, Guid spellId, Entity target)
    {
        SpellBase = SpellBase.Get(spellId);

        Attacker = attacker;
        Target = target;

        if (SpellBase == null || SpellBase.Combat.HotDotInterval < 1)
        {
            return;
        }

        // Does target have a cleanse buff? If so, do not allow this DoT when spell is unfriendly.
        if (!SpellBase.Combat.Friendly)
        {
            foreach (var status in Target.CachedStatuses)
            {
                if (status.Type == SpellEffect.Cleanse)
                {
                    return;
                }
            }
        }
        

        mInterval = Timing.Global.Milliseconds + SpellBase.Combat.HotDotInterval;
        Count = (SpellBase.Combat.Duration + SpellBase.Combat.HotDotInterval - 1) / SpellBase.Combat.HotDotInterval;
        target.DoT.TryAdd(Id, this);
        target.CachedDots = target.DoT.Values.ToArray();

        //Subtract 1 since the first tick always occurs when the spell is cast.
    }

    public Entity Target { get; }

    public void Expire()
    {
        if (Target != null)
        {
            Target.DoT?.TryRemove(Id, out DoT val);
            Target.CachedDots = Target.DoT?.Values.ToArray() ?? new DoT[0];
        }
    }

    public bool CheckExpired()
    {
        if (Target != null && !Target.DoT.ContainsKey(Id))
        {
            return false;
        }

        if (SpellBase == null || Count > 0)
        {
            return false;
        }

        Expire();

        return true;
    }

    public void Tick()
    {
        if (CheckExpired())
        {
            return;
        }

        if (mInterval > Timing.Global.Milliseconds)
        {
            return;
        }

        var deadAnimations = new List<KeyValuePair<Guid, Direction>>();
        var aliveAnimations = new List<KeyValuePair<Guid, Direction>>();
        if (SpellBase.TickAnimationId != Guid.Empty)
        {
            var animation = new KeyValuePair<Guid, Direction>(SpellBase.TickAnimationId, Direction.Up);
            deadAnimations.Add(animation);
            aliveAnimations.Add(animation);
        }
        else if (SpellBase.HitAnimationId != Guid.Empty)
        {
            var animation = new KeyValuePair<Guid, Direction>(SpellBase.HitAnimationId, Direction.Up);
            deadAnimations.Add(animation);
            aliveAnimations.Add(animation);
        }

        var damageHealth = SpellBase.Combat.VitalDiff[(int)Vital.Health];
        var damageMana = SpellBase.Combat.VitalDiff[(int)Vital.Mana];

        Attacker?.Attack(
            Target, damageHealth, damageMana,
            (DamageType)SpellBase.Combat.DamageType, (Enums.Stat)SpellBase.Combat.ScalingStat,
            SpellBase.Combat.Scaling, SpellBase.Combat.CritChance, SpellBase.Combat.CritMultiplier, deadAnimations,
            aliveAnimations, false
        );

        mInterval = Timing.Global.Milliseconds + SpellBase.Combat.HotDotInterval;
        Count--;
    }

}
