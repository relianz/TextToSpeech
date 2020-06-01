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
    using static System.Console;        // WriteLine

    using System;                       // Boolean, Environment
    using System.IO;                    // FileStream
    using System.Linq;                  // Count
    using System.Text;                  // UTF8Encoding, StringBuilder
    using System.Globalization;         // CultureInfo
    using System.Speech.Synthesis;      // SpeechSynthesizer
    using System.Speech.AudioFormat;    // SpeechAudioFormatInfo
    using System.Collections.Generic;   // IEnumerable

    using CommandLine;                  // Error

    class TextToSpeech
    {
        #region Main
        static void Main( string[] args )
        {
            string outputFileName = "";

            // Process command line:
            var result = Parser.Default.ParseArguments<Options>( args ).MapResult( (opts) => RunOptions( opts ),            // in case parser success.
                                                                                   (errs) => HandleParseError( errs ) );    // in  case parser fail.
            if( result != 0 ) {
                WaitForKeyThenExit( "Invalid or missing command line argument(s)", ErrorCode.CommandLineError );        
            }

            // Create Synthesizer:
            SpeechSynthesizer synth = new SpeechSynthesizer();

            if( displayVoices )
            {
                DisplayVoices( synth );
                WaitForKeyThenExit( "", ErrorCode.Success );
            }

            // Write speech to file? 
            if( recording )
            {
                try
                {
                    outputFileName = Path.GetFileName( outputFile );

                    synth.SetOutputToWaveFile( outputFile,
                                               new SpeechAudioFormatInfo( 16000,
                                                                          AudioBitsPerSample.Sixteen, AudioChannel.Mono ) );
                }
                catch( DirectoryNotFoundException ex ) {
                    WaitForKeyThenExit( ex.Message, ErrorCode.DirectoryNotFound );
                }
            }
            else
            {
                synth.SetOutputToDefaultAudioDevice();
            }

            if( twoLetterISOLanguageName != null )
            {
                if( !SetVoice( synth, VoiceGender.Female, twoLetterISOLanguageName ) )
                {
                    string msg = "Cannot set language of voice to " + twoLetterISOLanguageName;
                    WaitForKeyThenExit( msg, ErrorCode.CannotSetLanguage );
                }
            }

            // Read SSML from input file:
            string ssmlToSpeak = ReadSsmlFromFile( inputFile );

            try
            {
                // Now speak:
                synth.SpeakSsml( ssmlToSpeak );
            }
            catch( FormatException ex ) {
                WaitForKeyThenExit( ex.Message, ErrorCode.SsmlFormatError );
            }

            // Wait on key to terminate:
            if( recording && (outputFile.Length != 0) ) {
                WriteLine( "Generated audio file <{0}>, {1} bytes written.", outputFileName, GetFileSize( outputFile ) );
            }
            
             WaitForKeyThenExit( "Program terminated successfully.", 0 );

        } // Main

        static bool recording = false;
        static bool displayVoices = false;
        static bool verbose = false;

        static string inputFile = null;
        static string outputFile = null;
        static string twoLetterISOLanguageName = null;

        enum ErrorCode : int
        {
            Success = 0,
            DirectoryNotFound = 1,
            FileNotFound = 2,
            CannotSetLanguage = 3,
            SsmlFormatError = 4,
            CommandLineError = 5,
            VersionOrHelpRequired = 6
        }

        static void WaitForKeyThenExit( string msg, ErrorCode exitCode )
        {
            WriteLine( msg );
            WriteLine( "Please press ESC to exit ({0})!", exitCode.ToString( "g" ) );

            while( !(KeyAvailable && ReadKey( true ).Key == ConsoleKey.Escape) ) {
                ;
            }

            Environment.Exit( (int)exitCode );

        } // WaitForKeyThenExit
        #endregion

        #region Commandline
        static int RunOptions( Options options )
        {
            var exitCode = 0;

            if( options.Voices )
            {
                displayVoices = true;
                return exitCode;
            }

            if( options.Verbose )
                verbose = true;

            if( options.InputFile == null )
            {
                if( verbose )
                    WriteLine( "This version cannot read SSML text from standard input." );

                exitCode = Options.ErrorMissingInputFile;
                return exitCode;

            }
            else
            {
                inputFile = options.InputFile;
           
            } // SSML input.

            if( options.Language != null )
                twoLetterISOLanguageName = options.Language;

            if( options.Recording )
            {
                recording = true;

                if( options.OutputFile == null )
                {
                    if( verbose )
                        WriteLine( "This version cannot write audio data to standard output." );

                    exitCode = Options.ErrorMissingOutputFile;
                    return exitCode;
                }
                else
                {
                    outputFile = options.OutputFile;
                }

            } // recording.

            return exitCode;

        } // RunOptions

        // in case of errors or --help or --version
        static int HandleParseError( IEnumerable<Error> errs )
        {
            var result = ErrorCode.CommandLineError;

            WriteLine( "errors {0}", errs.Count() );

            if( errs.Any( x => x is HelpRequestedError || x is VersionRequestedError ) )
                result = ErrorCode.VersionOrHelpRequired;

            return (int)result;

        } // HandleParseError.

        #endregion

        #region Voices
        static void DisplayVoices( SpeechSynthesizer s )
        {
            // Display information on available displayVoices: 
            int numOfVoices = 1;
            WriteLine( "Installed voices -\n" );
            foreach( InstalledVoice voice in s.GetInstalledVoices() )
            {
                VoiceInfo info = voice.VoiceInfo;

                WriteLine( " -------------\n" );
                WriteLine( " No.:           " + numOfVoices++ );
                WriteLine( " Name:          " + info.Name );
                WriteLine( " Culture:       " + info.Culture );
                WriteLine( " Age:           " + info.Age );
                WriteLine( " Gender:        " + info.Gender );
                WriteLine( " Description:   " + info.Description );
                WriteLine( " ID:            " + info.Id );
                WriteLine( " Enabled:       " + voice.Enabled );

                string AudioFormats = "";
                foreach( SpeechAudioFormatInfo fmt in info.SupportedAudioFormats )
                {
                    AudioFormats += String.Format( "{0}\n",
                                                   fmt.EncodingFormat.ToString() );
                }

                Write( " Audio formats: " );
                if( info.SupportedAudioFormats.Count != 0 )
                {
                    WriteLine( AudioFormats );
                }
                else
                {
                    WriteLine( "No supported audio formats found!" );
                }

                string AdditionalInfo = "";
                foreach( string key in info.AdditionalInfo.Keys )
                {
                    AdditionalInfo += String.Format( " {0}: {1}\n", key, info.AdditionalInfo[key] );
                }

                WriteLine( "\n Additional information about the voice - \n" + AdditionalInfo );
                WriteLine();

            } // foreach InstalledVoice

        } // DisplayVoices

        static bool SetVoice( SpeechSynthesizer synthesizer, VoiceGender gender, string language )
        {
            bool voiceInstalled = false;
            CultureInfo culture = null;
            string name = null;

            // query installed voices:
            foreach( var v in synthesizer.GetInstalledVoices() )
            {
                culture = v.VoiceInfo.Culture;
                name = v.VoiceInfo.Name;

                // language found?
                if( culture.TwoLetterISOLanguageName.Equals( language, StringComparison.OrdinalIgnoreCase ) )
                {
                    if( v.VoiceInfo.Gender == gender )
                    {
                        if( verbose )
                            WriteLine( "Found voice {0}", name );

                        voiceInstalled = true;
                    }
                    else
                    {
                        v.Enabled = false;
                        if( verbose )
                            WriteLine( "Disabled voice {0}, due to gender {1}", name, v.VoiceInfo.Gender );
                    }
                }
                else
                {
                    v.Enabled = false;
                    if( verbose )
                        WriteLine( "Disabled voice {0}, due to language {1}", name, culture.TwoLetterISOLanguageName );
                }

            } // for all installed voices.

            if( voiceInstalled )
            {
                synthesizer.SelectVoiceByHints( gender, VoiceAge.NotSet, 0, culture );
                return true;
            }
            else
                return false;
            

        } // SetVoice
        #endregion

        #region Files
        static string ReadSsmlFromFile( string file )
        {
            // Read SSML from input file:
            StringBuilder ssml = new StringBuilder();

            UTF8Encoding encoding = new UTF8Encoding( true );
            try
            {
                using( FileStream fs = File.Open( file, FileMode.Open ) )
                {
                    byte[] b = new byte[ 1024 ];
                    int bytesRead;

                    while( (bytesRead = fs.Read( b, 0, b.Length )) > 0 )
                    {
                        string s = encoding.GetString( b, 0, bytesRead );
                        ssml.Append( s );
                    }
                }
            }
            catch( DirectoryNotFoundException ex ) {
                WaitForKeyThenExit( ex.Message, ErrorCode.DirectoryNotFound );
            }
            catch( FileNotFoundException ex ) {
                WaitForKeyThenExit( ex.Message, ErrorCode.FileNotFound );
            }

            return ssml.ToString();

        } // ReadSsmlFromFile

        static long GetFileSize( string filePath )
        {
            // Permissions?
            if( !Directory.Exists( Path.GetDirectoryName( filePath ) ) )
            {
                WriteLine( "Permission to <{0}> denied!", filePath );
                return -1;
            }
            else if( File.Exists( filePath ) )
            {
                return new FileInfo( filePath ).Length;
            }

            return 0;

        } // GetFileSize
        #endregion

    } // class TextToSpeech

} // namespace Relianz.TextToSpeech