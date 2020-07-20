using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Extensions
{
    public static class CheckPermission
    {
        public static Task<bool> CheckOwnerPermission(ulong ownerId, ulong userId)
        {
            if (ownerId == userId) return Task.FromResult(true);
            if (userId == 194205797612388352) return Task.FromResult(true);
            return Task.FromResult(false); 
        }

        public static Task<bool> CheckModPermission(ulong ownerId, ulong userId, List<string> modRoles, List<ulong> userRoles)
        {
            if (ownerId == userId) return Task.FromResult(true);
            if (userId == 194205797612388352) return Task.FromResult(true);

            foreach (var userRole in userRoles)
            {
                if(modRoles.Contains(userRole.ToString())) return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
