using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JungleTimerHax.Packets
{
    public enum HeaderList
    {
        KeyCheck = 0x00,
        MinionSpawn = 0x03,
        SellItem = 0x0B,
        EndSpawn = 0x11,
        AbilityLevel = 0x15,
        CreateEffect = 0x17,
        AutoAttack = 0x1A,
        ExperienceGain = 0x22,
        FogUpdate = 0x23,
        PlayAnimation = 0x29,
        PlayerInfo = 0x2A,
        SpawnProjectile = 0x3B,
        SwapItems = 0x3E,
        LevelUp = 0x3F,
        AttentionPing = 0x40,
        Emotion = 0x42,
        PlayerStats = 0x46,
        HeroSpawn = 0x4C,
        Announce = 0x4D, //not called
        SynchVersion = 0x54,
        DestroyProjectile = 0x5A,
        StartGame = 0x5C,
        DeathTimer = 0x5E,
        Waypoints = 0x61,
        StartSpawn = 0x62,
        JungleSpawn = 0x63,
        ChampionNames_Damage = 0x65,
        CharacterNames_Unk66 = 0x66,
        LoadScreen = 0x67,
        Chat = 0x68,
        SetTarget = 0x6A,
        BuyItem = 0x6F,
        ZyraPassive = 0x86,
        SpawnParticle = 0x87,
        GameId = 0x92,
        PingLoad = 0x95,
        KhazixEvo = 0x96,
        ZedShadow = 0x97,
        TurretSpawn = 0x9D,
        DeathRecap = 0xA3,
        SetHealth = 0xAE,
        SpellCast = 0xB5,
        UnitSpawn = 0xBA,
        GameTimer = 0xC0,
        GameTimerUpdate = 0xC1,
        CharStats = 0xC4,
        LevelPropSpawn = 0xD0,
        Teleport = 0xD8,
        Gold = 0xE4,
        JungleCampSpawn = 0xE9,
        ChangeTarget_InventoryUpdate = 0xFE,

        Unk85 = 0x85, //param netid, byte flag; float 24.14; float -1, 10, 90, 120, etc.
        Unk7f = 0x7F, //unit16 num,, net id...??
        Unkb7 = 0xB7,
        Unke0 = 0xE0,
        Unke1 = 0xE1,
        UnkC2 = 0xC2,
        Unk1c = 0x1c, // 06-01-00-00-16-44-00-00-00-00 + netid
        Unk76 = 0x76, // every ten seconds, ten packets, float float float, no net id
        Unk35 = 0x35,
        Unk51 = 0x51, //no content, param netid no player
        Unk50 = 0x50,// param netid, float float float, 0-1, abaaa3d or 0
        Unk6E = 0x6E, // attach child to parent? parent = param, child = byte0-3
        UnkFD = 0xFD, // next AA does something?
        Unk45 = 0x45, //announcer?
        Unk6B = 0x6B,
        Unk9F = 0x9F, //param player netid, byte 0127, byte 0-3, byte 0-1
        Unk7C = 0x7C,
        Unk7 = 0x7, //netid 0-4, only players
        Unk34 = 0x34, // param netid sometimes, then 5 bytes
        UnkB2 = 0xB2, //?? param netid, uint32 increasing 1++, uint16 flag, .2 or 0, zyra triggers the 0 on death, kha triggers 
        UnkE2 = 0xE2,
        Unk9E = 0x9E, //??, param new netid, byte03 netid, uint32 unk,uint32 unk,
        Unk38 = 0x38,
        Unk7B = 0x7B, //param netid, byte unk,  4byte hash, 0000
        Unk33 = 0x33, // only contains a netid,
        Unk32 = 0x32, //??, param netid, increasing most of the time 2 bytes
        Unk26 = 0x26, //param netid, always increasing, so maybe projectile netid?, byte 0 = num of netids in content, byte1-4 netid1, byte5-8 netid2
        UnkC3 = 0xC3, // jungle clear?
        Unk30 = 0x30, //not sure, plant/trap/movement?, netid @ 9, target? on param
        UnkC = 0xC,
        Unk44 = 0x44,
        Unk10 = 0x10, //netid, some random float? maybe zoom? time?
        Unk73 = 0x73, // param netid, float float float, might be recall
        Unk3F = 0x3F,
        Unk64 = 0x64,
        Unk18 = 0x18, //?? not much data, a byte and a float
        UnkB0 = 0xB0, //? spawn particle/effect?
        UnkCE = 0xCE,
        Unk1F = 0x1F, // param = netid, FF-FF or 2[ed]-2[ed]
        Unk21 = 0x21,
        Unk71 = 0x71,
        Unk1B = 0x1B, // ??, single float, 7 occurences in a game, 3 to 19
        UnkC8 = 0xC8, // 7 packets, maybe turret death?
        Unk2F = 0x2F, // 23 times, 3 floats, ~14k, ~14, even number 400 710 etc.
        UnkE3 = 0xE3, // net id at 10/12/20 minutes
        UnkA5 = 0xA5, // 2 packets, 7 bytes???
        Unk89 = 0x89, //unk param, 4 byte netid, 0000
        Unk2B = 0x2B, // only happened twice, F0-00
        UnkC9 = 0xC9, // surrender vote maybe?

    };
    public class Packet
    {
        public Boolean isPacketType = true;
        public UInt32 param;
        public Byte header;
        public Single time;
        public Byte[] content;
        public Packet(UInt32 param, Byte header, Single time, Byte[] content)
        {
            this.param = param;
            this.header = header;
            this.time = time;
            this.content = new Byte[content.Length];
            Array.Copy(content, this.content, content.Length);
        }
    }
}
