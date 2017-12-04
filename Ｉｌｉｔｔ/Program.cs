using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Net;
using System.IO;
using System.Net.Http;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Xml;
using System.Collections.Generic;

namespace Morbot
{
    class Program
    {
        public static DiscordClient discord;
        public static ConfigJSON configuration = new ConfigJSON();
        public static string version = "1.1.1";
        public class ConfigJSON
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
                configuration = JsonConvert.DeserializeObject<ConfigJSON>(json);
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
                NetworkCredential namepass = new NetworkCredential(configuration.Username, configuration.Password);
                foreach (DiscordAttachment attachment in e.Message.Attachments)
                {
                    Random rand = new Random();
                    string[] letters_nums = { "A", "a", "B", "b", "C", "d", "D", "d", "E", "e", "F", "f", "G", "g", "H", "h", "I", "i", "J", "j", "K", "k", "L", "l", "M", "m", "N", "n", "O", "o", "P", "p", "R", "r", "S", "s", "T", "t", "Q", "q", "U", "U", "V", "v", "X", "x", "Y", "y", "Z", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                    var listo = new List<string>();
                    HttpClient cl = new HttpClient();
                    HttpResponseMessage response = await cl.GetAsync(attachment.Url);
                    string generatedname = null;
                    HttpWebRequest hwrequest = (HttpWebRequest)WebRequest.Create(configuration.Address);
                    hwrequest.Method = $"PROPFIND";
                    hwrequest.Credentials = namepass;
                    using (HttpWebResponse hwresponse = (HttpWebResponse)hwrequest.GetResponse())
                    {
                        using (Stream stream = hwresponse.GetResponseStream())
                        {
                            XmlDocument xml = new XmlDocument();
                            xml.Load(stream);
                            XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(xml.NameTable);
                            xmlNsManager.AddNamespace("d", "DAV:");

                            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                            {
                                XmlNode xmlNode = node.SelectSingleNode("d:href", xmlNsManager);
                                listo.Add(Uri.UnescapeDataString(xmlNode.InnerXml.Remove(0, 19)));

                            }

                            var arrayo = listo.ToArray();
                            generatedname = attachment.FileName;
                            foreach (string name in arrayo)
                            {
                                if (attachment.FileName == name)
                                {
                                    generatedname = $"{attachment.FileName.Remove(attachment.FileName.Length - 4, 4)}_{letters_nums[rand.Next(letters_nums.Length)]}{letters_nums[rand.Next(letters_nums.Length)]}{letters_nums[rand.Next(letters_nums.Length)]}{letters_nums[rand.Next(letters_nums.Length)]}{letters_nums[rand.Next(letters_nums.Length)]}{attachment.FileName.Remove(0, attachment.FileName.Length - 4)}";
                                }
                            }

                            using (var client = new WebClient())
                            {
                                byte[] data = await response.Content.ReadAsByteArrayAsync();
                                client.Credentials = namepass;
                                client.UploadData($"{configuration.Address}{generatedname}", "PUT", data);
                                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(discord, configuration.RespondEmoji));
                            }
                        }
                    }
                }
            };
            discord.Ready += async e =>
                {
                    DiscordActivity game = new DiscordActivity
                    {
                        Name = "uping images to cloud!",
                        ActivityType = ActivityType.Playing
                    };
                    await discord.UpdateStatusAsync(game);
                };
            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}