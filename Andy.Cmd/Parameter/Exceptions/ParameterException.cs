﻿using System;

namespace Andy.Cmd.Parameter
{
    public abstract class ParameterException : Exception
    {
        public ParameterException(string message) : base(message)
        {
        }
    }
}