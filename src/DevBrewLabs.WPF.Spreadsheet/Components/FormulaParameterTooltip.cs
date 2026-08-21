using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DevBrewLabs.Evalis;

namespace DevBrewLabs.WPF.Spreadsheet.Components
{
    /// <summary>
    /// Represents the parameter signature and summary tooltip for active formula editing.
    /// </summary>
    [TemplatePart(Name = "PART_SignatureHost", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_SummaryText", Type = typeof(TextBlock))]
    public class FormulaParameterTooltip : Control
    {
        public static readonly DependencyProperty FormulaInfoProperty =
            DependencyProperty.Register(
                nameof(FormulaInfo),
                typeof(FormulaInfo),
                typeof(FormulaParameterTooltip),
                new PropertyMetadata(null, OnFormulaInfoChanged));

        public static readonly DependencyProperty ActiveArgumentIndexProperty =
            DependencyProperty.Register(
                nameof(ActiveArgumentIndex),
                typeof(int),
                typeof(FormulaParameterTooltip),
                new PropertyMetadata(0, OnActiveArgumentIndexChanged));

        private static readonly Brush HighlightBackground = new SolidColorBrush(Color.FromRgb(255, 242, 204));
        private static readonly Brush HighlightBorderBrush = new SolidColorBrush(Color.FromRgb(254, 215, 142));
        private static readonly Brush DefaultForeground = new SolidColorBrush(Color.FromRgb(31, 41, 55));

        static FormulaParameterTooltip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FormulaParameterTooltip),
                new FrameworkPropertyMetadata(typeof(FormulaParameterTooltip)));
            
            HighlightBackground.Freeze();
            HighlightBorderBrush.Freeze();
            DefaultForeground.Freeze();
        }

        private TextBlock _signatureHost;
        private TextBlock _summaryText;

        public FormulaInfo FormulaInfo
        {
            get => (FormulaInfo)GetValue(FormulaInfoProperty);
            set => SetValue(FormulaInfoProperty, value);
        }

        public int ActiveArgumentIndex
        {
            get => (int)GetValue(ActiveArgumentIndexProperty);
            set => SetValue(ActiveArgumentIndexProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _signatureHost = GetTemplateChild("PART_SignatureHost") as TextBlock;
            _summaryText = GetTemplateChild("PART_SummaryText") as TextBlock;

            UpdateContent();
        }

        private static void OnFormulaInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FormulaParameterTooltip)d).UpdateContent();
        }

        private static void OnActiveArgumentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FormulaParameterTooltip)d).UpdateContent();
        }

        public void Update(FormulaInfo formulaInfo, int activeArgumentIndex)
        {
            FormulaInfo = formulaInfo;
            ActiveArgumentIndex = activeArgumentIndex;
            UpdateContent();
        }

        private void UpdateContent()
        {
            if (_signatureHost == null)
                return;

            _signatureHost.Inlines.Clear();

            var info = FormulaInfo;
            if (info == null)
            {
                if (_summaryText != null) _summaryText.Text = string.Empty;
                return;
            }

            if (_summaryText != null)
            {
                _summaryText.Text = info.Description ?? string.Empty;
            }

            // Function name
            _signatureHost.Inlines.Add(new Run($"{info.Name}(")
            {
                FontWeight = FontWeights.Normal,
                Foreground = DefaultForeground
            });

            var args = info.Arguments;
            if (args != null && args.Count > 0)
            {
                for (int i = 0; i < args.Count; i++)
                {
                    if (i > 0)
                    {
                        _signatureHost.Inlines.Add(new Run(", ")
                        {
                            Foreground = DefaultForeground
                        });
                    }

                    var arg = args[i];
                    string argDisplayName = arg.Name;
                    if (arg.IsVariadic && !argDisplayName.Contains("..."))
                    {
                        argDisplayName = $"{argDisplayName}, ...";
                    }

                    bool isActive = (i == ActiveArgumentIndex) || (arg.IsVariadic && ActiveArgumentIndex >= i);

                    if (isActive)
                    {
                        var highlightBorder = new Border
                        {
                            Background = HighlightBackground,
                            BorderBrush = HighlightBorderBrush,
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(2),
                            Padding = new Thickness(3, 1, 3, 1),
                            Margin = new Thickness(1, 0, 1, 0),
                            Child = new TextBlock
                            {
                                Text = argDisplayName,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = Brushes.Black,
                                FontSize = 12,
                                FontFamily = _signatureHost.FontFamily
                            }
                        };

                        _signatureHost.Inlines.Add(new InlineUIContainer(highlightBorder)
                        {
                            BaselineAlignment = BaselineAlignment.Center
                        });
                    }
                    else
                    {
                        _signatureHost.Inlines.Add(new Run(argDisplayName)
                        {
                            Foreground = DefaultForeground,
                            FontWeight = FontWeights.Normal
                        });
                    }
                }
            }

            _signatureHost.Inlines.Add(new Run(")")
            {
                FontWeight = FontWeights.Normal,
                Foreground = DefaultForeground
            });
        }
    }
}
