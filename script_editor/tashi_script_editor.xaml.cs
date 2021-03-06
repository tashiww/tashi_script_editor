﻿using System;
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
using System.Windows.Controls.Primitives;
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
            StringSelector.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;

		}
		private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
		{
			if (StringSelector.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				//StringSelector.ItemContainerGenerator.StatusChanged	-= ItemContainerGenerator_StatusChanged;
				Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
					new Action(DelayedAction));
				

			}

		}
		private void DelayedAction()
        {
			StringSelector.UpdateLayout();
			//var i = StringSelector.ItemContainerGenerator.ContainerFromIndex(StringSelector.SelectedIndex) as ListBoxItem;
			//i.IsSelected = true;
			//i.Focus();
			StringSelector.ScrollIntoView(StringSelector.SelectedIndex);
		}
		public class ScriptString
        {
			public string prefix;
			public uint ptr_pos;
			public uint str_pos;
			public string ptr_hex;
			public string str_hex;
			public string table;
			public bool repoint;
			public string ptr_type;
			[JsonIgnore]
			public string ja_text;
			[JsonIgnore]
			public string en_text;
			public ScriptString(uint StrNum, string Prefix, uint PtrPos, uint StrPos, string Table, bool Repoint, string PtrType)
            {
				prefix = Prefix;
				ptr_pos = PtrPos;
                str_pos = StrPos;
				str_hex = StrPos.ToString("X4");
				table = Table;
				repoint = Repoint;
				ptr_type = PtrType;
			}
		}

        // list of strings from script, keyed by ptr_pos
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
				using (System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog.FileName))
				{
					ScriptString currentString = null;
					EN_textbox.Text = "";
					JA_textbox.Text = "";

					while ((line = file.ReadLine()) != null)
					{
						if (line.Length > 0)
						{
							if (line.StartsWith("{"))
							{
								if (currentString != null)
								{
									stringIDList.Add(currentString.ptr_pos);
									stringList[currentString.ptr_pos] = currentString;
								}

								currentString = JsonConvert.DeserializeObject<ScriptString>(line);
								currentString.ptr_hex = currentString.ptr_pos.ToString("X4");
								currentString.str_hex = currentString.str_pos.ToString("X4");

								// en_textbox.Text += currentString.ptr_pos + "\n";
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
					stringIDList.Add(currentString.ptr_pos);
					stringList[currentString.ptr_pos] = currentString;
				}
			}
			StringSelector.ItemsSource = stringIDList;
			
			if (stringIDList.Count > 0)
			{
				StringSelector.SelectedItem = StringSelector.Items.GetItemAt(0);

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
				scriptPath.Text = fn;
				SaveFile(fn);

            }
		}

		private void SaveFile(string fn)
        {
			if (File.Exists(fn))
			{
				(new FileStream(fn, FileMode.Truncate)).Close();
			}
			using StreamWriter sw = File.AppendText(fn);
			foreach (ScriptString script_string in stringList.Values)
			{
				string json_string = JsonConvert.SerializeObject(script_string);
				sw.WriteLine(json_string);
				sw.WriteLine("# " + script_string.ja_text.Replace("\n", "\n# "));
				sw.WriteLine(script_string.en_text);
				sw.WriteLine("\n");
			}
			msgbox.Text = "File saved successfully.";

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
					case Key.D6:
						EN_textbox.Text += "<npc9>";
						EN_textbox.Select(EN_textbox.Text.Length, 0);
						break;
					case Key.PageUp:
						if (StringSelector.SelectedIndex > 0)
						{
							StringSelector.SelectedIndex -= 1;
						}
						else
                        {
							StringSelector.SelectedIndex = StringSelector.Items.Count-1;
                        }
						break;
					case Key.PageDown:
						if (StringSelector.SelectedIndex < StringSelector.Items.Count-1)
                        {
							StringSelector.SelectedIndex += 1;
                        }
						else
                        {
							StringSelector.SelectedIndex = 0;
                        }
						break;
					case Key.S:
						SaveFile(scriptPath.Text);
						break;
					case Key.Return:
						if (StringSelector.SelectedIndex < StringSelector.Items.Count - 1)
						{
							StringSelector.SelectedIndex += 1;
						}
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
			if (StringSelector.Items.Count == 0)
            {
				return;
            }
			if (((System.Windows.Controls.ListBox)sender).SelectedItem != null)
			{
				selectedID = (uint)((System.Windows.Controls.ListBox)sender).SelectedItem;
			}
			else
            {
				StringSelector.ScrollIntoView(StringSelector.SelectedIndex);
				return;
            }
			selectedString = stringList[selectedID];
			JA_textbox.Text = selectedString.ja_text;
			EN_textbox.Text = selectedString.en_text;

			if (selectedString.repoint)
            {
				EN_textbox.IsEnabled = false;
				JA_textbox.IsEnabled = false;

			}
			else
            {
				JA_textbox.IsEnabled = true;
				EN_textbox.IsEnabled = true;
			}

			PointerOffset.Text = $"0x{selectedString.ptr_pos:X}";
			StringOffset.Text = $"0x{selectedString.str_pos:X}";

			int stringCount = stringList.Where(pair => pair.Value.repoint == false).Select(pair => pair.Key).Count();
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

			StringSelector.ScrollIntoView(selectedID);
			HighlightTranslated();
			msgbox.Text = "";
		}

		private void HighlightTranslated()
        {
			for (int i = 0; i < StringSelector.Items.Count; i++)
			{
				uint stringID = (uint)StringSelector.Items[i];
				bool translated = !String.IsNullOrEmpty(stringList[stringID].en_text);

				if (translated)
				{
					ListBoxItem li = (ListBoxItem)StringSelector.ItemContainerGenerator.ContainerFromIndex(i);
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

		private void StringSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
			if (StringSearch.Text.Length > 0)
			{
				var filteredIDList = new List<uint>();
				foreach (ScriptString line in stringList.Values)
				{
					if ((!String.IsNullOrEmpty(line.en_text) && line.en_text.Contains(StringSearch.Text)) || 
						(!String.IsNullOrEmpty(line.ja_text) && line.ja_text.Contains(StringSearch.Text)))
					{
						filteredIDList.Add(line.ptr_pos);
					}
				}
				StringSelector.ItemsSource = filteredIDList;
				StringSelector.SelectedIndex = 0;
			}
			else
            {
				StringSelector.ItemsSource = stringList.Keys;
				StringSelector.UpdateLayout();
				StringSelector.ScrollIntoView(StringSelector.SelectedIndex);


			}
		}
private void StringSelector_SourceUpdated(object sender, DataTransferEventArgs e)
        {
			StringSelector.UpdateLayout();
			StringSelector.ScrollIntoView(StringSelector.SelectedIndex);

		}

	}
}