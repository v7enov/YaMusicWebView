using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YaMusicWebView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlobalKeyboardHook hook;
        private JsInteraction jsInteraction;
        private readonly Dictionary<string, string> WinCodeActions = new Dictionary<string, string>();
        private IDeserializer deserializer;
        private ISerializer serializer;

        private string ConfigFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bindings.yaml");

        public MainWindow()
        {
            serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            InitializeComponent();

            deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            InitializeComponent();

            if (!File.Exists(ConfigFilePath))
                SetDefaultBindings();
            else
                WinCodeActions = deserializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ConfigFilePath));
            

            WebView.Source = new Uri("https://music.yandex.ru/");
            hook = new GlobalKeyboardHook();
            jsInteraction = new JsInteraction(WebView, hook, WinCodeActions);
        }

        private void SetDefaultBindings()
        {
            WinCodeActions.Add("Up", "VolumeUp");
            WinCodeActions.Add("Down", "VolumeDown");
            WinCodeActions.Add("Right", "NextTrack");
            WinCodeActions.Add("Left", "PreviousTrack");
            WinCodeActions.Add("F", "Like");
            WinCodeActions.Add("D", "Dislike");
            WinCodeActions.Add("M", "MuteUnmute");
            WinCodeActions.Add("P", "PausePlay");

            var yaml = serializer.Serialize(WinCodeActions);
            File.WriteAllText(ConfigFilePath, yaml);
        }
    }
}
