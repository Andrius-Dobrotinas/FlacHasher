using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FlacHasher.Win.UI
{
    public class StringListControl : UserControl
    {
        private readonly ListBox listBox;
        private readonly TextBox textBox;
        private readonly Button addButton;
        private readonly Button updateButton;
        private readonly Button removeButton;

        public StringListControl()
        {
            Height = 130;
            BorderStyle = BorderStyle.None;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));

            listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false
            };
            layout.SetColumnSpan(listBox, 4);

            textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 0)
            };

            addButton = new Button { Text = "Add", Dock = DockStyle.Fill, Margin = new Padding(5, 5, 0, 0) };
            updateButton = new Button { Text = "Update", Dock = DockStyle.Fill, Margin = new Padding(5, 5, 0, 0) };
            removeButton = new Button { Text = "Remove", Dock = DockStyle.Fill, Margin = new Padding(5, 5, 0, 0) };

            layout.Controls.Add(listBox, 0, 0);
            layout.Controls.Add(textBox, 0, 1);
            layout.Controls.Add(addButton, 1, 1);
            layout.Controls.Add(updateButton, 2, 1);
            layout.Controls.Add(removeButton, 3, 1);

            // Event handlers
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            addButton.Click += AddButton_Click;
            updateButton.Click += UpdateButton_Click;
            removeButton.Click += RemoveButton_Click;

            // Initial state
            updateButton.Enabled = false;
            removeButton.Enabled = false;

            Controls.Add(layout);
        }

        public IEnumerable<string> Values
        {
            get => listBox.Items.Cast<string>();
            set
            {
                listBox.Items.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        listBox.Items.Add(item);
                    }
                }
            }
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                textBox.Text = listBox.SelectedItem.ToString();
                updateButton.Enabled = true;
                removeButton.Enabled = true;
            }
            else
            {
                textBox.Clear();
                updateButton.Enabled = false;
                removeButton.Enabled = false;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox.Text) && !listBox.Items.Contains(textBox.Text))
            {
                listBox.Items.Add(textBox.Text);
                textBox.Clear();
                textBox.Focus();
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedItem != null && !string.IsNullOrWhiteSpace(textBox.Text) && !listBox.Items.Contains(textBox.Text))
            {
                int selectedIndex = listBox.SelectedIndex;
                listBox.Items[selectedIndex] = textBox.Text;
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                int selectedIndex = listBox.SelectedIndex;
                listBox.Items.RemoveAt(selectedIndex);
                textBox.Clear();

                if (selectedIndex < listBox.Items.Count)
                {
                    listBox.SelectedIndex = selectedIndex;
                }
                else if (listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                }
            }
        }
    }
}
