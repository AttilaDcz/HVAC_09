using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Components.Engineering;
using HVACDesigner.CoreUI.Icons;
using HVACDesigner.CoreUI.Theme;

namespace HVACDesigner.CoreUI.Components.Dialogs
{
    public enum EngineeringDialogSeverity
    {
        Neutral,
        Info,
        Success,
        Warning,
        Danger
    }

    public enum EngineeringDialogButtonSet
    {
        Ok,
        OkCancel,
        YesNo,
        SaveCancel,
        DeleteCancel,
        Custom
    }

    public enum EngineeringDialogSize
    {
        Compact,
        Small,
        Medium,
        Large,
        Wide,
        Custom
    }

    public sealed class EngineeringDialog : Form, IThemeable
    {
        private readonly DialogHeader header;
        private readonly Panel contentPanel;
        private readonly Panel validationPanel;
        private readonly Label validationLabel;
        private readonly Panel footerPanel;
        private readonly FlowLayoutPanel buttonPanel;
        private readonly EngineeringButton primaryButton;
        private readonly EngineeringButton secondaryButton;
        private readonly EngineeringButton tertiaryButton;

        private ThemePalette palette = ThemeManager.CurrentPalette;
        private EngineeringDialogSeverity severity = EngineeringDialogSeverity.Neutral;
        private EngineeringDialogButtonSet buttonSet = EngineeringDialogButtonSet.OkCancel;
        private EngineeringDialogSize dialogSize = EngineeringDialogSize.Medium;
        private string validationText = string.Empty;
        private DialogResult primaryResult = DialogResult.OK;
        private DialogResult secondaryResult = DialogResult.Cancel;
        private DialogResult tertiaryResult = DialogResult.No;

        public EngineeringDialog()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;
            Padding = new Padding(1);

            header = new DialogHeader
            {
                Dock = DockStyle.Top,
                Height = 76
            };

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 18, 20, 16),
                Tag = "NoTheme"
            };

            validationLabel = new Label
            {
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                Font = ThemeFonts.Caption,
                TextAlign = ContentAlignment.MiddleLeft,
                UseCompatibleTextRendering = false
            };

            validationPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                Padding = new Padding(18, 0, 18, 0),
                Visible = false,
                Tag = "NoTheme"
            };
            validationPanel.Controls.Add(validationLabel);

            primaryButton = CreateFooterButton("OK", EngineeringButtonVariant.Primary, DialogResult.OK);
            secondaryButton = CreateFooterButton("Mégse", EngineeringButtonVariant.Secondary, DialogResult.Cancel);
            tertiaryButton = CreateFooterButton("Nem", EngineeringButtonVariant.Secondary, DialogResult.No);

            buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0),
                RightToLeft = RightToLeft.Yes,
                Tag = "NoTheme"
            };

            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                Padding = new Padding(18, 12, 18, 12),
                Tag = "NoTheme"
            };
            footerPanel.Controls.Add(buttonPanel);

            Controls.Add(contentPanel);
            Controls.Add(validationPanel);
            Controls.Add(footerPanel);
            Controls.Add(header);

            ButtonSet = EngineeringDialogButtonSet.OkCancel;
            DialogSize = EngineeringDialogSize.Medium;

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme(palette);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel ContentPanel => contentPanel;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DialogTitle
        {
            get => header.Title;
            set
            {
                header.Title = value ?? string.Empty;
                Text = header.Title;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DialogSubtitle
        {
            get => header.Subtitle;
            set => header.Subtitle = value ?? string.Empty;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HvacIconKind? IconKind
        {
            get => header.IconKind;
            set => header.IconKind = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringDialogSeverity Severity
        {
            get => severity;
            set
            {
                severity = value;
                header.Severity = value;
                ApplyValidationStyle();
                Invalidate(true);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringDialogButtonSet ButtonSet
        {
            get => buttonSet;
            set
            {
                buttonSet = value;
                ConfigureButtons();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringDialogSize DialogSize
        {
            get => dialogSize;
            set
            {
                dialogSize = value;
                if (value != EngineeringDialogSize.Custom)
                    ClientSize = ResolveClientSize(value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PrimaryButtonText
        {
            get => primaryButton.Text;
            set
            {
                primaryButton.Text = value ?? string.Empty;
                primaryButton.AutoWidth = true;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SecondaryButtonText
        {
            get => secondaryButton.Text;
            set
            {
                secondaryButton.Text = value ?? string.Empty;
                secondaryButton.AutoWidth = true;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ValidationText
        {
            get => validationText;
            set
            {
                validationText = value ?? string.Empty;
                validationLabel.Text = validationText;
                validationPanel.Visible = !string.IsNullOrWhiteSpace(validationText);
                ApplyValidationStyle();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PrimaryButtonEnabled
        {
            get => primaryButton.Enabled;
            set => primaryButton.Enabled = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringButton PrimaryButton => primaryButton;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringButton SecondaryButton => secondaryButton;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EngineeringButton TertiaryButton => tertiaryButton;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding ContentPadding
        {
            get => contentPanel.Padding;
            set => contentPanel.Padding = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int HeaderHeight
        {
            get => header.Height;
            set => header.Height = Math.Max(52, value);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int FooterHeight
        {
            get => footerPanel.Height;
            set => footerPanel.Height = Math.Max(52, value);
        }

        public void SetContent(Control control)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            contentPanel.Controls.Clear();
            control.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(control);

            if (control is IThemeable themeable)
                themeable.ApplyTheme(palette);
        }

        public void ApplyTheme(ThemePalette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Border;
            ForeColor = palette.TextPrimary;

            header.ApplyTheme(palette);
            contentPanel.BackColor = palette.Surface;
            footerPanel.BackColor = palette.Surface;
            buttonPanel.BackColor = palette.Surface;
            validationPanel.BackColor = Blend(ResolveSeverityColor(), palette.Surface, 0.12);
            validationLabel.ForeColor = ResolveSeverityColor();

            ApplyThemeToChildren(contentPanel);

            primaryButton.ApplyTheme(palette);
            secondaryButton.ApplyTheme(palette);
            tertiaryButton.ApplyTheme(palette);
            ApplyValidationStyle();
            Invalidate(true);
        }

        public static DialogResult ShowMessage(
            IWin32Window? owner,
            string title,
            string subtitle,
            EngineeringDialogSeverity severity = EngineeringDialogSeverity.Info,
            HvacIconKind? iconKind = null)
        {
            using EngineeringDialog dialog = new EngineeringDialog
            {
                DialogTitle = title,
                DialogSubtitle = subtitle,
                IconKind = iconKind ?? ResolveDefaultIcon(severity),
                Severity = severity,
                ButtonSet = EngineeringDialogButtonSet.Ok,
                DialogSize = EngineeringDialogSize.Small
            };

            return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        }

        public static DialogResult ShowCompactMessage(
            IWin32Window? owner,
            string title,
            string subtitle,
            EngineeringDialogSeverity severity = EngineeringDialogSeverity.Info,
            HvacIconKind? iconKind = null)
        {
            using EngineeringDialog dialog = new EngineeringDialog
            {
                DialogTitle = title,
                DialogSubtitle = subtitle,
                IconKind = iconKind ?? ResolveDefaultIcon(severity),
                Severity = severity,
                ButtonSet = EngineeringDialogButtonSet.Ok,
                DialogSize = EngineeringDialogSize.Compact
            };

            return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        }

        public static DialogResult ShowConfirmation(
            IWin32Window? owner,
            string title,
            string subtitle,
            EngineeringDialogSeverity severity = EngineeringDialogSeverity.Warning,
            HvacIconKind? iconKind = null)
        {
            using EngineeringDialog dialog = new EngineeringDialog
            {
                DialogTitle = title,
                DialogSubtitle = subtitle,
                IconKind = iconKind ?? ResolveDefaultIcon(severity),
                Severity = severity,
                ButtonSet = EngineeringDialogButtonSet.YesNo,
                DialogSize = EngineeringDialogSize.Small
            };

            return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape && secondaryButton.Visible)
            {
                DialogResult = secondaryResult;
                Close();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && primaryButton.Visible && primaryButton.Enabled)
            {
                DialogResult = primaryResult;
                Close();
                e.Handled = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;

            base.Dispose(disposing);
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            if (!IsDisposed)
                ApplyTheme(e.Palette);
        }

        private EngineeringButton CreateFooterButton(
            string text,
            EngineeringButtonVariant variant,
            DialogResult result)
        {
            EngineeringButton button = new EngineeringButton
            {
                Text = text,
                Variant = variant,
                ButtonSize = EngineeringButtonSize.Normal,
                AutoWidth = true,
                MinimumSize = new Size(104, 0),
                Margin = new Padding(6, 0, 6, 0)
            };
            button.Click += (_, _) =>
            {
                DialogResult = button == primaryButton
                    ? primaryResult
                    : button == secondaryButton
                        ? secondaryResult
                        : tertiaryResult;
                Close();
            };
            return button;
        }

        private void ConfigureButtons()
        {
            buttonPanel.Controls.Clear();
            primaryButton.Visible = false;
            secondaryButton.Visible = false;
            tertiaryButton.Visible = false;

            primaryButton.Variant = EngineeringButtonVariant.Primary;
            secondaryButton.Variant = EngineeringButtonVariant.Secondary;
            tertiaryButton.Variant = EngineeringButtonVariant.Secondary;

            switch (buttonSet)
            {
                case EngineeringDialogButtonSet.Ok:
                    primaryButton.Text = "OK";
                    primaryResult = DialogResult.OK;
                    primaryButton.Visible = true;
                    buttonPanel.Controls.Add(primaryButton);
                    secondaryResult = DialogResult.Cancel;
                    break;

                case EngineeringDialogButtonSet.YesNo:
                    primaryButton.Text = "Igen";
                    primaryResult = DialogResult.Yes;
                    secondaryButton.Text = "Nem";
                    secondaryResult = DialogResult.No;
                    primaryButton.Visible = true;
                    secondaryButton.Visible = true;
                    buttonPanel.Controls.Add(primaryButton);
                    buttonPanel.Controls.Add(secondaryButton);
                    break;

                case EngineeringDialogButtonSet.SaveCancel:
                    primaryButton.Text = "Mentés";
                    secondaryButton.Text = "Mégse";
                    primaryResult = DialogResult.OK;
                    secondaryResult = DialogResult.Cancel;
                    primaryButton.Visible = true;
                    secondaryButton.Visible = true;
                    buttonPanel.Controls.Add(primaryButton);
                    buttonPanel.Controls.Add(secondaryButton);
                    break;

                case EngineeringDialogButtonSet.DeleteCancel:
                    primaryButton.Text = "Törlés";
                    primaryButton.Variant = EngineeringButtonVariant.Danger;
                    secondaryButton.Text = "Mégse";
                    primaryResult = DialogResult.OK;
                    secondaryResult = DialogResult.Cancel;
                    primaryButton.Visible = true;
                    secondaryButton.Visible = true;
                    buttonPanel.Controls.Add(primaryButton);
                    buttonPanel.Controls.Add(secondaryButton);
                    break;

                case EngineeringDialogButtonSet.Custom:
                case EngineeringDialogButtonSet.OkCancel:
                default:
                    primaryButton.Text = string.IsNullOrWhiteSpace(primaryButton.Text) ? "OK" : primaryButton.Text;
                    secondaryButton.Text = string.IsNullOrWhiteSpace(secondaryButton.Text) ? "Mégse" : secondaryButton.Text;
                    primaryResult = DialogResult.OK;
                    secondaryResult = DialogResult.Cancel;
                    primaryButton.Visible = true;
                    secondaryButton.Visible = true;
                    buttonPanel.Controls.Add(primaryButton);
                    buttonPanel.Controls.Add(secondaryButton);
                    break;
            }

            primaryButton.AutoWidth = true;
            secondaryButton.AutoWidth = true;
            tertiaryButton.AutoWidth = true;
            ApplyTheme(palette);
        }

        private void ApplyValidationStyle()
        {
            Color severityColor = ResolveSeverityColor();
            validationPanel.BackColor = Blend(severityColor, palette.Surface, 0.12);
            validationLabel.ForeColor = severityColor;
        }

        private void ApplyThemeToChildren(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is IThemeable themeable)
                {
                    themeable.ApplyTheme(palette);
                    continue;
                }

                if (child is Label label)
                {
                    label.BackColor = Color.Transparent;
                    label.ForeColor = palette.TextSecondary;
                    label.Font = ThemeFonts.Body;
                }
                else if (child is Panel panel)
                {
                    panel.BackColor = palette.Surface;
                }

                if (child.HasChildren)
                    ApplyThemeToChildren(child);
            }
        }

        private Color ResolveSeverityColor()
        {
            return severity switch
            {
                EngineeringDialogSeverity.Info => palette.Info,
                EngineeringDialogSeverity.Success => palette.Success,
                EngineeringDialogSeverity.Warning => palette.Warning,
                EngineeringDialogSeverity.Danger => palette.Danger,
                _ => palette.Accent
            };
        }

        private static HvacIconKind ResolveDefaultIcon(EngineeringDialogSeverity severity)
        {
            return severity switch
            {
                EngineeringDialogSeverity.Danger => HvacIconKind.Safety,
                EngineeringDialogSeverity.Warning => HvacIconKind.Safety,
                EngineeringDialogSeverity.Success => HvacIconKind.Certification,
                _ => HvacIconKind.Info
            };
        }

        private static Size ResolveClientSize(EngineeringDialogSize size)
        {
            return size switch
            {
                EngineeringDialogSize.Compact => new Size(360, 176),
                EngineeringDialogSize.Small => new Size(440, 240),
                EngineeringDialogSize.Large => new Size(720, 520),
                EngineeringDialogSize.Wide => new Size(860, 520),
                _ => new Size(560, 360)
            };
        }

        private static Color Blend(Color foreground, Color background, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            int r = (int)(background.R + ((foreground.R - background.R) * amount));
            int g = (int)(background.G + ((foreground.G - background.G) * amount));
            int b = (int)(background.B + ((foreground.B - background.B) * amount));
            return Color.FromArgb(r, g, b);
        }

        private sealed class DialogHeader : Control, IThemeable
        {
            private ThemePalette palette = ThemeManager.CurrentPalette;

            public DialogHeader()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);
            }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string Title { get; set; } = "Párbeszédablak";

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string Subtitle { get; set; } = string.Empty;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public HvacIconKind? IconKind { get; set; }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public EngineeringDialogSeverity Severity { get; set; } = EngineeringDialogSeverity.Neutral;

            public void ApplyTheme(ThemePalette palette)
            {
                this.palette = palette;
                BackColor = palette.Surface;
                ForeColor = palette.TextPrimary;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Graphics g = e.Graphics;
                g.Clear(palette.Surface);
                g.SmoothingMode = SmoothingMode.None;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Color accent = ResolveSeverityColor();
                using SolidBrush accentBrush = new SolidBrush(accent);
                g.FillRectangle(accentBrush, 0, 0, 4, Height);

                int left = 20;
                if (IconKind.HasValue)
                {
                    using Bitmap icon = HvacIconRenderer.RenderOutline(
                        IconKind.Value,
                        ThemeManager.CurrentThemeMode,
                        ThemeMetrics.IconSizeLarge,
                        accent);
                    int iconTop = 18;
                    g.DrawImage(icon, left, iconTop, ThemeMetrics.IconSizeLarge, ThemeMetrics.IconSizeLarge);
                    left += ThemeMetrics.IconSizeLarge + 12;
                }

                TextRenderer.DrawText(
                    g,
                    Title,
                    ThemeFonts.Section,
                    new Rectangle(left, 13, Math.Max(0, Width - left - 22), 24),
                    palette.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                if (!string.IsNullOrWhiteSpace(Subtitle))
                {
                    TextRenderer.DrawText(
                        g,
                        Subtitle,
                        ThemeFonts.Caption,
                        new Rectangle(left, 39, Math.Max(0, Width - left - 22), Math.Max(20, Height - 42)),
                        palette.TextSecondary,
                        TextFormatFlags.Left |
                        TextFormatFlags.Top |
                        TextFormatFlags.WordBreak |
                        TextFormatFlags.EndEllipsis);
                }

                using Pen linePen = new Pen(palette.Border);
                g.DrawLine(linePen, 0, Height - 1, Width, Height - 1);
            }

            private Color ResolveSeverityColor()
            {
                return Severity switch
                {
                    EngineeringDialogSeverity.Info => palette.Info,
                    EngineeringDialogSeverity.Success => palette.Success,
                    EngineeringDialogSeverity.Warning => palette.Warning,
                    EngineeringDialogSeverity.Danger => palette.Danger,
                    _ => palette.Accent
                };
            }
        }
    }
}
