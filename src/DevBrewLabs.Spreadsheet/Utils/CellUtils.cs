namespace DevBrewLabs.Spreadsheet.Utils
{
    internal static class CellUtils
    {
        public static long MakeKey(int row, int column)
        {
            return ((long)row << 32) | (uint)column;
        }

        public static int GetRow(long key)
        {
            return (int)(key >> 32);
        }

        public static int GetColumn(long key)
        {
            return (int)key;
        }
    }
}
