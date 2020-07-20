using Bot.Core;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Data.Interfaces
{
    public interface IOwnerTwitchCredentialRepository
    {
        Task<bool> SaveAsync(OwnerTwitchCredentials ownerTwitchCredentials);
    }
}
