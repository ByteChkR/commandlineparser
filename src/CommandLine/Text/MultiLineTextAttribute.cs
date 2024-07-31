// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Linq;
using System.Text;

namespace CommandLine.Text
{
    /// <summary>
    ///     Provides base properties for creating an attribute, used to define multiple lines of text.
    /// </summary>
    public abstract class MultilineTextAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MultilineTextAttribute" /> class. Used in derived type
        ///     using one line of text.
        /// </summary>
        /// <param name="line1">The first line of text.</param>
        protected MultilineTextAttribute(string line1)
            : this(line1, string.Empty, string.Empty, string.Empty, string.Empty) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultilineTextAttribute" /> class. Used in  type
        ///     using two lines of text.
        /// </summary>
        /// <param name="line1">The first line of text.</param>
        /// <param name="line2">The second line of text.</param>
        protected MultilineTextAttribute(string line1, string line2)
            : this(line1, line2, string.Empty, string.Empty, string.Empty) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultilineTextAttribute" /> class. Used in  type
        ///     using three lines of text.
        /// </summary>
        /// <param name="line1">The first line of text.</param>
        /// <param name="line2">The second line of text.</param>
        /// <param name="line3">The third line of text.</param>
        protected MultilineTextAttribute(string line1, string line2, string line3)
            : this(line1, line2, line3, string.Empty, string.Empty) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultilineTextAttribute" /> class. Used in type
        ///     using four lines of text.
        /// </summary>
        /// <param name="line1">The first line of text.</param>
        /// <param name="line2">The second line of text.</param>
        /// <param name="line3">The third line of text.</param>
        /// <param name="line4">The fourth line of text.</param>
        protected MultilineTextAttribute(string line1, string line2, string line3, string line4)
            : this(line1, line2, line3, line4, string.Empty) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultilineTextAttribute" /> class. Used in type
        ///     using five lines of text.
        /// </summary>
        /// <param name="line1">The first line of text.</param>
        /// <param name="line2">The second line of text.</param>
        /// <param name="line3">The third line of text.</param>
        /// <param name="line4">The fourth line of text.</param>
        /// <param name="line5">The fifth line of text.</param>
        protected MultilineTextAttribute(string line1, string line2, string line3, string line4, string line5)
        {
            if (line1 == null)
            {
                throw new ArgumentException("line1");
            }

            if (line2 == null)
            {
                throw new ArgumentException("line2");
            }

            if (line3 == null)
            {
                throw new ArgumentException("line3");
            }

            if (line4 == null)
            {
                throw new ArgumentException("line4");
            }

            if (line5 == null)
            {
                throw new ArgumentException("line5");
            }

            Line1 = line1;
            Line2 = line2;
            Line3 = line3;
            Line4 = line4;
            Line5 = line5;
        }

        /// <summary>
        ///     Gets the all non-blank lines as string.
        /// </summary>
        /// <value>A string of all non-blank lines.</value>
        public virtual string Value
        {
            get
            {
                StringBuilder value = new StringBuilder(string.Empty);
                string[] strArray = { Line1, Line2, Line3, Line4, Line5 };

                for (int i = 0; i < GetLastLineWithText(strArray); i++)
                {
                    value.AppendLine(strArray[i]);
                }

                return value.ToString();
            }
        }

        /// <summary>
        ///     Gets the first line of text.
        /// </summary>
        public string Line1 { get; }

        /// <summary>
        ///     Gets the second line of text.
        /// </summary>
        public string Line2 { get; }

        /// <summary>
        ///     Gets third line of text.
        /// </summary>
        public string Line3 { get; }

        /// <summary>
        ///     Gets the fourth line of text.
        /// </summary>
        public string Line4 { get; }

        /// <summary>
        ///     Gets the fifth line of text.
        /// </summary>
        public string Line5 { get; }

        internal HelpText AddToHelpText(HelpText helpText, Func<string, HelpText> func)
        {
            string[] strArray = { Line1, Line2, Line3, Line4, Line5 };

            return strArray.Take(GetLastLineWithText(strArray))
                           .Aggregate(helpText, (current, line) => func(line));
        }

        internal HelpText AddToHelpText(HelpText helpText, bool before)
        {
            // before flag only distinguishes which action is called, 
            // so refactor common code and call with appropriate func
            return before
                       ? AddToHelpText(helpText, helpText.AddPreOptionsLine)
                       : AddToHelpText(helpText, helpText.AddPostOptionsLine);
        }

        /// <summary>
        ///     Returns the last line with text. Preserves blank lines if user intended by skipping a line.
        /// </summary>
        /// <returns>
        ///     The last index of line of the non-blank line.
        /// </returns>
        /// <param name='value'>The string array to process.</param>
        protected virtual int GetLastLineWithText(string[] value)
        {
            int index = Array.FindLastIndex(value, str => !string.IsNullOrEmpty(str));

            // remember FindLastIndex returns zero-based index
            return index + 1;
        }
    }
}
