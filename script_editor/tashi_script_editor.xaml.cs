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

		// data structure?
		// array xx
		// {"str_num": 1253, "prefix": "0x7", "ptr_pos": 267904, "str_pos": 268138, "ptr_pos_hex": "0x41680",
		// "str_pos_hex": "0x4176a", "table": "normal", ja_text: "<player>「父さん<br> しっかりして！」", en_text: "test string XX"}
		//#<player>「父さん<br> しっかりして！」
		//   test string #1253<player>


		//       class StringBlock :
		//""" Basic data for script blocks: description of string block,

		//      def __init__(self, pc= None, abs_offset= None, table= None, redirect= False):

		//      self.prefix = pc
		//      self.pc_hex = f'0x{pc:00x}'
		//      self.abs_offset = abs_offset
		//      self.abs_offset_hex = f'0x{abs_offset:00x}'
		//      self.table = table
		//      self.redirect = redirect
		//      self.ja_text = None
		//      self.en_text = None

		public class ScriptString
        {
			public uint str_num;
			public string prefix;
			public uint ptr_pos;
			public uint str_pos;
			public string table;
			public bool repoint;
			public string ja_text;
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

		IDictionary<uint, ScriptString> stringList = new Dictionary<uint, ScriptString>();


		private void Button_OpenFile(object sender, RoutedEventArgs e)
		{
			string line;
			var stringIDList = new List<uint>();

			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{

				//ja_textbox.Text = File.ReadAllText(openFileDialog.FileName);
				scriptPath.Text = openFileDialog.FileName;
				// stringList.ItemsSource = new String[] { "a", "b", "c" };
				System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog.FileName);
				while ((line = file.ReadLine()) != null)
                {
					if (line.StartsWith("{"))
                    {
						
                        ScriptString foo = JsonConvert.DeserializeObject<ScriptString>(line);
						// en_textbox.Text += foo.str_num + "\n";
						string ja_line = file.ReadLine();
						if (ja_line.StartsWith("#") )
						{
							foo.ja_text = ja_line[1..];
                        }
						if (!foo.repoint)
                        {
							stringIDList.Add(foo.str_num);
							stringList[foo.str_num] = foo;
						}
					}
                }
				
			}
			stringSelector.ItemsSource = stringIDList;
		}

		private void Button_SaveFile(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			// saveFileDialog.FileName = "Document"; // Default file name
			saveFileDialog.DefaultExt = ".txt"; // Default file extension
			saveFileDialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
			if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				msgbox.Text = "Saved, I guess.";
				string fn = saveFileDialog.FileName;
				//File.WriteAllText(fn, txtEditor.Text);
			}
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			uint selectedID;
			// sender.id read ID from list -> populate english and japanese textbox
			//en_textbox.Text = ((System.Windows.Controls.ListBox)sender).SelectedItem.ToString();

			selectedID = (uint) ((System.Windows.Controls.ListBox)sender).SelectedItem;
			ja_textbox.Text = stringList[selectedID].ja_text;
            en_textbox.Text = stringList[selectedID].en_text;

		}

	}
}