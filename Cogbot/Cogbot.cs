using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CommandLine.Utility;
using OpenMetaverse;
using System.IO;


namespace cogbot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SoundFlags flags = SoundFlags.SyncSlave | SoundFlags.Loop;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new TextForm());
        }

     }
     public class CommandLineArgumentsException : Exception
     {
     }

     public class OtherMainProgram
     {
         public static string LoginURI;

         private static void Usage()
         {
             Console.WriteLine("Usage: " + Environment.NewLine +
                     "cogbot.exe --first firstname --last lastname --pass password [--loginuri=\"uri\"] [--startpos \"sim/x/y/z\"] [--master \"master name\"] [--masterkey \"master uuid\"] [--gettextures] [--scriptfile \"filename\"]");
         }

         static void MainUnused(string[] args)
         {
             Arguments arguments = new Arguments(args);

             TextForm manager;
             List<LoginDetails> accounts = new List<LoginDetails>();
             LoginDetails account;
             bool groupCommands = false;
             string masterName = String.Empty;
             UUID masterKey = UUID.Zero;
             string file = String.Empty;
             bool getTextures = false;
             string scriptFile = String.Empty;

             if (arguments["groupcommands"] != null)
                 groupCommands = true;

             if (arguments["masterkey"] != null)
                 masterKey = UUID.Parse(arguments["masterkey"]);

             if (arguments["master"] != null)
                 masterName = arguments["master"];

             if (arguments["loginuri"] != null)
                 LoginURI = arguments["loginuri"];
             if (String.IsNullOrEmpty(LoginURI))
                 LoginURI = Settings.AGNI_LOGIN_SERVER;
             Logger.Log("Using login URI " + LoginURI, Helpers.LogLevel.Info);

             if (arguments["gettextures"] != null)
                 getTextures = true;

             if (arguments["scriptfile"] != null)
             {
                 scriptFile = arguments["scriptfile"];
                 if (!File.Exists(scriptFile))
                 {
                     Logger.Log(String.Format("File {0} Does not exist", scriptFile), Helpers.LogLevel.Error);
                     return;
                 }
             }

             if (arguments["file"] != null)
             {
                 file = arguments["file"];

                 if (!File.Exists(file))
                 {
                     Logger.Log(String.Format("File {0} Does not exist", file), Helpers.LogLevel.Error);
                     return;
                 }

                 // Loading names from a file
                 try
                 {
                     using (StreamReader reader = new StreamReader(file))
                     {
                         string line;
                         int lineNumber = 0;

                         while ((line = reader.ReadLine()) != null)
                         {
                             lineNumber++;
                             string[] tokens = line.Trim().Split(new char[] { ' ', ',' });

                             if (tokens.Length >= 3)
                             {
                                 account = new LoginDetails();
                                 account.FirstName = tokens[0];
                                 account.LastName = tokens[1];
                                 account.Password = tokens[2];

                                 if (tokens.Length >= 4) // Optional starting position
                                 {
                                     char sep = '/';
                                     string[] startbits = tokens[3].Split(sep);
                                     account.StartLocation = NetworkManager.StartLocation(startbits[0], Int32.Parse(startbits[1]),
                                         Int32.Parse(startbits[2]), Int32.Parse(startbits[3]));
                                 }

                                 accounts.Add(account);
                             }
                             else
                             {
                                 Logger.Log("Invalid data on line " + lineNumber +
                                     ", must be in the format of: FirstName LastName Password [Sim/StartX/StartY/StartZ]",
                                     Helpers.LogLevel.Warning);
                             }
                         }
                     }
                 }
                 catch (Exception ex)
                 {
                     Logger.Log("Error reading from " + args[1], Helpers.LogLevel.Error, ex);
                     return;
                 }
             }
             else if (arguments["first"] != null && arguments["last"] != null && arguments["pass"] != null)
             {
                 // Taking a single login off the command-line
                 account = new LoginDetails();
                 account.FirstName = arguments["first"];
                 account.LastName = arguments["last"];
                 account.Password = arguments["pass"];

                 accounts.Add(account);
             }
             else if (arguments["help"] != null)
             {
                 Usage();
                 return;
             }

             foreach (LoginDetails a in accounts)
             {
                 a.GroupCommands = groupCommands;
                 a.MasterName = masterName;
                 a.MasterKey = masterKey;
                 a.URI = LoginURI;

                 if (arguments["startpos"] != null)
                 {
                     char sep = '/';
                     string[] startbits = arguments["startpos"].Split(sep);
                     a.StartLocation = NetworkManager.StartLocation(startbits[0], Int32.Parse(startbits[1]),
                             Int32.Parse(startbits[2]), Int32.Parse(startbits[3]));
                 }
             }

             // Login the accounts and run the input loop
             manager = new TextForm(accounts, getTextures);

             if (!String.IsNullOrEmpty(scriptFile))
                 manager.DoCommandAll("script " + scriptFile, UUID.Zero);

             // Then Run the ClientManager normally
             manager.Run();
         }
     }
}