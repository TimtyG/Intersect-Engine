﻿/*
    The MIT License (MIT)

    Copyright (c) 2015 JC Snider, Joe Bridges
  
    Website: http://ascensiongamedev.com
    Contact Email: admin@ascensiongamedev.com

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/
using Gwen;
using Gwen.Control;
using Intersect_Client.Classes.UI.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intersect_Client.Classes.UI
{
    public class OptionsWindow : IGUIElement
    {
        //Controls
        private Base _optionsMenu;
        private Label _resolutionLabel;
        private ComboBox _resolutionList;
        private LabeledCheckBox _fullscreen;
        private Label _soundLabel;
        private HorizontalSlider _soundSlider;
        private Label _musicLabel;
        private HorizontalSlider _musicSlider;
        private Button _applyBtn;
        private Button _backBtn;
        private Label _fpsLabel;
        private ComboBox _fpsList;

        private bool _gameWindow = false;
        private MainMenu _mainMenu = null;

        //Init
        public OptionsWindow(Base _parent, Boolean InGame, MainMenu mainMenu = null)
        {
            _gameWindow = InGame;
            _mainMenu = mainMenu;

            //Options Window
            if (_gameWindow)
            {
                _optionsMenu = new WindowControl(_parent, "Options");
                _optionsMenu.SetSize(200, 200);
                _optionsMenu.SetPosition(Graphics.ScreenWidth / 2 - 100, Graphics.ScreenHeight / 2 - 80);
                ((WindowControl)_optionsMenu).DisableResizing();
                _optionsMenu.Margin = Margin.Zero;
                _optionsMenu.Padding = Padding.Zero;
                _optionsMenu.IsHidden = true;
            }
            else
            {
                _optionsMenu = _parent;
            }

            //Options - Resolution Label
            _resolutionLabel = new Label(_optionsMenu);
            _resolutionLabel.SetText("Resolution:");
            _resolutionLabel.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 12);

            _resolutionList = new ComboBox(_optionsMenu);
            var myModes = Graphics.GetValidVideoModes();
            for (var i = 0; i < myModes.Count(); i++)
            {
                _resolutionList.AddItem(myModes[i].Width + "x" + myModes[i].Height);
            }
            _resolutionList.SetSize(120, 14);
            _resolutionList.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 28);
            _resolutionList.IsHidden = true;

            //Options - FPS Label
            _fpsLabel = new Label(_optionsMenu);
            _fpsLabel.SetText("Target FPS:");
            _fpsLabel.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 48);
            _fpsLabel.IsHidden = true;

            //Options - FPS List
            _fpsList = new ComboBox(_optionsMenu);
            _fpsList.AddItem("V-Sync");
            _fpsList.AddItem("30");
            _fpsList.AddItem("60");
            _fpsList.AddItem("90");
            _fpsList.AddItem("120");
            _fpsList.AddItem("No Limit");
            _fpsList.SetSize(80, 14);
            _fpsList.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 64);
            _fpsList.IsHidden = true;

            //Options - Fullscreen Checkbox
            _fullscreen = new LabeledCheckBox(_optionsMenu) { Text = "FS" };
            _fullscreen.SetToolTipText("Fullscreen");
            _fullscreen.SetSize(40, 14);
            _fullscreen.SetPosition(_fpsList.X + _fpsList.Width + 4, 64);

            //Options - Sound Label
            _soundLabel = new Label(_optionsMenu);
            _soundLabel.SetText("Sound Volume: 100%");
            _soundLabel.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 80);

            //Options - Sound Slider
            _soundSlider = new HorizontalSlider(_optionsMenu);
            _soundSlider.SetSize(120, 14);
            _soundSlider.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 92);
            _soundSlider.Min = 0;
            _soundSlider.Max = 100;
            _soundSlider.ValueChanged += _soundSlider_ValueChanged;

            //Options - Music Label
            _musicLabel = new Label(_optionsMenu);
            _musicLabel.SetText("Music Volume: 100%");
            _musicLabel.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 106);

            //Options - Music Slider
            _musicSlider = new HorizontalSlider(_optionsMenu);
            _musicSlider.SetSize(120, 14);
            _musicSlider.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 118);
            _musicSlider.Min = 0;
            _musicSlider.Max = 100;
            _musicSlider.ValueChanged += _musicSlider_ValueChanged;

            //Options - Apply Button
            _applyBtn = new Button(_optionsMenu);
            _applyBtn.SetText("Apply");
            _applyBtn.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 136);
            _applyBtn.SetSize(120, 32);
            _applyBtn.Clicked += ApplyBtn_Clicked;

            if (!InGame)
            {
                _applyBtn.SetPosition(_optionsMenu.Width / 2 - 120 / 2, 136);
                _applyBtn.SetSize(56, 32);

                //Options - Back Button
                _backBtn = new Button(_optionsMenu);
                _backBtn.SetText("Back");
                _backBtn.SetPosition(_optionsMenu.Width / 2 + 4, 136);
                _backBtn.SetSize(56, 32);
                _backBtn.IsHidden = true;
                _backBtn.Clicked += BackBtn_Clicked;

            }
        }


        //Methods
        public void Update()
        {

        }
        public void Show()
        {
            if (Graphics.GetValidVideoModes().Count > 0)
            {
                _resolutionList.SelectByText(Graphics.GetValidVideoModes()[Graphics.DisplayMode].Width + "x" + Graphics.GetValidVideoModes()[Graphics.DisplayMode].Height);
            }
            switch (Graphics.FPS)
            {
                case -1: //Unlimited
                     _fpsList.SelectByText("No Limit");
                    break;
                case 0: //VSYNC
                     _fpsList.SelectByText("V-Sync");
                    break;
                case 1:
                     _fpsList.SelectByText("30");
                    break;
                case 2:
                     _fpsList.SelectByText("60");
                    break;
                case 3:
                     _fpsList.SelectByText("90");
                     break;
                case 4:
                     _fpsList.SelectByText("120");
                    break;
                default:
                    _fpsList.SelectByText("V-Sync");
                    break;
            }
            _fullscreen.IsChecked = Graphics.FullScreen;
            _musicSlider.Value = Globals.MusicVolume;
            _soundSlider.Value = Globals.SoundVolume;
            if (_gameWindow) { _optionsMenu.IsHidden = false; }
            _resolutionLabel.IsHidden = false;
            _resolutionList.IsHidden = false;
            _fullscreen.IsHidden = false;
            _soundLabel.IsHidden = false;
            _soundSlider.IsHidden = false;
            _musicLabel.IsHidden = false;
            _musicSlider.IsHidden = false;
            _applyBtn.IsHidden = false;
            _fpsList.IsHidden = false;
            _fpsLabel.IsHidden = false;
            if (!_gameWindow) { _backBtn.IsHidden = false; }
        }

        public bool IsVisible()
        {
            return !_resolutionList.IsHidden;
        }

        public void Hide()
        {
            if (_gameWindow) { _optionsMenu.IsHidden = true; }
            _resolutionLabel.IsHidden = true;
            _resolutionList.IsHidden = true;
            _fullscreen.IsHidden = true;
            _soundLabel.IsHidden = true;
            _soundSlider.IsHidden = true;
            _musicLabel.IsHidden = true;
            _musicSlider.IsHidden = true;
            _applyBtn.IsHidden = true;
            _fpsList.IsHidden = true;
            _fpsLabel.IsHidden = true;
            if (!_gameWindow) { _backBtn.IsHidden = true; }
        }

        //Input Handlers
        void BackBtn_Clicked(Base sender, ClickedEventArgs arguments)
        {
            _mainMenu.Reset();
        }
        void _musicSlider_ValueChanged(Base sender, EventArgs arguments)
        {
            _musicLabel.Text = "Music Volume: " + (int)_musicSlider.Value + "%";
        }

        void _soundSlider_ValueChanged(Base sender, EventArgs arguments)
        {
            _soundLabel.Text = "Sound Volume: " + (int)_soundSlider.Value + "%";
        }
        void ApplyBtn_Clicked(Base sender, ClickedEventArgs arguments)
        {
            var mi = _resolutionList.SelectedItem;
            var myModes = Graphics.GetValidVideoModes();
            for (var i = 0; i < myModes.Count(); i++)
            {
                if (mi.Text != myModes[i].Width + "x" + myModes[i].Height) continue;
                Graphics.DisplayMode = i;
                Graphics.MustReInit = true;
            }
            if (Graphics.FullScreen != _fullscreen.IsChecked)
            {
                Graphics.FullScreen = _fullscreen.IsChecked;
                Graphics.MustReInit = true;
            }
            var newFps = 0;
            switch (_fpsList.SelectedItem.Text)
            {
                case "V-Sync":
                    //Stick with 0
                    break;
                case "No Limit":
                    newFps = -1;
                    break;
                case "30":
                    newFps = 1;
                    break;
                case "60":
                    newFps = 2;
                    break;
                case "90":
                    newFps = 3;
                    break;
                case "120":
                    newFps = 4;
                    break;
                    
            }
            if (newFps != Graphics.FPS) {
                Graphics.FPS = newFps;
                Graphics.MustReInit = true;
            }
            Globals.MusicVolume = (int)_musicSlider.Value;
            Globals.SoundVolume = (int)_soundSlider.Value;
            Database.SaveOptions();
        }
    }
}