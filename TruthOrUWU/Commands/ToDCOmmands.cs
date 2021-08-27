using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Timers;
using System.Threading;
using DSharpPlus.Entities;

namespace truthOrUwU.Commands
{
    public class ToDCommands : BaseCommandModule
    {
        private List<User> queue = new List<User>();
        private ulong randPoint;
        private bool gameState = false;
        private bool active = false;
        private string list;
        private DiscordChannel channel;
        private System.Timers.Timer timer;

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync("Pong!");
        }
        [Command("q")]
        public async Task AddQueue(CommandContext ctx)
        {
            if (queue.Exists(Player => Player.id == ctx.User.Id))
            {
                await ctx.RespondAsync($"{ctx.User.Username}, You are already in the queue.");
            }
            else
            {
                User user = new User(ctx.User.Id, ctx.User.Username);
                queue.Add(user);
                await ctx.RespondAsync($"{ctx.User.Username}, you have been added to the queue.");
            }
        }
        [Command("r")]
        public async Task RemoveFromQueue(CommandContext ctx)
        {
            if (queue.Exists(Player => Player.id == ctx.User.Id))
            {
                if (queue.Count > 2)
                {
                    if (randPoint == ctx.User.Id)
                    {
                        if (queue[0].id == randPoint)
                        {
                            queue.RemoveAt(0);
                            await queue.Shuffle();
                            randPoint = queue[queue.Count - 1].id;
                            FormatQ();
                            await ctx.RespondAsync($"{ctx.User.Username} has been removed from the queue causing a reshuffle." + Environment.NewLine + $"<@{queue[queue.Count - 1].id}> is now asking <@{queue[0].id}>." + list);
                        }
                        else
                        {
                            randPoint = queue[queue.FindIndex(Player => Player.id == randPoint) - 1].id;
                            queue.RemoveAt(queue.FindIndex(Player => Player.id == randPoint) + 1);
                        }
                    }
                    else if (queue.FindIndex(Player => Player.id == ctx.User.Id) < 2)
                    {
                        queue.RemoveAll(Player => Player.id == ctx.User.Id);
                        FormatQ();
                        await ctx.RespondAsync($"{ctx.User.Username}, you have been removed from the queue." + Environment.NewLine + $"So, <@{queue[queue.Count - 1].id}> is now asking <@{queue[0].id}>." + list);
                    }
                    else
                    {
                        queue.RemoveAll(Player => Player.id == ctx.User.Id);
                        await ctx.RespondAsync($"{ctx.User.Username}, you have been removed from the queue.");
                    }
                }
                else if (gameState == true)
                {
                    gameState = false;
                    queue.RemoveAll(Player => Player.id == ctx.User.Id);
                    randPoint = 0;
                    await ctx.RespondAsync($"{ctx.User.Username}, you have been removed from the queue. There are no longer enough players for play to continue, the game has been stopped.");
                }
                else
                {
                    queue.RemoveAll(Player => Player.id == ctx.User.Id);
                    await ctx.RespondAsync($"{ctx.User.Username}, you have been removed from the queue.");
                }
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Username}, you were not in the queue to begin with. Regardless, congratulations you are not in the queue, mission accomplished!");
            }
        }
        [Command("p")]
        public async Task StartQueue(CommandContext ctx)
        {
            if (gameState == false)
            {
                if (queue.Exists(Player => Player.id == ctx.User.Id))
                {
                    if (queue.Count > 1)
                    {
                        await queue.Shuffle();
                        randPoint = queue[queue.Count - 1].id;
                        gameState = true;
                        FormatQ();
                        await ctx.RespondAsync("Queue started!" + Environment.NewLine + $"<@{queue[queue.Count - 1].id}> is currently asking <@{queue[0].id}>." + list);
                        channel = ctx.Channel;
                        // Create a 30 min timer 
                        timer = new System.Timers.Timer(1800000);
                        // Hook up the Elapsed event for the timer.
                        timer.Elapsed += OnTimedEvent;
                        timer.Enabled = true;
                    }
                    else
                    {
                        await ctx.RespondAsync("Not enough players to start the game. You can join with the 'q' command.");
                    }
                }
                else
                {
                    await ctx.RespondAsync("You must be in the queue to start the game, you may join the game with the 'q' command.");
                }
            }
            else
            {
                await ctx.RespondAsync("A game is already in session, you can join with the 'q' command.");
            }
        }
        [Command("n")]
        public async Task Next(CommandContext ctx)
        {
            if (gameState == true)
            {
                active = true;
                if (randPoint == queue[0].id)
                {
                    await queue.Shuffle();
                    randPoint = queue[queue.Count - 1].id;
                    FormatQ();
                    await ctx.RespondAsync($"Queue shuffled!" + Environment.NewLine + $"<@{queue[queue.Count - 1].id}> is currently asking <@{queue[0].id}>." + list);
                }
                else
                {
                    queue.Add(queue[0]);
                    queue.RemoveAt(0);
                    FormatQ();
                    await ctx.RespondAsync($"<@{queue[queue.Count - 1].id}> is currently asking <@{queue[0].id}>." + list);
                }
            }
            else
            {
                await ctx.RespondAsync("The game is not currently in progress, if you would like to start the game use the 'p' command.");
            }
        }
        [Command("s")]
        public async Task SkipAsker(CommandContext ctx)
        {
            if (gameState == true)
            {
                if (queue.Exists(Player => Player.id == ctx.User.Id))
                {
                    if (queue[1].id == ctx.User.Id)
                    {
                        if (queue.Count < 3)
                        {
                            queue.RemoveAt(0);
                            gameState = false;
                            randPoint = 0;
                            await ctx.RespondAsync("Not enough players in the queue, game stopped.");
                        }
                        else if (queue[0].id == randPoint)
                        {
                            queue.RemoveAt(0);
                            await queue.Shuffle();
                            randPoint = queue[queue.Count - 1].id;
                            FormatQ();
                            await ctx.RespondAsync($"{ctx.User.Username} has been removed from the queue causing a reshuffle." + Environment.NewLine + $"<@{queue[queue.Count - 1].id}> is now asking <@{queue[0].id}>." + list);
                        }
                        else
                        {
                            queue.RemoveAt(0);
                            FormatQ();
                            await ctx.RespondAsync("Askee skipped and removed from the queue." + Environment.NewLine + $"<@{queue[queue.Count - 1].id}> is now asking <@{queue[0].id}>." + list);
                        }
                    }
                    else if (queue.Count < 3)
                    {
                        queue.RemoveAt(1);
                        gameState = false;
                        randPoint = 0;
                        await ctx.RespondAsync("Not enough players in the queue, game stopped.");
                    }
                    else if (queue[1].id == randPoint)
                    {
                        randPoint = queue[0].id;
                        queue.RemoveAt(1);
                        FormatQ();
                        await ctx.RespondAsync("Asker skipped and removed from the queue. Asker replaced and shuffle moved." + Environment.NewLine + $"<@{queue[queue.Count - 1].id}> is now asking <@{queue[0].id}>." + list);
                    }
                    else
                    {
                        queue.RemoveAt(1);
                        FormatQ();
                        await ctx.RespondAsync("Asker skipped and removed from the queue. The asker has been replaced." + Environment.NewLine + $"<@{queue[queue.Count - 1].id}> is now asking <@{queue[0].id}>." + list);
                    }
                }
                else
                {
                    await ctx.RespondAsync("You may not skip if you are not in the queue.");
                }
            }
            else
            {
                await ctx.RespondAsync("The game is not currently in progress, this command is only for use in active games.");
            }
        }
        [Command("purge"), RequireRolesAttribute(RoleCheckMode.All, "Staff")]
        public async Task PurgeQueue(CommandContext ctx)
        {
            queue.Clear();
            await ctx.RespondAsync($"All users removed from the queue.");
            if (gameState == true)
            {
                gameState = false;
                randPoint = 0;
                await ctx.RespondAsync("Game stopped.");
            }
        }
        [Command("ro"), RequireRolesAttribute(RoleCheckMode.All, "Staff")]
        public async Task KickUser(CommandContext ctx, ulong userid)
        { 
            int x = queue.FindIndex(Player => Player.id == userid);
            if (-1 < x && x < queue.Count)
            {
                if (queue.Count > 2)
                {
                    if (randPoint == queue[x].id)
                    {
                        if (queue[0].id == randPoint)
                        {
                            queue.RemoveAt(0);
                            await queue.Shuffle();
                            randPoint = queue[queue.Count - 1].id;
                            FormatQ();
                            await ctx.RespondAsync($"User <@{userid}> has been removed from the queue causing a reshuffle." + Environment.NewLine + $"<@{queue[queue.Count - 1].id}> is now asking <@{queue[0].id}>." + list);
                        }
                        else
                        {
                            randPoint = queue[queue.FindIndex(Player => Player.id == randPoint) - 1].id;
                            queue.RemoveAt(queue.FindIndex(Player => Player.id == randPoint) + 1);
                            await ctx.RespondAsync($"User <@{userid}> removed from the queue.");

                        }
                    }
                    else if (x == 0 | x == queue.Count - 1)
                    {
                        queue.RemoveAt(x);
                        FormatQ();
                        await ctx.RespondAsync($"User <@{userid}> has been removed from the queue causing a change in asker/askee." + Environment.NewLine + $"So, <{queue[queue.Count - 1].id}> is now asking <@{queue[0].id}>." + list);
                    }
                    else
                    {
                        queue.RemoveAt(x);
                        await ctx.RespondAsync($"User <@{userid}> removed from the queue.");
                    }
                }
                else if (gameState == true)
                {
                    gameState = false;
                    queue.RemoveAt(x);
                    randPoint = 0;
                    await ctx.RespondAsync($"User <@{userid}> has been removed from the queue. There are no longer enough players for play to continue, the game has been stopped.");
                }
                else
                {
                    queue.RemoveAt(x);
                    await ctx.RespondAsync($"User <@{userid}> removed from the queue.");
                }
            }
            else
            {
                await ctx.RespondAsync("Invalid uid or user not in queue.");
            }
        }
        [Command("d")]
        public async Task DisplayQ(CommandContext ctx)
        {
            FormatQ();
            if (gameState == true)
            {
                await ctx.RespondAsync($"{ctx.User.Username}, the current queue is:" + list);
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Username}, the game is not currently in progress, if you wish to start the game use the p command." + Environment.NewLine + "List of users currently in queue:" + list);
            }
        }
        [Command("?"), Aliases("help")]
        public async Task Help(CommandContext ctx)
        {
            await ctx.RespondAsync("```Truth or Dare Bot" + Environment.NewLine + Environment.NewLine + "Command | Function" + Environment.NewLine + "!q      | adds you to the queue" + Environment.NewLine + "!r      | removes you from the queue" + Environment.NewLine + "!p      | starts the game" + Environment.NewLine + "!n      | advances the queue by one" + Environment.NewLine + "!d      | displays the current queue. " + Environment.NewLine + "!s      | skips the current asker and removes them from the queue unless used by the asker, in that case it removes the askee." + Environment.NewLine + Environment.NewLine + "Staff Commands:" + Environment.NewLine + "!purge  | removes all users from the queue." + Environment.NewLine + "!ro  uid | removes user from queue with specified uid.```");
        }
        [Command("Lily")]
        public async Task Lily(CommandContext ctx)
        {
            await ctx.RespondAsync("Is a bunny!");
            await ctx.RespondAsync("https://cdn.discordapp.com/attachments/826797235856998440/880219143029280798/783483021813743636.png");
        }
        [Command("Nira")]
        public async Task Nira(CommandContext ctx)
        {
            await ctx.RespondAsync("Is cute.");
        }
        public void FormatQ()
        {
            string bar = "-----------------------------------------------------";
            list = Environment.NewLine + bar + Environment.NewLine;
            for (var i = 0; i < queue.Count; i++)
            {
                if (queue[i].id == randPoint)
                {
                    list = (list + (i + 1) + ": " + queue[i].username + " :game_die:" + Environment.NewLine);
                }
                else
                {
                    list = (list + (i + 1) + ": " + queue[i].username + Environment.NewLine);
                }
            }
            list = list + bar + Environment.NewLine + "Shuffles after the player with the :game_die: is asked.";
        }
        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (active)
            {
                active = false;
            }
            else
            {
                queue.Clear();
                gameState = false;
                var msg = new DiscordMessageBuilder()
                    .WithContent($"All users removed from the queue due to inactivity.")
                    .SendAsync(channel);
                timer.Enabled = false;
            }

        }
    }
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
    static class MyExtensions
    {
        public static Task Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            };
            return Task.FromResult<object>(list);
        }
    }
}

