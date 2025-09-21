using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Models
{
    public class IAReques
    {
        public class ChatMessage
        {
            public string Role { get; set; } 
            public string Content { get; set; }
        }
        public class ChatRequest
        {
            public List<ChatMessage> History { get; set; }
        }
    }
}