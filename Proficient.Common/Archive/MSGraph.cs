using Autodesk.Revit.DB;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Proficient.Archive
{
    class MSGraph
    {
        private static GraphConfig config;
        private const string configFile = Names.File.CommonSettings;

        public static async void OpenKNFile(string pn)
        {
            GraphServiceClient graphClient = await GetGraphClient();
            var file = await GetKNFile(graphClient, pn);
            System.Diagnostics.Process.Start(file.WebUrl);
        }

        public static async Task<List<KeynoteEntry>> GetKNData(string pn)
        {
            List<KeynoteEntry> knList = new List<KeynoteEntry>();
            GraphServiceClient graphClient = await GetGraphClient();

            var curFile = await GetKNFile(graphClient, pn);

            var session = await graphClient.Groups[config.AllMorrisseyGroupId].Drive.Items[curFile.Id].Workbook
                .CreateSession(false)
                .Request()
                .PostAsync();

            var xlFile = await graphClient.Groups[config.AllMorrisseyGroupId].Drive.Items[curFile.Id].Workbook.Worksheets
                .Request()
                .Header("workbook-session-id", $"{session.Id}")
                .GetAsync();

            foreach (var ws in xlFile)
            {
                WorkbookRange rng = new WorkbookRange();
                try
                {

                    rng = await graphClient.Groups[config.AllMorrisseyGroupId].Drive.Items[curFile.Id].Workbook.Worksheets[ws.Id].Range().UsedRange()
                        .Request()
                        .Header("workbook-session-id", $"{session.Id}")
                        .GetAsync();

                }
                catch 
                {
                    Console.WriteLine($"No used range in {ws.Name}");
                    continue;
                    
                }
                knList.Add(new KeynoteEntry(ws.Name, string.Empty));
                foreach (var row in rng.Values.RootElement.EnumerateArray())
                {
                    if (row.GetArrayLength() >= 2)
                    {
                        string knNum = row[0].ToString();
                        string knTxt = row[1].ToString();
                        if (knNum != null && knNum != string.Empty && knTxt != null && knTxt != string.Empty)
                        {
                            try
                            {
                                knList.Add(new KeynoteEntry(knNum, ws.Name, knTxt));
                            }
                            catch { }
                        } 
                    }
                        
                }
            }

            await graphClient.Groups[config.AllMorrisseyGroupId].Drive.Items[curFile.Id].Workbook
                .CloseSession()
                .Request()
                .Header("workbook-session-id", $"{session.Id}")
                .PostAsync();

            return knList;
        }

        private static async Task<DriveItem> GetKNFile(GraphServiceClient graphClient, string pn)
        {
            var files = await graphClient.Groups[config.AllMorrisseyGroupId].Drive.Items[config.KeynoteFolderId].Children
                .Request()
                .GetAsync();

            if (files.Where(f => f.Name == $"{pn}.xlsx").Count() > 0)
            {
                return files.Where(f => f.Name == $"{pn}.xlsx").FirstOrDefault();
            }
            else
            {
                var name = $"{pn}.xlsx";

                ItemReference knFolderRef = new ItemReference
                {
                    DriveId = config.KeynoteFolderDriveId,
                    Id = config.KeynoteFolderId
                };

                await graphClient.Groups[config.AllMorrisseyGroupId].Drive.Items[config.TemplateId]
                            .Copy(name, knFolderRef)
                            .Request()
                            .PostAsync();

                do
                {
                    files = await graphClient.Groups[config.AllMorrisseyGroupId].Drive.Items[config.KeynoteFolderId].Children
                        .Request()
                        .GetAsync();
                }
                while (files.Where(f => f.Name == $"{pn}.xlsx").Count() == 0);

                return files.Where(f => f.Name == $"{pn}.xlsx").FirstOrDefault();
            }
        }

        private async static Task<GraphServiceClient> GetGraphClient()
        {
            string json = System.IO.File.ReadAllText(configFile);
            config = JsonConvert.DeserializeObject<GraphConfig>(json);

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                .Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();

            string[] scopes = new string[] { $"{config.ApiUrl}.default" };

            AuthenticationResult authResult = await app.AcquireTokenForClient(scopes)
                                                        .ExecuteAsync();

            GraphServiceClient graphClient = new GraphServiceClient("https://graph.microsoft.com/v1.0/",
                new DelegateAuthenticationProvider((requestMessage) => {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", authResult.AccessToken);
                    return Task.CompletedTask;
                }));


            return await Task.FromResult(graphClient);
        }
    }
}
