using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;

public class SkinManager
{
    private List<string> Skins = new List<string>();
    private int SelectedSkin;
    private bool Initialize = true;
    private MenuWrapper.BoolLink enabled;
    private MenuWrapper.StringListLink skinList;

    public void AddToMenu(MenuWrapper.SubMenu menu)
    {
        if (Skins.Count == 0)
            return;
        
        MenuWrapper.SubMenu skinMenu = menu.AddSubMenu("皮肤 选择(失效)");
        
        enabled = skinMenu.AddLinkedBool("开 启");
        skinList = skinMenu.AddLinkedStringList("选 择", Skins.ToArray());

        SelectedSkin = skinList.Value.SelectedIndex;
    }

    public void Add(string skin)
    {
        Skins.Add(skin);
    }

    public void Update()
    {
        if (!enabled.Value)
            return;
    
        int skin = skinList.Value.SelectedIndex;
        if (Initialize || skin != SelectedSkin)
        {
            Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(ObjectManager.Player.NetworkId, skin, ObjectManager.Player.ChampionName)).Process();
            SelectedSkin = skin;
            Initialize = false;
        }
    }
}