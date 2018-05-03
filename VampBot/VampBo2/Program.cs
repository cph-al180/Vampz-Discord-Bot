using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Discord.Net;
using Discord.WebSocket;
using Discord;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Data = Google.Apis.Sheets.v4.Data;
using System.IO;
using Google.Apis.Util.Store;
using System.Threading;

namespace VampBot2
{
    class Program
    {
        private DiscordSocketClient _client;

        private static string _googleAPIKey = "AIzaSyBLUbm3-tJ5LZ04-77dmuwTIDaIJz1ugUg";
        private static string _googleSpreadsheetId = "1-b3VEuDPxjw3ShSUOUXmnHGsm3PpFPB1RBV9zapIyZ0";
        private static bool _includeGridData = true;     

        public async Task<Data.Spreadsheet> GetSpreadSheet(List<string> ranges, bool includeGridData, string googleAPIKey, string googleSpreadsheetId)
        {
            SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                ApiKey = googleAPIKey
            });

            SpreadsheetsResource.GetRequest request = sheetsService.Spreadsheets.Get(googleSpreadsheetId);
            request.Ranges = ranges;
            request.IncludeGridData = includeGridData;

            Data.Spreadsheet response = await request.ExecuteAsync();
            return response;
        }

        public async void UpdateSpreadSheet(string updateRange, string newCellValue, string googleAPIKey, string googleSpreadsheetId)
        {
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);


                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            Data.ValueRange valueRange = new Data.ValueRange();
            valueRange.MajorDimension = "COLUMNS";

            var objectList = new List<object>() { newCellValue };
            valueRange.Values = new List<IList<object>> { objectList };

            SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "VampBot"
            });

            
            SpreadsheetsResource.ValuesResource.UpdateRequest update = sheetsService.Spreadsheets.Values.Update(valueRange, googleSpreadsheetId, updateRange);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            Data.UpdateValuesResponse result2 = await update.ExecuteAsync();
        }

        public async Task<int> GetLastRowSpreadSheet()
        {
            List<string> ranges = new List<string>();
            Data.Spreadsheet spreadSheet = await GetSpreadSheet(ranges, _includeGridData, _googleAPIKey, _googleSpreadsheetId);

            return spreadSheet.Sheets[1].Data[0].RowData.Count;
        }

        public async Task<string> GetCurrentNodeWarDate()
        {
            List<string> ranges = new List<string>();
            Data.Spreadsheet spreadSheet = await GetSpreadSheet(ranges, _includeGridData, _googleAPIKey, _googleSpreadsheetId);

            var row = spreadSheet.Sheets[0].Data[0].RowData.Where(x => x.Values[1].UserEnteredValue.NumberValue == 1).First();

            if (row.Values.Count <= 1)
                return "none";
            else
            {
                return row.Values[0].UserEnteredValue.StringValue;
            }

        }

        public async Task<bool> IsNameAlreadySigned(string name, string nodewardate)
        {
            bool alreadySigned = true;

            List<string> signedUsers = await GetSignedMembers(nodewardate);
            if (!signedUsers.Contains(name))
            {
                alreadySigned = false;
            }

            return alreadySigned;
        }

        public async Task<List<string>> GetSignedMembers(string nodeWarDate)
        {
            List<string> signedUsers = new List<string>();
            List<string> ranges = new List<string>();

            Data.Spreadsheet spreadSheet = await GetSpreadSheet(ranges, _includeGridData, _googleAPIKey, _googleSpreadsheetId);

            signedUsers = spreadSheet.Sheets[1].Data[0].RowData.Where(x => x.Values[0].UserEnteredValue.StringValue == nodeWarDate).Select(x => x.Values[1].UserEnteredValue.StringValue).ToList();

            return signedUsers;
        }

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(
                 new DiscordSocketConfig()
                 {
                     LogLevel = LogSeverity.Verbose,
                     WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
                 });

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            string token = "NDE2NzM1MzQ4MjYxOTEyNTg2.DcqMXg.Go2c-GOmLGCwxd1vbuHHQzgHhxg";

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;

            if (message.Content == "!merc")
            {
                await message.Channel.SendMessageAsync("merc is a fag");
            }

            if (message.Content == "!nextnw")
            {
                string currentNodeWarDate = await GetCurrentNodeWarDate();

                if (currentNodeWarDate == "none")
                {
                    await message.Channel.SendMessageAsync("No active nodewar scheduled");
                }

                else
                {

                    var builder = new EmbedBuilder();
                    builder.WithTitle("NodeWar Schedule");
                    builder.AddInlineField("Date", currentNodeWarDate);
                    builder.AddInlineField("Participants", "<unknown>");
                    builder.AddInlineField("Confirmed Participants", "<unknown>");
                    builder.WithColor(Color.DarkRed);

                    await message.Channel.SendMessageAsync("", false, builder);
                }

            }

            if (message.Content.StartsWith("!joinnw"))
            {
                string currentNodeWarDate = await GetCurrentNodeWarDate();

                if (currentNodeWarDate == "none")
                {
                    await message.Channel.SendMessageAsync("No active nodewar scheduled");
                }

                else
                {
                    if (!(await IsNameAlreadySigned(user.Nickname, currentNodeWarDate)))
                    {
                        int appendRow = (await GetLastRowSpreadSheet()) + 1;
                        string[] stringList = message.Content.Split(new char[0]);
                        UpdateSpreadSheet("registrations!A" + appendRow, currentNodeWarDate, _googleAPIKey, _googleSpreadsheetId);
                        UpdateSpreadSheet("registrations!B" + appendRow, user.Nickname, _googleAPIKey, _googleSpreadsheetId);
                        UpdateSpreadSheet("registrations!C" + appendRow, stringList[1], _googleAPIKey, _googleSpreadsheetId);
                        UpdateSpreadSheet("registrations!D" + appendRow, stringList[2], _googleAPIKey, _googleSpreadsheetId);
                        UpdateSpreadSheet("registrations!E" + appendRow, stringList[3], _googleAPIKey, _googleSpreadsheetId);

                        var builder = new EmbedBuilder();
                        builder.WithTitle("Registered for NodeWar");
                        builder.AddInlineField("Date", currentNodeWarDate);
                        builder.AddInlineField("Name", user.Nickname);
                        builder.AddInlineField("GearScore", stringList[1]);
                        builder.AddInlineField("Class", stringList[2]);
                        builder.AddInlineField("Level", stringList[3]);
                        builder.WithColor(Color.Red);

                        await message.Channel.SendMessageAsync("", false, builder);
                    }
                }
            }
            
            if (message.Content == "!signups")
            {
                string currentNodeWarDate = await GetCurrentNodeWarDate();

                if (currentNodeWarDate == "none")
                {

                }
                else
                {
                    List<string> signedMembers = await GetSignedMembers(currentNodeWarDate);
                    var builder = new EmbedBuilder();
                    builder.WithTitle("NodeWar "+ currentNodeWarDate + " signups");
                    signedMembers.ForEach(x =>
                    {
                        builder.AddInlineField("Name", x);
                    });
                    builder.WithColor(Color.Red);

                    await message.Channel.SendMessageAsync("", false, builder);
                }
                
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
