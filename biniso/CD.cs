using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace biniso
{
    class CD
    {
        public List<byte> contents = new List<byte>();
        public string sourcefile, targetfile;
        public byte[] sourcedata;

        public CD(string source, string target)
        {   
            
            // initialise empty sectors (0x0 -> 0x7FFF)
            for(int i = 0; i < (0x800 * 16); i++)
                contents.Add(0);

            targetfile = target;
            sourcefile = source;
        }

        public void Build()
        {
            primary_vol();
            boot_record();
            vol_terminator();
            validation_entry();
            data_vol();

            try
            {
                File.WriteAllBytes(targetfile, contents.ToArray());
                Console.WriteLine($"file '{targetfile}' generated");
            } catch (Exception e)
            {
                Console.WriteLine($"exception: {e}");
            }
        }

        private void primary_vol()
        {
            this.contents.AddRange(new List<byte>()
            {
                0x01,   // volume descriptor - always 0x01 for primary volumes
                0x43, 0x44, 0x30, 0x30, 0x31,   // 'CD001'
                0x01    // always 0x01
            });

            // padding (skipping over system identifer)
            for(int i = 0; i < 33; i++)
            {
                contents.Add(0);
            }

            // adding volume id - 'biniso primary volume' with 0 padding -> dirty?
            contents.AddRange(new List<byte>() 
            {
                0x62, 0x69, 0x6E, 0x69, 0x73, 0x6F, 0x20, 0x70,
                0x72, 0x69, 0x6D, 0x61, 0x72, 0x79, 0x20, 0x76, 0x6F,
                0x6C, 0x75, 0x6D, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00
            });

            // pad until end of sector (we know this is directly after the empty sector so 0x800 * 17 is the end)
            for (int i = 0; contents.Count < (0x800 * 17); i++)
                contents.Add(0);
        }

        private void boot_record()
        {

            contents.AddRange(new List<byte>
            {
                0x00,   // volume descriptor
                0x43, 0x44, 0x30, 0x30, 0x31, // 'CD001'
                0x01,
                // 'EL TORITO SPECIFICATION'
                0x45, 0x4C, 0x20, 0x54, 0x4F, 0x52, 0x49, 0x54, 0x4F, 0x20, 0x53,
                0x50, 0x45, 0x43, 0x49, 0x46, 0x49, 0x43, 0x41, 0x54, 0x49, 0x4F, 0x4E,
            });

            // zero padding
            for (int i = 0; i < 41; i++)
                contents.Add(0);

            contents.Add(0x13); // hard coded location for validation entry

            // pad until end of sector
            for (int i = 0; contents.Count < (0x800 * 18); i++)
                contents.Add(0);

        }


        private void vol_terminator()
        {
            contents.AddRange(new List<byte> { 
                0xFF,
                0x43, 0x44, 0x30, 0x30, 0x31,
                0x01
            });


            for (int i = 0; contents.Count < (0x800 * 19); i++)
                contents.Add(0);

        }

        private void validation_entry()
        {
            // validation entry
            contents.AddRange(new List<byte> { 
                0x01, // header id
                0x00, // platform id
                0x00, 0x00, // reserved
                // id string to identify developer of cd-rom (set to 'biniso boot record'
                0x62, 0x69, 0x6E ,0x69 ,0x73 ,0x6f ,0x20 , 0x62 , 0x6f , 0x6f , 0x74 , 0x20 , 0x72 , 0x65 , 0x63 , 0x6f , 0x72 , 0x64,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // zero padding for id string
                0x00, 0x00, // checksum
                0x55, 0xAA // key
            });


            // default choice
            contents.AddRange(new List<byte>
            {
                0x88, // bootable code
                0x00, // no emulation
                0x00, 0x00, // load segment
                0x00, // system type
                0x00, //unused
                0x01, 0x00, // number of sectors to load
                0x14 // hard coded distance to data
            });

            for (int i = 0; contents.Count < (0x800 * 20); i++)
                contents.Add(0);


        }


        private void data_vol()
        {
            try
            {
                sourcedata = File.ReadAllBytes(sourcefile);
            } catch (Exception e)
            {
                Console.WriteLine($"exception: {e}");
                return;
            }

            contents.AddRange(sourcedata);

            for (int i = 0; contents.Count < (0x800 * 22); i++)
                contents.Add(0);
        }


    }
}
