using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class Soulsong : MonoBehaviour {

	public KMAudio Audio;
	public KMBombModule Module;
	public KMBombInfo Bomb;
	public KMBossModule Boss;

	public TextMesh[] Text;
	public KMSelectable[] Songs;
	private string[] ignoredModules = { "Soulsong", "Soulscream", "OmegaForget", "14", " Brainf---", " Forget Enigma", " Forget Everything", " Forget It Not", " Forget Me Not", " Forget Me Later", " Forget Perspective", " Forget The Colors", " Forget Them All", " Forget This", " Forget Us Not", " Iconic", " Organization", " RPS Judging", " Simon Forgets", " Simon's Stages", " Souvenir", " Tallordered Keys", " The Twin", " The Very Annoying Button", " Ultimate Custom Night", "Übermodule" };
	static private int _moduleIdCounter = 1;
	private int _moduleId;

	private string Answer, input;
	private string[] StageStorage = new string[5];
	private int Stage, Time, Solves, itsgonnabreakeverything;
	private float solvepoints, pps = 2f;
	private bool Active = true, counting, final, solved, pleasewait;

	private static string SOULS = "!XAHUEOI";
    private List<int> StageRecovery = new List<int>();
	private static string[] BINARY = { "0000", "0001", "0010", "0011", "0100", "0101", "0110", "0111", "1000", "1001", "1010", "1011", "1100", "1101", "1110", "1111" };
	private static string[] BINARY2 = { "000", "001", "010", "011", "100", "101", "110", "111" };

	void Awake()
	{
		_moduleId = _moduleIdCounter++;
		string[] ingore = Boss.GetIgnoredModules(Module, ignoredModules);
		if (ingore != null)
			ignoredModules = ingore;

		for (byte i = 0; i < Songs.Length; i++)
		{
			KMSelectable btn = Songs[i];
			btn.OnInteract += delegate
			{
				HandlePress(btn);
				return false;
			};
		}
	}
	// Use this for initialization
	void Start() {
		GenerateStage();
	}

	// Update is called once per frame
	void Update() {
		if (Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != Solves)
		{
			if (!Application.isEditor && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != 0)
			{
				if (Active)
				{
					Debug.LogFormat("[Soulsong #{0}]: Module Solved. 2 minutes.", _moduleId);
					Active = false;
					StartCoroutine(AddTime(120, true));
				}
				else
				{
					StartCoroutine(AddTime(30, false));
					Debug.LogFormat("[Soulsong #{0}]: Module Solved. Adding 30 seconds.", _moduleId);
				}
				Solves++;
			}
			else
			{
				++Stage;
				Solves++;
				GenerateStage();
			}

		}
	}
	void HandlePress(KMSelectable btn)
	{
		int X = Array.IndexOf(Songs, btn);
		Songs[X].AddInteractionPunch();
		Audio.PlaySoundAtTransform("invertedv", Songs[X].transform);
		string temp = "";
		char[] temptemp;
		if (!solved && !pleasewait)
		{
			switch (X)
			{
				case 0: temp += "0"; break;
				case 1: temp += "1"; break;
				case 2: temp += "2"; break;
				case 3: temp += "3"; break;
				case 4: temp += "4"; break;
				case 5: temp += "5"; break;
				case 6: temp += "6"; break;
				case 7: temp += "7"; break;
				case 8: temp += "8"; break;
				case 9: temp += "9"; break;
			}
			if (Answer[itsgonnabreakeverything].ToString() == temp && final)
			{
				temptemp = input.ToCharArray();
				temptemp[itsgonnabreakeverything] = temp[0];
				input = temptemp.Join("");
				if (input.Length > 9)
				{
					Text[0].text = (itsgonnabreakeverything < 5 ? input.Substring(0, 10) : itsgonnabreakeverything + 5 < input.Length ? input.Substring(itsgonnabreakeverything - 5, 10) : input.Substring(input.Length - 10));
				}
				else
				{
					Text[0].text = input;
				}
				itsgonnabreakeverything++;
				Text[1].text = "[" + (input.Length - itsgonnabreakeverything).ToString().PadLeft(3, '0') + "]";
				if (Text[1].text == "[000]")
				{
					Module.HandlePass();
					solved = true;
					final = false;
				}
			}
			else if (final)
			{
				Debug.LogFormat("[Soulsong #{0}]: You pressed {1} when I wanted {2}.", _moduleId, X, Answer[itsgonnabreakeverything]);
				Module.HandleStrike();
				StartCoroutine(RefreshStages());
			}
			else
			{
				Debug.LogFormat("[Soulsong #{0}]: Too early...", _moduleId);
				Module.HandleStrike();
			}
		}
	}
	void GenerateStage()
	{
		if (Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Solves != 0)
		{
			Debug.LogFormat("[Soulsong #{0}]: Entering into Stage #{1}.", _moduleId, Stage);
			StageStorage[2] = "";
			while (StageStorage[2] == "")
			{
				StageStorage[0] = "" + Rnd.Range(0, 10) + Rnd.Range(0, 10) + Rnd.Range(0, 10) + Rnd.Range(0, 10) + Rnd.Range(0, 10) + Rnd.Range(0, 10);
				string temp = "";
				for(int i = 0; i < 6; i++)
                {
					temp = "";
					temp = BINARY[StageStorage[0][i] - '0'];
					temp = temp.Replace('0', '!');
					temp = temp.Replace('1', '0');
					temp = temp.Replace('!', '1');
					StageStorage[1] += temp;
                }
				for (int i = 0; i < 8; i++)
				{
					StageStorage[2] += Array.IndexOf(BINARY2, ("" + StageStorage[1][i * 3] + StageStorage[1][i * 3 + 1] + StageStorage[1][i * 3 + 2]).Join(""));
				}
				Text[1].text = "[" + (Stage < 100 ? "0" : "") + (Stage < 10 ? "0" : "") + Stage + "]";
			}
			solvepoints += pps;
			StageStorage[3] = StageStorage[2].Select(a => SOULS[int.Parse(a.ToString())]).Join("");
			Text[0].text = StageStorage[3];
			Debug.LogFormat("[Soulsong #{0}]: The displayed scream for this stage is {1}.", _moduleId, StageStorage[3]);
			StageRecovery.Add(int.Parse(StageStorage[0]));
			Debug.LogFormat("[Soulsong #{0}]: The answer for stage {1} is {2}.", _moduleId, Stage, StageStorage[0]);
			Answer += StageStorage[0];
			for(int i=0; i<5; i++)
            {
				StageStorage[i] = "";
            }

		}
		else
		{
			if (Solves != 0)
			{
				//Break and enter submission mode, the souls can be put to rest//
				Debug.LogFormat("[Soulsong #{0}]: The final input song is {1}.", _moduleId, Answer);
				final = true;
				Text[0].text = "" + (Answer.Length < 10 ? "".PadRight(Answer.Length, '-') : "----------");
				input = "-".PadRight(Answer.Length, '-');
				Text[1].text = "[" + (input.Length - 0).ToString().PadLeft(3, '0') + "]";
				final = true;

			}
			else
			{
				Debug.LogFormat("[Soulsong #{0}]: Unable to generate stages :pensive:.", _moduleId);
				Module.HandlePass();
			}
		}
	}
	IEnumerator Incinerate()
	{
		while (Time > 0 && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != 0)
		{
			if (counting)
			{
				Text[0].text = Time / 60 + ":" + (Time % 60 < 10 ? "0" + (Time % 60).ToString() : (Time % 60).ToString());
				yield return new WaitForSeconds(1f);
				Time--;
				if (Time <= 0 && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != 0)
				{
					Debug.LogFormat("[Soulsong #{0}]: ginr ginR! Entering stage #{1}.", _moduleId, ++Stage);
					GenerateStage();
					Active = true;
					Audio.PlaySoundAtTransform("inverteda", Songs[3].transform);
				}
				else if (Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() == 0)
				{
					Debug.LogFormat("[Soulsong #{0}]: Entering Finale...", _moduleId, ++Stage);
					GenerateStage();
					Active = true;
					counting = false;
					Time = 0;
					Audio.PlaySoundAtTransform("inverteda", Songs[3].transform);
				}
			}
			yield return null;
		}
	}
	IEnumerator AddTime(int add, bool what)
	{
		Audio.PlaySoundAtTransform("daolnwod", Songs[3].transform);
		int i = 0;
		counting = false;
		while (i < add && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != 0)
		{
			i++;
			Time++;
			Text[0].text = Time / 60 + ":" + (Time % 60 < 10 ? "0" + (Time % 60).ToString() : (Time % 60).ToString());
			yield return new WaitForSeconds(.02f);
		}
		if (what && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != 0)
		{
			StartCoroutine(Incinerate());
			counting = true;
		}
		else if (Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != 0)
		{
			counting = true;
		}
	}
	IEnumerator RefreshStages()
    {
		pleasewait = true;
		for(int i = 0; i < StageRecovery.Count; i++)
        {
			Text[0].text = StageRecovery[i].ToString();
			yield return new WaitForSeconds(2);
		}
		pleasewait = false;
		Text[0].text = input.Length > 9 ? Text[0].text = (itsgonnabreakeverything < 5 ? input.Substring(0, 10) : itsgonnabreakeverything + 5 < input.Length ? input.Substring(itsgonnabreakeverything - 5, 10) : input.Substring(input.Length - 10)) : input;
	}
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press 3521 (Presses the song-quence 3521)";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		bool Valid = true;
		Match m;
		if ((m = Regex.Match(command, @"^\s*press\s+(.*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
		{
			var a = m.Groups[1].Value;
			string stupid = a.Join("").ToUpper();
			Debug.Log(a);
			Debug.Log(stupid);
			if (!stupid.All(char.IsNumber))
			{
				Valid = false;
			}
			if (!Valid)
			{
				yield return "sendtochaterror Invalid number. Valid numbers are 0, 1, 2, 3, 4, 5, 6, 7, 8, and 9.";
			}
			else if (pleasewait)
            {
				yield return "sendtochaterror Module has struck, please wait.";
			}
			else
			{
				yield return null;
				yield return "solve";
				yield return "awardpointsonsolve " + solvepoints;
				for (int i = 0; i < stupid.Length; i++)
				{
					Songs[stupid[i] - '0'].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
		else
			yield return "sendtochaterror Incorrect Syntax. Use '!{1} press 3'.";
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		while (!final || pleasewait)
		{ //Wait until submission time
			yield return true;
		}
		for (int i = itsgonnabreakeverything; i < Answer.Length; i++)
		{
			Songs[int.Parse(Answer[i].ToString())].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
	}
}
