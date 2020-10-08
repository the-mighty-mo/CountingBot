using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotBase.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private const string help = "cmd\n" +
            "  - [info about cmd]";

        [Command("help")]
        public async Task HelpAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle(SecurityInfo.botName)
                .WithCurrentTimestamp();

            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Prefix")
                    .WithValue(CommandHandler.prefix +
                        "\n**or**\n" +
                        Context.Client.CurrentUser.Mention + "\n\u200b")
            };

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Commands")
                .WithValue(help);
            fields.Add(field);
            embed.WithFields(fields);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
