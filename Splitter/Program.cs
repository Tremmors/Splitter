using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Splitter
{
    class Program
    {
        static void Main(string[] args)
        {
            NameValuePair nvp = new NameValuePair(Environment.CommandLine);
            string strFileName = nvp["FILE"];
            string strChunkSize = nvp["CHUNKSIZE"];

            if (strFileName == string.Empty)
            {   // No filename specified
                Console.WriteLine("ERROR: FileName not specified.");
                Console.WriteLine("");
                HowDoIUseThis();
            }   // No filename specified
            else
            {   // Filename specified
                if (nvp["ACTION"] == "COMBINE")
                {   // Combine
                    int CurrentFileIndexer = 1;

                    using (System.IO.FileStream fileOutput = new System.IO.FileStream(strFileName, System.IO.FileMode.Create))
                    {   // Output File Open
                        while (true)
                        {   // Infinite Loop
                            string strInputFile = string.Format("{0}.part{1}", strFileName, CurrentFileIndexer);
                            if (!System.IO.File.Exists(strInputFile))
                            {   // File not found
                                break;
                            }   // File not found
                            else
                            {   // File found
                                using (System.IO.FileStream fileInput = System.IO.File.OpenRead(strInputFile))
                                {   // InputFile Open
                                    while (fileInput.Position < fileInput.Length)
                                    {   // Loop through all bytes in file
                                        int byteval = fileInput.ReadByte();
                                        if (byteval != -1)
                                        {   // Not yet at end of file
                                            fileOutput.WriteByte((byte)byteval);
                                        }   // Not yet at end of file
                                    }   // Loop through all bytes in file
                                    fileInput.Close();
                                }   // InputFile Open
                            }   // File found.
                            CurrentFileIndexer++;
                        }   // Infinite loop
                        fileOutput.Close();
                    }   // Output File Open
                }   // Combine
                else
                {   // Split
                    if (!System.IO.File.Exists(strFileName))
                    {
                        Console.WriteLine("ERROR: Input file does not exist.");
                        Console.WriteLine("");
                        HowDoIUseThis();
                    }
                    else
                    {   // Input File Exists
                        long ChunkSize = GetBytes(strChunkSize);

                        if (ChunkSize == -1)
                        {   // Split by Unknown Chunk Size
                            Console.WriteLine("ERROR: Chunksize invalid or not specified");
                            Console.WriteLine("");
                            HowDoIUseThis();
                        }   // Split by Unknown Chunk Size
                        else
                        {   // ChunkSize specified
                            System.IO.FileInfo fi = new System.IO.FileInfo(strFileName);
                            using (System.IO.FileStream fileInput = fi.OpenRead())
                            {   // Input file is open
                                int CurrentFileIndexer = 1;
                                while (fileInput.Position < fileInput.Length)
                                {   // Stepping through the input file
                                    string strOutputFile = string.Format("{0}.part{1}", strFileName, CurrentFileIndexer);
                                    using (System.IO.FileStream fileOutput = System.IO.File.OpenWrite(strOutputFile))
                                    {   // Output File is open
                                        for (int i = 0; i < ChunkSize; i++)
                                        {   // Step through byte by byte
                                            int currentval = fileInput.ReadByte();
                                            if (currentval == -1)
                                            {   // Reached end of Input File
                                                break;
                                            }   // Reached end of Input File
                                            fileOutput.WriteByte((byte)currentval);
                                        }   // Step through byte by byte
                                        fileOutput.Close();
                                        CurrentFileIndexer++;
                                    }   // Output File is open
                                }   // Stepping through the input file
                                fileInput.Close();
                            }   // Input file is open
                        }   // ChunkSize specified
                    }   // Input File Exists
                }   // Split
            }   // Filename specified
        }

        /// <summary>
        ///     Output to the screen how to use this program.
        /// </summary>
        static void HowDoIUseThis()
        {
            Console.WriteLine("Splitter.exe Action={Split|Combine} File={Filename} [ChunkSize={size}{units}]");
            Console.WriteLine("     Action     What to do with the file(s)");
            Console.WriteLine("                Split   - Take one large file and chop it into smaller chunks");
            Console.WriteLine("                Combine - Take many smaller files and combine into one larger one");
            Console.WriteLine("     FileName   The filename of the file being split or combined");
            Console.WriteLine("     ChunkSize  How big each filepart should be (only applicable on Split)");
            Console.WriteLine("         Size   How big");
            Console.WriteLine("         Unit   What unit of measurement (case insensitive)");
            Console.WriteLine("                   B         = Bytes");
            Console.WriteLine("                   BYTES     = Bytes");
            Console.WriteLine("                   K         = KiloBytes");
            Console.WriteLine("                   KB        = KiloBytes");
            Console.WriteLine("                   KILOBYTES = KiloBytes");
            Console.WriteLine("                   M         = MegaBytes");
            Console.WriteLine("                   MB        = MegaBytes");
            Console.WriteLine("                   MEG       = MegaBytes");
            Console.WriteLine("                   MEGS      = MegaBytes");
            Console.WriteLine("                   MEGABYTES = MegaBytes");
            Console.WriteLine("                   G         = GigaBytes");
            Console.WriteLine("                   GB        = GigaBytes");
            Console.WriteLine("                   GIG       = GigaBytes");
            Console.WriteLine("                   GIGS      = GigaBytes");
            Console.WriteLine("                   GIGABYTES = GigaBytes");
        }

        /// <summary>
        ///     Get a long denoting how many bytes are represented by a given string
        /// </summary>
        /// <param name="ChunkSize">
        ///     A string like:
        ///         123kb
        ///         1Meg
        ///         10bytes
        ///         etc.
        /// </param>
        /// <returns>
        ///     How many bytes that represents.
        /// </returns>
        public static long GetBytes(string ChunkSize)
        {   // GetBytes
            long output = -1;
            Regex r = new Regex("^([0-9]+)([a-zA-Z]+)");
            Match mc = r.Match(ChunkSize);
            if (mc.Groups.Count == 3)
            {   // Scalar + Unit
                long scalar;
                long Multiplier = 1;
                if (long.TryParse(mc.Groups[1].Value, out scalar))
                {   // Scalar parsable
                    switch (mc.Groups[2].Value)
                    {   // Switch for the Unit
                        case "K":
                        case "KB":
                        case "KILOBYTES":
                            Multiplier = 1024;
                            break;
                        case "M":
                        case "MB":
                        case "MEG":
                        case "MEGS":
                        case "MEGABYTES":
                            Multiplier = 1024 * 1024;
                            break;
                        case "G":
                        case "GB":
                        case "GIG":
                        case "GIGS":
                        case "GIGABYTES":
                            Multiplier = 1024 * 1024 * 1024;
                            break;
                    }   // Switch for the Unit
                    output = scalar * Multiplier;
                }   // Scalar parsable
            }   // Scalar + Unit
            return output;
        }   // GetBytes
    }
}
