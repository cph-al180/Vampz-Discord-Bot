using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace VampzBot.Modules
{
    public class UtilityModule : ModuleBase<SocketCommandContext>
    {

        [Command("ping")]
        public async Task NextNodeWar()
        {
            await ReplyAsync("Pong!");
        }

        [Command("help")]
        public async Task Help()
        {
            var builder = new EmbedBuilder();

            builder.WithTitle("Commands");
            builder.AddField("!joinnw [Gear Score] [Class] [Level]", "Signs you up for the up and coming Node War");
            builder.AddField("!signups", "View all sign ups for the up and coming Node War");
            builder.AddField("!nextnw", "Shows the Date and Time of the up and coming Node War");
            builder.WithColor(Color.DarkRed);

            await ReplyAsync("", false, builder);
        }

        [Command("joinnw")]
        public async Task JoinNodeWar(string input)
        {
            await ReplyAsync(Context.Message.Author.Mention + " Incorret format. Please use: !joinnw [Gear Score] [Class] [Level]");
        }

        [Command("joinnw")]
        public async Task JoinNodeWar(string input1, string input2)
        {
            await ReplyAsync(Context.Message.Author.Mention + " Incorret format. Please use: !joinnw [Gear Score] [Class] [Level]");
        }

        [Command("joinnw")]
        public async Task JoinNodeWar(string input1, string input2, string input3, string input4)
        {
            await ReplyAsync(Context.Message.Author.Mention + " Incorret format. Please use: !joinnw [Gear Score] [Class] [Level]");
        }

        [Command("joinnw")]
        public async Task JoinNodeWar()
        {
            await ReplyAsync(Context.Message.Author.Mention + " Incorret format. Please use: !joinnw [Gear Score] [Class] [Level]");
        }

    }
}
