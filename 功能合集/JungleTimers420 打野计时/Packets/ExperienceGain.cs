using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JungleTimerHax.Packets
{
    class ExperienceGain : Packet
    {
        public UInt32 RecvNetId { get { return BitConverter.ToUInt32(content, 0); } }
        public UInt32 GivingNetId { get { return BitConverter.ToUInt32(content, 4); } }
        public Single Amount { get { return BitConverter.ToSingle(content, 8); } }
        public ExperienceGain(Packet p)
            : base(p.param, p.header, p.time, p.content)
        {
        }
    }
}
