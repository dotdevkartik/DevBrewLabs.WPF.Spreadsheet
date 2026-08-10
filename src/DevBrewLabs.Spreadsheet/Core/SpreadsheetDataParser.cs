using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DevBrewLabs.Spreadsheet.Core
{
    public static class SpreadsheetDataParser
    {
        public static string[,] ParseTextData(string text, char delimiter)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var lines = SplitIntoLines(text);
            int lineCount = lines.Length;

            if (lineCount > 1 && string.IsNullOrEmpty(lines[lineCount - 1]) && (text.EndsWith("\n") || text.EndsWith("\r")))
            {
                lineCount--;
            }

            if (lineCount == 0)
                return null;

            var parsedRows = new List<string[]>();
            int maxColumns = 0;

            for (int i = 0; i < lineCount; i++)
            {
                var rowCells = ParseLine(lines[i], delimiter);
                
                if (rowCells.Length > maxColumns)
                    maxColumns = rowCells.Length;

                parsedRows.Add(rowCells);
            }

            if (maxColumns == 0)
                return null;

            var data = new string[parsedRows.Count, maxColumns];
            for (int r = 0; r < parsedRows.Count; r++)
            {
                var rowCells = parsedRows[r];
                for (int c = 0; c < maxColumns; c++)
                {
                    if (c < rowCells.Length)
                    {
                        data[r, c] = rowCells[c];
                    }
                    else
                    {
                        data[r, c] = null;
                    }
                }
            }

            return data;
        }

        private static string[] SplitIntoLines(string text)
        {
            // Simple split by newline. 
            // In a real CSV, newlines can exist inside quotes, but this is a simplified version.
            return text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        private static string[] ParseLine(string line, char delimiter)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentToken = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        currentToken.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }
            
            result.Add(currentToken.ToString());
            return result.ToArray();
        }

        public static string FormatTsvCell(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Contains("\t") || text.Contains("\n") || text.Contains("\r") || text.Contains("\""))
            {
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }

            return text;
        }
    }
}
