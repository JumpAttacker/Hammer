using Divine;
using Divine.Menu;
using Divine.Menu.Items;

namespace Hammer
{
    public class Bootstrap : Bootstrapper
    {
        public RootMenu BaseMenu { get; set; }

        protected override void OnActivate()
        {
            BaseMenu = MenuManager.CreateRootMenu("MeteorHammer");
            var engine = new MeteorHammerEngine(this);
            engine.Start();
        }
    }
}