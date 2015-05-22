using System;
using LeagueSharp.Common;

namespace xSaliceResurrected.Managers
{
    public static class HitChanceManager
    {
        private static Menu _menuCombo;
        private static Menu _menuHarass;

        private static readonly HitChance[] MyHitChances = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh };

        public static Menu AddHitChanceMenuCombo(Boolean q, Boolean w, Boolean e, Boolean r)
        {
            _menuCombo = new Menu("连招命中率", "Hitchance");
            
            if(q)
                _menuCombo.AddItem(new MenuItem("qHitCombo", "Q 命中率", true).SetValue(new StringList(new[] { "低", "正常", "高", "很高" }, 2)));
            if(w)
                _menuCombo.AddItem(new MenuItem("wHitCombo", "W 命中率", true).SetValue(new StringList(new[] { "低", "正常", "高", "很高"  }, 2)));
            if(e)
                _menuCombo.AddItem(new MenuItem("eHitCombo", "E 命中率", true).SetValue(new StringList(new[] { "低", "正常", "高", "很高"  }, 2)));
            if(r)
                _menuCombo.AddItem(new MenuItem("rHitCombo", "R 命中率", true).SetValue(new StringList(new[] { "低", "正常", "高", "很高"  }, 2)));

            return _menuCombo;
        }

        public static Menu AddHitChanceMenuHarass(Boolean q, Boolean w, Boolean e, Boolean r)
        {
            _menuHarass = new Menu("骚扰命中率", "Hitchance");

            if (q)
                _menuHarass.AddItem(new MenuItem("qHitHarass", "Q 命中率", true).SetValue(new StringList(new[] { "低", "正常", "高", "很高"  }, 2)));
            if (w)
                _menuHarass.AddItem(new MenuItem("wHitHarass", "W 命中率", true).SetValue(new StringList(new[] { "低", "正常", "高", "很高"  }, 2)));
            if (e)
                _menuHarass.AddItem(new MenuItem("eHitHarass", "E 命中率", true).SetValue(new StringList(new[] { "低", "正常", "高", "很高"  }, 2)));
            if (r)
                _menuHarass.AddItem(new MenuItem("rHitHarass", "R 命中率", true).SetValue(new StringList(new[] { "低", "正常", "高", "很高"  }, 2)));

            return _menuHarass;
        }

        public static HitChance GetQHitChance(string source)
        {
            if (source == "Combo")
            {
                return MyHitChances[_menuCombo.Item("qHitCombo", true).GetValue<StringList>().SelectedIndex];
            }
            else if(source == "Null")
            {
                return HitChance.Low;
            }
            return MyHitChances[_menuHarass.Item("qHitHarass", true).GetValue<StringList>().SelectedIndex];
        }

        public static HitChance GetWHitChance(string source)
        {
            if (source == "Combo")
            {
                return MyHitChances[_menuCombo.Item("wHitCombo", true).GetValue<StringList>().SelectedIndex];
            }
            return MyHitChances[_menuHarass.Item("wHitHarass", true).GetValue<StringList>().SelectedIndex];
        }

        public static HitChance GetEHitChance(string source)
        {
            if (source == "Combo")
            {
                return MyHitChances[_menuCombo.Item("eHitCombo", true).GetValue<StringList>().SelectedIndex];
            }
            return MyHitChances[_menuHarass.Item("eHitHarass", true).GetValue<StringList>().SelectedIndex];
        }

        public static HitChance GetRHitChance(string source)
        {
            if (source == "Combo")
            {
                return MyHitChances[_menuCombo.Item("rHitCombo", true).GetValue<StringList>().SelectedIndex];
            }
            return MyHitChances[_menuHarass.Item("rHitHarass", true).GetValue<StringList>().SelectedIndex];
        }

    }
}
