﻿using System.Text.RegularExpressions;

namespace Model
{
    public class LogMessage
    {
        #region Constructors
        public LogMessage(string content, LogMessageType type, int contextId, DateTime date)
        {
            Content = content;
            Type = type;
            Date = date;
            ContextId = contextId;
        }
        #endregion Constructors

        #region Enums
        [Flags]
        public enum LogMessageType : byte
        {
            None = 0, Info = 1, Warning = 2, Error = 4, Fatal = 8, Validation = 16
        }
        #endregion Enums

        #region Properties
        public LogMessageType Type { get; } //From [LOG_LEVEL] in [dbo].[LOGGING].
        public string Content { get; } //From [LOG_MESSAGE] in [dbo].[LOGGING].
        public DateTime Date { get; } //From [CREATED] in [dbo].[LOGGING].
        public int ContextId { get; } //From [CONTEXT_ID] in [dbo].[LOGGING].
        public Manager Manager { get; set; } //Based on [CONTEXT_ID] in [dbo].[LOGGING], read function necessary, GetManagerById - returns a manager where Id = [CONTEXT_ID].
        #endregion Properties

        public override string ToString()
        {
            return $"{Date} [{Type}]: {Content}";
        }
    }
}
