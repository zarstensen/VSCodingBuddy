using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSCodingBuddy.ToolWindows;

namespace VSCodingBuddy.ToolWindows
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        internal PersonalitiesPage settings;

        public void Initialize()
        {
            foreach(var key in settings.Personalities.Keys)
                PersonalityList.Items.Add(key);
        }

        private void PersonalityList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(PersonalityList.SelectedIndex == -1)
            {
                NameTextBox.Text = "";
                ExceptionTextArea.Text = "";
                CompileErrorTextArea.Text = "";
            }
            else
            {
                string target_key = PersonalityList.Items[PersonalityList.SelectedIndex] as string;
                NameTextBox.Text = target_key;
                ExceptionTextArea.Text = settings.Personalities[target_key].ExceptionPrompt;
                CompileErrorTextArea.Text = settings.Personalities[target_key].CompilePrompt;
            }
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            if (NameTextBox.Text == String.Empty || PersonalityList.Items.Contains(NameTextBox.Text))
                return;

            PersonalityList.Items.Add(NameTextBox.Text);

            settings.Personalities.Add(NameTextBox.Text, new(CompileErrorTextArea.Text, ExceptionTextArea.Text));
            
            PersonalityList.SelectedIndex = PersonalityList.Items.Count - 1;
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (PersonalityList.SelectedIndex == -1)
                return;

            string prev_name = PersonalityList.Items[PersonalityList.SelectedIndex] as string;

            PersonalityList.Items[PersonalityList.SelectedIndex] = NameTextBox.Text;

            settings.Personalities.Remove(prev_name);

            settings.Personalities.Add(NameTextBox.Text, new(CompileErrorTextArea.Text, ExceptionTextArea.Text));
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (PersonalityList.SelectedIndex == -1)
                return;

            string target_key = PersonalityList.Items[PersonalityList.SelectedIndex] as string;
            PersonalityList.Items.RemoveAt(PersonalityList.SelectedIndex);
            settings.Personalities.Remove(target_key);
        }
    }
}
