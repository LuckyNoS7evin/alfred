using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.TLRW
{
    interface IApiEvents
    {
        event EventHandler<string> Abc;
        event EventHandler<string> def;
    }
}
