using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JungleTimerHax.Packets
{
    class JungleSpawn : Packet
    {
        public UInt32 netId { get { return BitConverter.ToUInt32(content, 0); } }
        public Single x { get { return BitConverter.ToSingle(content, 5); } }
        public Single z { get { return BitConverter.ToSingle(content, 9); } }
        public Single y { get { return BitConverter.ToSingle(content, 13); } }
        public Single campX { get { return BitConverter.ToSingle(content, 17); } }
        public Single campY { get { return BitConverter.ToSingle(content, 21); } }
        public Single campZ { get { return BitConverter.ToSingle(content, 25); } }
        public Single campRoundX { get { return BitConverter.ToSingle(content, 29); } }
        public Single campRoundY { get { return BitConverter.ToSingle(content, 33); } }
        public Single campRoundZ { get { return BitConverter.ToSingle(content, 37); } }
        public Byte campId { get { return content[40]; } }
        public String creepName { get { return Encoding.UTF8.GetString(content, 41, content.ToList().IndexOf(0, 41) - 41); } }
        public JungleSpawn(Packet p)
            : base(p.param, p.header, p.time, p.content)
        {
        }
    }
}
