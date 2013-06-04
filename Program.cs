using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.Data;
using System.Data.SQLite;
using System.Net;
using System.IO;
using ArmaServerInfo;

namespace ConsoleApplication9
{
    class Program
    {
        static SteamClient steamClient;
        static CallbackManager manager;
        static int MsgsInt;
        static int MsgsTotal;
        static SteamUser steamUser;
        static SteamFriends steamFriends;
        static GameServer server;
        static DateTime lastcmd;
        static bool isRunning;
        static int rageCounter = 0;
        static string user, pass;

        static void Main(string[] args)
        {
            Console.Title = "Noah-Bawt";
            user = "";
            pass = "";

            steamClient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);

            manager = new CallbackManager(steamClient);

            steamUser = steamClient.GetHandler<SteamUser>();

            steamFriends = steamClient.GetHandler<SteamFriends>();

            new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);
            new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);

            new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);
            new Callback<SteamUser.LoggedOffCallback>(OnLoggedOff, manager);

            new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, manager);
            new Callback<SteamFriends.FriendsListCallback>(OnFriendsList, manager);
            new Callback<SteamFriends.PersonaStateCallback>(OnPersonaState, manager);
            new Callback<SteamFriends.FriendAddedCallback>(OnFriendAdded, manager);
            new Callback<SteamFriends.ChatEnterCallback>(OnChatEnter, manager);
            new Callback<SteamFriends.ChatMsgCallback>(OnChatMsgRecieved, manager);
            lastcmd = DateTime.Now;
            isRunning = true;

            Console.WriteLine("Connecting...");
            steamClient.Connect();

            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }
        
        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);
                isRunning = false;
                return;
            }

            Console.WriteLine("Connected to Steam! Logging in '{0}'...", user);
            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,
            });
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");
            steamClient.Connect();
            Console.WriteLine("Trying to reconnect");
            Console.ReadLine();
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);
                isRunning = false;
                return;
            }

            Console.WriteLine("Successfully logged on!");
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        static void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            SteamID Broma = Convert.ToUInt64(###############);
            steamFriends.JoinChat(Broma);
        }

        static void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            Console.WriteLine("{0} is now a friend", callback.PersonaName);            
        }

        static void OnChatEnter(SteamFriends.ChatEnterCallback callback)
        {
            Console.WriteLine("Chatroom {0} joined owned by {1}.\n\n", callback.ChatID, callback.ClanID);
        }

        static void OnChatMsgRecieved(SteamFriends.ChatMsgCallback callback)
        {
            MsgsTotal += 1;
            MsgsInt += 1;
            SteamID postID = callback.ChatterID;
            string postName = "";
            string gameName = "";
            if (callback.ChatMsgType == EChatEntryType.ChatMsg)
            {
                
                if (callback.Message == "!a2")
                {                
                    if ((lastcmd - DateTime.Now).TotalSeconds < -5)
                    {
                        rageCounter = 0;
                        lastcmd = DateTime.Now;
                        GameServer server = new GameServer("91.121.140.129", 2702);
                        server.Update();
                        Console.WriteLine((lastcmd - DateTime.Now).TotalSeconds);
                        string srvDetails = "Players: " + (server.Players.Count - 2).ToString();
                        steamFriends.SendChatRoomMessage(Convert.ToUInt64(###############), EChatEntryType.ChatMsg, srvDetails);
                    }
                    else
                    {
                        rageCounter += 1;
                        if (rageCounter > 3)
                        {
                            steamFriends.SendChatRoomMessage(Convert.ToUInt64(###############), EChatEntryType.ChatMsg, "Fuck off.");
                            rageCounter = 0;
                        }
                    }
                }
                postName = steamFriends.GetFriendPersonaName(Convert.ToUInt64(postID));
                gameName = steamFriends.GetFriendGamePlayedName(Convert.ToUInt64(postID));
                Console.Clear();
                Console.WriteLine("*****************Noah-Bawt*****************");
                Console.WriteLine("Total Messages: {0}", MsgsTotal.ToString());
                Console.WriteLine("*******************************************");
                Console.WriteLine();
            }
            else if (callback.ChatMsgType == EChatEntryType.WasBanned || callback.ChatMsgType == EChatEntryType.WasKicked)
            {
                postName = "Madmin";
                gameName = "";
                Console.Clear();
                Console.WriteLine("*****************Noah-Bawt*****************");
                Console.WriteLine("Total Messages: {0}", MsgsTotal.ToString());
                Console.WriteLine("*******************************************");
                Console.WriteLine();           
            }
            if (callback.ChatMsgType == EChatEntryType.WasBanned || (callback.ChatMsgType == EChatEntryType.WasKicked && callback.Message != "!a2" && callback.Message != "!a2list") || callback.ChatMsgType == EChatEntryType.ChatMsg)
            {
                try
                {
                    SQLiteConnection cnn = new SQLiteConnection("Data Source=steamchat.sqlite");
                    SQLiteCommand mycommand = new SQLiteCommand(cnn);
                    mycommand.CommandText = "INSERT INTO logs (user, steamID, msg, time, game) VALUES(@p1, @p2, @p3, @p4, @p5)";
                    mycommand.CommandType = CommandType.Text;
                    mycommand.Parameters.Add(new SQLiteParameter("@p1", postName));
                    mycommand.Parameters.Add(new SQLiteParameter("@p2", Convert.ToUInt64(postID)));
                    mycommand.Parameters.Add(new SQLiteParameter("@p3", callback.Message));
                    mycommand.Parameters.Add(new SQLiteParameter("@p4", Convert.ToString(DateTime.Now)));
                    mycommand.Parameters.Add(new SQLiteParameter("@p5", gameName));
                    cnn.Open();
                    mycommand.ExecuteNonQuery();
                    cnn.Close();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            if (MsgsInt == 100)
            {
                MsgsInt = 0;
            }
        }

        public void SendChatRoomMsg(SteamID chatId, EChatEntryType msgType, string Msg)
        {
            steamFriends.SendChatRoomMessage(chatId, msgType, Msg);
        }

        static void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
        }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }
    }
}