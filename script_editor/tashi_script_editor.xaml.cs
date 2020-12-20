using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Newtonsoft.Json;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
/*
* Regarding Json.net:
* Copyright (c) 2007 James Newton-King
* Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace script_editor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public class ScriptString
        {
			public uint str_num;
			public string prefix;
			public uint ptr_pos;
			public uint str_pos;
			public string table;
			public bool repoint;
			[JsonIgnore]
			public string ja_text;
			[JsonIgnore]
			public string en_text;
			public ScriptString(uint StrNum, string Prefix, uint PtrPos, uint StrPos, string Table, bool Repoint)
            {
				str_num = StrNum;
				prefix = Prefix;
				ptr_pos = PtrPos;
                str_pos = StrPos;
				table = Table;
				repoint = Repoint;
			}
		}

        // list of strings from script, keyed by str_num
        readonly IDictionary<uint, ScriptString> stringList = new Dictionary<uint, ScriptString>();

        private void Button_OpenFile(object sender, RoutedEventArgs e)
		{
			string line;
			var stringIDList = new List<uint>();

			OpenFileDialog openFileDialog = new OpenFileDialog()
			{
				Filter = "Text documents (.txt)|*.txt" // Filter files by extension
            };

			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{

                //ja_textbox.Text = File.ReadAllText(openFileDialog.FileName);
                scriptPath.Text = openFileDialog.FileName;
				
				// this should be a function "parseScriptFile" or something.
				System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog.FileName);
				ScriptString currentString = null;
				EN_textbox.Text = "";
				JA_textbox.Text = "";

				while ((line = file.ReadLine()) != null)
                {
					if (line.Length > 0)
					{
						if (line.StartsWith("{"))
						{
							if (currentString != null && !currentString.repoint)
							{
								stringIDList.Add(currentString.str_num);
								stringList[currentString.str_num] = currentString;
							}

							currentString = JsonConvert.DeserializeObject<ScriptString>(line);
							// en_textbox.Text += currentString.str_num + "\n";
						}
						else if (line.StartsWith("#") && currentString != null)
						{
							if (!String.IsNullOrEmpty(currentString.ja_text))
							{
								currentString.ja_text += "\n";
							}
							currentString.ja_text += line[1..].Replace("<br>", "<br>\n").Trim();
						}
						else if (currentString != null)
                        {
							if (!String.IsNullOrEmpty(currentString.en_text))
							{
								currentString.en_text += "\n";
							}
								currentString.en_text += line;
                        }

					}
                }
				stringIDList.Add(currentString.str_num);
				stringList[currentString.str_num] = currentString;
			}
			stringSelector.ItemsSource = stringIDList;
			
			if (stringIDList.Count > 0)
			{
				stringSelector.SelectedItem = stringSelector.Items.GetItemAt(0);

			}

		}

		private void Button_SaveFile(object sender, RoutedEventArgs e)
		{
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                // saveFileDialog.FileName = "Document"; // Default file name
                DefaultExt = ".txt", // Default file extension
                Filter = "Text documents (.txt)|*.txt" // Filter files by extension
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string fn = saveFileDialog.FileName;
				(new FileStream(fn, FileMode.Truncate)).Close();
                using StreamWriter sw = File.AppendText(fn);
                foreach (ScriptString script_string in stringList.Values)
                {
                    string json_string = JsonConvert.SerializeObject(script_string);
                    sw.WriteLine(json_string);
                    sw.WriteLine("# " + script_string.ja_text.Replace("\n", "\n# "));
                    sw.WriteLine(script_string.en_text);
                    sw.WriteLine("\n");

                }

                //File.WriteAllText(fn, txtEditor.Text);
            }
		}

		private void EN_textbox_KeyDown(object sender, KeyEventArgs e)
		{
			// make this switch/case
			if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
			{
				switch(e.Key)
                {
					case Key.D1:
						EN_textbox.Text += "<player>";
						EN_textbox.Select(EN_textbox.Text.Length, 0);
						break;
                    case Key.D2:
						EN_textbox.Text += "<pc_itm>";
						EN_textbox.Select(EN_textbox.Text.Length, 0);
						break;
					case Key.D3:
						EN_textbox.Text += "<val>";
						EN_textbox.Select(EN_textbox.Text.Length, 0);
						break;
					case Key.D4:
						EN_textbox.Text += "<npc1>";
						EN_textbox.Select(EN_textbox.Text.Length, 0);
						break;
					case Key.D5:
						EN_textbox.Text += "<scroll>";
						EN_textbox.Select(EN_textbox.Text.Length, 0);
						break;
					case Key.PageUp:
						if (stringSelector.SelectedIndex > 0)
						{
							stringSelector.SelectedIndex -= 1;
						}
						else
                        {
							stringSelector.SelectedIndex = stringSelector.Items.Count-1;
                        }
						break;
					case Key.PageDown:
						if (stringSelector.SelectedIndex < stringSelector.Items.Count-1)
                        {
							stringSelector.SelectedIndex += 1;
                        }
						else
                        {
							stringSelector.SelectedIndex = 0;
                        }
						break;

					case Key.Return:
						stringSelector.SelectedItem = stringSelector.Items.GetItemAt(stringSelector.SelectedIndex+1);
						break;
					

				}
			}
		}


		public ScriptString selectedString;
		private void ListBox_SelectionChanged(object sender, System.EventArgs e)
		{
			uint selectedID;
			// sender.id read ID from list -> populate english and japanese textbox
			//en_textbox.Text = ((System.Windows.Controls.ListBox)sender).SelectedItem.ToString();

			selectedID = (uint) ((System.Windows.Controls.ListBox)sender).SelectedItem;
			selectedString = stringList[selectedID];
			JA_textbox.Text = selectedString.ja_text;
			EN_textbox.Text = selectedString.en_text;
			
			JA_textbox.IsEnabled = true;
			EN_textbox.IsEnabled = true;

			PointerOffset.Text = $"0x{selectedString.ptr_pos:X}";
			StringOffset.Text = $"0x{selectedString.str_pos:X}";

			int stringCount = stringList.Count;
			var matches = stringList.Where(pair => pair.Value.en_text != null).Select(pair => pair.Key);
            int tledCount = matches.Count();
			float completionPercent = (float)tledCount / (float)stringCount;

			Progress.Text = $"{tledCount} / {stringCount}  {completionPercent:P2}";

			if (selectedString.table == "normal")
            {
				NormalRB.IsChecked = true;
            }
			else
            {
				MenuRB.IsChecked = true;
            }

			stringSelector.ScrollIntoView(selectedID);
			HighlightTranslated();


		}

        private void HighlightTranslated()
        {
			for (int i = 0; i < stringSelector.Items.Count; i++)
			{
				uint stringID = (uint)stringSelector.Items[i];
				bool translated = !String.IsNullOrEmpty(stringList[stringID].en_text);

				if (translated)
				{
					ListBoxItem li = (ListBoxItem)stringSelector.ItemContainerGenerator.ContainerFromIndex(i);
					if (li != null)
					{
						var tledColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF2C3E40"));
						li.Background = tledColor;

					}
				}
			}
		}
		private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
			//scriptPath.Text = ((System.Windows.Controls.RadioButton)sender).Content.ToString().ToLower();
			selectedString.table = ((System.Windows.Controls.RadioButton)sender).Content.ToString().ToLower();

		}

        private void EN_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
			TextBox enTextbox = (TextBox) sender;
			selectedString.en_text = enTextbox.Text;
        }

        private void JA_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
			TextBox jaTextbox = (TextBox)sender;
			selectedString.ja_text = jaTextbox.Text;
		}
    }
}