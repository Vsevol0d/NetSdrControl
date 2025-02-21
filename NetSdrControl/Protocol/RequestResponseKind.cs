using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrControl.Protocol
{
    public enum RequestResponseKind : byte
    {
        Get = 1,
        Set = 0,
        GetRange = 2,
        Unsolicited = 1
    }
}
