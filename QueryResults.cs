using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RedknifeSQL
{
    public class QueryResults
    {

        public string[] Headers { get; private set; }
        public int[] LongestValues { get; private set; }
        public List<string[]> Rows { get; private set; }

        public QueryResults(SqlDataReader reader)
        {
            this.ExtractData(reader);
            reader.Close();
        }

        private void ExtractData(SqlDataReader reader)
        {
            if (reader == null) throw new Exception("ERROR! Can't print table - reader is null");
            if (reader.IsClosed) throw new Exception("ERROR! Can't print table - reader is closed");
            if (!reader.HasRows)
            {
                this.Headers = new string[0];
                this.LongestValues = new int[0];
                this.Rows = new List<string[]>();
                return;
            }

            // Set up the vars for data extraction
            this.Headers = new string[reader.FieldCount];
            this.LongestValues = new int[reader.FieldCount];
            this.Rows = new List<string[]>();

            bool parsedHeaders = false;

            // Read the data from the reader
            while (reader.Read())
            {
                if (!parsedHeaders)
                {
                    // Parse the headers
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string header = reader.GetName(i);
                        this.Headers[i] = header;
                        this.LongestValues[i] = header.Length;
                    }
                    parsedHeaders = true;
                }

                // Extract each row
                string[] row = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader.GetValue(i).ToString();
                    if (row[i].Length > this.LongestValues[i]) this.LongestValues[i] = row[i].Length;
                }
                // Add the row
                this.Rows.Add(row);
            }
        }

    }
}
