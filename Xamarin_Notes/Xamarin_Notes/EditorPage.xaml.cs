using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin_Notes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditorPage : ContentPage
    {
        public DateTime NowTime;
        public string NoteText { get; set; }
        public string memory = "";
        public EditorPage()
        {
            InitializeComponent();
            NowTime = DateTime.Now;
            DataSymbols.Text = NowTime.ToString();
        }
        public EditorPage(string text_note, DateTime now_time)
        {
            InitializeComponent();
            NoteText = text_note;
            FieldNote.Text = NoteText;
            NowTime = now_time;
            DataSymbols.Text = NowTime.ToString() + " | " + NoteText.Length.ToString();
        }

        private void Add_Clicked(object sender, EventArgs e)
        {
            NowTime = DateTime.Now;
            NoteText = FieldNote.Text;
            memory = "";
            Navigation.PopAsync();
        }
    }
}