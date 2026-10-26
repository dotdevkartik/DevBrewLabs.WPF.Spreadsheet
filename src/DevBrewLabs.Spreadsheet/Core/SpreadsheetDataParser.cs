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

            var rows = new List<List<string>>();
            var currentRow = new List<string>();
            var currentCell = new StringBuilder();
            bool inQuotes = false;
            int len = text.Length;

            for (int i = 0; i < len; i++)
            {
                char c = text[i];

                if (c == '\"')
                {
                    if (inQuotes && i + 1 < len && text[i + 1] == '\"')
                    {
                        currentCell.Append('\"');
                        i++; // Skip escaped quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    currentRow.Add(currentCell.ToString());
                    currentCell.Clear();
                }
                else if ((c == '\r' || c == '\n') && !inQuotes)
                {
                    // Handle \r\n
                    if (c == '\r' && i + 1 < len && text[i + 1] == '\n')
                    {
                        i++;
                    }

                    currentRow.Add(currentCell.ToString());
                    currentCell.Clear();
                    rows.Add(currentRow);
                    currentRow = new List<string>();
                }
                else
                {
                    currentCell.Append(c);
                }
            }

            if (currentCell.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentCell.ToString());
                rows.Add(currentRow);
            }

            if (rows.Count > 1 && rows[rows.Count - 1].Count == 1 && string.IsNullOrEmpty(rows[rows.Count - 1][0]) && (text.EndsWith("\n") || text.EndsWith("\r")))
            {
                rows.RemoveAt(rows.Count - 1);
            }

            if (rows.Count == 0)
                return null;

            int maxCols = 0;
            foreach (var row in rows)
            {
                if (row.Count > maxCols)
                    maxCols = row.Count;
            }

            if (maxCols == 0)
                return null;

            var data = new string[rows.Count, maxCols];
            for (int r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                for (int c = 0; c < maxCols; c++)
                {
                    data[r, c] = c < row.Count ? row[c] : null;
                }
            }

            return data;
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
