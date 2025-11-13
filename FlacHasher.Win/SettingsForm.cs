using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win
{
    public partial class SettingsForm : Form
    {
        private readonly Dictionary<PropertyInfo, Control> _propertyControls = new Dictionary<PropertyInfo, Control>();
        private Button _okButton;
        private Button _cancelButton;
        private TableLayoutPanel _mainLayout;
        private FlowLayoutPanel _buttonPanel;

        public Settings Result { get; private set; }

        public SettingsForm(Settings settings)
        {
            InitializeForm();
            BuildDynamicControls(settings);
            LoadSettingsIntoControls(settings);
        }

        private string FormatPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            // Insert spaces before capital letters (except the first one)
            // Handle consecutive capitals by keeping them together until a lowercase letter appears
            var result = Regex.Replace(propertyName, "(?<!^)(?=[A-Z](?![A-Z])|(?<=[a-z])[A-Z])", " ");
            return result;
        }

        private void InitializeForm()
        {
            Text = "Settings";
            Width = 720;
            Height = 600;
            MinimumSize = new Size(700, 400);
            MaximumSize = new Size(900, Screen.PrimaryScreen.WorkingArea.Height - 50);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            Padding = new Padding(10);

            // Main layout
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = SystemColors.Control,
                Padding = new Padding(0)
            };
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content area - fills available space
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Button area - fixed height

            // Button panel
            _buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5),
                WrapContents = false
            };

            _okButton = new Button
            {
                Text = "OK",
                Width = 80,
                Height = 30,
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Width = 80,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };
            _cancelButton.Click += CancelButton_Click;

            _buttonPanel.Controls.Add(_cancelButton);
            _buttonPanel.Controls.Add(_okButton);

            _mainLayout.Controls.Add(_buttonPanel, 0, 1);

            Controls.Add(_mainLayout);
            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void BuildDynamicControls(Settings settings)
        {
            // Get all public instance properties from the Settings class and its base classes
            // Exclude properties decorated with OperationParamAttribute
            var properties = settings.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => !p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == nameof(OperationParamAttribute)))
                .OrderBy(p => p.Name)
                .ToList();

            // Group properties by SettingsAspect attribute
            var groupedProperties = properties
                .GroupBy(p =>
                {
                    var aspectAttr = p.GetCustomAttributes(true)
                        .FirstOrDefault(attr => attr.GetType().Name == nameof(SettingsAspectAttribute));
                    
                    if (aspectAttr != null)
                    {
                        var nameProperty = aspectAttr.GetType().GetProperty(nameof(SettingsAspectAttribute.Name));
                        return nameProperty?.GetValue(aspectAttr)?.ToString() ?? string.Empty;
                    }
                    return string.Empty; // No attribute = empty group name
                })
                .OrderBy(g => string.IsNullOrEmpty(g.Key) ? 1 : 0) // Untitled group goes last
                .ThenBy(g => g.Key) // Sort named groups alphabetically
                .ToList();

            // Create a panel to hold all group boxes
            var contentPanel = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Padding = new Padding(5)
            };

            foreach (var group in groupedProperties)
            {
                var groupBox = CreateGroupBox(group.Key, group.ToList());
                if (groupBox != null)
                {
                    contentPanel.RowCount++;
                    contentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    contentPanel.Controls.Add(groupBox);
                }
            }

            // Wrap content panel in a scrollable panel
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0)
            };
            scrollPanel.Controls.Add(contentPanel);

            _mainLayout.Controls.Add(scrollPanel, 0, 0);
        }

        private GroupBox CreateGroupBox(string groupName, List<PropertyInfo> properties)
        {
            var groupBox = new GroupBox
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(5),
                Text = groupName // Empty string for properties without the attribute
            };

            var groupPanel = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                Padding = new Padding(5)
            };

            foreach (var property in properties)
            {
                var propertyPanel = CreatePropertyPanel(property);
                if (propertyPanel != null)
                {
                    groupPanel.RowCount++;
                    groupPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    groupPanel.Controls.Add(propertyPanel);
                }
            }

            groupBox.Controls.Add(groupPanel);
            return groupBox;
        }

        private Panel CreatePropertyPanel(PropertyInfo property)
        {
            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(560, 0),
                Padding = new Padding(5),
                Margin = new Padding(0, 5, 0, 5),
                Dock = DockStyle.Fill
            };

            int yPosition = 5;
            bool isBooleanField = property.PropertyType == typeof(bool);

            // Property name label
            var nameLabel = new Label
            {
                Text = FormatPropertyName(property.Name),
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(5, yPosition),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            panel.Controls.Add(nameLabel);

            // For boolean fields, place checkbox on the same line as the label
            Control inputControl = CreateInputControl(property);
            if (isBooleanField && inputControl != null)
            {
                inputControl.Location = new Point(nameLabel.Right + 10, yPosition - 2);
                inputControl.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                panel.Controls.Add(inputControl);
                _propertyControls[property] = inputControl;
                yPosition += Math.Max(nameLabel.Height, inputControl.Height) + 3;
            }
            else
            {
                yPosition += nameLabel.Height + 3;
            }

            // Description label (if ParameterDescription attribute exists)
            var descriptionAttr = property.GetCustomAttribute<ParameterDescriptionAttribute>();
            if (descriptionAttr != null && !string.IsNullOrWhiteSpace(descriptionAttr.Description))
            {
                var descLabel = new Label
                {
                    Text = descriptionAttr.Description,
                    AutoSize = true,
                    MaximumSize = new Size(540, 0),
                    ForeColor = SystemColors.GrayText,
                    Location = new Point(5, yPosition),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                panel.Controls.Add(descLabel);
                yPosition += descLabel.Height + 5;
            }

            // For non-boolean fields, place input control below
            if (!isBooleanField && inputControl != null)
            {
                inputControl.Location = new Point(5, yPosition);
                inputControl.Width = 540;
                inputControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                panel.Controls.Add(inputControl);
                _propertyControls[property] = inputControl;
                yPosition += inputControl.Height + 5;
            }

            // Set minimum height based on content
            panel.MinimumSize = new Size(panel.MinimumSize.Width, yPosition + 5);
            return panel;
        }

        private Control CreateInputControl(PropertyInfo property)
        {
            var propertyType = property.PropertyType;

            // Handle string
            if (propertyType == typeof(string))
            {
                return new TextBox
                {
                    Height = 23
                };
            }

            // Handle string array
            if (propertyType == typeof(string[]))
            {
                return CreateStringArrayControl();
            }

            // Handle bool
            if (propertyType == typeof(bool))
            {
                return new CheckBox
                {
                    Height = 20
                };
            }

            // Handle int
            if (propertyType == typeof(int))
            {
                return new NumericUpDown
                {
                    Minimum = int.MinValue,
                    Maximum = int.MaxValue,
                    Height = 23
                };
            }

            // Handle enums
            if (propertyType.IsEnum)
            {
                var comboBox = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Height = 23
                };
                comboBox.Items.AddRange(Enum.GetNames(propertyType));
                return comboBox;
            }

            // Default: read-only label for unsupported types
            return new Label
            {
                Text = $"[Unsupported type: {propertyType.Name}]",
                ForeColor = SystemColors.GrayText,
                AutoSize = true
            };
        }

        private Control CreateStringArrayControl()
        {
            var container = new Panel
            {
                Height = 130,
                BorderStyle = BorderStyle.None
            };

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

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false
            };
            layout.SetColumnSpan(listBox, 4);

            var textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 0)
            };

            var addButton = new Button { Text = "Add", Dock = DockStyle.Fill, Margin = new Padding(5, 5, 0, 0) };
            var updateButton = new Button { Text = "Update", Dock = DockStyle.Fill, Margin = new Padding(5, 5, 0, 0) };
            var removeButton = new Button { Text = "Remove", Dock = DockStyle.Fill, Margin = new Padding(5, 5, 0, 0) };

            layout.Controls.Add(listBox, 0, 0);
            layout.Controls.Add(textBox, 0, 1);
            layout.Controls.Add(addButton, 1, 1);
            layout.Controls.Add(updateButton, 2, 1);
            layout.Controls.Add(removeButton, 3, 1);

            // Event handlers
            listBox.SelectedIndexChanged += (s, e) =>
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
            };

            addButton.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text) && !listBox.Items.Contains(textBox.Text))
                {
                    listBox.Items.Add(textBox.Text);
                    textBox.Clear();
                    textBox.Focus();
                }
            };

            updateButton.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null && !string.IsNullOrWhiteSpace(textBox.Text) && !listBox.Items.Contains(textBox.Text))
                {
                    int selectedIndex = listBox.SelectedIndex;
                    listBox.Items[selectedIndex] = textBox.Text;
                }
            };

            removeButton.Click += (s, e) =>
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
            };

            // Initial state
            updateButton.Enabled = false;
            removeButton.Enabled = false;

            container.Controls.Add(layout);
            container.Tag = listBox;

            return container;
        }

        private void LoadSettingsIntoControls(Settings settings)
        {
            foreach (var kvp in _propertyControls)
            {
                var property = kvp.Key;
                var control = kvp.Value;

                try
                {
                    var value = property.GetValue(settings);
                    SetControlValue(control, property.PropertyType, value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading value for {property.Name}: {ex.Message}",
                        "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void SetControlValue(Control control, Type propertyType, object value)
        {
            if (control is Panel && control.Tag is ListBox && propertyType == typeof(string[]))
            {
                var listBox = (ListBox)control.Tag;
                listBox.Items.Clear();
                var array = value as string[];
                if (array != null)
                {
                    listBox.Items.AddRange(array);
                }
            }
            else if (control is TextBox textBox)
            {
                if (propertyType == typeof(string))
                {
                    textBox.Text = value?.ToString() ?? string.Empty;
                }
                else if (propertyType == typeof(string[]))
                {
                    var array = value as string[];
                    textBox.Text = array != null ? string.Join(Environment.NewLine, array) : string.Empty;
                }
            }
            else if (control is CheckBox checkBox && propertyType == typeof(bool))
            {
                checkBox.Checked = (bool)(value ?? false);
            }
            else if (control is NumericUpDown numericUpDown && propertyType == typeof(int))
            {
                numericUpDown.Value = (int)(value ?? 0);
            }
            else if (control is ComboBox comboBox && propertyType.IsEnum)
            {
                if (value != null)
                {
                    comboBox.SelectedItem = value.ToString();
                }
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                Result = SaveControlsToSettings();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ParameterMissingException ex)
            {
                MessageBox.Show($"Provide value for: {FormatPropertyName(ex.ParameterProperty.Name)}",
                    "Configuration problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
            catch (ParameterEmptyException ex)
            {
                MessageBox.Show($"Provide value for: {FormatPropertyName(ex.ParameterName)}",
                    "Configuration problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
            catch (ParameterValueException ex)
            {
                MessageBox.Show($"Problem with value of {FormatPropertyName(ex.ParameterName)}. {ex.Message}",
                    "Configuration problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
            catch (ParameterException ex)
            {
                MessageBox.Show($"Problem with configuartion: {ex.Message}",
                    "Configuration problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}",
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Result = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private Settings SaveControlsToSettings()
        {
            var values = _propertyControls.Select(x => new { x.Key, Value = GetControlValue(x.Value, x.Key.PropertyType) })
                .ToDictionary(
                    x => x.Key.Name.ToString(),
                    x =>
                    {
                        if (x.Value == null) return null;
                        if (x.Value is string[] s) return s;
                        return new[] { x.Value.ToString() };
                    });

            var paramReader = new ParameterReader(new ParameterValueResolver());
            return paramReader.GetParameters<Settings>(values);
        }

        private object GetControlValue(Control control, Type propertyType)
        {
            if (control is Panel && control.Tag is ListBox && propertyType == typeof(string[]))
            {
                var listBox = (ListBox)control.Tag;
                return listBox.Items.Cast<string>().ToArray();
            }
            else if (control is TextBox textBox)
            {
                if (propertyType == typeof(string))
                {
                    return textBox.Text;
                }
                else if (propertyType == typeof(string[]))
                {
                    return textBox.Text
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            else if (control is CheckBox checkBox && propertyType == typeof(bool))
            {
                return checkBox.Checked;
            }
            else if (control is NumericUpDown numericUpDown && propertyType == typeof(int))
            {
                return (int)numericUpDown.Value;
            }
            else if (control is ComboBox comboBox && propertyType.IsEnum)
            {
                if (comboBox.SelectedItem != null)
                {
                    return Enum.Parse(propertyType, comboBox.SelectedItem.ToString());
                }
            }

            return null;
        }
    }
}
