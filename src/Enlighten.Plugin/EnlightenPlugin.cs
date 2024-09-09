﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Enlighten.src.Enlighten.Plugin
{
	[Plugin("Enlighten")]
	public class EnlightenPlugin
	{
		private UI ui;
		private ExtensionButton extensionButton;

		[Init]
		private void Init()
		{
			SceneManager.sceneLoaded += SceneLoaded;

			AssetBundle bundle = LoadBundle();
			ui = new UI(bundle);
			extensionButton = MakeExtensionButton(bundle);
			extensionButton.Click += () => ui.OnExtensionButtonPressed();
			bundle.UnloadAsync(false);
		}

		private AssetBundle LoadBundle()
		{
			System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enlighten.enlighten_bundle");
			AssetBundle assetBundle = AssetBundle.LoadFromStream(stream);
			stream.Close();
			return assetBundle;
		}

		private ExtensionButton MakeExtensionButton(AssetBundle bundle)
		{
			ExtensionButton button = new ExtensionButton
			{
				Tooltip = "Enlighten",
				Icon = bundle.LoadAsset<Sprite>("Assets/Images/icon.png")
			};
			ExtensionButtons.AddButton(button);
			return button;
		}

		private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			if (arg0.buildIndex == 3) // Mapping Scene
			{
				ui.OnMappingSceneLoaded();
			}
		}
	}
}