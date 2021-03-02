using System.Linq;
using Divine;
using Divine.SDK.Extensions;

namespace Hammer
{
    public static class Helpers
    {
        public static bool CanHit(this Ability ability, params Unit[] targets)
        {
            return targets.All(x => x.Distance2D(ability.Owner) < ability.CastRange);
        }

        public static bool CanBeCastedOnTarget(this Ability ability, Unit target)
        {
            return ability.Cooldown != 0 && ability.CanHit(target) && ability.ManaCost <= ((Hero) ability.Owner).Mana;
        }

        public static float GetDamage(this Ability ability)
        {
            return ability.GetAbilitySpecialData("damage");
        }

        public static float GetCalculatedDamage(this Ability ability, params Unit[] targets)
        {
            var totalDamage = 0.0f;

            var damage = ability.GetDamage();
            // var amplify = (ability.Owner as Hero).GetSpellAmplification();
            var amplify = 0;
            foreach (var target in targets)
            {
                var reduction = ability.GetDamageReduction(target, ability.DamageType);
                totalDamage += DamageHelpers.GetSpellDamage(damage, amplify, reduction);
            }

            return totalDamage;
        }
    }
}