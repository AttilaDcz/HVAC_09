using System;
using System.Drawing;
using System.Windows.Forms;

namespace HVACDesigner
{
    public class MenuSection : Panel
    {
        private readonly Panel _itemsContainer;
        public Control.ControlCollection Items => _itemsContainer.Controls;


        public MenuSection(string title)
        {
            Width = 200;
            AutoSize = false;
            Height = 40;
            Margin = new Padding(0, 0, 0, 15);
            BackColor = Color.FromArgb(37, 42, 52);

            var label = new Label
            {
                Text = title.ToUpper(),
                Dock = DockStyle.Top,
                Height = 36,
                ForeColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI Semibold", 13F),
                Padding = new Padding(14, 5, 0, 2),
                BackColor = Color.FromArgb(40, 45, 55)
            };
            var accent = new Panel
            {
                Width = 2,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(90, 90, 90)
            };
            label.Controls.Add(accent);

            // ⭐ Vékony világos elválasztó vonal
            var separator = new Panel
            {
                Height = 1,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(85, 85, 85)
            };

            _itemsContainer = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 0
            };

            Controls.Add(_itemsContainer);
            Controls.Add(separator);
            Controls.Add(label);

            // biztosítjuk, hogy a sorrend: label → separator → items
            Controls.SetChildIndex(label, 2);
            Controls.SetChildIndex(separator, 1);
            Controls.SetChildIndex(_itemsContainer, 0);
        }


        public Button AddItem(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Height = 44,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 42, 52),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                Font = new Font("Segoe UI", 10F),
                ImageAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 3, 0, 3),
                TextImageRelation = TextImageRelation.ImageBeforeText
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;

            // Hover effekt
            btn.MouseEnter += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(47, 128, 237))
                    btn.BackColor = Color.FromArgb(50, 55, 65);
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(47, 128, 237))
                    btn.BackColor = Color.FromArgb(37, 42, 52);
            };

            // Active jelölés (bal oldali kék csík)
            btn.Paint += (s, e) =>
            {
                if (btn.BackColor == Color.FromArgb(47, 128, 237))
                {
                    using var brush = new SolidBrush(Color.FromArgb(47, 128, 237));
                    e.Graphics.FillRectangle(brush, 0, 0, 4, btn.Height);
                }
            };

            _itemsContainer.Controls.Add(btn);
            _itemsContainer.Controls.SetChildIndex(btn, 0);
            _itemsContainer.Padding = new Padding(0, 4, 0, 4);


            _itemsContainer.Height += btn.Height;
            Height += btn.Height;

            return btn;
        }
    }
}


