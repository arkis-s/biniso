using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace biniso
{
    class Program
    {
        static void Main(string[] args)
        {

            // structure:
            //      0x0000 to 0x7FFF    : set to zero (sys reserved)                                                v/
            //      0x8000 to 0x87FF    : primary vol descriptor                                                    v/
            //      0x8800 to 0x8FFF    : boot record vol descriptor    (ref to validation entry)
            //      0x9000 to 0x97FF    : vol descriptor set terminator                                             v/
            //      0x9800 to 0x9FFF    : validation entry                                                          v/
            //      0xA000 to 0xA7FF    : data 
            //          ^ can this be dynamic?


            CD obj = new CD(args[0], args[1]);
            obj.Build();
        }
    }
}
