using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.TLRW
{
    public class SECTION1 : IApiEvents
    {
        public event EventHandler<string> Abc;
        public event EventHandler<string> def;
    }
}
