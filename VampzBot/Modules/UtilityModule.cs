using System.Threading.Tasks;

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

    }
}
