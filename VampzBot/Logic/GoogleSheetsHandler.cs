using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using System.Linq;

using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace VampzBot.Logic
{
    public static class GoogleSheetsHandler
    {
        public static string _googleAPIKey;
        public static string _googleSpreadsheetId;
        private static bool _includeGridData = true;

        public static async Task<Spreadsheet> GetSpreadSheet(List<string> ranges)
        {
            SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                ApiKey = _googleAPIKey
            });

            SpreadsheetsResource.GetRequest request = sheetsService.Spreadsheets.Get(_googleSpreadsheetId);
            request.Ranges = ranges;
            request.IncludeGridData = _includeGridData;

            Spreadsheet response = await request.ExecuteAsync();
            return response;
        }

        public static async void UpdateSpreadSheet(string updateRange, string newCellValue, string googleAPIKey, string googleSpreadsheetId)
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
                ValueRange valueRange = new ValueRange();
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
                UpdateValuesResponse result2 = await update.ExecuteAsync();
            }
        }

        public static async Task<int> GetLastRowSpreadSheet()
        {
            List<string> ranges = new List<string>();
            Spreadsheet spreadSheet = await GetSpreadSheet(ranges);

            return spreadSheet.Sheets[1].Data[0].RowData.Count;
        }

        public static async Task<string> GetCurrentNodeWarDate()
        {
            List<string> ranges = new List<string>();
            Spreadsheet spreadSheet = await GetSpreadSheet(ranges);

            var row = spreadSheet.Sheets[0].Data[0].RowData.Where(x => x.Values[1].UserEnteredValue.NumberValue == 1).First();

            if (row.Values.Count <= 1)
                return "none";
            else
            {
                return row.Values[0].UserEnteredValue.StringValue;
            }

        }

        public static async Task<bool> IsNameAlreadySigned(string name, string nodewardate)
        {
            bool alreadySigned = true;

            List<string> signedUsers = await GetSignedMembers(nodewardate);
            if (!signedUsers.Contains(name))
            {
                alreadySigned = false;
            }

            return alreadySigned;
        }

        public static async Task<List<string>> GetSignedMembers(string nodeWarDate)
        {
            List<string> signedUsers = new List<string>();
            List<string> ranges = new List<string>();

            Spreadsheet spreadSheet = await GetSpreadSheet(ranges);

            signedUsers = spreadSheet.Sheets[1].Data[0].RowData.Where(x => x.Values[0].UserEnteredValue.StringValue == nodeWarDate).Select(x => x.Values[1].UserEnteredValue.StringValue).ToList();

            return signedUsers;
        }
    }
}
