﻿using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Base
{
    public class BotSource<T> where T : SwitchBotConfig
    {
        public readonly SwitchRoutineExecutor<T> Bot;
        private CancellationTokenSource Source = new CancellationTokenSource();

        public BotSource(SwitchRoutineExecutor<T> bot) => Bot = bot;

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        public void Stop()
        {
            if (!IsRunning)
                return;

            Source.Cancel();
            Source = new CancellationTokenSource();

            // Detach Controllers
            Task.Run(() => Bot.Connection.SendAsync(SwitchCommand.DetachController(), CancellationToken.None));
            IsPaused = IsRunning = false;
        }

        public void Pause()
        {
            IsPaused = true;
            Bot.SoftStop();
        }

        public void Start()
        {
            if (IsPaused)
                Stop(); // can't soft-resume; just re-launch

            if (IsRunning)
                return;

            Task.Run(() => Bot.RunAsync(Source.Token), Source.Token);
            IsRunning = true;
        }

        public void Resume()
        {
            Start();
        }
    }
}