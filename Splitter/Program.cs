//--------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="John Trimis">
// Copyright (c) 2011 John Trimis 
//
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in 
// the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
// the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS 
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copywrite>
//--------------------------------------------------------------------------------------------
namespace Splitter
{   //namespace : Splitter

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     A file split and recombine utility.
    ///     Can split a large file into smaller chunks and then recombine it later
    /// </summary>
    public class Program
    {   // class : Program

        /// <summary>
        ///     The size of the file buffer
        /// </summary>
        private const int BufferSize = 1024;

        /// <summary>
        ///     Main Entry Point
        /// </summary>
        /// <param name="args">
        ///     NameValuepair containing all options:
        ///         FILENAME, CHUNKSIZE, ACTION
        /// </param>
        public static void Main(string[] args)
        {

            NameValuePair nvp = new NameValuePair(Environment.CommandLine);

            string strFileName = nvp["FILENAME"];
            string strChunkSize = nvp["CHUNKSIZE"];

            if (strFileName == string.Empty)
            {   // No filename specified

                Console.WriteLine("ERROR: FileName not specified.");
                HowDoIUseThis();

            }   // No filename specified
            else
            {   // Filename specified

                if (nvp["ACTION"] == "COMBINE")
                {   // Combine

                    CombineFiles(strFileName);

                }   // Combine
                else
                {   // Split

                    if (!System.IO.File.Exists(strFileName))
                    {   // Inputfile does not exist

                        Console.WriteLine("ERROR: Input file does not exist.");
                        HowDoIUseThis();

                    }   // Inputfile does not exist
                    else
                    {   // Input File Exists

                        if (string.IsNullOrEmpty(strChunkSize))
                        {   // No chunksize specified

                            strChunkSize = "5MB";

                        }   // No chunksize specified

                        long chunkSize = GetBytes(strChunkSize);

                        if (chunkSize == -1)
                        {   // Split by Unknown Chunk Size

                            Console.WriteLine("ERROR: Chunksize invalid or not specified");
                            HowDoIUseThis();

                        }   // Split by Unknown Chunk Size
                        else
                        {   // ChunkSize specified

                            SplitFile(strFileName, chunkSize);

                        }   // ChunkSize specified

                    }   // Input File Exists

                }   // Split

            }   // Filename specified

        }

        /// <summary>
        ///     Output to the screen how to use this program.
        /// </summary>
        public static void HowDoIUseThis()
        {   // HowDoIUseThis

            Console.WriteLine("Splitter.exe Action={Split|Combine} FileName={Filename} [ChunkSize={size}{units}]");
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

        }   // HowDoIUseThis

        /// <summary>
        ///     Get a long denoting how many bytes are represented by a given string
        /// </summary>
        /// <param name="chunkSize">
        ///     A string like:
        ///         123kb
        ///         1Meg
        ///         10bytes
        ///         etc.
        /// </param>
        /// <returns>
        ///     How many bytes that represents.
        /// </returns>
        public static long GetBytes(string chunkSize)
        {   // GetBytes

            long output = -1;
            Regex r = new Regex("^([0-9]+)([a-zA-Z]+)");
            Match mc = r.Match(chunkSize);
            if (mc.Groups.Count == 3)
            {   // Scalar + Unit

                long scalar;
                long multiplier = 1;
                if (long.TryParse(mc.Groups[1].Value, out scalar))
                {   // Scalar parsable

                    switch (mc.Groups[2].Value.ToUpper())
                    {   // Switch for the Unit

                        case "K":
                        case "KB":
                        case "KILOBYTES":
                            multiplier = 1024;
                            break;
                        case "M":
                        case "MB":
                        case "MEG":
                        case "MEGS":
                        case "MEGABYTES":
                            multiplier = 1024 * 1024;
                            break;
                        case "G":
                        case "GB":
                        case "GIG":
                        case "GIGS":
                        case "GIGABYTES":
                            multiplier = 1024 * 1024 * 1024;
                            break;
                    }   // Switch for the Unit

                    output = scalar * multiplier;
                }   // Scalar parsable

            }   // Scalar + Unit

            return output;
        }   // GetBytes

        /// <summary>
        ///     Combine a series of files back into one single file
        /// </summary>
        /// <param name="baseFileName">
        ///     The basefilename (the one that will be generated when fully combined)
        /// </param>
        private static void CombineFiles(string baseFileName)
        {   // CombineFiles

            int currentFileIndexer = 1;
            using (System.IO.FileStream fileOutput = new System.IO.FileStream(baseFileName, System.IO.FileMode.Create))
            {   // Output File Open

                while (true)
                {   // Infinite Loop

                    string strInputFile = string.Format("{0}.part{1}", baseFileName, currentFileIndexer);
                    if (!System.IO.File.Exists(strInputFile))
                    {   // File not found

                        // This presumably means we read all the files in this sequence and we are done.
                        break;

                    }   // File not found
                    else
                    {   // File found

                        byte[] buffer = new byte[BufferSize];
                        using (System.IO.FileStream fileInput = System.IO.File.OpenRead(strInputFile))
                        {   // InputFile Open

                            while (fileInput.Position < fileInput.Length)
                            {   // Loop through all bytes in file

                                int bytesRead = fileInput.Read(buffer, 0, buffer.Length);
                                if (bytesRead < 1)
                                {   // Reached the end of file

                                    break;

                                }   // Reached the end of file
                                else
                                {   // At least one more block to read

                                    fileOutput.Write(buffer, 0, bytesRead);

                                }   // At least one more block to read

                            }   // Loop through all bytes in file

                            fileInput.Close();

                        }   // InputFile Open

                    }   // File found.

                    currentFileIndexer++;

                }   // Infinite loop
                fileOutput.Close();

            }   // Output File Open

        }   // CombineFiles

        /// <summary>
        ///     Split a specified file into component files of the specified size
        /// </summary>
        /// <param name="fileName">
        ///     The name of the file
        /// </param>
        /// <param name="chunkSize">
        ///     The size (in bytes) of each file to make
        /// </param>
        private static void SplitFile(string fileName, long chunkSize)
        {   // SplitFile

            int currentFileIndexer = 1;
            using (System.IO.FileStream fileInput = System.IO.File.OpenRead(fileName))
            {   // Input File Open           

                while (fileInput.Position < fileInput.Length)
                {   // Infinite Loop

                    string strOutputFileName = string.Format("{0}.part{1}", fileName, currentFileIndexer);
                    using (System.IO.FileStream fileOutput = System.IO.File.OpenWrite(strOutputFileName))
                    {   // Outputfile Open

                        byte[] buffer = new byte[BufferSize];

                        while (fileOutput.Length < chunkSize)
                        {   // File not yet completely written

                            long bytesLeft = chunkSize - fileOutput.Length;
                            int bytesToRead = bytesLeft < buffer.Length ? (int)bytesLeft : buffer.Length;

                            int bytesRead = fileInput.Read(buffer, 0, bytesToRead);
                            if (bytesRead < 1)
                            {   // InputFile read completely

                                break;

                            }   // InputFile read completely
                            else
                            {   // Still reading from inputfile

                                fileOutput.Write(buffer, 0, bytesRead);
                            
                            }   // Still reading from inputfile

                        }   // File not yet completely written

                        fileOutput.Close();

                    }   // Outputfile Open

                    currentFileIndexer++;

                }   // Infinite loop
                fileInput.Close();

            }   // Output File Open

        }   // SplitFile

    }   // class : Program

}   //namespace : Splitter
