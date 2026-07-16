using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services;

namespace HVACDesigner.CoreUI.Components.Engineering
{
    public enum EngineeringLabelPosition
    {
        Top,
        Left
    }

    public class EngineeringTextBox : UserControl, IThemeable
    {
        private readonly Label titleLabel;
        private readonly InputSurface inputSurface;
        private readonly TextBox valueTextBox;
        private readonly Label unitLabel;

        private ThemePalette palette = ThemeManager.CurrentPalette;
        private bool isFocused;
        private bool isInvalid;
        private bool isUpdatingText;
        private bool isSynchronizingText;
        private bool isSuppressingTextChanged;
        private bool isAdjustingBounds;
        private double? valueSi;
        private QuantityKind quantityKind = QuantityKind.None;
        private EngineeringLabelPosition labelPosition = EngineeringLabelPosition.Top;

        public EngineeringTextBox()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            titleLabel = new Label
            {
                AutoEllipsis = true,
                Text = "Mennyiség",
                TextAlign = ContentAlignment.MiddleLeft,
                UseCompatibleTextRendering = false
            };

            inputSurface = new InputSurface
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Height = Math.Max(30, ThemeMetrics.TextBoxHeight + 8)
            };

            valueTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Right
            };

            unitLabel = new Label
            {
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                UseCompatibleTextRendering = false
            };

            inputSurface.Controls.Add(valueTextBox);
            Controls.Add(titleLabel);
            Controls.Add(inputSurface);
            Controls.Add(unitLabel);

            Size = new Size(180, 58);
            MinimumSize = new Size(96, 34);

            valueTextBox.TextChanged += OnValueTextChanged;
            valueTextBox.Enter += (_, _) => SetFocusState(true);
            valueTextBox.Leave += (_, _) =>
            {
                SetFocusState(false);
                CommitText();
            };

            valueTextBox.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CommitText();
                    e.SuppressKeyPress = true;
                }
            };

            ApplyTheme(palette);
            UpdateUnitLabel();
            UpdateTextFromValue();
            PerformLayout();
            UnitContext.UnitChanged += UnitContext_UnitChanged;
        }

        public event EventHandler? ValueSiChanged;

        [Category("Engineering")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public QuantityKind QuantityKind
        {
            get => quantityKind;
            set
            {
                if (quantityKind == value)
                    return;

                quantityKind = value;
                UpdateUnitLabel();
                UpdateTextFromValue();
                PerformLayout();
                Invalidate();
            }
        }

        [Category("Engineering")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double? ValueSi
        {
            get => valueSi;
            set => SetValueSi(value, updateText: true, raiseEvent: true);
        }

        [Category("Engineering")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double? MinSi { get; set; }

        [Category("Engineering")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double? MaxSi { get; set; }

        [Category("Engineering")]
        [DefaultValue(false)]
        public bool IsRequired { get; set; }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool UnitVisible
        {
            get => unitLabel.Visible;
            set
            {
                unitLabel.Visible = value;
                PerformLayout();
            }
        }

        [Category("Engineering")]
        [DefaultValue(true)]
        public bool LabelVisible
        {
            get => titleLabel.Visible;
            set
            {
                titleLabel.Visible = value;
                PerformLayout();
            }
        }

        [Category("Engineering")]
        [DefaultValue("Mennyiség")]
        public string LabelText
        {
            get => titleLabel.Text;
            set
            {
                titleLabel.Text = NormalizeLineBreaks(value);
                PerformLayout();
            }
        }

        [Category("Engineering")]
        [DefaultValue(EngineeringLabelPosition.Top)]
        public EngineeringLabelPosition LabelPosition
        {
            get => labelPosition;
            set
            {
                if (labelPosition == value)
                    return;

                labelPosition = value;
                PerformLayout();
            }
        }

        [Category("Engineering")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => valueTextBox.ReadOnly;
            set
            {
                valueTextBox.ReadOnly = value;
                inputSurface.IsReadOnly = value;
                ApplyTheme(palette);
            }
        }

        [Browsable(false)]
        public string UnitLabel => unitLabel.Text;

        [Category("Engineering")]
        [DefaultValue("")]
        public string PlaceholderText
        {
            get => valueTextBox.PlaceholderText;
            set => valueTextBox.PlaceholderText = value ?? string.Empty;
        }

        [Category("Engineering")]
        [DefaultValue(false)]
        public bool HasValidationError
        {
            get => isInvalid;
            set => SetInvalid(value);
        }

        [Category("Engineering")]
        [DefaultValue(HorizontalAlignment.Right)]
        public HorizontalAlignment TextAlign
        {
            get => valueTextBox.TextAlign;
            set => valueTextBox.TextAlign = value;
        }

        [AllowNull]
        public override string Text
        {
            get => valueTextBox.Text;
            set => SetTextCore(value ?? string.Empty, raiseTextChanged: true);
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));

            BackColor = Color.Transparent;
            ForeColor = palette.TextPrimary;

            titleLabel.BackColor = Color.Transparent;
            titleLabel.ForeColor = palette.TextSecondary;
            titleLabel.Font = ThemeFonts.Caption;

            inputSurface.ApplyTheme(palette);
            inputSurface.IsFocused = isFocused;
            inputSurface.IsInvalid = isInvalid;
            inputSurface.IsReadOnly = ReadOnly;

            valueTextBox.BackColor = ReadOnly ? palette.Surface : palette.SurfaceAlt;
            valueTextBox.ForeColor = Enabled
                ? palette.TextPrimary
                : palette.TextDisabled;
            valueTextBox.Font = ThemeFonts.Body;

            unitLabel.BackColor = Color.Transparent;
            unitLabel.ForeColor = Enabled
                ? palette.TextSecondary
                : palette.TextDisabled;
            unitLabel.Font = ThemeFonts.Caption;

            Invalidate(true);
        }

        public void RefreshDisplayedValue()
        {
            UpdateUnitLabel();

            if (quantityKind != QuantityKind.None)
                UpdateTextFromValue();
        }

        public void CommitPendingText()
        {
            CommitText();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                UnitContext.UnitChanged -= UnitContext_UnitChanged;

            base.Dispose(disposing);
        }

        private void UnitContext_UnitChanged(object? sender, EventArgs e)
        {
            RefreshDisplayedValue();
            PerformLayout();
            Invalidate(true);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            valueTextBox.Enabled = Enabled;
            ApplyTheme(palette);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (isSuppressingTextChanged)
                return;

            base.OnTextChanged(e);
        }

        protected override void SetBoundsCore(
            int x,
            int y,
            int width,
            int height,
            BoundsSpecified specified)
        {
            if (!isAdjustingBounds &&
                ShouldUseInputAsLayoutOrigin() &&
                (specified & BoundsSpecified.Y) == BoundsSpecified.Y)
            {
                y -= GetTopLabelOffset();
            }

            isAdjustingBounds = true;
            try
            {
                base.SetBoundsCore(x, y, width, height, specified);
            }
            finally
            {
                isAdjustingBounds = false;
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            int inputHeight = Math.Max(30, ThemeMetrics.TextBoxHeight + 8);
            int gap = ThemeMetrics.MarginSmall;
            bool hasExplicitLineBreak = HasExplicitLineBreak(titleLabel.Text);
            int singleLineLabelHeight = Math.Max(17, TextRenderer.MeasureText("Ag", titleLabel.Font).Height);
            int labelHeight = hasExplicitLineBreak
                ? Math.Max(singleLineLabelHeight * 2, inputHeight)
                : singleLineLabelHeight;

            Rectangle inputRowBounds;

            if (titleLabel.Visible && labelPosition == EngineeringLabelPosition.Top)
            {
                titleLabel.Location = new Point(0, 0);
                titleLabel.Size = new Size(Width, labelHeight);
                inputRowBounds = new Rectangle(0, labelHeight + gap, Width, inputHeight);
            }
            else if (titleLabel.Visible)
            {
                int labelWidth = Math.Min(Math.Max(76, Width / 3), 140);
                int leftLabelHeight = hasExplicitLineBreak
                    ? inputHeight
                    : singleLineLabelHeight;
                int leftLabelTop = hasExplicitLineBreak
                    ? 0
                    : Math.Max(0, (inputHeight - leftLabelHeight) / 2);

                titleLabel.Location = new Point(0, leftLabelTop);
                titleLabel.Size = new Size(labelWidth, leftLabelHeight);
                inputRowBounds = new Rectangle(
                    labelWidth + gap,
                    0,
                    Math.Max(40, Width - labelWidth - gap),
                    inputHeight);
            }
            else
            {
                titleLabel.Bounds = Rectangle.Empty;
                inputRowBounds = new Rectangle(0, 0, Width, inputHeight);
            }

            int unitWidth = !unitLabel.Visible || string.IsNullOrWhiteSpace(unitLabel.Text)
                ? 0
                : Math.Min(74, Math.Max(30, TextRenderer.MeasureText(unitLabel.Text, unitLabel.Font).Width + 6));
            int horizontalPadding = 10;
            int unitGap = unitWidth > 0 ? ThemeMetrics.MarginSmall : 0;
            int inputWidth = Math.Max(36, inputRowBounds.Width - unitWidth - unitGap);

            inputSurface.Location = new Point(inputRowBounds.Left, inputRowBounds.Top);
            inputSurface.Size = new Size(inputWidth, inputRowBounds.Height);

            unitLabel.Location = new Point(inputSurface.Right + unitGap, inputRowBounds.Top);
            unitLabel.Size = new Size(unitWidth, inputRowBounds.Height);

            int textBoxTop = Math.Max(5, (inputSurface.Height - valueTextBox.PreferredHeight) / 2);

            valueTextBox.Location = new Point(horizontalPadding, textBoxTop);
            valueTextBox.Size = new Size(
                Math.Max(20, inputSurface.Width - horizontalPadding * 2),
                valueTextBox.PreferredHeight);
        }

        private void OnValueTextChanged(object? sender, EventArgs e)
        {
            if (isUpdatingText || isSynchronizingText)
                return;

            RaiseSynchronizedTextChanged();

            if (quantityKind == QuantityKind.None)
            {
                return;
            }

            if (IsTransientText(valueTextBox.Text))
            {
                SetInvalid(false);
                return;
            }

            if (TryParseDisplayValue(valueTextBox.Text, out double displayValue))
            {
                double newValueSi = QuantityUnitService.FromDisplay(quantityKind, displayValue);
                bool valid = IsInsideRange(newValueSi);
                SetInvalid(!valid);

                if (valid)
                    SetValueSi(newValueSi, updateText: false, raiseEvent: true);
            }
            else
            {
                SetInvalid(true);
            }
        }

        private void CommitText()
        {
            if (quantityKind == QuantityKind.None)
            {
                SetInvalid(false);
                return;
            }

            if (string.IsNullOrWhiteSpace(valueTextBox.Text))
            {
                SetInvalid(IsRequired);

                if (!IsRequired)
                    SetValueSi(null, updateText: false, raiseEvent: true);

                return;
            }

            if (!TryParseDisplayValue(valueTextBox.Text, out double displayValue))
            {
                SetInvalid(true);
                return;
            }

            double newValueSi = QuantityUnitService.FromDisplay(quantityKind, displayValue);
            bool valid = IsInsideRange(newValueSi);
            SetInvalid(!valid);

            if (valid)
                SetValueSi(newValueSi, updateText: true, raiseEvent: true);
        }

        private void SetValueSi(double? newValue, bool updateText, bool raiseEvent)
        {
            if (valueSi == newValue)
            {
                if (updateText)
                    UpdateTextFromValue();

                return;
            }

            valueSi = newValue;

            if (updateText)
                UpdateTextFromValue();

            if (raiseEvent)
                ValueSiChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateTextFromValue()
        {
            if (quantityKind == QuantityKind.None)
                return;

            SetTextCore(
                valueSi.HasValue
                    ? QuantityUnitService.FormatDisplay(quantityKind, valueSi.Value)
                    : string.Empty,
                raiseTextChanged: false);
        }

        private void UpdateUnitLabel()
        {
            unitLabel.Text = QuantityUnitService.GetUnitLabel(quantityKind);
        }

        private void SetFocusState(bool focused)
        {
            isFocused = focused;
            inputSurface.IsFocused = focused;
            inputSurface.Invalidate();
        }

        private void SetInvalid(bool invalid)
        {
            if (isInvalid == invalid)
                return;

            isInvalid = invalid;
            inputSurface.IsInvalid = invalid;
            inputSurface.Invalidate();
        }

        private void SetTextCore(string text, bool raiseTextChanged)
        {
            string normalized = text ?? string.Empty;
            bool textChanged = valueTextBox.Text != normalized;

            isUpdatingText = !raiseTextChanged;
            isSynchronizingText = true;
            isSuppressingTextChanged = !raiseTextChanged;

            try
            {
                if (textChanged)
                    valueTextBox.Text = normalized;
            }
            finally
            {
                isSynchronizingText = false;
                isUpdatingText = false;
                isSuppressingTextChanged = false;
            }

            if (base.Text != normalized)
            {
                base.Text = normalized;
            }
            else if (raiseTextChanged && textChanged)
            {
                base.OnTextChanged(EventArgs.Empty);
            }
        }

        private void RaiseSynchronizedTextChanged()
        {
            string currentText = valueTextBox.Text;

            if (base.Text != currentText)
            {
                base.Text = currentText;
                return;
            }

            base.OnTextChanged(EventArgs.Empty);
        }

        private bool IsInsideRange(double siValue)
        {
            if (MinSi.HasValue && siValue < MinSi.Value)
                return false;

            if (MaxSi.HasValue && siValue > MaxSi.Value)
                return false;

            return true;
        }

        private static bool IsTransientText(string text)
        {
            string trimmed = text.Trim();
            return trimmed.Length == 0 ||
                   trimmed == "-" ||
                   trimmed == "+" ||
                   trimmed == "," ||
                   trimmed == "." ||
                   trimmed.EndsWith(",", StringComparison.Ordinal) ||
                   trimmed.EndsWith(".", StringComparison.Ordinal);
        }

        private static bool TryParseDisplayValue(string text, out double value)
        {
            string normalized = text.Trim().Replace(',', '.');
            return double.TryParse(
                normalized,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }

        private bool ShouldUseInputAsLayoutOrigin()
        {
            return titleLabel != null &&
                   titleLabel.Visible &&
                   labelPosition == EngineeringLabelPosition.Top;
        }

        private int GetTopLabelOffset()
        {
            if (!ShouldUseInputAsLayoutOrigin())
                return 0;

            bool hasExplicitLineBreak = HasExplicitLineBreak(titleLabel.Text);
            int singleLineLabelHeight = Math.Max(17, TextRenderer.MeasureText("Ag", titleLabel.Font).Height);
            int inputHeight = Math.Max(30, ThemeMetrics.TextBoxHeight + 8);
            int labelHeight = hasExplicitLineBreak
                ? Math.Max(singleLineLabelHeight * 2, inputHeight)
                : singleLineLabelHeight;

            return labelHeight + ThemeMetrics.MarginSmall;
        }

        private static string NormalizeLineBreaks(string? text)
        {
            return (text ?? string.Empty)
                .Replace("\\n", Environment.NewLine, StringComparison.Ordinal)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\r", "\n", StringComparison.Ordinal)
                .Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        }

        private static bool HasExplicitLineBreak(string text)
        {
            return text.Contains('\n') || text.Contains('\r');
        }

        private sealed class InputSurface : Panel
        {
            private ThemePalette palette = ThemeManager.CurrentPalette;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool IsFocused { get; set; }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool IsInvalid { get; set; }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool IsReadOnly { get; set; }

            public InputSurface()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);
            }

            public void ApplyTheme(ThemePalette palette)
            {
                this.palette = palette;
                BackColor = IsReadOnly ? palette.Surface : palette.SurfaceAlt;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
                using SolidBrush backgroundBrush = new SolidBrush(IsReadOnly ? palette.Surface : palette.SurfaceAlt);
                e.Graphics.FillRectangle(backgroundBrush, bounds);

                Color borderColor = IsInvalid
                    ? palette.Danger
                    : IsFocused
                        ? palette.BorderStrong
                        : palette.Border;

                using Pen borderPen = new Pen(borderColor, 1f);
                e.Graphics.DrawRectangle(borderPen, bounds);

                if (IsFocused || IsInvalid)
                {
                    Rectangle innerBounds = new Rectangle(1, 1, Width - 3, Height - 3);
                    e.Graphics.DrawRectangle(borderPen, innerBounds);
                }
            }
        }
    }
}
