﻿using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedField.Global

namespace IctBaden.Stonehenge3.Test.Serializer
{
    public class SimpleClass
    {
        public int Integer { get; set; }
        public bool Boolean { get; set; }
        public double FloatingPoint { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }

        public string PrivateText;

        public SimpleClass()
        {
            Timestamp = DateTime.Now;
        }
    }
}