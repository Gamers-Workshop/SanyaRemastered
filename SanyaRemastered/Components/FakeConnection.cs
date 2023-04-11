using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered
{
    public class FakeConnection : NetworkConnectionToClient
    {
        private static int Id = int.MaxValue;

        public FakeConnection() : base(Id--, false, 0f)
        {

        }

        public override string address => "localhost";

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
        }
        public override void Disconnect()
        {
        }
    }
}
