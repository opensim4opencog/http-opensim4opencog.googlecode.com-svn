using System;
using System.Threading;

namespace MushDLR223.ScriptEngines
{
    public class CmdResult : IAsyncResult
    {
        public String Message;
        public bool Success;
        public bool InvalidArgs;

        public CmdResult(string usage, bool b)
        {
            Message = usage;
            Success = b;
            IsCompleted = true;
            CompletedSynchronously = true;
            InvalidArgs = false;
        }
        public override string ToString()
        {
            if (!Success) return string.Format("ERROR: {0}", Message);
            return Message;
        }

        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets a System.Threading.WaitHandle that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// A System.Threading.WaitHandle that is used to wait for an asynchronous operation to complete.
        public WaitHandle AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// Returns: A user-defined object that qualifies or contains information about an asynchronous operation.
        public object AsyncState
        {
            get
            {
                return this;/* throw new NotImplementedException();*/
            }
        }

        /// <summary>
        /// true if the asynchronous operation completed synchronously; otherwise, false.
        /// </summary>
        public bool CompletedSynchronously { get; set; }
    }
}