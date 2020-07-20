using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.TLRW
{
    public class TLRW : IApiEvents
    {
        public event EventHandler<string> Abc;
        public event EventHandler<string> def;

        public SECTION1 Streams;
        public SECTION2 Users;
        public TLRW()
        {
            Streams.Abc += TLRW_ABC;
            Streams.def += TLRW_DEF;
            Users.Abc += TLRW_ABC;
            Users.def += TLRW_DEF;
        }

        private void TLRW_ABC(object sender, string e)
        {
            Abc?.Invoke(sender, e);
        }
        private void TLRW_DEF(object sender, string e)
        {
            def?.Invoke(sender, e);
        }
    }
}
