using System.Threading;

namespace Telegram_Bot
{
    public class _MyTask
    {
        //delegate int EngineDelegate();
        private int TASK_ITERATION_DELAY_MS = 1000;
        private CancellationTokenSource _cts;
        Program.EngineDelegate myTaskDelegate;
        public _MyTask(int delay_ms = 1000)
        {
            
            TASK_ITERATION_DELAY_MS = delay_ms;
            this._cts = new CancellationTokenSource();
        }
        public void StartExecution(Program.EngineDelegate myEngineDelegate)
        {
            myTaskDelegate = myEngineDelegate;
            System.Threading.Tasks.Task.Factory.StartNew(this.OwnCodeCancelableTask_EveryNSeconds, this._cts.Token);
        }

        public void CancelExecution()
        {
            this._cts.Cancel();
        }

        /// <summary>
        /// "Infinite" loop that runs every N seconds. Good for checking for a heartbeat or updates.
        /// </summary>
        /// <param name="taskState">The cancellation token from our _cts field, passed in the StartNew call</param>
        private async void OwnCodeCancelableTask_EveryNSeconds(object taskState)
        {
            var token = (CancellationToken)taskState;

            while (!token.IsCancellationRequested)
            {
                myTaskDelegate();
                await System.Threading.Tasks.Task.Delay(TASK_ITERATION_DELAY_MS, token);
            }
        }
    }
}
