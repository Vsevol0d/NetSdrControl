using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrControl.ControlItems
{
    public class ControlItemCommandData
    {
        public ControlItemCommandData(byte[] messageBytes, TaskCompletionSource<byte[]> taskCompletionSource)
        {
            MessageBytes = messageBytes;
            CompletionSource = taskCompletionSource;
        }

        public byte[] MessageBytes { get; set; }
        public TaskCompletionSource<byte[]> CompletionSource { get; set; }
    }
}
