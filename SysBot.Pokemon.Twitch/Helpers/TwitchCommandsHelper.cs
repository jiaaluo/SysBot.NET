using PKHeX.Core;

namespace SysBot.Pokemon.Twitch
{
    public static class TwitchCommandsHelper
    {
        // Helper functions for commands
        public static bool AddToWaitingList(string setstring, string display, string username, out string msg)
        {
            ShowdownSet set = TwitchShowdownUtil.ConvertToShowdown(setstring);

            if (set.Species < 1)
            {
                msg = $"trivialGrr Skipping trade, {username}: Please follow the correct procedure or ask a Moderator. trivialGrr";
                return false;
            }

            if (set.InvalidLines.Count != 0)
            {
                msg = $"trivialGrr Skipping trade, {username}: Unable to parse Showdown Set:\n{string.Join("\n", set.InvalidLines)} trivialGrr";
                return false;
            }

            var sav = AutoLegalityWrapper.GetTrainerInfo(PKX.Generation);
            PKM pkm = sav.GetLegal(set, out _);

            if (!pkm.CanBeTraded())
            {
                msg = "Provided Pok�mon content is blocked from trading!";
                return false;
            }

            var valid = new LegalityAnalysis(pkm).Valid;
            if (valid && pkm is PK8 pk8)
            {
                var tq = new TwitchQueue(pk8, new PokeTradeTrainerInfo(display), username);
                TwitchBot.QueuePool.Add(tq);
                msg = $"trivialSimba {username} - Please whisper a 4 digit code! trivialSimba";
                return true;
            }

            msg = $"trivialMonkaS Skipping trade, {username}: Unable to legalize the Pok�mon. trivialMonkaS";
            return false;
        }

        public static string ClearTrade(string user)
        {
            var result = TwitchBot.Info.ClearTrade(user);
            return GetClearTradeMessage(result);
        }

        public static string ClearTrade(ulong userID)
        {
            var result = TwitchBot.Info.ClearTrade(userID);
            return GetClearTradeMessage(result);
        }

        private static string GetClearTradeMessage(QueueResultRemove result)
        {
            return result switch
            {
                QueueResultRemove.CurrentlyProcessing => "Looks like you're currently being processed! Removed from queue.",
                QueueResultRemove.Removed => "Removed you from the queue.",
                _ => "trivialMonkaS Sorry, you are not currently in the queue. trivialMonkaS"
            };
        }
    }
}
