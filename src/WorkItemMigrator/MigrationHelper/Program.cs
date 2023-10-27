using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MigrationHelper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Swami's Organization credentials
            string devOps_PAT = "<personal access token>";
            string devOps_Organization_Name = "<DevOps Organization Name>";
            string devOps_Project_Name = "<DevOps Project Name>";
            int devops_WorkItem_Id = 1; //DevOps workitem id

            string folderPath = "<Path of the folder you want your files to be generated in>";

            //await GenerateRepositoryMapJsonFile(devOps_Organization_Name, devOps_Project_Name, devOps_PAT, folderPath);

            //await GenerateAdoWorkItemDetailsJsonFile(devOps_Organization_Name, devops_WorkItem_Id, devOps_PAT, folderPath);
        }

        public static async Task GenerateRepositoryMapJsonFile(string organizationName, string projectName, string PAT, string folderPath)
        {
            string apiUrl = $"https://dev.azure.com/{organizationName}/_apis/Contribution/HierarchyQuery?api-version=5.0-preview.1";

            // Create a JSON payload
            string jsonPayload = $@"{{
                                      ""contributionIds"": [
                                        ""ms.vss-work-web.azure-boards-external-connection-data-provider""
                                      ],
                                      ""dataProviderContext"": {{
                                        ""properties"": {{
                                          ""includeInvalidConnections"": true,
                                          ""sourcePage"": {{
                                            ""url"": ""https://dev.azure.com/{organizationName}/JiraToDevOpsMigration/_settings/boards-external-integration"",
                                            ""routeId"": ""ms.vss-admin-web.project-admin-hub-route"",
                                            ""routeValues"": {{
                                              ""project"": ""{projectName}"",
                                              ""adminPivot"": ""boards-external-integration"",
                                              ""controller"": ""ContributedPage"",
                                              ""action"": ""Execute""
                                            }}
                                          }}
                                        }}
                                      }}
                                    }}";

            List<RepositoryItem> repositories = new List<RepositoryItem>();

            using (HttpClient client = new HttpClient())
            {
                // Set the authorization header using your personal access token
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{PAT}")));

                // Create the HttpContent with JSON payload
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    string responseBodyString = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON into a JObject
                    JObject responseBody = JObject.Parse(responseBodyString);

                    var hasReadPermission = (bool)responseBody["dataProviders"]["ms.vss-work-web.azure-boards-external-connection-data-provider"]["hasReadPermission"];
                    if (!hasReadPermission)
                    {
                        Console.WriteLine("Error: User doesn't have read permission.");
                        return;
                    }

                    var externalConnections = responseBody["dataProviders"]["ms.vss-work-web.azure-boards-external-connection-data-provider"]
                        ["externalConnections"];

                    foreach (var externalConnection in externalConnections)
                    {
                        // Navigate to the "externalGitRepos" array
                        var externalGitRepos = externalConnection["externalGitRepos"];

                        foreach (var externalGitRepo in externalGitRepos)
                        {
                            var repoNameWithOwner = externalGitRepo["additionalProperties"]["repoNameWithOwner"];
                            var repoInternalId = externalGitRepo["additionalProperties"]["repoInternalId"];
                            repositories.Add(new RepositoryItem() { source = (string)repoNameWithOwner, target = (string)repoInternalId });
                        }
                    }

                    if (repositories.Count > 0)
                    {
                        // Specify the file path for the JSON file
                        string filePath = $"{folderPath}\\repository-map.json";

                        var jsonObject = new
                        {
                            repository = repositories
                        };

                        string jsonString = JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions
                        {
                            WriteIndented = true // for pretty-printing
                        });

                        // Write the JSON string to a file
                        File.WriteAllText(filePath, jsonString);

                        Console.WriteLine($"JSON data has been written to {filePath}");
                    }
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                }
            }
        }

        public static async Task GenerateAdoWorkItemDetailsJsonFile(string organizationName, int workItemId, string PAT, string folderPath)
        {
            using (HttpClient client = new HttpClient())
            {
                // Set the base URL for Azure DevOps REST API
                client.BaseAddress = new Uri($"https://dev.azure.com/{organizationName}/_apis/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{PAT}")));

                // Define the API endpoint to get work item details
                string apiEndpoint = $"wit/workitems/{workItemId}?$expand=all&api-version=7.1-preview.3";

                HttpResponseMessage response = await client.GetAsync(apiEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Specify the file path for the JSON file
                    string filePath = $"{folderPath}\\AdoWorkItem_{workItemId}.json";

                    string formattedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(responseBody), new JsonSerializerOptions
                    {
                        WriteIndented = true // Pretty-print the JSON
                    });

                    // Write the JSON string to a file
                    File.WriteAllText(filePath, formattedJson);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }
    }

    class RepositoryItem
    {
        public string source { get; set; }
        public string target { get; set; }
    }
}
