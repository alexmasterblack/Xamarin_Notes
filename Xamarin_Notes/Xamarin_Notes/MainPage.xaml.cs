using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;

namespace Xamarin_Notes
{
    public partial class MainPage : ContentPage
    {
        private List<string> colors = new List<string> { Color.Pink.ToHex(), Color.PapayaWhip.ToHex(), Color.Plum.ToHex(), Color.PaleTurquoise.ToHex(), Color.PaleGreen.ToHex() };
        private bool tap = false;

        private void Count(IList<View> stack_one, IList<View> stack_two)
        {
            if (LeftNotes.Children.Count == 0 && RightNotes.Children.Count == 1)
            {
                var element = RightNotes.Children[RightNotes.Children.Count - 1];
                RightNotes.Children.Remove(element);
                LeftNotes.Children.Add(element);
            }
            if (stack_one.Count - stack_two.Count >= 2)
            {
                var element = stack_one[stack_one.Count - 1];
                stack_one.Remove(element);
                stack_two.Add(element);
            }
        }

        private void SaveNotes()
        {
            var file_notes = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "saved_notes.json");
            JArray left = new JArray();
            JArray right = new JArray();
            foreach(Frame note in LeftNotes.Children)
            {
                left.Add(new JObject()
                {
                    { "text", ((Label)((StackLayout)note.Content).Children[0]).Text },
                    { "date", ((Label)((StackLayout)note.Content).Children[1]).Text },
                    { "color", note.BackgroundColor.ToHex() }
                });
            }
            foreach (Frame note in RightNotes.Children)
            {
                right.Add(new JObject()
                {
                    { "text", ((Label)((StackLayout)note.Content).Children[0]).Text },
                    { "date", ((Label)((StackLayout)note.Content).Children[1]).Text },
                    { "color", note.BackgroundColor.ToHex() }
                });
            }
            JObject saved = new JObject()
            {
                { "left", left },
                { "right", right }
            };
            File.WriteAllText(file_notes, saved.ToString());
        }

        private void AddNote(string text_note, DateTime now_time, string color = "", string position = "")
        {
            var LabelText = new Label
            {
                Margin = new Thickness(0, 0, 0, 0),
                LineBreakMode = LineBreakMode.WordWrap,
                Text = text_note
            };
            var LabelInfo = new Label
            {
                Margin = new Thickness(0, 0, 0, 0),
                LineBreakMode = LineBreakMode.WordWrap,
                Text = now_time.ToString()
            };
            StackLayout NewNote = new StackLayout
            {
                Margin = new Thickness(0, 0, 0, 0),
                Children = {
                    LabelText,
                    LabelInfo
                }
            };
            if (color == "")
            {
                Random rand = new Random();
                color = colors[rand.Next(0, 5)];
            }
            Frame NoteFrame = new Frame
            {
                BackgroundColor = Color.FromHex(color),
                CornerRadius = 10,
                Margin = new Thickness(10, 10, 10, 10),
                Padding = new Thickness(15, 15, 15, 15),
                Content = NewNote
            };
            var pan = new PanGestureRecognizer();
            double info = 0;
            pan.PanUpdated += async (s, e) =>
            {
                switch (e.StatusType)
                {
                    case GestureStatus.Started:
                        NoteFrame.TranslationX = 0;
                        break;
                    case GestureStatus.Running:
                        if (e.TotalX > 0 && RightNotes.Children.Contains(NoteFrame))
                        {
                            info = e.TotalX;
                        }
                        else if (e.TotalX < 0 && LeftNotes.Children.Contains(NoteFrame))
                        {
                            info = e.TotalX;
                        }
                        break;
                    case GestureStatus.Completed:
                        if (info > 0 && RightNotes.Children.Contains(NoteFrame))
                        {
                            await NoteFrame.TranslateTo(NoteFrame.Width / 2, 0, 300, Easing.CubicInOut);
                            if (await DisplayAlert("Delete", "Are you sure?", "Yes", "No"))
                            {
                                await NoteFrame.FadeTo(0, 1000, Easing.CubicInOut);
                                RightNotes.Children.Remove(NoteFrame);
                                Count(LeftNotes.Children, RightNotes.Children);
                                SaveNotes();
                            } else
                            {
                                await NoteFrame.TranslateTo(0, 0, 300, Easing.CubicInOut);
                            }
                        }
                        else if (info < 0 && LeftNotes.Children.Contains(NoteFrame))
                        {
                            await NoteFrame.TranslateTo(-NoteFrame.Width / 2, 0, 300, Easing.CubicInOut);
                            if (await DisplayAlert("Delete", "Are you sure?", "Yes", "No"))
                            {
                                await NoteFrame.FadeTo(0, 1000, Easing.CubicInOut);
                                LeftNotes.Children.Remove(NoteFrame);
                                Count(RightNotes.Children, LeftNotes.Children);
                                SaveNotes();
                            } else
                            {
                                await NoteFrame.TranslateTo(0, 0, 300, Easing.CubicInOut);
                            }
                        }
                        break;
                }
            };
            NewNote.GestureRecognizers.Add(pan);
            var tap = new TapGestureRecognizer();
            tap.Tapped += (sender, e) =>
            {
                EditorPage edit = new EditorPage(text_note, now_time);
                edit.Disappearing += (send, ev) =>
                {
                    Add.IsVisible = false;
                    Clear.IsVisible = false;
                    this.tap = false;
                    if (edit.NoteText.Length == 0)
                    {
                        if (RightNotes.Children.Contains(NoteFrame))
                        {
                            RightNotes.Children.Remove(NoteFrame);
                            Count(LeftNotes.Children, RightNotes.Children);
                            SaveNotes();
                        }
                        if (LeftNotes.Children.Contains(NoteFrame))
                        {
                            LeftNotes.Children.Remove(NoteFrame);
                            Count(RightNotes.Children, LeftNotes.Children);
                            SaveNotes();
                        }
                        return;
                    } else
                    {
                        text_note = edit.NoteText;
                        LabelText.Text = text_note;
                        now_time = edit.NowTime;
                        LabelInfo.Text = now_time.ToString();
                    }
                };
                if (Navigation.NavigationStack.Count == 1)
                {
                    Navigation.PushAsync(edit);
                }
            };
            NewNote.GestureRecognizers.Add(tap);
            if (position == "")
            {
                if (LeftNotes.Height <= RightNotes.Height)
                {
                    LeftNotes.Children.Add(NoteFrame);
                }
                else
                {
                    RightNotes.Children.Add(NoteFrame);
                }
            } else
            {
                if (position == "left")
                {
                    LeftNotes.Children.Add(NoteFrame);
                } else
                {
                    RightNotes.Children.Add(NoteFrame);
                }
            }
        }
        public MainPage()
        {
            InitializeComponent();
            var file_notes = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "saved_notes.json");
            if (File.Exists(file_notes))
            {
                JObject notes = JObject.Parse(File.ReadAllText(file_notes));
                foreach (JObject note in notes["right"])
                {
                    AddNote((string)note["text"], DateTime.Parse((string)note["date"]), (string)note["color"], "right");
                }
                foreach (JObject note in notes["left"])
                {
                    AddNote((string)note["text"], DateTime.Parse((string)note["date"]), (string)note["color"], "left");
                }
            }
        }

        private void Add_Clicked(object sender, EventArgs e)
        {
            var edit = new EditorPage();
            edit.Disappearing += (send, ev) =>
            {
                Add.IsVisible = false;
                Clear.IsVisible = false;
                tap = false;
                if (edit.NoteText == null)
                {
                    return;
                }
                AddNote(edit.NoteText, edit.NowTime);
                SaveNotes();
            };
            if (Navigation.NavigationStack.Count == 1)
            {
                Navigation.PushAsync(edit);
            }
        }

        private void FindNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (Frame note in LeftNotes.Children)
            {
                Label text_note = (Label)((StackLayout)note.Content).Children[0];
                if (text_note.Text.ToLower().Contains(FindNote.Text.ToLower()))
                {
                    note.IsVisible = true;
                } else
                {
                    note.IsVisible = false;
                }
            }
            foreach (Frame note in RightNotes.Children)
            {
                Label text_note = (Label)((StackLayout)note.Content).Children[0];
                if (text_note.Text.ToLower().Contains(FindNote.Text.ToLower()))
                {
                    note.IsVisible = true;
                }
                else
                {
                    note.IsVisible = false;
                }
            }
        }

        private async void Clear_Clicked(object sender, EventArgs e)
        {
            if (LeftNotes.Children.Count != 0 && RightNotes.Children.Count != 0)
            {
                if (await DisplayAlert("Delete all", "Are you sure?", "Yes", "No"))
                {
                    LeftNotes.Children.Clear();
                    RightNotes.Children.Clear();
                    var file_notes = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "saved_notes.json");
                    File.Delete(file_notes);
                }
            }
        }

        private void Open_Clicked(object sender, EventArgs e)
        {
            if (!tap)
            {
                Add.TranslationY = 0;
                Clear.TranslationY = 0;
                Add.TranslateTo(0, -70, 300, Easing.CubicInOut);
                Clear.TranslateTo(0, -140, 300, Easing.CubicInOut);
                Add.IsVisible = true;
                Clear.IsVisible = true;
                tap = true;
            } else
            {
                Add.TranslationY = -70;
                Clear.TranslationY = -140;
                Add.TranslateTo(0, 0, 300, Easing.CubicInOut);
                Clear.TranslateTo(0, 0, 300, Easing.CubicInOut);
                tap = false;
            }
        }
    }
}
