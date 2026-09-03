using DevBrewLabs.Spreadsheet;
using DevBrewLabs.Spreadsheet.Drawing;
using DevBrewLabs.Spreadsheet.Formatters;
using DevBrewLabs.WPF.Spreadsheet.Elements;
using DevBrewLabs.WPF.Spreadsheet.Rendering;
using DevBrewLabs.WPF.Spreadsheet.Styling;
using DevBrewLabs.WPF.Spreadsheet.UI.Editors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevBrewLabs.WPF.Spreadsheet.CellTypes
{
    /// <summary>
    /// Cell type that renders interactive, clickable hyperlinks with configurable hover/visited colors,
    /// underline modes, Hand cursor, and automatic or custom URL navigation.
    /// </summary>
    public class HyperlinkCellType : BaseCellType
    {
        private HyperlinkElement _hyperlinkElement;
        private readonly HashSet<string> _visitedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets an explicit target URL or address.
        /// When null, the cell's raw value is interpreted as the destination URL.
        /// </summary>
        public string LinkAddress { get; set; }

        /// <summary>
        /// Gets or sets explicit display text for the hyperlink.
        /// When null, the cell's value or destination URL is displayed.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the brush used for the hyperlink text in normal state.
        /// </summary>
        public Brush LinkColor { get; set; }

        /// <summary>
        /// Gets or sets the brush used for the hyperlink text when hovered.
        /// </summary>
        public Brush HoverLinkColor { get; set; }

        /// <summary>
        /// Gets or sets the brush used for visited hyperlink text.
        /// </summary>
        public Brush VisitedLinkColor { get; set; }

        /// <summary>
        /// Gets or sets the brush used for visited hyperlink text when hovered.
        /// </summary>
        public Brush VisitedHoverLinkColor { get; set; }

        /// <summary>
        /// Gets or sets when an underline should be drawn beneath the hyperlink.
        /// </summary>
        public HyperlinkUnderlineMode UnderlineMode { get; set; } = HyperlinkUnderlineMode.Always;

        /// <summary>
        /// Gets or sets whether to automatically open the destination URL in the default system browser on click.
        /// Set to false to handle navigation exclusively through the <see cref="Click"/> or <see cref="RequestNavigate"/> events.
        /// </summary>
        public bool OpenUrlOnClick { get; set; } = true;

        /// <summary>
        /// Gets or sets whether visited URLs are visually tracked with <see cref="VisitedLinkColor"/>.
        /// </summary>
        public bool TrackVisited { get; set; } = true;

        /// <summary>
        /// Gets or sets the command executed when the hyperlink is clicked.
        /// Passes <see cref="CellHyperlinkClickedEventArgs"/> as the command parameter.
        /// </summary>
        public ICommand Command { get; set; }

        /// <summary>
        /// Fires when the hyperlink cell is clicked.
        /// </summary>
        public event EventHandler<CellHyperlinkClickedEventArgs> Click;

        /// <summary>
        /// Fires when navigation is requested for the hyperlink.
        /// </summary>
        public event EventHandler<CellHyperlinkClickedEventArgs> RequestNavigate;

        public override IEnumerable<CellElement> GetElements(ISheetView view, int row, int col)
        {
            if (_hyperlinkElement == null)
            {
                _hyperlinkElement = new HyperlinkElement(this);
            }
            _hyperlinkElement.View = view;
            _hyperlinkElement.Row = row;
            _hyperlinkElement.Column = col;
            yield return _hyperlinkElement;
        }

        public override void DrawCell(IRenderContext renderContext, object value, IStyle style, IFormatter formatter, Rect cellRect)
        {
            base.DrawCell(renderContext, value, style, formatter, cellRect);

            string displayText = ResolveDisplayText(value);
            if (string.IsNullOrEmpty(displayText)) return;

            if (formatter != null)
            {
                displayText = formatter.Format(displayText);
            }

            string url = ResolveTargetUrl(value);
            bool visited = IsVisited(url);

            var linkBrush = visited
                ? (VisitedLinkColor ?? SheetUtils.HyperlinkVisitedBrush)
                : (LinkColor ?? SheetUtils.HyperlinkBrush);

            bool underline = UnderlineMode == HyperlinkUnderlineMode.Always;

            var hAlign = style?.HorizontalAlignment ?? CellHorizontalAlignment.Left;
            if (hAlign == CellHorizontalAlignment.Auto) hAlign = CellHorizontalAlignment.Left;

            var vAlign = style?.VerticalAlignment ?? CellVerticalAlignment.Center;
            if (vAlign == CellVerticalAlignment.Auto) vAlign = CellVerticalAlignment.Center;

            renderContext.DrawText(
                displayText,
                cellRect,
                style?.FontFamily,
                style != null ? style.FontSize : 11,
                style != null ? style.FontWeight : DrawingFontWeight.Normal,
                style != null ? style.FontStyle : DrawingFontStyle.Normal,
                linkBrush,
                hAlign,
                vAlign,
                style != null ? style.TextTrimming : CellTextTrimming.None,
                style?.AllowMultiLineText == true,
                underline);
        }

        internal void DrawHoverOrPressed(IRenderContext context, Rect bounds, CellElementState state, int row, int col)
        {
            var ws = context.SheetView?.WorkSheet;
            var value = ws?.GetValue(row, col);
            var sheetRow = ws?.Rows?.GetItem(row);
            var sheetColumn = ws?.Columns?.GetItem(col);
            var style = ws?.GetCellStyle(row, col, sheetRow, sheetColumn);

            string displayText = ResolveDisplayText(value);
            if (string.IsNullOrEmpty(displayText)) return;

            string url = ResolveTargetUrl(value);
            bool visited = IsVisited(url);

            var hoverBrush = visited
                ? (VisitedHoverLinkColor ?? SheetUtils.HyperlinkVisitedHoverBrush)
                : (HoverLinkColor ?? SheetUtils.HyperlinkHoverBrush);

            bool underline = UnderlineMode != HyperlinkUnderlineMode.Never;

            var cellRect = context.GetCellRect(row, col);
            if (cellRect.Width <= 0 || cellRect.Height <= 0) cellRect = bounds;

            // Clear the cell background to prevent double-rendering / ghosting of non-hovered text underneath
            var backBrush = (style != null && style.BackColor != DrawingColor.Transparent)
                ? WpfResourceCache.GetBrush(style.BackColor)
                : context.SheetView?.Spread?.Background ?? Brushes.White;

            context.DrawRectangle(backBrush, null, cellRect);

            var hAlign = style?.HorizontalAlignment ?? CellHorizontalAlignment.Left;
            if (hAlign == CellHorizontalAlignment.Auto) hAlign = CellHorizontalAlignment.Left;

            var vAlign = style?.VerticalAlignment ?? CellVerticalAlignment.Center;
            if (vAlign == CellVerticalAlignment.Auto) vAlign = CellVerticalAlignment.Center;

            context.DrawText(
                displayText,
                cellRect,
                style?.FontFamily,
                style != null ? style.FontSize : 11,
                style != null ? style.FontWeight : DrawingFontWeight.Normal,
                style != null ? style.FontStyle : DrawingFontStyle.Normal,
                hoverBrush,
                hAlign,
                vAlign,
                style != null ? style.TextTrimming : CellTextTrimming.None,
                style?.AllowMultiLineText == true,
                underline);
        }

        public virtual void OnClick(ISheetView view, int row, int col)
        {
            var worksheet = view?.WorkSheet as Worksheet;
            var sheetCol = ((Columns)worksheet?.Columns)?.GetItem(col);
            var sheetRow = ((Rows)worksheet?.Rows)?.GetItem(row);

            bool locked = (worksheet?.GetLocked(row, col) == true) ||
                (sheetRow != null && sheetRow.Locked) ||
                (sheetCol != null && sheetCol.Locked);

            if (locked) return;

            var value = worksheet?.GetValue(row, col);
            string url = ResolveTargetUrl(value);
            string displayText = ResolveDisplayText(value);

            var args = new CellHyperlinkClickedEventArgs(view, row, col, this, url, displayText);
            Click?.Invoke(this, args);
            RequestNavigate?.Invoke(this, args);

            if (Command != null && Command.CanExecute(args))
            {
                Command.Execute(args);
            }

            if (TrackVisited && !string.IsNullOrEmpty(url))
            {
                _visitedUrls.Add(url);
                view.Spread?.Refresh();
            }

            if (OpenUrlOnClick && !args.Handled && !string.IsNullOrWhiteSpace(url))
            {
                OpenLink(url);
            }
        }

        public string ResolveTargetUrl(object value)
        {
            if (!string.IsNullOrEmpty(LinkAddress))
                return LinkAddress;

            if (value is Uri uri)
                return uri.OriginalString;

            if (value != null)
            {
                string text = value.ToString().Trim();
                if (text.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                {
                    return "https://" + text;
                }
                return text;
            }

            return string.Empty;
        }

        public string ResolveDisplayText(object value)
        {
            if (!string.IsNullOrEmpty(Text))
                return Text;

            if (value != null)
                return value.ToString();

            if (!string.IsNullOrEmpty(LinkAddress))
                return LinkAddress;

            return string.Empty;
        }

        public bool IsVisited(string url)
        {
            return TrackVisited && !string.IsNullOrEmpty(url) && _visitedUrls.Contains(url);
        }

        public void ClearVisited()
        {
            _visitedUrls.Clear();
        }

        private static void OpenLink(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp ||
                     uri.Scheme == Uri.UriSchemeHttps ||
                     uri.Scheme == Uri.UriSchemeMailto ||
                     uri.Scheme == Uri.UriSchemeFtp))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = uri.AbsoluteUri,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                else if (url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                         url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
            catch
            {
                // Fallback: suppress OS errors when URL cannot be launched
            }
        }

        public override bool SupportsEditing => true;

        public override ICellEditor CreateEditor(IEditorContext context)
        {
            return new TextCellEditor();
        }
    }
}
