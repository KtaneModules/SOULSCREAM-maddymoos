using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class Soulstrike : MonoBehaviour
{

	public KMAudio Audio;
	public KMBombModule Module;
	public KMBombInfo Bomb;
	public KMBossModule Boss;

	public TextMesh[] Text;
	public KMSelectable[] Screams;
	private string[] ignoredModules = { "Soulstrike", "Soulsong", "Soulscream", "OmegaForget", "14", " Brainf---", " Forget Enigma", " Forget Everything", " Forget It Not", " Forget Me Not", " Forget Me Later", " Forget Perspective", " Forget The Colors", " Forget Them All", " Forget This", " Forget Us Not", " Iconic", " Organization", " RPS Judging", " Simon Forgets", " Simon's Stages", " Souvenir", " Tallordered Keys", " The Twin", " The Very Annoying Button", " Ultimate Custom Night", "Übermodule" };
	static private int _moduleIdCounter = 1;
	private int _moduleId;

	private string Answer, input, Bases = "0000", HitType;
	private int[] GameStatStorage = new int[5];
	private int[] Scores = new int[2];
	private int Stage, Time, Solves, itsgonnabreakeverything;
	private float solvepoints, pps = 2f;
	private bool Active = true, counting, final, solved, pleasewait;

	private List<string> StageRecovery = new List<string>();

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
	void Start()
	{
		GenerateStage();
		StartCoroutine(Incinerate());
	}

	// Update is called once per frame
	void Update()
	{
		if (Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != Solves)
		{
			if (!Application.isEditor && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != 0)
			{
			}
			else
			{
				Solves++;
			}

		}
	}
	void HandlePress(KMSelectable btn)
	{
		int X = Array.IndexOf(Screams, btn);
		if (!pleasewait && final)
		{
			switch (X)
			{
				case 10: input += "-"; break;
				case 0: input += X.ToString(); break;
				case 1: input += X.ToString(); break;
				case 2: input += X.ToString(); break;
				case 3: input += X.ToString(); break;
				case 4: input += X.ToString(); break;
				case 5: input += X.ToString(); break;
				case 6: input += X.ToString(); break;
				case 7: input += X.ToString(); break;
				case 8: input += X.ToString(); break;
				case 9: input += X.ToString(); break;
			}
			if (input != Answer.Substring(0, input.Length))
			{
				Module.HandleStrike();
				input = input.Substring(0, input.Length - 1);
				StartCoroutine(RefreshStages());
			}
			else if (input == Answer)
			{
				Module.HandlePass();
			}
			Text[0].text = input;
		}
	}
	void GenerateStage()
	{
		if (Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Solves != 0)
		{
            Debug.LogFormat("[Soulstrike #{0}]: PLAY #{1}", _moduleId, Stage);
			Debug.LogFormat("[Soulstrike #{0}]: Here comes the pitch...", _moduleId, Stage);
            if (Rnd.Range(0, 2) == 0)
            {
                if (Rnd.Range(0, 3) == 0)
                {
                    Debug.LogFormat("[Soulstrike #{0}]: There's a ball.", _moduleId);
                    HitType = "BALL";
					GameStatStorage[1]++;
				}
                else
                {
                    Debug.LogFormat("[Soulstrike #{0}]: That one's a strike.", _moduleId);
                    HitType = "STRIKE";
					GameStatStorage[0]++;
                }
            }
            else
            {
                if (Rnd.Range(0, 3) == 0)
                {
                    if (Rnd.Range(0, 4) == 0)
                    {
                        Debug.LogFormat("[Soulstrike #{0}]: There's out number {1}.", _moduleId, ++GameStatStorage[3]);
                        HitType = "OUT";
						GameStatStorage[2]++;
					}
                    else
                    {
                        Debug.LogFormat("[Soulstrike #{0}]: That's a foul ball.", _moduleId);
                        HitType = "FOWL";
						if(GameStatStorage[0] != 2)
						GameStatStorage[0]++;
					}
                }
                else
                {
                    Debug.LogFormat("[Soulstrike #{0}]: And a hit!", _moduleId);
                    int temp = Rnd.Range(0, 25);
                    if (temp == 25)
                    {
                        Debug.LogFormat("[Soulstrike #{0}]: And that one's outta the park! HOME RUN!", _moduleId);
                        HitType = "HOME RUN";
						HitRun(4);
					}
                    else if (temp > 20)
                    {
                        Debug.LogFormat("[Soulstrike #{0}]: And that one's a triple, quite a good hit.", _moduleId);
                        HitType = "TRIPLE";
						HitRun(3);
					}
                    else if (temp > 12)
                    {
                        Debug.LogFormat("[Soulstrike #{0}]: And that one's a double, quite nice.", _moduleId);
                        HitType = "DOUBLE";
						HitRun(2);
					}
                    else
                    {
                        HitType = "SINGLE";
                        Debug.LogFormat("[Soulstrike #{0}]: And that one's a single.", _moduleId);
						HitRun(1);
                    }

                }
            }
            Text[0].text = HitType;
            StageRecovery.Add(HitType);
			if(GameStatStorage[0] == 3){
				GameStatStorage[0] = 0;
				GameStatStorage[2]++;
				Debug.LogFormat("[Soulstrike #{0}]: The batter has struck out.", _moduleId);
			}
			if(GameStatStorage[2] == 3)
            {
				GameStatStorage[2] = 0;
				GameStatStorage[4]++;
				GameStatStorage[4]%=2;
				Bases = "0000";
				Debug.LogFormat("[Soulstrike #{0}]: Three outs have been reached. This Inning is now an Outing.", _moduleId);
			}

		}
		else
		{
			if (Solves != 0)
			{
				Answer = Scores[0].ToString() + "-" + Scores[1].ToString();
				Debug.LogFormat("[Soulstrike #{0}]: The game has ended. The Score is {1}.", _moduleId, Answer);
				Text[1].text = "[???]";
				Text[0].text = "";
				final = true;

			}
			else
			{
				Debug.LogFormat("[Soulstrike #{0}]: Unable to generate stages :pensive:.", _moduleId);
				Module.HandlePass();
			}
		}
	}
	void HitRun(int a)
    {
		if (Bases[3] == '1')
		{
			Scores[GameStatStorage[4]]++;
			Debug.LogFormat("[Soulstrike #{0}]: A run has been scored!", _moduleId);
		}
		Bases = "1" + Bases.Substring(0, 3);
		for (int i=0; i < a - 1; i++)
        {
			if(Bases[3] == '1')
            {
				Scores[GameStatStorage[4]]++;
				Debug.LogFormat("[Soulstrike #{0}]: A run has been scored!", _moduleId);
			}
			Bases = "0" + Bases.Substring(0, 3);
		}
	}
    IEnumerator Incinerate()
    {
		while (true)
		{
			Text[1].text = "[" + Time + "]";
			yield return new WaitForSeconds(1f);
			Time--;
			if (Time <= 0 && Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() != 0)
			{
				GenerateStage();
				Stage++;
				Active = true;
				Audio.PlaySoundAtTransform("invertedv", Screams[3].transform);
				Time = 30;
			}
			else if (Bomb.GetSolvableModuleNames().Where(a => !ignoredModules.Contains(a)).Count() - Bomb.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count() == 0)
			{
				Debug.LogFormat("[Soulstrike #{0}]: THE GODS HAVE DECLARED THE GAME IS OVER.", _moduleId, ++Stage);
				GenerateStage();
				Active = true;
				counting = false;
				Time = 30;
				Audio.PlaySoundAtTransform("inverteda", Screams[3].transform);
			yield break;
			}
		}
	}

	IEnumerator RefreshStages()
	{
		pleasewait = true;
		for (int i = 0; i < StageRecovery.Count; i++)
		{
			Text[1].text = StageRecovery[i].ToString();
			yield return new WaitForSeconds(2);
		}
		pleasewait = false;
	}
/* #pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press AOOI (Presses the scream-quence AOOI)";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		
	}*/
}
