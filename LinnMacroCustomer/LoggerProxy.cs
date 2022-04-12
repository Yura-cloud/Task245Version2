using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinnMacroCustomer
{
    public class LoggerProxy : LinnworksMacroHelpers.Interfaces.ILogger
    {
        public void WriteDebug(string message)
        {
            Console.WriteLine("DEBUG: " + message);
        }

        public void WriteError(string message)
        {
            Console.WriteLine("ERROR: " + message);
        }

        public void WriteInfo(string message)
        {
            Console.WriteLine("INFO: " + message);
        }

        public void WriteWarning(string message)
        {
            Console.WriteLine("WARNING: " + message);
        }
    }
}
