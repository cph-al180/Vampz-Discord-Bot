﻿using System.Collections.Generic;
using System.Threading.Tasks;

using VampzBot.Logic;

using Discord;
using Discord.Commands;
using System;
using VampzBot.Models;
using System.Linq;

namespace VampzBot.Modules
{
    public class NodeWarModule : ModuleBase<SocketCommandContext>
    {

        [Command("joinnw")]
        public async Task JoinNodeWar(string gearScore, string playerClass, string level)
        {
            var user = (IGuildUser)Context.User;
            string currentNodeWarDate = await GoogleSheetsHandler.GetCurrentNodeWarDate();

            if (currentNodeWarDate == "none")
            {
                await ReplyAsync("No active nodewar scheduled");
            }

            if(IsClassViable(playerClass) == false)
            {
                var errorBuilder = new EmbedBuilder();
                var classes = Enum.GetValues(typeof(CharacterClass));
                string classesString = "";
                errorBuilder.WithTitle("Bad character class input. Please use an existing class.");
                foreach (CharacterClass c in classes)
                    classesString += c.ToString() + "\n";
                errorBuilder.AddField("Character classes", classesString);
                await ReplyAsync(Context.Message.Author.Mention, false, errorBuilder);
            }

            else
            {
                if (!(await GoogleSheetsHandler.IsNameAlreadySigned(user.Nickname, currentNodeWarDate)))
                {
                    int appendRow = (await GoogleSheetsHandler.GetLastRowSpreadSheet()) + 1;

                    AddSignUpToSpreadSheet("registrations!A" + appendRow, currentNodeWarDate);
                    AddSignUpToSpreadSheet("registrations!B" + appendRow, user.Nickname);
                    AddSignUpToSpreadSheet("registrations!C" + appendRow, gearScore);
                    AddSignUpToSpreadSheet("registrations!D" + appendRow, playerClass);
                    AddSignUpToSpreadSheet("registrations!E" + appendRow, level);

                    var builder = new EmbedBuilder();
                    builder.WithTitle("Registered for NodeWar");
                    builder.AddField("Date", Utility.FormatGoogleSheetDate(currentNodeWarDate));
                    builder.AddField("Name", user.Nickname);
                    builder.AddField("GearScore", gearScore);
                    builder.AddField("Class", playerClass);
                    builder.AddField("Level", level);
                    builder.WithColor(Color.Red);

                    await ReplyAsync("", false, builder);
                }
                else
                {
                    await ReplyAsync(Context.Message.Author.Mention + " You are already signed up for the Node War");
                }
            }
        }

        [Command("signups")]
        public async Task ViewSignUps()
        {
            string currentNodeWarDate = await GoogleSheetsHandler.GetCurrentNodeWarDate();
            string participants = "";

            if (currentNodeWarDate == "none")
            {
                await ReplyAsync(Context.Message.Author.Mention + " No Node War is currently planned");
            }
            else
            {
                List<string> signedMembers = await GoogleSheetsHandler.GetSignedMembers(currentNodeWarDate);
                var builder = new EmbedBuilder();
                builder.WithTitle("NodeWar " + Utility.FormatGoogleSheetDate(currentNodeWarDate));
                signedMembers.ForEach(x =>
                {
                    participants += "\n"+x;
                });
                builder.AddField("Signups", participants);
                builder.WithColor(Color.Red);

                await ReplyAsync("", false, builder);
            }
        }

        [Command("nextnw")]
        public async Task NextNodeWar()
        {
            var builder = new EmbedBuilder();
            string currentNodeWarDate = await GoogleSheetsHandler.GetCurrentNodeWarDate();

            builder.WithTitle("NodeWar Schedule");
            builder.AddField("Date", Utility.FormatGoogleSheetDate(currentNodeWarDate));
            builder.WithColor(Color.DarkRed);

            await ReplyAsync("", false, builder);
        }

        //WIP
        [Command("signoff")]
        public async Task SignOffNodeWar()
        {
            var user = (IGuildUser)Context.User;
            string nickname = user.Nickname;
            string date = await GoogleSheetsHandler.GetCurrentNodeWarDate();
            List<string> signedUsers = await GoogleSheetsHandler.GetSignedMembers(date);

            if (!await GoogleSheetsHandler.IsNameAlreadySigned(nickname, date))
            {
                await ReplyAsync(Context.Message.Author.Mention + " You have not signed up yet");
            }
            else
            {
                int row = 0;
                foreach(string signedUser in signedUsers)
                {
                    if (signedUser.Equals(nickname))
                    {
                        //delete
                    }
                    row++;
                }
            }
        }

        private void AddSignUpToSpreadSheet(string updateRange, string input)
        {
            GoogleSheetsHandler.UpdateSpreadSheet(updateRange, input, GoogleSheetsHandler._googleAPIKey, GoogleSheetsHandler._googleSpreadsheetId);
        }

        private bool IsClassViable(string input)
        {
            var classes = Enum.GetValues(typeof(CharacterClass));
            foreach(CharacterClass c in classes)
            {
                string lowerCase = c.ToString().ToLower();
                if (input.Equals(c.ToString()) || input.Equals(lowerCase))
                {
                    return true;
                }
            }
            return false;
        }

    }
}