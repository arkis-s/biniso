# biniso

## To-do:
1. why do this & what is the goal of this
2. how was the file made?
3. explain the different sections
4. perhaps a little website plug? hmm

## What is it?
A dirty command line utility used to create a bootable iso file using a bin file (or really any format, as long as the binary is bootable).

The code needs cleaning up and reworking, but it works - this was mainly an exercise in reversing and then using that knowledge to implement your own solution.

---
## Theory

### Creating the ISO file
To have something to compare & reverse, a bootable iso was made using MagicISO - the bin file used for this was created using NASM and contained the following:

```asm
mov ah, 0x0e
mov al, 'C'
int 0x10
jmp $
var8	db 0
var9	db 0
```
Pass the file through NASM using ``nasm -f bin example.txt -o example.bin``

### Quick Notes
Opening the created iso file with a hex editor (I'm using [HxD](https://mh-nexus.de/en/hxd/) because it has an option to open a CD/DVD file and it shows the sector count), you'll see a lot of zeroes with little bits of data scattered around.

Before continuing, it is important to note that each sector of the ISO file is 0x800/0d2048 bytes large but this can be changed in the ``primary volume descriptor`` which will be covered later.

---

## ISO Structure

### Sector 0 to 15 - 0x0000 to 0x7FFF
This is defined by the spec as being ``system reserved``, and whilst you can write data in here, is isn't relevant to what we're trying to do.

### Sector 16 - 0x8000 to 0x87FF
This is the ``primary volume descriptor`` as mentioned before - this is a large section which defines a lot of information relevant to reading and understanding the information stored on the iso, but we're only interested in setting the bare minimum to get a bootable file.

Comparing the iso file we have, with a table from the [osdev wiki](https://wiki.osdev.org/ISO_9660), we can see exactly what bytes corresepond with what flags.

We'll set ``type code``, ``standard identifer`` and the ``version`` fields and fill the rest of the sector with zeroes. Of course, the flags and data are there for a reason but we'll gloss over it for now.

This is also where you would set the different sector size: a 16bit little/big endian integer at offset 0d128.

### Sector 17 - 0x8800 to 0x8FFF
This is the ``boot record volume descriptor``, and it section is used to point the computer to the start of the boot catalog. It's a simple entry according to the [official documentation](https://pdos.csail.mit.edu/6.828/2014/readings/boot-cdrom.pdf). 

You must note that the absolute pointer is encoded as a little endian value, so as its defined as a dword, a value like ``1A 00 00 00`` in the hex editor will actually look like ``0x0000001A``.

To work out where the pointer goes to, multiply the value value by the sector size - so in this case our sector size is 0x800 and the pointer value is 0x1A so ``0x800 * 0x1A = 0xD000`` -- this is an important value to remember, as we'll come back to this!

### Sector 18 - 0x9000 to 0x97FF
The structure for this entry seems similar to the ``primary volume descriptor``, but the ``type code`` is set to 0x2 - I think this might have something to do with the ``Joliet`` expansion for ISO-9660, but it's not reqiured for booting so we'll skip this sector.

### Sector 19 - 0x9800 to 0x9FFF
Now that all the volumes are declared, we can use a ``volume set terminator`` to mark this - it's a simple entry and an example of it is on the [osdev wiki](https://wiki.osdev.org/ISO_9660).

### Sector 20 to 25 - 0xA000 to 0xCFFF
Sector 20 & 21 contain some information but I haven't been able to establish exactly what its for, and the rest of this section is empty so again, we'll skip this.

### Sector 26 - 0xD000 to 0xD7FF
Remember that value fom earlier? Well this is the sector it was pointing to earlier, so lets have deeper look into it.

There are two blocks here, the ``validation entry`` which is followed directly by the ``initial/default entry``.

Both of these entries are described by the [documentation](https://pdos.csail.mit.edu/6.828/2014/readings/boot-cdrom.pdf), but this is the final section required to make the bootable iso.

There's a little bit of amiguity for the ``checksum word`` in the ``validation entry``, but I believe it's referring to the data at offset 0x2-0x3.

After this is the ``initial/default entry``, it describes whether the entry it bootable, what media type to emulate and more but most importantly it tells the computer where the data is stored (using the same offset trick used earlier to work out 0xD000) and how many sectors will be stored in the memory when loaded.

For example, taking the address 0xD000, we can see from the [documentation](https://pdos.csail.mit.edu/6.828/2014/readings/boot-cdrom.pdf) that the ``load RBA`` is at offset 0x08 - 0x0B. If we look there, we can see that our values are ``1B 00 00 00``, and remembering it's in little endian, the calculation we need to do to work out the position of the data sector is ``0x1B * 0x800 = 0xD800``.

### Sector 27 - 0xD800 to 0xDFFF
This is where the assembly binary is stored! Once the computer successfully understands that the iso is bootable, this section of code will be loaded into memory and executed.

It seems that you're limited to 512 bytes (https://wiki.osdev.org/Bootloader) for a bootloader until the CPU is set up into 32bit/64bit mode, though it does mention that El-Torito ISOs (which this writeup is about) are exempt but I haven't tested it.