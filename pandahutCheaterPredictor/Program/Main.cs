using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

class Program {
    
    static async Task CheckStats(string id, string serverType, bool checkHeadShots, bool checkKD, float headshotRate, float maxKdRatio) {
        using (HttpClient client = new HttpClient()) {
            try {
                HttpResponseMessage response = await client.GetAsync($"https://api.pandahut.net/api/KillStats/{serverType}/{id}");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                JObject jsonObject = JObject.Parse(responseBody);
                float kills = (int)jsonObject.GetValue("kills");
                float headshots = (int)jsonObject.GetValue("headshots");
                float deaths = (int)jsonObject.GetValue("deaths");

                if (headshots == 0) {
                    return;
                }

                if (kills == 0) {
                    return;
                }

                if (deaths == 0) {
                    return;
                }
                
                if (checkHeadShots) {
                    if (headshots / kills > headshotRate && headshots / kills < 1) {
                        Console.WriteLine($"\n\n[-] Player has a high headshot rate\n[-] SteamID: {id} Kills: {kills} Headshots: {headshots} Rate: {headshots / kills}% \n");
                    }
                    else {
                        Console.Write(".");
                    }
                }

                if (checkKD) {
                    if (kills/deaths > maxKdRatio) {

                        Console.WriteLine($"\n\n[-] Player has a KD ratio\n[-] SteamID: {id} Kills: {kills} Headshots: {headshots} KD: {kills/deaths} \n");
                    }
                    else {
                        Console.Write(".");
                    }
                }
            }
            catch (HttpRequestException ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    static async Task Main() {

        Console.WriteLine("[+] Currently supported servers #4, #19, #26, #30 or type exit to close");
        Console.WriteLine("[+] Press CTRL + C to close at anytime, highlight and press enter to copy");
        Console.Write("[+] Input server number: ");
        string inputS = Console.ReadLine();

        Console.Write($"[+] Check for headshots true/false: ");
        bool checkHeadshots = bool.Parse(Console.ReadLine());

        float headshotRate = 0;
        if (checkHeadshots) {
            Console.Write($"[+] Input headshot rate as a decimal: ");
            headshotRate = float.Parse(Console.ReadLine());
        }

        Console.Write($"[+] Check for KD true/false: ");
        bool checkKD = bool.Parse(Console.ReadLine());

        float maxKdRatio = 0;
        if (checkKD) {
            Console.Write($"[+] Input KD ratio as a decimal: ");
            maxKdRatio = float.Parse(Console.ReadLine());
        }

        if (inputS.ToLower() == "exit") {
            return;
        }

        if (!checkKD || !checkHeadshots) {
            Console.Write($"\n\n\n\n\n");
            await Main();
        }

        int input = Convert.ToInt32(inputS);
        string serverNumber = "0";
        string serverType = "0";

        Console.Write($"\n[+] Fun fact each dot is a player without a high headshot rate!!\n\n[-] Results:\n");

        switch (input) {
            case 4:
                serverNumber = "60ac00235c6e0392f933bae4";
                serverType = "Semi-Vanilla";
                break;
            case 19:
                serverNumber = "625b796a6bceb9c2a0636a82";
                serverType = "Unturnov";
                break;
            case 26:
                serverNumber = "625b796f6bceb9c2a0636ab2";
                serverType = "VanillaPlus";
                break;
            case 30:
                serverNumber = "60f3fee2ff0fa5c5f22b68c1";
                serverType = "Unturnov";
                break;
        }

        if (serverNumber == null ) {
            return;
        }

        using (HttpClient client = new HttpClient()) {
        try {
            HttpResponseMessage response = await client.GetAsync($"https://api.pandahut.net/api/ServerProxy/PublicServerStatus/{serverNumber}");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            List<string> idList = ParseIdsFromJson(responseBody);

            foreach (string id in idList) {
                await CheckStats(id, serverType, checkHeadshots, checkKD, headshotRate, maxKdRatio);
            }

                Console.WriteLine($"\n\nAll players checked!!!!\n\n\n\n\n");
            }
            catch (HttpRequestException ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        //Console.Read();

        await Main();
    }

    static List<string> ParseIdsFromJson(string jsonString) {
        List<string> idList = new List<string>();

        JObject jsonObject = JObject.Parse(jsonString);

        JArray playersArray = (JArray)jsonObject["Players"];

        if (playersArray != null) {
            foreach (JObject playerObject in playersArray) {
                string id = (string)playerObject["ID"];
                if (!string.IsNullOrEmpty(id)) {
                    idList.Add(id);
                }
            }
        }

        return idList;
    }
}