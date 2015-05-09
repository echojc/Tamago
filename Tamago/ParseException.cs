﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamago
{
    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        { }

        public ParseException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
