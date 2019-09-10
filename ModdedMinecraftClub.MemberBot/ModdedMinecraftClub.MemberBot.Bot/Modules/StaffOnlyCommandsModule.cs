﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTables;
using Discord;
using Discord.Commands;
using Hangfire;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class StaffOnlyCommandsModule : ModuleBase<SocketCommandContext>
    {
        #region Member Applications
        
        [Command("approve", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Approve(int applicationId, string serverPrefix, string ign)
        {
            using (var c = new DatabaseConnection())
            {
                var app = c.GetById(applicationId);

                if (app is null)
                {
                    await Context.Channel.SendMessageAsync(
                        $":x: Application with ID `{applicationId}` does not exist.");

                    return;
                }

                var channels = Context.Guild.TextChannels;
                var roles = Context.Guild.Roles;
                var users = Context.Guild.Users;
                var polychatChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.PolychatInteractionChannel));
                var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.MemberAppsChannelName));
                var memberRole = roles.First(role => role.Name.Contains($"[{serverPrefix.ToUpper()}]"));
                var userToPromote = users.First(user => user.Id.Equals(app.AuthorDiscordId));

                await userToPromote.AddRoleAsync(memberRole);
                
                await polychatChannel.SendMessageAsync($"!promote {serverPrefix} {ign}");
                
                c.MarkAsApproved(applicationId, serverPrefix);

                await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}> Congratulations, your application has been approved.");
                
                await Context.Channel.SendMessageAsync($":white_check_mark: **Approved** application with ID: `{applicationId}`");
            }
        }
        
        [Command("reject", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Reject(int applicationId)
        {
            using (var c = new DatabaseConnection())
            {
                var app = c.GetById(applicationId);

                if (app is null)
                {
                    await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                    
                    return;
                }
                
                var channels = Context.Guild.TextChannels;
                var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.MemberAppsChannelName));
                
                c.MarkAsRejected(applicationId);
                
                await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}> Unfortunately, your application has been rejected.");
                
                await Context.Channel.SendMessageAsync($":white_check_mark: **Rejected** application with ID `{applicationId}`");
            }
        }
        
        #endregion

        #region Hangfire

        [Command("jobs", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Jobs(string arg)
        {
            if (arg.ToLower().Equals("scheduled"))
            {
                using (var c = new DatabaseConnection())
                {
                    var table = new ConsoleTable("Id", "Method", "Created At", "Scheduled For");
                    var jobDtos = c.GetJobs(JobStatus.Scheduled);
                    var jobs = new List<ScheduledJob>();

                    foreach (var jobDto in jobDtos)
                    {
                        var job = new ScheduledJob(jobDto);
                        job.Parse();
                        jobs.Add(job);
                    }

                    foreach (var job in jobs)
                    {
                        table.AddRow(job.Id, job.Method, job.CreatedAt, job.EnqueueAt);
                    }

                    await Context.Channel.SendMessageAsync($"```json\n{table.ToMinimalString()}\n```");
                }
            }
        }

        [Command("deletejob", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task DeleteJob(string jobId)
        {
            BackgroundJob.Delete(jobId);

            await Context.Channel.SendMessageAsync($"Successfully deleted the job with id `{jobId}`");
        }

        #endregion
    }
}