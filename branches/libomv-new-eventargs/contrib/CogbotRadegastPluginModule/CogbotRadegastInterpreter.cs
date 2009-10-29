using cogbot;
using Radegast;

namespace CogbotRadegastPluginModule
{
    public class CogbotRadegastInterpreter: Radegast.Commands.ICommandInterpreter
    {
        private ClientManager clientManager;
        private BotClient botClient;
        public BotClient BotClient
        {
            get
            {
                return botClient;
            }
            set
            {
                if (botClient == value)
                    return;
                botClient = value;
            }
        }


        public CogbotRadegastInterpreter(ClientManager manager)
        {
            clientManager = manager;
            botClient = manager.LastBotClient;
        }
        public RadegastInstance RadegastInstance;
        public bool IsValidCommand(string cmdline)
        {
            if (cmdline.StartsWith("/")) return true;
            return false;
        }

        public void ExecuteCommand(ConsoleWriteLine WriteLine, string cmdline)
        {
            while (cmdline.StartsWith("/"))
            {
                cmdline = cmdline.Substring(1);
            }
            OutputDelegate newOutputDelegate = new OutputDelegate(WriteLine);
            cogbot.Actions.CmdResult result;
            if (botClient == null)
            {
                result = clientManager.ExecuteCommand(cmdline,newOutputDelegate);
            }
            else
            {
                result = botClient.ExecuteCommand(cmdline,newOutputDelegate);
            }

            if (result != null)
                WriteLine(result.ToString());
            else WriteLine("No result returned: {0}", cmdline);
        }

        public void Help(string helpArgs, ConsoleWriteLine WriteLine)
        {
            WriteLine(clientManager.ExecuteCommand("help " + helpArgs, new OutputDelegate(WriteLine)).ToString());         
        }

        public void Dispose()
        {
           clientManager.Dispose();
        }

        public void StartInterpreter(RadegastInstance inst)
        {
            RadegastInstance = inst;
        }

        public void StopInterpreter(RadegastInstance inst)
        {
            Dispose();
        }
    }
}