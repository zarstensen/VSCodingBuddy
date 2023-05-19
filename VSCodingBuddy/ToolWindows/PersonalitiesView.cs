using Microsoft.VisualStudio.Threading;
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
    public partial class PersonalitiesView : UserControl
    {
        public PersonalitiesView()
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

            textChanged();
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

            // IMPORTANT: The personalities in the settings are updated *before* the name of the item in the listbox.
            // if done the other way around, this would invoke the SelectedIndexChanged event,
            // which in turn would revert the changes in the textbox to the original contents
            
            settings.Personalities.Remove(prev_name);
            settings.Personalities.Add(NameTextBox.Text, new(CompileErrorTextArea.Text, ExceptionTextArea.Text));


            PersonalityList.Items[PersonalityList.SelectedIndex] = NameTextBox.Text;

            textChanged();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (PersonalityList.SelectedIndex == -1)
                return;

            string target_key = PersonalityList.Items[PersonalityList.SelectedIndex] as string;
            PersonalityList.Items.RemoveAt(PersonalityList.SelectedIndex);
            settings.Personalities.Remove(target_key);
        }

        private void CompressException_Click(object sender, EventArgs e)
        {
            if (settings.Speaker == null)
                return;

            string compressed_prompt = Task.Run(() => settings.Speaker.compressPrompt(ExceptionTextArea.Text)).Result;

            ExceptionTextArea.Text = compressed_prompt;
        }

        private void CompressCompileError_Click(object sender, EventArgs e)
        {
            if (settings.Speaker == null)
                return;

            string compressed_prompt = Task.Run(() => settings.Speaker.compressPrompt(CompileErrorTextArea.Text)).Result;

            CompileErrorTextArea.Text = compressed_prompt;
        }

        private void NameTextBox_TextChanged(object sender, EventArgs e) => textChanged();

        private void ExceptionTextArea_TextChanged(object sender, EventArgs e) => textChanged();

        private void CompileErrorTextArea_TextChanged(object sender, EventArgs e) => textChanged();

        // invoked whenever a textfield changes
        private void textChanged()
        {
            // check if all text fields contents match with the stored settings.
            // if not, the save button should be enabled, to indicate that the settings needs to be stored.
            bool can_save = false;

            if (PersonalityList.SelectedIndex != -1)
            {
                string name = PersonalityList.Items[PersonalityList.SelectedIndex] as string;
                SpeakerPersonality personality = settings.Personalities[name];

                if (ExceptionTextArea.Text != personality.ExceptionPrompt ||
                    CompileErrorTextArea.Text != personality.CompilePrompt ||
                    NameTextBox.Text != name)
                    can_save = true;
            }

            SaveButton.Enabled = can_save;

            // if the current name does not exist in the personalities dictionary, a new one is able to be created,
            // therefore the new button should be enabled
            NewButton.Enabled = !settings.Personalities.ContainsKey(NameTextBox.Text);
        }

    }
}
