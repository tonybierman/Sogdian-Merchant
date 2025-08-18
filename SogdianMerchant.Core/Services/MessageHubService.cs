using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SogdianMerchant.Core.Services
{
    public class MessageHubService : IMessageHubService
    {
        private StringBuilder _messageBuffer;

        public MessageHubService()
        {
            _messageBuffer = new StringBuilder();
        }

        public void Publish<T>(T message)
        {
            _messageBuffer.AppendLine(message.ToString());
        }

        public override string ToString()
        {
            return _messageBuffer.ToString();
        }

        public void Clear()
        {
            _messageBuffer.Clear();
        }
    }
}
