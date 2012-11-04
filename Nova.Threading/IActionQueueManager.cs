using System;

namespace Nova.Threading
{
    public interface IActionQueueManager : IDisposable
    {
        void Queue(IAction action);
    }
}