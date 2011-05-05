using System;
using System.Collections.Generic;
using System.Text;

namespace Splitter
{
    /// <summary>
    ///     A container for a set of NameValue Pairs.  Can parse this out of a properly formatted string
    /// </summary>
    public class NameValuePair : System.Collections.IEnumerable
    {
        private Dictionary<string, string> _DataStore;
        /// <summary>
        ///     Create a new NameValuePair object based on the RawText
        /// </summary>
        /// <param name="RawText">
        ///     String in the format of:
        ///     Attribute1=value Attribute2="value 2" Attribute3=value3
        /// </param>
        public NameValuePair(string RawText)
        {
            this._DataStore = new Dictionary<string, string>();

            List<string> commands = SmartSplit(RawText, " ");
            foreach (string thisCmd in commands)
            {
                List<string> namevalue = SmartSplit(thisCmd, "=");

                if (namevalue.Count > 1)
                {
                    this[namevalue[0]] = namevalue[1];
                }
            }


        }

        /// <summary>
        ///     Gets or sets a specific value from the this NameValuePair
        ///     Note: Key and value are both case insensitive
        /// </summary>
        /// <param name="Indexer">
        ///     A key that the value is indexed under
        /// </param>
        /// <returns>
        ///     The value (all uppercase) or string.Empty if no match is found.
        /// </returns>
        public string this[string Indexer]
        {   // Indexer
            get
            {   // GETTER
                if (this._DataStore.ContainsKey(Indexer.ToUpper()))
                {   // Exist
                    return this._DataStore[Indexer.ToUpper()];
                }   // Exists
                else
                {   // Blank
                    return string.Empty;
                }   // Blank
            }   // GETTER
            set
            {   // SETTER
                if (this._DataStore.ContainsKey(Indexer.ToUpper()))
                {   // Update
                    this._DataStore[Indexer.ToUpper()] = value.ToUpper();
                }   // Update
                else
                {   // Add
                    this._DataStore.Add(Indexer.ToUpper(), value.ToUpper());
                }   // Add
            }   // SETTER
        }   // Indexer

        /// <summary>
        ///     Perform a smart split, don't split when inside quotes.  Leave quoted data in tact
        /// </summary>
        /// <param name="RawText">
        ///     The Raw string to search through and split out.
        /// </param>
        /// <param name="Delimiters">
        ///     The Delimiter to use in the split
        /// </param>
        /// <returns>
        ///     An collection of strings containing all the string values split out
        /// </returns>
        private List<string> SmartSplit(string RawText, string Delimiters)
        {
            List<string> output = new List<string>();

            string Quotes = "\"\'";
            string Segment = string.Empty;
            bool Parse = true;

            foreach (char thisChar in RawText)
            {	//<StepThroughAllCharsInString>
                if (Parse)
                {	//<OutSideOfQuotes>
                    if (Delimiters.IndexOf(thisChar) != -1)
                    {	//<DelimiterChar>
                        output.Add(Segment);
                        Segment = string.Empty;
                    }	//</DelimiterChar>
                    else
                    {	//<NonDelimiterChar>
                        if (Quotes.IndexOf(thisChar) != -1)
                        {	//<FoundQuote>
                            Parse = false;
                        }	//</FoundQuote>
                        else
                        {	//<NonQuoteChar>
                            Segment += thisChar;
                        }	//</NonQuoteChar>
                    }	//</NonDelimiterChar>
                }	//</OutSideOfQuotes>
                else
                {	//<InsideQuotes>
                    if (Quotes.IndexOf(thisChar) != -1)
                    {	//<FoundQuote>
                        Parse = true;
                    }	//</FoundQuote>
                    else
                    {	//<NonQuoteChar>
                        Segment += thisChar;
                    }	//</NonQuoteChar>
                }	//</InsideQuotes>
            }	//</StepThroughAllCharsInString>
            //if (endofline == true && Segment == )

            if (Segment != string.Empty) output.Add(Segment);
            else output.Add("");
            // the else is necessary because this indicates that the segment is blank.  for cases where the last column was blank, this was causing the
            //  segment list to have 1 less entry (missing the last blank column) and result in an invalid index error when trying to access the expected column.
            //  Again, this was only applicable when the user is allowing blank columns and filling them in with a default value.


            return output;
        }


        public System.Collections.IEnumerator GetEnumerator()
        {
            return this._DataStore.GetEnumerator();
        }
    }
}
