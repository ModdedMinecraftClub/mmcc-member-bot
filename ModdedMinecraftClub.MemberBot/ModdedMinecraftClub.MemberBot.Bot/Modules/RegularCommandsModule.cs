﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    public class RegularCommandsModule : ModuleBase<SocketCommandContext>
    {
        [Command("pending", RunMode = RunMode.Async)]
        public async Task Pending()
        {
            using var c = new DatabaseConnection();
            var list = c.GetAllPending();

            if (!list.Any())
            {
                await Context.Channel.SendMessageAsync("There are no pending applications at the moment.");
            }
            else
            {
                var s = list.Aggregate("Pending applications:\n-------------------------------------------\n", 
                    (current, app) => current + $"[{app.AppId}] {app.AuthorName}; {app.AppTime}\n\n");

                await Context.Channel.SendMessageAsync($"```{s}```");
            }
        }
        
        [Command("approved", RunMode = RunMode.Async)]
        public async Task Approved()
        {
            using var c = new DatabaseConnection();
            var list = c.GetLast20Approved();

            if (!list.Any())
            {
                await Context.Channel.SendMessageAsync("You haven't approved any applications yet.");
            }
            else
            {
                var s = list.Aggregate("Last 20 approved applications:\n-------------------------------------------\n", 
                    (current, app) => current + $"[{app.AppId}] {app.AuthorName}; {app.AppTime}\n\n");

                await Context.Channel.SendMessageAsync($"```{s}```");
            }
        }
        
        [Command("rejected", RunMode = RunMode.Async)]
        public async Task Rejected()
        {
            using var c = new DatabaseConnection();
            var list = c.GetLast20Rejected();

            if (!list.Any())
            {
                await Context.Channel.SendMessageAsync("You haven't rejected any applications yet.");
            }
            else
            {
                var s = list.Aggregate("Last 20 rejected applications:\n-------------------------------------------\n", 
                    (current, app) => current + $"[{app.AppId}] {app.AuthorName}; {app.AppTime}\n\n");

                await Context.Channel.SendMessageAsync($"```{s}```");
            }
        }
        
        [Command("view", RunMode = RunMode.Async)]
        public async Task View(int applicationId)
        {
            using var c = new DatabaseConnection();
            
            var app = c.GetById(applicationId);

            if (app is null)
            {
                await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                    
                return;
            }
            
            var b = new EmbedBuilder();
            b.AddField($"{app.AppStatus.ToString().ToUpper()}: Application by {app.AuthorName}", $"Author's Discord ID: {app.AuthorDiscordId}\nApplication ID: {app.AppId}");
            
            if (app.MessageContent is null || app.MessageContent.Equals(""))
            {
                b.AddField("Provided details","*Player did not provide any details.*");
            }
            else
            {
                b.AddField("Provided details", app.MessageContent);
            }

            b.AddField("Link to original message", app.MessageUrl);
            b.WithThumbnailUrl(app.ImageUrl);
            b.WithFooter($"Applied at {app.AppTime}");
                
            switch (app.AppStatus)
            {
                case ApplicationStatus.Approved:
                    b.WithColor(Color.Green);
                    break;
                case ApplicationStatus.Rejected:
                    b.WithColor(Color.Red);
                    break;
                default:
                    b.WithColor(Color.Blue);
                    break;
            }
                
            await Context.Channel.SendMessageAsync("", false, b.Build());
        }
    }
}