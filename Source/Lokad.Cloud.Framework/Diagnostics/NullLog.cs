﻿#region Copyright (c) Lokad 2009-2012
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Xml.Linq;

namespace Lokad.Cloud.Diagnostics
{
    /// <summary> <see cref="ILog"/> that does not do anything.</summary>
    [Serializable]
    public sealed class NullLog : ILog
    {
        /// <summary>
        /// Singleton instance of the <see cref="NullLog"/>.
        /// </summary>
        public static readonly ILog Instance = new NullLog();

        NullLog()
        {
        }

        void ILog.Log(LogLevel level, object message, params XElement[] meta)
        {
        }

        void ILog.Log(LogLevel level, Exception ex, object message, params XElement[] meta)
        {
        }
    }
}
