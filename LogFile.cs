/* PROGRAMLOGGER
 * http://codebit.de/programlogger
 * 
 * Copyright (c) 2011, Tom Schreiber
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *      * Redistributions of source code must retain the above copyright notice, 
 *        this list of conditions and the following disclaimer.
 *      * Redistributions in binary form must reproduce the above copyright notice, 
 *        this list of conditions and the following disclaimer in the documentation 
 *        and/or other materials provided with the distribution.
 *      * Neither the name of the author Tom Schreiber nor the names of its contributors 
 *        may be used to endorse or promote products derived from this software without 
 *        specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
 * AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */

using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace YWP
{
    /// <summary>
    /// Simple and small class to log infos to file.
    /// </summary>
    public class LogFile
    {
        /// <summary>
        /// Supported log-types
        /// </summary>
        public enum LogType
        {
            /// <summary>simple text-log</summary>
            Txt,
            /// <summary>xhtml-formatted log</summary>
            XhtmlPlain
        };

        /// <summary>
        /// Supported log-Levels
        /// </summary>
        [Flags]
        public enum LogLevel
        {
            /// <summary>debug-level</summary>
            Debug = 0x01,
            /// <summary>info-level</summary>
            Info = 0x02,
            /// <summary>warn-level</summary>
            Warn = 0x04,
            /// <summary>errorlevel</summary>
            Error = 0x08,
            /// <summary>user-defined level 1</summary>
            User1 = 0x10,
            /// <summary>user-defined level 2</summary>
            User2 = 0x20,
            /// <summary>all levels</summary>
            All = 0xFF
        };

        /** PRIVATE GLOBAL VARS *****/
        private readonly string _filename;
        private readonly LogType _type;
        private LogLevel _level;
        private LogLevel _defaultLevel;

        ///<summary>Constructor with filename (no append)</summary> 
        ///<param name="filename">Filename</param>
        ///<exception cref="System.IO.IOException"> ;
        ///IOException - some trouble with wrting header to log-file
        ///</exception>
        public LogFile(string filename)
            : this(filename, false, LogType.Txt, LogLevel.All, "")
        { }

        ///<summary>Constructor with filename and append-info</summary> 
        ///<param name="filename">Filename</param>
        ///<param name="append">Append to existing file (true/false)</param>
        ///<exception cref="System.IO.IOException"> ;
        ///IOException - some trouble with wrting header to log-file (if append=false)
        ///</exception>
        public LogFile(string filename, bool append)
            : this(filename, append, LogType.Txt, LogLevel.All, "")
        { }

        ///<summary>Constructor with filename, append-info and log-type</summary> 
        ///<param name="filename">Filename</param>
        ///<param name="append">Append to existing file (true/false)</param>
        ///<param name="type">Log-Type</param>
        ///<exception cref="System.IO.IOException"> ;
        ///IOException - some trouble with wrting header to log-file (if append=false)
        ///</exception>
        public LogFile(string filename, bool append, LogType type)
            : this(filename, append, type, LogLevel.All, "")
        { }

        ///<summary>Constructor with filename, append-info and log-type</summary> 
        ///<param name="filename">Filename</param>
        ///<param name="append">Append to existing file (true/false)</param>
        ///<param name="type">Log-Type</param>
        ///<param name="level">Log-Level</param>
        ///<exception cref="System.IO.IOException"> ;
        ///IOException - some trouble with wrting header to log-file (if append=false)
        ///</exception>
        public LogFile(string filename, bool append, LogType type, LogLevel level)
            : this(filename, append, type, level, "")
        { }

        ///<summary>Constructor with filename, append-info, log-type, log-level and log-title</summary> 
        ///<param name="filename">Filename</param>
        ///<param name="append">Append to existing file (true/false)</param>
        ///<param name="type">Log-Type</param>
        ///<param name="level">Log-Level</param>
        ///<param name="title">Log-Title (first line in TXT or title-tag in HTML</param>
        ///<exception cref="System.IO.IOException"> ;
        ///IOException - some trouble with wrting header to log-file (if append=false)
        ///</exception>
        public LogFile(string filename, bool append, LogType type, LogLevel level, string title)
        {
            // set global vars
            _filename = filename;
            _type = type;
            _level = level;
            _defaultLevel = LogLevel.Debug;

            // write header
            if (!append)
            {
                // clear file
                WriteLine("", false);

                // if type=HTML write header
                if (_type == LogType.XhtmlPlain)
                {
                    LogRaw("<?xml version='1.0' ?>");
                    LogRaw("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' " +
                        "'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
                    LogRaw("<html xmlns='http://www.w3.org/1999/xhtml'>");
                    LogRaw("<head>");
                    if (title == "")
                        LogRaw("<title>Logfile " + DateTime.Today.ToShortDateString() + "</title>");
                    else
                        LogRaw("<title>" + title + "</title>");
                    LogRaw("<style type='text/css'>body{font-family:monospace;}</style>");
                    LogRaw("</head><body>");
                }
                // if type=TXT write title
                else if (_type == LogType.Txt)
                {
                    if (title != "") LogRaw(title);
                }
            }
        }

        /// <summary>
        /// Write footer (only needed for a XHTML-log, if you want a valid XHTML-file)
        /// </summary>
        public void WriteFooter()
        {
            // if type is HTML end it
            if (_type == LogType.XhtmlPlain)
                LogRaw("</body></html>");
        }

        /// <summary>
        /// Write a single line to log (without Date/Time and Log-Level)
        /// </summary>
        /// <param name="text">text to write</param>
        public void LogRaw(string text)
        {
            WriteLine(text, true);
        }

        /// <summary>
        /// Write a single line to log with Date/Time. Use the default Log-Level
        /// </summary>
        /// <param name="text">text to write</param>
        public void Log(string text)
        {
            Log(text, _defaultLevel);
        }

        /// <summary>
        /// Write a single line to log with Date/Time and Log-Level
        /// </summary>
        /// <param name="text">text to write</param>
        /// <param name="level">log-level</param>
        public void Log(string text, LogLevel level)
        {
            // Check Level
            if ((level & _level) == 0) return;
            if (level == LogLevel.All) level = DefaultLogLevel;

            // format pre-string
            string prestring;
            switch (level)
            {
                case (LogLevel.Debug): prestring = "DEBUG " + DateTime.Now + " "; break;
                case (LogLevel.Info): prestring = "INFO  " + DateTime.Now + " "; break;
                case (LogLevel.Warn): prestring = "WARN  " + DateTime.Now + " "; break;
                case (LogLevel.Error): prestring = "ERROR " + DateTime.Now + " "; break;
                case (LogLevel.User1): prestring = "USER1 " + DateTime.Now + " "; break;
                case (LogLevel.User2): prestring = "USER2 " + DateTime.Now + " "; break;
                default: prestring = ""; break;
            }

            // format text depening on type
            string formattedText;
            switch (_type)
            {
                // HTML_Plain
                case LogType.XhtmlPlain: formattedText = prestring + text + "<br/>"; break;

                // PLAINTEXT
                default: formattedText = prestring + text; break;
            }

            // write to file and flush
            WriteLine(formattedText, true);
        }

        /// <summary>
        /// returns current log-type (read-only)
        /// </summary>
        public LogType CurrentLogType
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// returns or sets current log-level 
        /// </summary>
        public LogLevel CurrentLogLevel
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }

        /// <summary>
        /// returns or sets default log-level 
        /// </summary>
        public LogLevel DefaultLogLevel
        {
            get
            {
                return _defaultLevel;
            }
            set
            {
                _defaultLevel = value;
            }
        }

        /// <summary>
        /// ProgramLogger Version
        /// </summary>
        public Version Version
        {
            get
            {
                return (Assembly.GetExecutingAssembly().GetName().Version);
            }
        }

        /*** PRIVATE FUNCTIONS ***********************************************/

        // write line to file
        private void WriteLine(string text, bool append)
        {
            // open file
            // If an error occurs throw it to the caller.
            var writer = new StreamWriter(_filename, append, Encoding.UTF8);
            if (text != "") writer.WriteLine(text);
            writer.Flush();
            writer.Close();
        }
    }
}
