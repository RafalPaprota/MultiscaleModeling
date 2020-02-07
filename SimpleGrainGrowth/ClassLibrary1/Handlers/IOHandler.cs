using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains.Utilities.IOHandlers
{
    public abstract class IOHandler
    {
        protected string filter;

        public IOHandler(string filter)
        {
            this.filter = filter;
        }

        public abstract string GetPath();
    }
}
