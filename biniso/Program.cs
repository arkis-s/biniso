using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace biniso
{
    class Program
    {
        static int Main(string[] args)
        {
            // TODO: Add inital/default entry values


            // structure:
            //      0x0000 to 0x7FFF    : set to zero (sys reserved)                                                v/
            //      0x8000 to 0x87FF    : primary vol descriptor                                                    v/
            //      0x8800 to 0x8FFF    : boot record vol descriptor    (ref to validation entry)
            //      0x9000 to 0x97FF    : vol descriptor set terminator                                             v/
            //      0x9800 to 0x9FFF    : validation entry                                                          v/
            //      0xA000 to 0xA7FF    : data 
            //          ^ can this be dynamic?


            List<byte> generated_file = new List<byte>();

            //  === SYSTEM RESERVED AREA ===
            for(int i = 0; i < 0x8000; i++)
                generated_file.Add(0);


            // === PRIMARY VOLUME DESCRIPTOR ===
            byte[] primary_vol = descriptor_primary_vol();

            foreach(byte x in primary_vol)
                generated_file.Add(x);


            // == BOOT RECORD DESCRIPTOR ===
            byte[] boot_vol = boot_record_vol();
            foreach (byte x in boot_vol)
                generated_file.Add(x);


            // === VOL DESCRIPTOR SET TERMINATOR ===
            byte[] set_terminator = vol_terminator();
            foreach (byte x in set_terminator)
                generated_file.Add(x);


            // === VALIDATION ENTRY ===
            byte[] valid = validation_entry();
            foreach(byte x in valid)
            {
                generated_file.Add(x);
            }

            byte[] data = data_vol(args[0]);
            int space_til_next_sector = 2048 - data.Length;
            foreach (byte x in data)
                generated_file.Add(x);

            for(int i = 0; i == space_til_next_sector; i++)
            {
                generated_file.Add(0);
            }


            try
            {
                File.WriteAllBytes(args[1], generated_file.ToArray());
                Console.WriteLine($"file '{args[1]}' generated");
            } catch (Exception e)
            {
                Console.WriteLine($"exception caught {e}");
                return -1;
            }


            //Console.WriteLine($"bytes: {generated_file.Count}\tsector: {generated_file.Count / 0x800}");

            int z = 0;
            foreach(byte x in generated_file)
            {
                
                if (x != 0){
                    Console.WriteLine($"count: {z}\tsector: {Math.Abs(z / 0x800)+1}\tvalue: {x}");
                }
                z++;
            }


            return 0;

        }


        static byte[] descriptor_primary_vol()
        {

            List<byte> mimic_primary_vol = new List<byte>()
            {
                0x01,
                0x43, 0x44, 0x30, 0x30, 0x31,   // CD001
                0x01
            };

            for (int i = mimic_primary_vol.Count; i < 0x800; i++)
            {
                mimic_primary_vol.Add(0);
            }


            return mimic_primary_vol.ToArray();

        }


        static byte[] boot_record_vol()
        {
            List<byte> mimic_boot = new List<byte>()
            {
                0x00,
                0x43, 0x44, 0x30, 0x30, 0x31,
                0x01,
                0x45, 0x4C, 0x20, 0x54, 0x4F, 0x52, 0x49, 0x54, 0x4F, 0x20, 0x53, 0x50, 0x45, 0x43, 0x49, 0x46, 0x49, 0x43, 0x41, 0x54, 0x49, 0x4F, 0x4E,
            };

            for(int i = 0; i < 41; i++)
            {
                mimic_boot.Add(0);
            }

            mimic_boot.Add(0x13);

            for(int i = mimic_boot.Count; i < 0x800; i++)
            {
                mimic_boot.Add(0);
            }

            return mimic_boot.ToArray();

        }

        static byte[] vol_terminator()
        {
            List<byte> mimic_set_terminator = new List<byte>()
            {
                0xFF,   // set terminator descriptor
                0x43, 0x44, 0x30, 0x30, 0x31,   // CD001
                0x01    // volume descriptor version
            };

            for (int i = mimic_set_terminator.Count; i < 0x800; i++)
            {
                mimic_set_terminator.Add(0);
            }


            return mimic_set_terminator.ToArray();

        }


        static byte[] validation_entry()
        {
            List<byte> mimic_entry = new List<byte>()
            {
                0x01,
                0x00,
                0x00, 0x00,
                0x62, 0x69, 0x6e, 0x69, 0x73, 0x6f, 0x20, 0x62, 0x6f, 0x6f, 0x74, 0x20, 0x65, 0x6e, 0x74, 0x72, 0x79, // "biniso boot entry"
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x55,
                0xAA
            };

            for (int i = mimic_entry.Count; i < 0x800; i++)
            {
                mimic_entry.Add(0);
            }

            return mimic_entry.ToArray();
        }


        static byte[] data_vol(string filename)
        {
            try
            {
                byte[] data = File.ReadAllBytes(filename);
                Console.WriteLine($"opened {filename} successfully");
                return data;

            } catch(Exception e)
            {
                Console.WriteLine($"caught exception: {e}");
                return null;
            }




        }
    }
}
