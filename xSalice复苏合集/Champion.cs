using System;
using LeagueSharp;
using LeagueSharp.Common;
using xSaliceResurrected.Base;
using xSaliceResurrected.Managers;

namespace xSaliceResurrected
{
    class Champion : SpellBase
    {
        protected readonly Obj_AI_Hero Player = ObjectManager.Player;

        protected Champion()
        {
            //Events
            Game.OnUpdate += Game_OnGameUpdateEvent;
            Drawing.OnDraw += Drawing_OnDrawEvent;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPosibleToInterruptEvent;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloserEvent;
            GameObject.OnCreate += GameObject_OnCreateEvent;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCastEvent;
            GameObject.OnDelete += GameObject_OnDeleteEvent;
            Obj_AI_Base.OnIssueOrder += ObjAiHeroOnOnIssueOrderEvent;
            Spellbook.OnUpdateChargedSpell += Spellbook_OnUpdateChargedSpellEvent;
            Orbwalking.AfterAttack += AfterAttackEvent;
            Orbwalking.BeforeAttack += BeforeAttackEvent;
        }

        public Champion(bool load)
        {
            if (load)
                GameOnLoad();
        }

        //Orbwalker instance
        protected static Orbwalking.Orbwalker Orbwalker;
        protected static AzirManager AzirOrb;

        //Menu
        public static Menu menu;
        private static readonly Menu OrbwalkerMenu = new Menu("走砍", "Orbwalker");

        private void GameOnLoad()
        {
            Game.PrintChat("<font color = \"#FFB6C1\">xSalice's 澶嶈嫃鍚堥泦</font>浣滆€咃細xSalice<font color = \"#00FFFF\">鍔犺浇鎴愬姛! 姹夊寲 by Ergou! QQ缇361630847 !</font>");
            Game.PrintChat("<font color = \"#87CEEB\">xSalice浣滆€卲aypal鎹愯禒鍦板潃|:</font> <font color = \"#FFFF00\">xSalicez@gmail.com</font>");

            menu = new Menu("xSalice's复苏合集："+Player.ChampionName, Player.ChampionName, true);

            //Info
            menu.AddSubMenu(new Menu("信息", "Info"));
            menu.SubMenu("Info").AddItem(new MenuItem("Author", "作者： xSalice"));
            menu.SubMenu("Info").AddItem(new MenuItem("Paypal", "捐赠地址: xSalicez@gmail.com"));
            menu.SubMenu("Info").AddItem(new MenuItem("hanhua", "Ergou完整汉化"));
            menu.SubMenu("Info").AddItem(new MenuItem("qqqun", "唯一QQ群：361630847"));

            //Target selector
            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            if (Player.ChampionName.ToLower() == "azir")
            {
                menu.AddSubMenu(OrbwalkerMenu);
                AzirOrb = new AzirManager(OrbwalkerMenu);
            }
            else
            {
                menu.AddSubMenu(OrbwalkerMenu);
                Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);
            }
            //Item Menu
            var itemMenu = new Menu("物品和召唤师技能", "Items");
            ItemManager.AddToMenu(itemMenu);
            menu.AddSubMenu(itemMenu);
            
            //Lag Manager
            LagManager.AddLagManager(menu);

            menu.AddToMainMenu();

            new PluginLoader();
        }

        protected Orbwalking.Orbwalker GetorbOrbwalker()
        {
            return Orbwalker;
        }

        private void Drawing_OnDrawEvent(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            Drawing_OnDraw(args);
        }

        protected virtual void Drawing_OnDraw(EventArgs args)
        {
            //for champs to use
        }

        private void AntiGapcloser_OnEnemyGapcloserEvent(ActiveGapcloser gapcloser)
        {
            AntiGapcloser_OnEnemyGapcloser(gapcloser);
        }

        protected virtual void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //for champs to use
        }

        private void Interrupter_OnPosibleToInterruptEvent(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            Interrupter_OnPosibleToInterrupt(unit, spell);
        }

        protected virtual void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            //for champs to use
        }

        private void Game_OnGameUpdateEvent(EventArgs args)
        {
            if (LagManager.Enabled)
                if (!LagManager.ReadyState)
                    return;

            //check if player is dead
            if (Player.IsDead) return;

            Game_OnGameUpdate(args);
        }

        protected virtual void Game_OnGameUpdate(EventArgs args)
        {
            //for champs to use
        }

        private void GameObject_OnCreateEvent(GameObject sender, EventArgs args)
        {
            GameObject_OnCreate(sender, args);
        }

        protected virtual void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            //for champs to use
        }

        private void GameObject_OnDeleteEvent(GameObject sender, EventArgs args)
        {
            GameObject_OnDelete(sender, args);
        }

        protected virtual void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            //for champs to use
        }

        private void Obj_AI_Base_OnProcessSpellCastEvent(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            Obj_AI_Base_OnProcessSpellCast(unit, args);
        }

        protected virtual void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            //for champ use
        }

        private void AfterAttackEvent(AttackableUnit unit, AttackableUnit target)
        {
            AfterAttack(unit, target);
        }

        protected virtual void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            //for champ use
        }

        private void BeforeAttackEvent(Orbwalking.BeforeAttackEventArgs args)
        {
            BeforeAttack(args);
        }

        protected virtual void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            //for champ use
        }

        private void ObjAiHeroOnOnIssueOrderEvent(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            ObjAiHeroOnOnIssueOrder(sender, args);
        }

        protected virtual void ObjAiHeroOnOnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            //for champ use
        }

        private void Spellbook_OnUpdateChargedSpellEvent(Spellbook sender, SpellbookUpdateChargedSpellEventArgs args)
        {
            Spellbook_OnUpdateChargedSpell(sender, args);
        }

        protected virtual void Spellbook_OnUpdateChargedSpell(Spellbook sender, SpellbookUpdateChargedSpellEventArgs args)
        {
            //for champ use
        }
    }
}