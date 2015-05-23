using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElAlistarReborn
{
    internal class Drawings
    {
        public static void OnDraw(EventArgs args)
        {
            var drawOff = ElAlistarMenu._menu.Item("ElAlistar.Draw.off").GetValue<bool>();
            var drawQ = ElAlistarMenu._menu.Item("ElAlistar.Draw.Q").GetValue<Circle>();
            var drawW = ElAlistarMenu._menu.Item("ElAlistar.Draw.W").GetValue<Circle>();
            var drawE = ElAlistarMenu._menu.Item("ElAlistar.Draw.E").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (Alistar.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Alistar.spells[Spells.Q].Range, Color.White);

            if (drawE.Active)
                if (Alistar.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Alistar.spells[Spells.E].Range, Color.White);

            if (drawW.Active)
                if (Alistar.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Alistar.spells[Spells.W].Range, Color.White);
        }
    }
}