using System.Linq;
using System.Threading.Tasks;

using Divine.Entity;
using Divine.Entity.Entities.Abilities;
using Divine.Entity.Entities.Abilities.Components;
using Divine.Entity.Entities.Units.Heroes;
using Divine.Extensions;
using Divine.Game;
using Divine.Menu.Items;
using Divine.Modifier;
using Divine.Update;

namespace Hammer
{
    public class MeteorHammerEngine : IEngine
    {
        public MeteorHammerEngine(Bootstrap bootstrap)
        {
            IsEnabled = bootstrap.BaseMenu.CreateSwitcher("Enable", false);
            IsEnabled.SetTooltip("Use meteor hammer on OD's astral imprisonment prison ability");

            DontUseIfDie = bootstrap.BaseMenu.CreateSwitcher("Dont waste");
            DontUseIfDie.SetTooltip("dont use meteor if target will die after astral damage");

            ExtraTiming = bootstrap.BaseMenu.CreateSlider("Extra timing", 0, -100, 100);
        }

        private Hero Me { get; set; }
        private MenuSwitcher IsEnabled { get; }

        private MenuSlider ExtraTiming { get; }

        private MenuSwitcher DontUseIfDie { get; }

        public void Start()
        {
            UpdateManager.CreateGameUpdate(500, () => { });
            Me = EntityManager.LocalHero;

            ModifierManager.ModifierAdded += args =>
            {
                if (!IsEnabled.Value)
                    return;
                var name = args.Modifier?.Name;
                if (name != "modifier_obsidian_destroyer_astral_imprisonment_prison") return;
                var meteor = Me.Inventory.GetItemsById(AbilityId.item_meteor_hammer).FirstOrDefault();
                if (meteor == null) return;
                var target = args.Modifier.Owner as Hero;
                var leftTime = args.Modifier.RemainingTime;
                if (leftTime >= 2.95f && target != null && target.Team != Me.Team)
                    UpdateManager.BeginInvoke(10, async () =>
                    {
                        while (true)
                        {
                            var caster = args.Modifier.Caster as Hero;
                            var damage = GetAstral(caster ?? Me).GetCalculatedDamage(target);

                            var modifier =
                                target.GetModifierByName(
                                    "modifier_obsidian_destroyer_astral_imprisonment_prison");

                            if (modifier == null)
                                break;
                            if (DontUseIfDie.Value)
                            {
                                var health = target.Health;
                                var willRegenerate = target.BaseHealthRegeneration * modifier.RemainingTime;
                                var totalHealth = health + willRegenerate;
                                if (totalHealth <= damage) break;
                            }

                            var remaining = modifier.RemainingTime + GameManager.Ping / 1000 + ExtraTiming.Value;
                            if (remaining <= 2.95 && remaining >= 2.5f)
                            {
                                if (meteor.CanBeCastedOnTarget(target))
                                {
                                    await Task.Delay(10);
                                    continue;
                                }
                                meteor.Cast(target.Position);
                                break;
                            }

                            await Task.Delay(10);
                        }
                    });
            };
        }

        private static Ability GetAstral(Hero hero)
        {
            return hero.GetAbilityById(AbilityId.obsidian_destroyer_astral_imprisonment);
        }
    }
}