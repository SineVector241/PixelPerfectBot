using Newtonsoft.Json;

namespace PixelPerfectBot
{
    public class Config
    {
        private const string _configFolder = "Resources";
        private const string _configFile = "config.json";
        public static BotConfig BotConfiguration;

        static Config()
        {
            try
            {
                if (!Directory.Exists(_configFolder))
                {
                    Directory.CreateDirectory(_configFolder);
                }

                if (!File.Exists(_configFolder + "/" + _configFile))
                {
                    BotConfiguration = new BotConfig();
                    string botConfigJson = JsonConvert.SerializeObject(BotConfiguration, Formatting.Indented);
                    File.WriteAllText(_configFolder + "/" + _configFile, botConfigJson);
                }
                else
                {
                    string botConfigJson = File.ReadAllText(_configFolder + "/" + _configFile);
                    BotConfiguration = JsonConvert.DeserializeObject<BotConfig>(botConfigJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[31mERROR\u001b[97m] => An error occured in Config.cs \nError Info:\n{ex}");
            }
        }

        public void UpdateConfig()
        {
            string botConfigJson = JsonConvert.SerializeObject(BotConfiguration, Formatting.Indented);
            File.WriteAllText(_configFolder + "/" + _configFile, botConfigJson);
        }

        public struct BotConfig
        {
            public string Token { get; set; }
            public ulong GeneralSupportCategory { get; set; }
            public ulong ContentSupportCategory { get; set; }
            public ulong MCBEServerSupportCategory { get; set; }
            public ulong ApplicationChannel { get; set; }
            public ulong TicketLoggingChannel { get; set; }
            public ulong VIPApplicationChannel { get; set; }
            public ulong SuggestionChannel { get; set; }
            public ulong TopSuggestionChannel { get; set; }
            public ulong PollChannel { get; set; }

            public ulong ContentCreatorRoleId { get; set; }
            public ulong ChatModRoleId { get; set; }
            public ulong TicketManagerRoleId { get; set; }
            public ulong VIPRoleId { get; set; }
        }
    }
}
