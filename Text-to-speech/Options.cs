/*
	The MIT License
	Copyright 2020, Dr.-Ing. Markus A. Stulle, Munich (markus@stulle.zone)
 
	Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
	and associated documentation files (the "Software"), to deal in the Software without restriction, 
	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
	subject to the following conditions:
	The above copyright notice and this permission notice shall be included in all copies 
	or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
	IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
	WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
	OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace Relianz.TextToSpeech
{
    using CommandLine;                  // Option

    class Options
    {
        [Option( 'i', "input", Required = false, HelpText = "Input SSML file to be processed.")]
        public string InputFile { get; set; }

        [Option( 'o', "output", Required = false, HelpText = "Output audio file.")]
        public string OutputFile { get; set; }

        [Option( 'r', "recording", Required = false, HelpText = "Recording mode.")]
        public bool Recording { get; set; }

        [Option( 'd', "display_voices", Required = false, HelpText = "Display information on available voices.")]
        public bool Voices { get; set; }

        [Option( 'v', "verbose", Required = false, HelpText = "Set console output to verbose messages.")]
        public bool Verbose { get; set; }

        public static int ErrorMissingInputFile  = 1;
        public static int ErrorMissingOutputFile = 2;
        
    } // class Options.

} // namespace Relianz.TextToSpeech
