﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ModdedMinecraftClub.MemberBot.Bot.Services.Hosted
{
    public class CommandHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}