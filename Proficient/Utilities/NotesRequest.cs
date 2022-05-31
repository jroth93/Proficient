using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Proficient.Forms;

namespace Proficient.Utilities
{
    public class NotesRequest
    {
        private int m_request = 0;

        /// <summary>
        ///   Take - The Idling handler calls this to obtain the latest request. 
        /// </summary>
        /// <remarks>
        ///   This is not a getter! It takes the request and replaces it
        ///   with 'None' to indicate that the request has been "passed on".
        /// </remarks>
        /// 
        public NotesType Take()
        {
            return (NotesType)Interlocked.Exchange(ref m_request, 0);
        }

        /// <summary>
        ///   Make - The Dialog calls this when the user presses a command button there. 
        /// </summary>
        /// <remarks>
        ///   It replaces any older request previously made.
        /// </remarks>
        /// 
        public void Make(NotesType request)
        {
            Interlocked.Exchange(ref m_request, (int)request);
        }
        
    }
}
