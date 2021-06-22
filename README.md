# biniso

## The Goal
To create a command line utility that will create a bootable iso file from a bin file.

An iso file was reverse engineered along side with documents (referenced at the end), in order to derive the theory and (hopefully) create a working application.

!! this writeup is a little bit wrong, will fix soon !!

## Theory

### Creating the ISO file
The mentioned iso file was created like so:

1. A regular text file ``example.txt`` was created with the following contents:
    ```asm
    mov ah, 0x0e
    mov al, 'C'
    int 0x10
    jmp $
    var8	db 0
    var9	db 0
    ```
2. A bin file was created ``example.bin`` using NASM:
    ```
    nasm -f bin example.txt -o example.bin
    ```
3. Then finally creating an iso file using MagicISO

### Examining & Reversing
If we open the final iso file using a hex editor, we should see a whole lot of nothing, along with the occasional block of data that MagicISO created. 

From the documentation<sup>1</sup>, we can see that sector 0 to the end of sector 15 is system reserved and therefore pretty much unused - knowing that each sector is 2048 bytes long, we can calculate where the end of the system reserved section is - (2048 * 16) - 1 = 32,767d or 7FFFh. 

<br>

Looking into sector 16, we have our first block which continues for 1 sector (8000h to 8800h) - this block is our ``primary volume descriptor`` and is described like so:

|offset from address|length|description|
|--|--|--|
|0|1|Volume Descriptor Type Code (see below)|
|1|5|Identifer (always 'CD001')|
|6|1|Version (always 0x01)|
|7|2041|Data|

<br>

|value|description|
|--|--|
|0|Boot Record|
|1|Primary Volume Descriptor|
|2|Supplementary Volume Descriptor|
|3|Volume Partition Descriptor|
|4-254|Reserved|
|255|Volume Descriptor Set Terminator|

<br><br>

Sector 17 stores the ``boot record volume descriptor``, which is described like so:

|offset from address|description|
|--|--|
|0|boot record indicator, set to 0|
|1-5|iso-9660 identifier, "CD001"|
|6|version descriptor 0x01|
|7-26|boot system identifier ``EL TORITO SPECIFICATION`` with zero padding|
|27-46|set to 0|
|47-4A|absolute pointer to first sector of boot catalog|
|4B-7FF|set to 0|

An absolute pointer calculation example:
```
# example entry
        00  01  02  03  04  05  06  07  08  09  0A  0B  0C  0D  0E  0F
8840    00  00  00  00  00  00  00  1A  00  00  00  00  00  00  00  00

so here we can see that 8847 to 884A is 1A 00 00 00

because it is in little endian, the true value is 0x0000001A

0x0800 * 0x1A = 0xD000 <- this is the location of the validation entry
(0x0800 is 0d2048)

0x1A = 0d26 meaning the entry is on sector 26.
```

If you followed the creation pipeline specified earlier using NASM and MagicISO, you will have a second volume descriptor at sector 18, which will be ignored.

<br><br>

Sector 19 contains the ``volume descriptor set terminator`` which is described as:

|offset|description|
|--|--|
|0|255 indicates volume descriptor set terminator|
|1-5|"CD001"|
|6|volume descriptor version "0x01"|


<br><br>

Sector 26 contains the ``validation entry``, which is the sector/address referenced in the boot record volume descriptor.

|offset|description|
|--|--|
|0|header id (set to 0x01)|
|1|platform id (0 = x86, 1 = powerpc, 2 = mac|
|2-3|reserved (set to 0)|
|4-1B|id string for manufacturer/dev|
|1C-1D|checksum of all words (should equal 0)|
|1E|0x55|
|1F|0xAA|


<br><br>

Sector 27 contains the data, and is padded to the end of the nearest sector.

<br><br>

## References & Materials

1. https://pdos.csail.mit.edu/6.828/2014/readings/boot-cdrom.pdf
2. https://wiki.osdev.org/ISO_9660
3. https://alt.os.development.narkive.com/hideJ268/bootable-iso-9660-image
4. http://will.tip.dhappy.org/projects/unsorted/xp_cds/eltorito_extraction.html
