﻿using System;

namespace Andy.Cmd.Parameter
{
    public class ParameterException : Exception
    {
        public ParameterException(string message, string paramName) : base($"{message}: {paramName}")
        {
        }
    }
}