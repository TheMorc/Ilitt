using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Net;
using System.IO;
using System.Net.Http;
using System;
using System.Text;
using Newtonsoft.Json;

namespace Morbot
{
    class Program
    {
        public static DiscordClient discord;
        public static configJSON configuration = new configJSON();
        public static string version = "1.1";
        public class configJSON
        {
            public string DiscordBotToken { get; set; }
            public string Address { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string RespondEmoji { get; set; }
        }

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            if (!File.Exists("config.json"))
            {
                File.Create("config.json");
            }
            else
            {
                var json = "";
                using (var fs = File.OpenRead("config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();
                configuration = JsonConvert.DeserializeObject<configJSON>(json);
            }

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = configuration.DiscordBotToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            });
            discord.MessageCreated += async e =>
            {
                foreach (DiscordAttachment attachment in e.Message.Attachments)
                {
                    HttpClient cl = new HttpClient();
                    HttpResponseMessage response = await cl.GetAsync(attachment.Url);

                    using (var client = new WebClient())
                    {
                        Random rand = new Random();
                        string[] letters_nums = { "A", "a", "B", "b", "C", "d", "D", "d", "E", "e", "F", "f", "G", "g", "H", "h", "I", "i", "J", "j", "K", "k", "L", "l", "M", "m", "N", "n", "O", "o", "P", "p", "R", "r", "S", "s", "T", "t", "Q", "q", "U", "U", "V", "v", "X", "x", "Y", "y", "Z", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                        byte[] data = await response.Content.ReadAsByteArrayAsync();
                        client.Credentials = new NetworkCredential(configuration.Username, configuration.Password);
                        string generatedname = null;
                        if (attachment.Url.Remove(0, 77) == "unknown.png")
                        {
                            generatedname = letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + letters_nums[rand.Next(letters_nums.Length)] + ".png";
                        }
                        else
                        {
                            generatedname = attachment.Url.Remove(0, 77);
                        }

                        client.UploadData($"{configuration.Address}{generatedname}", "PUT", data);
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(discord, configuration.RespondEmoji));
                    }

                }
            };
            discord.Ready += async e =>
            {
                await e.Client.EditCurrentUserAsync(username: $"Ｉｌｉｔｔ [ver: {version}]");
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}