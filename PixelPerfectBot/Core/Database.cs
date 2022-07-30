using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PixelPerfectBot.Core
{
    public class Database
    {
        private const string _dbFolder = "Resources";
        private const string _dbFile = "Database.json";
        public static DatabaseData? DBData;

        public Database()
        {
            try
            {
                if (!Directory.Exists(_dbFolder))
                {
                    Directory.CreateDirectory(_dbFolder);
                }

                if (!File.Exists(_dbFolder + "/" + _dbFile))
                {
                    DBData = new DatabaseData();
                    string botConfigJson = JsonConvert.SerializeObject(DBData, Formatting.Indented);
                    File.WriteAllText(_dbFolder + "/" + _dbFile, botConfigJson);
                }
                else
                {
                    string botConfigJson = File.ReadAllText(_dbFolder + "/" + _dbFile);
                    DBData = JsonConvert.DeserializeObject<DatabaseData>(botConfigJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[31mERROR\u001b[97m] => An error occured in Database.cs \nError Info:\n{ex}");
            }
        }

        public void CreateTicket(int TicketType, ulong UserId)
        {
            Ticket ticket = new Ticket
            {
                Type = TicketType,
                UserId = UserId
            };

            if (DBData.Tickets.Exists(x => x.UserId == ticket.UserId && x.Type == ticket.Type))
                return;
            DBData.Tickets.Add(ticket);

            string botConfigJson = JsonConvert.SerializeObject(DBData, Formatting.Indented);
            File.WriteAllText(_dbFolder + "/" + _dbFile, botConfigJson);
        }

        public void DeleteTicket(int TicketType, ulong UserId)
        {
            if (DBData.Tickets.Exists(x => x.UserId == UserId && x.Type == TicketType))
            {
                DBData.Tickets.Remove(DBData.Tickets.Find(x => x.UserId == UserId && x.Type == TicketType));
                string botConfigJson = JsonConvert.SerializeObject(DBData, Formatting.Indented);
                File.WriteAllText(_dbFolder + "/" + _dbFile, botConfigJson);
            }
        }

        public User? GetUser(ulong Id)
        {
            return DBData.Users.Find(x => x.Id == Id);
        }

        public void CreateUserIfNotExists(ulong Id)
        {
            if(!DBData.Users.Exists(x => x.Id == Id))
            {
                DBData.Users.Add(new User()
                {
                    Id = Id
                });
                string botConfigJson = JsonConvert.SerializeObject(DBData, Formatting.Indented);
                File.WriteAllText(_dbFolder + "/" + _dbFile, botConfigJson);
            }
        }

        public void UpdateUser(User user)
        {
            var data = DBData.Users.FirstOrDefault(x => x.Id == user.Id);
            data = user;
            string botConfigJson = JsonConvert.SerializeObject(DBData, Formatting.Indented);
            File.WriteAllText(_dbFolder + "/" + _dbFile, botConfigJson);
        }

        public class DatabaseData
        {
            public List<Ticket> Tickets { get; set; } = new List<Ticket>();
            public List<User> Users { get; set; } = new List<User>();
        }

        public class Ticket
        {
            public int Type { get; set; }
            public ulong UserId { get; set; }
            public DateTime Created { get; set; } = DateTime.Now;
        }

        public class User
        {
            public ulong Id { get; set; }

            public DateTime GeneralSupportCooldown { get; set; }
            public DateTime ContentSupportCooldown { get; set; }
            public DateTime ServerSupportCooldown { get; set; }
            public DateTime ClaimVIPCooldown { get; set; }
            public DateTime ContentCreatorCooldown { get; set; }
            public DateTime RemoveVIPRole { get; set; }
            public DateTime SuggestionCooldown { get; set; }

            public bool SentContentCreator { get; set; }
            public bool SentClaimVIP { get; set; }
        }
    }
}
