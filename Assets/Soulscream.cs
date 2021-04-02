using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class Soulscream : MonoBehaviour {

	public KMAudio Audio;
	public KMBombModule Module;
	public KMBombInfo Bomb;
	public KMBossModule Boss;

	public TextMesh[] Text;
	public KMSelectable[] Screams;
	private string[] ignoredModules = { "Soulscream", "OmegaForget", "14", " Brainf---", " Forget Enigma", " Forget Everything", " Forget It Not", " Forget Me Not", " Forget Me Later", " Forget Perspective", " Forget The Colors", " Forget Them All", " Forget This", " Forget Us Not", " Iconic", " Organization", " RPS Judging", " Simon Forgets", " Simon's Stages", " Souvenir", " Tallordered Keys", " The Twin", " The Very Annoying Button", " Ultimate Custom Night", "Übermodule" };
	static private int _moduleIdCounter = 1;
	private int _moduleId;

	private string Answer, input;
	private string[] StageStorage = new string[4];
	private int Stage, Time, Solves, itsgonnabreakeverything;
	private bool Active = true, counting, final;

	private static string SOULS = "-XAHUEOI";
	private static string[] BINARY  = { "0000", "0001", "0010", "0011", "0100", "0101", "0110", "0111", "1000", "1001" };
	private static string[] BINARY2 = { "000", "001", "010", "011", "100", "101", "110", "111" };

	void Awake()
	{
		_moduleId = _moduleIdCounter++;
		string[] ingore = Boss.GetIgnoredModules(Module, ignoredModules);
		if (ingore != null)
			ignoredModules = ingore;

		for (byte i = 0; i < Screams.Length; i++)
		{
			KMSelectable btn = Screams[i];
			btn.OnInteract += delegate
			{
				HandlePress(btn);
				return false;
			};
		}
	}
	// Use this for initialization
	void Start () {
		GenerateStage();
	}
	
	// Update is called once per frame
	void Update () {
		if(Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != Solves)
        {
			if (!Application.isEditor && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Count() != 0)
			{
				if (Active)
				{
					Debug.LogFormat("[Soulscream #{0}]: Module Solved. 5 minutes.", _moduleId);
					Active = false;
					StartCoroutine(AddTime(300, true));
				}
				else
				{
					StartCoroutine(AddTime(120, false));
					Debug.LogFormat("[Soulscream #{0}]: Module Solved. Adding 2 minutes", _moduleId);
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
		int X = Array.IndexOf(Screams, btn);
		Screams[X].AddInteractionPunch();
		Audio.PlaySoundAtTransform("v", Screams[X].transform);
		string temp = "";
		char[] temptemp;
        switch (X)
        {
            case 0: temp += "X"; break;
            case 1: temp += "A"; break;
            case 2: temp += "H"; break;
            case 3: temp += "U"; break;
            case 4: temp += "E"; break;
            case 5: temp += "O"; break;
            case 6: temp += "I"; break;
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
			Text[1].text = "["+ (input.Length - itsgonnabreakeverything).ToString().PadLeft(3, '0') + "]";
			if(Text[1].text == "[000]")
            {
				Module.HandlePass();
				final = false;
            }
		}
        else
        {
			Debug.LogFormat("[Soulscream #{0}]: You pressed {1} when I wanted {2}.", _moduleId, SOULS[X + 1], Answer[itsgonnabreakeverything]);
			Module.HandleStrike();
        }
	}
	void GenerateStage()
	{
		if (Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Solves != 0)
		{
			Debug.LogFormat("[Soulscream #{0}]: Entering into Stage #{1}.", _moduleId, Stage);
			StageStorage[2] = "";
			while (StageStorage[2] == "")
			{
				StageStorage[0] = "" + Rnd.Range(0, 10) + Rnd.Range(0, 10) + Rnd.Range(0, 10) + Rnd.Range(0, 10) + Rnd.Range(0, 10);
				Text[0].text = StageStorage[0];
				Text[1].text = "[" + (Stage < 100 ? "0" : "") + (Stage < 10 ? "0" : "") + Stage + "]";
				//Generate a stage normally
				StageStorage[1] = "";
				StageStorage[2] = "";
				for (int i = 0; i < 5; i++)
				{
					StageStorage[1] += BINARY[int.Parse(StageStorage[0][i].ToString())];
				}
				StageStorage[1] += StageStorage[1].Substring(0, 13);
				for (int i = 0; i < 11; i++)
				{
					StageStorage[2] += Array.IndexOf(BINARY2, ("" + StageStorage[1][i * 3] + StageStorage[1][i * 3 + 1] + StageStorage[1][i * 3 + 2]).Join(""));
					if (StageStorage[2][i] == '0')
					{
						StageStorage[2] = StageStorage[2].Substring(0, StageStorage[2].Length - 1);
						i = 11;
					}
				}
			}
			StageStorage[3] = StageStorage[2].Select(a => SOULS[int.Parse(a.ToString())]).Join("");
			Debug.LogFormat("[Soulscream #{0}]: The displayed number for this stage is {1}.", _moduleId, StageStorage[0]);
			Debug.LogFormat("[Soulscream #{0}]: The scream for stage {1} is {2}.", _moduleId, Stage, StageStorage[3]);
			Answer += StageStorage[3];


		}
		else
        {
			if (Solves != 0)
			{
				//Break and enter submission mode, the souls can be put to rest//
				Debug.LogFormat("[Soulscream #{0}]: The final input soul is {1}.", _moduleId, Answer);
				final = true;
				Text[0].text = "" + (Answer.Length < 10 ? "".PadRight(Answer.Length, '-') : "----------");
				input = "-".PadRight(Answer.Length, '-');
				Text[1].text = "[" + (input.Length - 0).ToString().PadLeft(3, '0') + "]";
			}
            else
            {
				Debug.LogFormat("[Soulscream #{0}]: Unable to generate stages :pensive:.", _moduleId);
				Module.HandlePass();
			}
		}
	}
	IEnumerator Incinerate()
    {
		while (Time > 0 || Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Count() != 0)
		{
			if (counting)
			{
				Text[0].text = Time / 60 + ":" + (Time % 60 < 10 ? "0" + (Time % 60).ToString() : (Time % 60).ToString());
				yield return new WaitForSeconds(1f);
				Time--;
				if (Time <= 0)
				{
					Debug.LogFormat("[Soulscream #{0}]: Ring ring! Entering stage #{1}.", _moduleId, ++Stage);
					GenerateStage();
					Active = true;
					Audio.PlaySoundAtTransform("a", Screams[3].transform);
				}
				else if (Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Count() == 0)
                {
					Debug.LogFormat("[Soulscream #{0}]: Entering Finale...", _moduleId, ++Stage);
					GenerateStage();
					Active = true;
					counting = false;
					Time = 0;
					Audio.PlaySoundAtTransform("a", Screams[3].transform);
				}
			}
			yield return null;
		}
    }
	IEnumerator AddTime(int add, bool what)
    {
		Audio.PlaySoundAtTransform("download", Screams[3].transform);
		int i = 0;
		counting = false;
        while (i < add && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Count() != 0)
        {
			i++;
			Time++;
			Text[0].text = Time / 60 + ":" + (Time % 60 < 10 ? "0" + (Time % 60).ToString() : (Time % 60).ToString());
			yield return new WaitForSeconds(.02f);
        }
		if (what && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Count() != 0)
        {
			StartCoroutine(Incinerate());
			counting = true;
		}
        else if (Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Count() != 0)
        {
			counting = true;
        }
	}
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press AOOI (Presses the scream-quence AOOI)";
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
			for (int i = 0; i < stupid.Length; i++)
			{
				if (Array.IndexOf(SOULS.ToArray(), stupid[i]) <= 0)
				{
					Valid = false;
				}
			}
			if (!Valid)
			{
				yield return "sendtochaterror Invalid letter. Valid letters are A, I, E, O, U, X, and H.";
			}
		else
		{
			yield return null;
			yield return "solve";
			for (int i = 0; i < stupid.Length; i++)
			{
				Screams[Array.IndexOf(SOULS.ToArray(), stupid[i]) - 1].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
		}
		else
			yield return "sendtochaterror Incorrect Syntax. Use '!{1} press X'.";
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		while (!final) //Wait until submission time
			yield return true;
		for (int i = 0; i < Answer.Length; i++)
		{
			Screams[Array.IndexOf(SOULS.ToArray(), Answer[i]) - 1].OnInteract();
			yield return new WaitForSeconds(0.05f);
		}
	}
}
