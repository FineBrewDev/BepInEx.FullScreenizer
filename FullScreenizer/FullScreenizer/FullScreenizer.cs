using System;
using System.IO;
using BepInEx;
using UnityEngine;


namespace Fullscreenizer
{
    [BepInPlugin("FineBrewDev.Fullscreenizer", "Fullscreenizer", "1.0")]
    internal class Fullscreenizer : BaseUnityPlugin
    {
        private static readonly string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private static readonly string path = Path.Combine(Path.GetDirectoryName(strExeFilePath), "FullScreenizerSettings.ini");

        private static string[] lines;
        private static string hotkey;

        private static int scaleMode;
        private static int width;
        private static int height;

        Resolution[] resolutions;
        Resolution bestRes;
        private void Awake()
        {
            //Finds supported resolution and picks the biggest supported resolution
            resolutions = Screen.resolutions;
            foreach (Resolution res in resolutions)
            {
                print(res.width + "x" + res.height);
                bestRes = res;
            }

            //Creates file if it doesn't exist and fills it with info and settings
            if (!File.Exists(path))
            {
                lines = new string[]
                {
                    "[Hotkey For Fullscreen Toggle]",
                    "# Default hotkey is F4 and in the event of invalid hotkey.",
                    "Hotkey = F4",
                    "",
                    "[Scaling Mode]",
                    "# Note: Generally leaving it on the default will work for most games. In the event it doesn't work you can try this alternate scaling mode.",
                    "# Warning: Alternate scaling may cause GUI issues.",
                    "# Setting Options: Default = 1 Alternate = 2",
                    "Scaling Mode = 1",
                    "",
                    "# Warning: These are the intial window dimensions. Don't change these as the plugin reads from it.",
                    "Intial Width = " + Screen.width,
                    "Intial Height = " + Screen.height
                };

                using (StreamWriter writer = new StreamWriter(path))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        writer.WriteLine(lines[i]);
                    }
                }
            }
        }

        private void Start()
        {
            CheckSettings();
            Debug.Log("hotkey = " + hotkey);
            Debug.Log("scaleMode = " + scaleMode);
            Debug.Log("width = " + width);
            Debug.Log("height = " + height);
        }

        private void LateUpdate()
        {
            //Custom hotkey
            if (!string.IsNullOrEmpty(hotkey))
            {
                CustomHotKey();
            }
            //Default hotkey
            else
            {
                DefaultHotKey();
            }
        }

        //Calculates scaling for fullscreen and return the smaller aspect of the two
        private int CalculateScaling()
        {
            int widthAspect = bestRes.width / width;
            int heightAspect = bestRes.height / height;

            if (widthAspect > heightAspect)
            {
                return heightAspect;
            }
            else
            {
                return widthAspect;
            }
        }

        //Reads UFHotkeySetting.txt file and applies settings
        private void CheckSettings()
        {
            using (var streamReader = new StreamReader(path))
            {
                try
                {
                    string line;
                    while (!streamReader.EndOfStream)
                    {
                        line = streamReader.ReadLine();
                        if (line.Contains("="))
                        {
                            int position = line.IndexOf("=");
                            if (line.Contains("Hotkey"))
                            {
                                hotkey = line.Substring(position + 1).Trim().ToUpper();
                            }

                            if (line.Contains("Scaling Mode"))
                            {
                                scaleMode = int.Parse(line.Substring(position + 1).Trim());
                            }

                            if (line.Contains("Intial Width"))
                            {
                                width = int.Parse(line.Substring(position + 1).Trim());
                            }

                            if (line.Contains("Intial Height"))
                            {
                                height = int.Parse(line.Substring(position + 1).Trim());
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Something went wrong chief");
                    Debug.Log(e.Message);
                }
            }
        }

        private void SelectScalingMode(int scaleMode)
        {
            //Alternative Scaling
            if (scaleMode == 2)
            {
                if (!Screen.fullScreen)
                {
                    Screen.SetResolution(width * CalculateScaling(), height * CalculateScaling(), true);
                }
                else
                {
                    Screen.SetResolution(width, height, false);
                }
            }
            //Default Scaling
            else
            {
                if (!Screen.fullScreen)
                {
                    Screen.SetResolution(Screen.width, Screen.height, true);
                }
                else
                {
                    Screen.SetResolution(Screen.width, Screen.height, false);
                }
            }
        }

        //Sets fullscreen toggle key to whatever key set in UFHotkeySetting.txt file
        private void CustomHotKey()
        {
            try
            {
                if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), hotkey)))
                {
                    SelectScalingMode(scaleMode);
                }
            }
            catch
            {
                DefaultHotKey();
                Debug.Log("Invalid hotkey set. Using default key instead.");
            }
        }

        //Defaults to F4 key for fullscreen toggle
        private void DefaultHotKey()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                SelectScalingMode(scaleMode);
            }
        }
    }
}

