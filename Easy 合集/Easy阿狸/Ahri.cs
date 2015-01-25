using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

public class Ahri : Champion
{
    private Items.Item ItemDFG;
	        
	public Ahri() : base("Ahri")
	{
		
	}

	protected override void OnInit()
	{
		ItemDFG = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline ? new Items.Item((int)ItemId.Blackfire_Torch, 750) : new Items.Item((int)ItemId.Deathfire_Grasp, 750);
	}

    protected override void OnInitSkins()
    {
        Skins.Add("Ahri");
        Skins.Add("Dynasty Ahri");
        Skins.Add("Midnight Ahri");
        Skins.Add("Foxfire Ahri");
        Skins.Add("Popstar Ahri");
    }

	protected override void OnInitSpells()
	{
		Q = new Spell(SpellSlot.Q, 950f);
        W = new Spell(SpellSlot.W, 800f);
        E = new Spell(SpellSlot.E, 975f);
        R = new Spell(SpellSlot.R, 450f);

        Q.SetSkillshot(0.25f, 100f, 1250f, false, SkillshotType.SkillshotLine);
        E.SetSkillshot(0.25f, 60f, 1500f, true, SkillshotType.SkillshotLine);
	}
	
	protected override void OnInitMenu()
	{
		MenuWrapper.SubMenu comboMenu = Menu.MainMenu.AddSubMenu("连招");
        BoolLinks.Add("combo_q", comboMenu.AddLinkedBool("使用 Q", true));
        BoolLinks.Add("combo_w", comboMenu.AddLinkedBool("使用 W", true));
        BoolLinks.Add("combo_e", comboMenu.AddLinkedBool("使用 E", true));

        MenuWrapper.SubMenu harassMenu = Menu.MainMenu.AddSubMenu("骚扰");
        BoolLinks.Add("harass_q", harassMenu.AddLinkedBool("使用 Q", true));
        BoolLinks.Add("harass_w", harassMenu.AddLinkedBool("使用 W", false));
        BoolLinks.Add("harass_e", harassMenu.AddLinkedBool("使用 E", false));
        SliderLinks.Add("harass_mana", harassMenu.AddLinkedSlider("保持丨蓝量", 200, 0, 500));

        MenuWrapper.SubMenu autoMenu = Menu.MainMenu.AddSubMenu("自动");
        BoolLinks.Add("auto_q", autoMenu.AddLinkedBool("使用 Q", false));
        BoolLinks.Add("auto_w", autoMenu.AddLinkedBool("使用 W", false));
        BoolLinks.Add("auto_e", autoMenu.AddLinkedBool("使用 E", false));
        BoolLinks.Add("auto_e_interrupt", autoMenu.AddLinkedBool("U使用 E 打断技能", true));
        BoolLinks.Add("auto_e_slows", autoMenu.AddLinkedBool("使用 E 延缓敌人", true));
        BoolLinks.Add("auto_e_stuns", autoMenu.AddLinkedBool("使用 E 控制敌人", true));
        BoolLinks.Add("auto_e_gapclosers", autoMenu.AddLinkedBool("Use E on gapclosers", true));
        SliderLinks.Add("auto_mana", autoMenu.AddLinkedSlider("保持丨蓝量", 200, 0, 500));

        MenuWrapper.SubMenu drawingMenu = Menu.MainMenu.AddSubMenu("范围 选项");
        BoolLinks.Add("drawing_q", drawingMenu.AddLinkedBool("Draw Q 范围", true));
        BoolLinks.Add("drawing_w", drawingMenu.AddLinkedBool("Draw W 范围", true));
        BoolLinks.Add("drawing_e", drawingMenu.AddLinkedBool("Draw E 范围", true));
        BoolLinks.Add("drawing_r", drawingMenu.AddLinkedBool("Draw R 范围", true));
        BoolLinks.Add("drawing_damage", drawingMenu.AddLinkedBool("显示组合技能伤害", true));

        MenuWrapper.SubMenu miscMenu = Menu.MainMenu.AddSubMenu("杂项");
        KeyLinks.Add("misc_charm", miscMenu.AddLinkedKeyBind("使用 E 按键", 'T', KeyBindType.Press));
	}
	
	protected override void OnCombo()
	{
		if (BoolLinks["combo_e"].Value)
			Spells.CastSkillshot(E, TargetSelector.DamageType.Magical);
        if (BoolLinks["combo_q"].Value)
        	Spells.CastSkillshot(Q, TargetSelector.DamageType.Magical);
        if (BoolLinks["combo_w"].Value)
        	Spells.CastSelf(W, TargetSelector.DamageType.Magical);
	}
	
	protected override void OnHarass()
	{
		if (BoolLinks["harass_e"].Value && GetSpellData(SpellSlot.E).ManaCost + SliderLinks["harass_mana"].Value.Value <= Player.Mana)
			Spells.CastSkillshot(E, TargetSelector.DamageType.Magical);
        if (BoolLinks["harass_q"].Value && GetSpellData(SpellSlot.Q).ManaCost + SliderLinks["harass_mana"].Value.Value <= Player.Mana)
        	Spells.CastSkillshot(Q, TargetSelector.DamageType.Magical);
        if (BoolLinks["harass_w"].Value && GetSpellData(SpellSlot.W).ManaCost + SliderLinks["harass_mana"].Value.Value <= Player.Mana)
        	Spells.CastSelf(W, TargetSelector.DamageType.Magical);
	}
	
	protected override void OnAuto()
	{
		if (BoolLinks["auto_e"].Value && GetSpellData(SpellSlot.E).ManaCost + SliderLinks["auto_mana"].Value.Value <= Player.Mana)
			Spells.CastSkillshot(E, TargetSelector.DamageType.Magical);
		if (BoolLinks["auto_q"].Value && GetSpellData(SpellSlot.Q).ManaCost + SliderLinks["auto_mana"].Value.Value <= Player.Mana)
			Spells.CastSkillshot(Q, TargetSelector.DamageType.Magical);
        if (BoolLinks["auto_w"].Value && GetSpellData(SpellSlot.W).ManaCost + SliderLinks["auto_mana"].Value.Value <= Player.Mana)
        	Spells.CastSelf(W, TargetSelector.DamageType.Magical);
	}

    protected override void OnUpdate()
    {
        if (E.IsReady())
        {
            if (KeyLinks["misc_charm"].Value.Active)
                Spells.CastSkillshot(E, TargetSelector.DamageType.Magical);

            foreach (Obj_AI_Hero enemy in Enemies.Where(x => Player.Distance(x, false) <= E.Range))
            {
                if (BoolLinks["auto_e_stuns"].Value && enemy.HasBuffOfType(BuffType.Stun))
                    Spells.CastSkillshot(E, enemy);
                if (BoolLinks["auto_e_slows"].Value && enemy.HasBuffOfType(BuffType.Slow))
                    Spells.CastSkillshot(E, enemy);
            }
        }
    }

	protected override void OnDraw()
	{
		if (BoolLinks["drawing_q"].Value)
            Utility.DrawCircle(Player.Position, Q.Range, Color.FromArgb(100, 0, 255, 0));
        if (BoolLinks["drawing_w"].Value)
            Utility.DrawCircle(Player.Position, W.Range, Color.FromArgb(100, 0, 255, 0));
        if (BoolLinks["drawing_e"].Value)
            Utility.DrawCircle(Player.Position, E.Range, Color.FromArgb(100, 0, 255, 0));
        if (BoolLinks["drawing_r"].Value)
            Utility.DrawCircle(Player.Position, R.Range, Color.FromArgb(100, 0, 255, 0));
        
        Utility.HpBarDamageIndicator.Enabled = BoolLinks["drawing_damage"].Value;
        Utility.HpBarDamageIndicator.DamageToUnit = DamageCalculation;
	}
	
	protected override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
    {
        if (BoolLinks["auto_e_interrupt"].Value)
            Spells.CastSkillshot(E, unit);
    }

    protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
    {
        if (BoolLinks["auto_e_gapclosers"].Value)
            Spells.CastSkillshot(E, gapcloser.Sender);
    }
	
	private float DamageCalculation(Obj_AI_Base hero)
    {
        float damage = 0;

        if (ItemDFG.IsReady())
            damage += (float)Player.GetItemDamage(hero, Damage.DamageItems.Dfg) / 1.2f;
        if (Q.IsReady())
            damage += (float)Player.GetSpellDamage(hero, SpellSlot.Q) * 2;
        if (W.IsReady())
            damage += (float)Player.GetSpellDamage(hero, SpellSlot.W);
        if (E.IsReady())
            damage += (float)Player.GetSpellDamage(hero, SpellSlot.E);
        if (R.IsReady())
            damage += (float)Player.GetSpellDamage(hero, SpellSlot.R) * 3;

        return damage * (ItemDFG.IsReady() ? 1.2f : 1f);
    }
}