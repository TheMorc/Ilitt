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
        public static string version = "1.1.2";
        public static int File_Count = 0;
        public static NetworkCredential namepass = null;
        public static List<string> listo = new List<string>();
        public static Array arrayo = null;
        public static DiscordChannel tempChannel;
        public static DiscordMember owner;

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

        public static void getFiles()
        {
            File_Count = 0;
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
                        File_Count++;
                    }
                    arrayo = listo.ToArray();
                }
            }

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
            namepass = new NetworkCredential(configuration.Username, configuration.Password);
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = configuration.DiscordBotToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            });
            discord.ClientErrored += async e =>
            {
                await owner.SendMessageAsync(e.Exception.Message);
            };
            discord.MessageCreated += async e =>
            {
                tempChannel = e.Channel;
                if (e.Message.Content.ToLower().StartsWith("ilitt_exclude"))
                {
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":ilitt:"));
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":x:"));
                }
                else if (e.Message.Content.ToLower().StartsWith("ilitt_info"))
                {
                    getFiles();
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":ilitt:"));
                    await e.Message.RespondAsync($"**Ｉｌｉｔｔ** is a bot which has only one thing to do, and so to upload images to **cloud** using **WebDAV**.\nFile count on cloud: `{File_Count}`\n\n**Made in Slovakia** by: {discord.CurrentApplication.Owner.Mention} with **D#+ Library**.\n**D#+ Library Version:** `{discord.VersionString}`");
                }
                else
                {
                    foreach (DiscordAttachment attachment in e.Message.Attachments)
                    {
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":ilitt:"));
                        Random rand = new Random();
                        string[] letters_nums = { "A", "a", "B", "b", "C", "d", "D", "d", "E", "e", "F", "f", "G", "g", "H", "h", "I", "i", "J", "j", "K", "k", "L", "l", "M", "m", "N", "n", "O", "o", "P", "p", "R", "r", "S", "s", "T", "t", "Q", "q", "U", "U", "V", "v", "X", "x", "Y", "y", "Z", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                        HttpClient cl = new HttpClient();
                        HttpResponseMessage response = await cl.GetAsync(attachment.Url);
                        string generatedname = attachment.FileName;
                        getFiles();
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
            discord.GuildAvailable += async e =>
            {

                foreach (DiscordMember member in e.Guild.Members)
                {
                    if (member.Id == e.Client.CurrentApplication.Owner.Id)
                    {
                        owner = member;
                    }
                }

            };
            await Task.Delay(-1);

        }
    }
}