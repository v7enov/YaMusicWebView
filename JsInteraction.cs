using Microsoft.Web.WebView2.Wpf;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static YaMusicWebView.EnumHelper;

namespace YaMusicWebView
{
    public class JsInteraction
    {
        private enum InteractionKeyCodes
        {
            VolumeUp = 61,
            VolumeDown = 45,
            MuteUnmute = 48,
            PausePlay = 112,
            PreviousTrack = 107,
            NextTrack = 108,
            Like = 102,
            Dislike = 100
        }

        private string InjectSimulateKeyFunction = """
            function simulateKey (keyCode, type, modifiers) {
            	var evtName = (typeof(type) === "string") ? "key" + type : "keydown";	
            	var modifier = (typeof(modifiers) === "object") ? modifier : {};

            	var event = document.createEvent("HTMLEvents");
            	event.initEvent(evtName, true, false);
            	event.keyCode = keyCode;

            	for (var i in modifiers) {
            		event[i] = modifiers[i];
            	}

            	document.dispatchEvent(event);
            }
            """;
        private readonly WebView2 webView;
        private readonly GlobalKeyboardHook globalKeyboardHook;
        private readonly Dictionary<string, string> appConfig;
        private readonly Dictionary<Keys, Action> KeyboardBindings = new Dictionary<Keys, Action>();

        public JsInteraction(WebView2 webView, GlobalKeyboardHook globalKeyboardHook, Dictionary<string, string> appConfig)
        {
            this.webView = webView;
            this.globalKeyboardHook = globalKeyboardHook;
            this.appConfig = appConfig;

            foreach (var item in appConfig)
            {
                var winKey = GetEnumValue<Keys>(item.Key);
                var jsAction = GetEnumValue<InteractionKeyCodes>(item.Value);

                KeyboardBindings.Add(winKey, async () => { await PressButtonJs((int)jsAction); });
            }

            this.globalKeyboardHook.KeyboardPressed += KeyboardKeyPressed;
        }

        private void KeyboardKeyPressed(object? sender, GlobalKeyboardHookEventArgs e)
        {
            if (e.KeyboardState != GlobalKeyboardHook.KeyboardState.KeyDown) return;
            var key = e.KeyboardData.Key;

            if (!KeyboardBindings.TryGetValue(key, out var handle)) return;

            handle();
        }


        private async Task PressButtonJs(int jsKeyCode)
        {
            if (!webView.IsLoaded) return;
            await webView.ExecuteScriptAsync(InjectSimulateKeyFunction);
            await webView.ExecuteScriptAsync($"simulateKey({jsKeyCode}, \"press\")");
        }
    }
}
