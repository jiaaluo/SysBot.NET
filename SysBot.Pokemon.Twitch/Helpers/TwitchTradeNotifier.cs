using System;
using System.Linq;
using PKHeX.Core;
using SysBot.Base;
using TwitchLib.Client;

namespace SysBot.Pokemon.Twitch
{
    public class TwitchTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private string Username { get; }
        private TwitchClient Client { get; }
        private string Channel { get; }
        private TwitchSettings Settings { get; }

        public TwitchTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string username, TwitchClient client, string channel, TwitchSettings settings)
        {
            Data = data;
            Info = info;
            Code = code;
            Username = username;
            Client = client;
            Channel = channel;
            Settings = settings;

            LogUtil.LogText($"Created trade details for {Username} - {Code}");
        }

        public Action<PokeRoutineExecutor> OnFinish { private get; set; }

        public void SendNotification(PokeRoutineExecutor routine, PokeTradeDetail<T> info, string message)
        {
            LogUtil.LogText(message);
            SendMessage(message, Settings.NotifyDestination);
        }

        public void TradeCanceled(PokeRoutineExecutor routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            OnFinish?.Invoke(routine);
            var line = $"trivialBruh Trade canceled: {msg} trivialBruh";
            LogUtil.LogText(line);
            SendMessage(line, Settings.TradeCanceledDestination);
        }

        public void TradeFinished(PokeRoutineExecutor routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = tradedToUser != 0 ? $"trivialSquirtle Trade finished. Enjoy your {(Species)tradedToUser}! trivialSquirtle" : "Trade finished! trivialSquirtle";
            LogUtil.LogText(message);
            SendMessage(message, Settings.TradeFinishDestination);
        }

        public void TradeInitialize(PokeRoutineExecutor routine, PokeTradeDetail<T> info)
        {
            var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
            Client.SendMessage(Channel, $"Initializing trade{receive} with you, {info.Trainer.TrainerName} (ID: {info.ID}). Please be ready. Use the code you whispered me to search!");
        }

        public void TradeSearching(PokeRoutineExecutor routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", {name}";
            var message = $"I'm waiting for you{trainer}! My IGN is {routine.InGameName}.";
            var dest = Settings.TradeSearchDestination;
            if (dest == TwitchMessageDestination.Channel)
                message += " Use the code you whispered me to search!";
            else if (dest == TwitchMessageDestination.Whisper)
                message += $" Your trade code is: {info.Code:0000}";
            LogUtil.LogText(message);
            SendMessage(message, dest);
        }

        public void SendNotification(PokeRoutineExecutor routine, PokeTradeDetail<T> info, PokeTradeSummary message)
        {
            var msg = message.Summary;
            if (message.Details.Count > 0)
                msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
            LogUtil.LogText(msg);
            SendMessage(msg, Settings.SeedCheckResultDestination);
        }

        public void SendNotification(PokeRoutineExecutor routine, PokeTradeDetail<T> info, T result, string message)
        {
            var msg = $"Details for {result.FileName}: " + message;
            LogUtil.LogText(msg);
            SendMessage(msg, Settings.DumpResultDestination);
        }

        private void SendMessage(string message, TwitchMessageDestination dest)
        {
            switch (dest)
            {
                case TwitchMessageDestination.Channel:
                    Client.SendMessage(Channel, message);
                    break;
                case TwitchMessageDestination.Whisper:
                    Client.SendWhisper(Username, message);
                    break;
            }
        }
    }
}
