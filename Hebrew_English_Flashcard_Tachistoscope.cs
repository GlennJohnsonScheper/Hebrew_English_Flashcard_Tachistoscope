/*
 * Hebrew_English_Flashcard_Tachistoscope.cs
 * in project Hebrew_English_Flashcard_Tachistoscope
 * This file has the whole brain work to do the job.
 * It was added to a Visual Studio 2019, new project, Windows Form Application.
 */
 
/*
 * Usage:
 * Execute Hebrew_English_Flashcard_Tachistoscope.exe on a Windows computer.
 * Hebrew, ..., and English text should appear just below the top of screen.
 * From time to time, it will change to a new randomly chosen Hebrew font.
 * To exit app, click visible text, or press any key when app has focus.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

/* this is practically a one-file program,
 * just need to add this into the WinForm:
 * 			MainFormFinishConstructor();
 */

namespace Hebrew_English_Flashcard_Tachistoscope
{
	public partial class Form1 : Form
	{

		void MainFormFinishConstructor()
		{
			this.TopMost = true;
			this.ControlBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Left = 0;
			this.Width = Screen.PrimaryScreen.WorkingArea.Width;
			this.Top = 0;
			this.Height = Screen.PrimaryScreen.WorkingArea.Height / 8;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = FormStartPosition.Manual;
			this.TransparencyKey = Color.Lime;
			// this.BackColor = Color.Lime;

			this.ShowInTaskbar = false;

			this.Load += new EventHandler(Form1_Load);
		}

		static Random rand = new Random();
		static TextBox tb = new TextBox();
		static Timer repaintTimer = new Timer();
		static float fontHeight = 30.0f;
		static string[] CurrentLineParts = null;
		static int CurrentState = 0;
		static Font HebrewFont = null;
		static Font EnglishFont = null;
		static Font TranslitFont = null;

		// Leftovers from Hebrew Font experiments:

		// Range 0x0BBX contains vowel points,
		// and CX has other punctuation marks,
		// then DX and EX contain body glyphs,
		// and 0x0bFX seems to have ligatures.
		const char loHebrew = '\u05B0';
		const char hiHebrew = '\u05FF';

		// You may also want to use these someday:
		const char markerLTR = '\u200E';
		const char markerRTL = '\u200F';

		// Dug out of \Prog ... Microsoft SDKs ... WinNT.h
		const int LANG_ENGLISH = 0x09;
		const int LANG_HEBREW = 0x0d;

		static Regex NotHebrew = new Regex("[^" + loHebrew + "-" + hiHebrew + "]");
		static Regex NotPrints = new Regex("[^ -~]");

		static string allHebrewGlyphs = "";
		static string bodyHebrewGlyphs = "";
		static string pointyHebrewGlyphs = "";

		// These Hebrew fonts were measured several years ago.
		// These are the only Hebrew font names the app knows.
		// It would be nice today to catalog new Hebrew fonts.

		internal sealed class SzNm
		{
			internal int sz { get; set; }
			internal string nm { get; set; }
		};

		static SzNm[] WellKnownFontSzNm = {
			new SzNm {sz = 1884, nm = "Dorian CLM"}, // 1884, 1622
			new SzNm {sz = 1747, nm = "hasida"}, // 1747, 1418
			new SzNm {sz = 1726, nm = "Shmulik CLM"}, // 1726, 1466
			new SzNm {sz = 1713, nm = "ktav"}, // 1713, 1377
			new SzNm {sz = 1594, nm = "Tahoma"}, // 1594, 1387
			new SzNm {sz = 1573, nm = "Miriam Mono CLM"}, // 1573, 1418
			new SzNm {sz = 1573, nm = "Miriam Fixed"}, // 1573, 1418
			new SzNm {sz = 1573, nm = "Courier New"}, // 1573, 1418
			new SzNm {sz = 1545, nm = "Shlomo"}, // 1545, 1368
			new SzNm {sz = 1545, nm = "Shlomo Stam"}, // 1545, 1368
			new SzNm {sz = 1545, nm = "Shlomo semiStam"}, // 1545, 1368
			new SzNm {sz = 1545, nm = "Shlomo LB"}, // 1545, 1368
			new SzNm {sz = 1545, nm = "Shlomo Bold"}, // 1545, 1368
			new SzNm {sz = 1545, nm = "Ezra SIL"}, // 1545, 1370
			new SzNm {sz = 1545, nm = "Ezra SIL SR"}, // 1545, 1368
			new SzNm {sz = 1537, nm = "Cardo"}, // 1537, 1378
			new SzNm {sz = 1535, nm = "Torah Sofer"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "SPTiberian"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "SPEzra"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "SPDamascus"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Shruti"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Sefer AH"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Sakkal Majalla"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Moses Judaika"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Moses Judaika Word"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Miri"}, // 1535, 1823
			new SzNm {sz = 1535, nm = "Meiryo"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Meiryo UI"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Lateef"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Lashon Tov"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Kohelet"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Hebrew"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Hebpar"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Charis SIL"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Bwtransh"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Bwheba"}, // 1535, 1360
			new SzNm {sz = 1535, nm = "Abyssinica SIL"}, // 1535, 1360
			new SzNm {sz = 1498, nm = "Lucida Sans Unicode"}, // 1498, 1305
			new SzNm {sz = 1498, nm = "HebrewUniversal"}, // 1498, 1327
			new SzNm {sz = 1497, nm = "Levenim MT"}, // 1497, 1314
			new SzNm {sz = 1475, nm = "Leelawadee"}, // 1475, 1307
			new SzNm {sz = 1462, nm = "Tanach"}, // 1462, 1295
			new SzNm {sz = 1461, nm = "Shebrew"}, // 1461, 1295
			new SzNm {sz = 1436, nm = "Reclame"}, // 1436, 1272
			new SzNm {sz = 1433, nm = "Simple CLM"}, // 1433, 1227
			new SzNm {sz = 1417, nm = "Onyx"}, // 1417, 1255
			new SzNm {sz = 1398, nm = "Yehuda CLM"}, // 1398, 1288
			new SzNm {sz = 1398, nm = "Web Hebrew Monospace"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Web Hebrew AD"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Vijaya"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Tzipporah"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "The Jewish Bitmap"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Sholom"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Shalom"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Sarcastic BRK"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Ruth Fancy"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Pecan_ Sonc_ Hebrew"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Nyala"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Noam New Hebrew"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Nehama"}, // 1398, 1025
			new SzNm {sz = 1398, nm = "MingLiU-ExtB"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "MingLiU_HKSCS-ExtB"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "MingLiU_HKSCS"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "MingLiU"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "MigdalHaemeq"}, // 1398, 1493
			new SzNm {sz = 1398, nm = "Jerusalem"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Jerusalem Shadow"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "ElroNet Proportional"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "ElroNet Monospace"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "David New Hebrew"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Bwhebl"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Bwhebb"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "BSTHebrew"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Alex"}, // 1398, 1239
			new SzNm {sz = 1398, nm = "Aegyptus"}, // 1398, 1239
			new SzNm {sz = 1397, nm = "Stam Ashkenaz CLM"}, // 1397, 1220
			new SzNm {sz = 1385, nm = "Stam Sefarad CLM"}, // 1385, 1173
			new SzNm {sz = 1385, nm = "Nachlieli CLM"}, // 1385, 1192
			new SzNm {sz = 1383, nm = "Alef"}, // 1383, 1203
			new SzNm {sz = 1372, nm = "Hadasim CLM"}, // 1372, 1164
			new SzNm {sz = 1364, nm = "Mekorot-Rashi"}, // 1364, 1205
			new SzNm {sz = 1360, nm = "Taamey David CLM"}, // 1360, 1207
			new SzNm {sz = 1359, nm = "Shofar"}, // 1359, 1156
			new SzNm {sz = 1354, nm = "Arial"}, // 1354, 1183
			new SzNm {sz = 1333, nm = "Nachlaot"}, // 1333, 986
			new SzNm {sz = 1315, nm = "David CLM"}, // 1315, 1167
			new SzNm {sz = 1309, nm = "Miriam CLM"}, // 1309, 1153
			new SzNm {sz = 1280, nm = "Shuneet Heavy"}, // 1280, 1116
			new SzNm {sz = 1277, nm = "Taamey Ashkenaz"}, // 1277, 1093
			new SzNm {sz = 1274, nm = "David"}, // 1274, 1127
			new SzNm {sz = 1273, nm = "Narkisim"}, // 1273, 1097
			new SzNm {sz = 1270, nm = "Mashkit"}, // 1270, 1085
			new SzNm {sz = 1264, nm = "Shuneet Square"}, // 1264, 1082
			new SzNm {sz = 1251, nm = "CJHebLtx"}, // 1251, 1115
			new SzNm {sz = 1251, nm = "CJHebLSm"}, // 1251, 1115
			new SzNm {sz = 1246, nm = "Keter YG"}, // 1246, 1066
			new SzNm {sz = 1244, nm = "Shuneet "}, // 1244, 1089
			new SzNm {sz = 1212, nm = "Taamey Frank CLM"}, // 1212, 1032
			new SzNm {sz = 1212, nm = "Frank CurledLamed"}, // 1212, 1032
			new SzNm {sz = 1205, nm = "Shuneet Demi"}, // 1205, 1055
			new SzNm {sz = 1199, nm = "Aharoni CLM"}, // 1199, 1033
			new SzNm {sz = 1197, nm = "Frank Ruehl CLM"}, // 1197, 1024
			new SzNm {sz = 1196, nm = "Times New Roman"}, // 1196, 1028
			new SzNm {sz = 1152, nm = "Mekorot-Vilna"}, // 1152, 972
			new SzNm {sz = 1139, nm = "FrankRuehl"}, // 1139, 991
			new SzNm {sz = 1136, nm = "Shuneet Oblique"}, // 1136, 996
			new SzNm {sz = 1104, nm = "Shuneet Book"}, // 1104, 969
			new SzNm {sz = 1101, nm = "Shuneet Medium"}, // 1101, 962
			new SzNm {sz = 1101, nm = "Shuneet Classic"}, // 1101, 962
			new SzNm {sz = 1096, nm = "Shuneet Thin"}, // 1096, 961
			new SzNm {sz = 1078, nm = "Shuneet Light"}, // 1078, 942
		};

		// I still need this to index numerically [0 up]:
		static List<FontFamily> availableHebrewFonts = new List<FontFamily>();

		// I need this to get the relative sizes -- index by name
		static Dictionary<string, int> availHebrewFontsNamesSizes = new Dictionary<string, int>();

		private void Form1_Load(object sender, EventArgs e)
		{

			// Heavy development herein:
			try
			{
				int RandomFactors = Environment.TickCount;
				RandomFactors <<= 3;
				RandomFactors ^= System.DateTime.Now.DayOfYear;
				RandomFactors <<= 3;
				RandomFactors ^= System.DateTime.Now.Hour;
				RandomFactors <<= 3;
				RandomFactors ^= System.DateTime.Now.Minute;
				RandomFactors <<= 3;
				RandomFactors ^= System.DateTime.Now.Millisecond;
				rand = new Random(RandomFactors);


				this.WindowState = FormWindowState.Normal;
				this.Controls.Add(tb);
				tb.BorderStyle = BorderStyle.None;
				tb.Top = 0;
				tb.Left = 0;
				tb.Height = ClientRectangle.Height;
				tb.Width = ClientRectangle.Width;
				tb.Multiline = true;
				tb.AcceptsReturn = false;
				tb.TextAlign = HorizontalAlignment.Center;
				tb.BackColor = Color.Lime;
				tb.ForeColor = Color.Black;
				HebrewFont = new Font(FontFamily.GenericSansSerif, fontHeight);
				TranslitFont = new Font(FontFamily.GenericMonospace, fontHeight);
				tb.Font = EnglishFont = new Font(FontFamily.GenericSerif, fontHeight);
				tb.Text = " " + "Hebrew_English_Flashcard_Tachistoscope";

				// Many Hebrew fonts were first installed.

				// More HEBREW FONT APPEARANCE research:
				// omitted...

				Graphics gx = this.CreateGraphics();

				foreach (FontFamily ff in FontFamily.Families)
				{
					bool isHebrew = false;
					int sizeRatio = 1000;

					foreach (SzNm sznm in WellKnownFontSzNm)
					{
						if (ff.Name == sznm.nm)
						{
							isHebrew = true;
							sizeRatio = sznm.sz; // ranges about 1000 - 2000
							break;
						}
					}
					if (isHebrew)
					{
						Font f = new Font(ff.Name, fontHeight);
						SizeF sz = gx.MeasureString(pointyHebrewGlyphs, f);
						SizeF sz2 = gx.MeasureString(bodyHebrewGlyphs, f);
						availableHebrewFonts.Add(ff);
						availHebrewFontsNamesSizes.Add(ff.Name, sizeRatio);
						// TMI:
						//    "Arial", // {Width=1354.899, Height=106.2419}
						//    "Times New Roman", // {Width=1196.045, Height=106.2419}
						//    "Web Hebrew AD", // {Width=1398.844, Height=106.2419}
						// Now with measured widths!
						// new SzNm {sz = 1, nm = "A"},
						// allFonts.Add("new SzNm {sz = " + (int)sz.Width + ", nm = \"" + ff.Name + "\"}, // " + (int)sz.Width+ ", " + (int)sz2.Width); // ready to re-import into code
					}
				}

				availableHebrewFonts.Sort((x, y) => { return string.Compare(x.Name, y.Name); });

				// User can see the randomly chosen font,
				// to abort and restart program if not desired.
				if (availableHebrewFonts.Count > 0)
				{
					// Choose an Hebrew capable font at random.
					// But scaled to approximately equal sizes.
					int n = rand.Next(availableHebrewFonts.Count); // get first, to access sizeRatio, some 1000 - 2000
					HebrewFont = new Font(availableHebrewFonts[n], fontHeight * 1500 / availHebrewFontsNamesSizes[availableHebrewFonts[n].Name]);
				}

				tb.Select(0, 0);
				tb.Click += new EventHandler(tb_Click);
				tb.KeyPress += new KeyPressEventHandler(tb_KeyPress);

				repaintTimer.Tick += new EventHandler(repaintTimer_Tick);
				repaintTimer.Interval = 1000;
				repaintTimer.Start();

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Form1_Load exception", MessageBoxButtons.OK);
				Application.Exit();
			}

		}

		void tb_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		void tb_KeyPress(object sender, EventArgs e)
		{
			Application.Exit();
		}

		void repaintTimer_Tick(object sender, EventArgs e)
		{
			repaintTimer.Stop();
			int msToShow = 1000; // default

			try
			{
				// Initialization did show program name during initialization.
				// Case 0 repaint state (0++) will show Hebrew fonts count for 1 sec.
				// Case 1 repaint state (1++) will show random font choice for 1 sec.
				// Case 2 repaint state (2++) will blank all, and initialize for next random item choice.
				// (I used to have two different sets of words harvested from the web:)
				// Two sets of states (100++) or (200++) will work the two list types.
				switch (CurrentState++)
				{
					case 0:
						tb.Font = EnglishFont;
						tb.Text = " " + "Found " + availableHebrewFonts.Count + " Hebrew fonts.";
						break;
					case 1:
						tb.Font = EnglishFont;
						tb.Text = " " + "Random Font Choice = (" + HebrewFont.Name + ")";
						msToShow = 2000;
						break;

					default:
					case 2:
						tb.Font = EnglishFont; // in case of any error
						tb.Text = " " + ""; // rest for 2 seconds
						msToShow = 2000;
						{
							// Parse a new random line from WordData.vocabulary list. E.g.,
							// "תורה	tora	Torah / theory / her turn	1722	",

							// This worked, all words were equally frequent:
							// int n = rand.Next(WordData.vocabulary.Length);

							// but I want the head words, say, 256 * as often as the tail words.
							// Using the crude base 2 logarithm, 2^8 = 256:
							// Theorizing... rand(1) -> m=0, so k=0
							// Theorizing... rand(2) -> m=0-1, so k=0-1
							// Theorizing... rand(4) -> m=0-3, so k=0-2
							// Theorizing... rand(256) -> m=0-256, so k=0-8
							int m = rand.Next(1024) >> 2; // discard 2 lsb
							int k = 0;
							while (m != 0)
							{
								m >>= 1;
								k++;
							}
							// In the range k = 0 - 8:
							// 8 is most probable(50%), 0 is rare.
							int choice = 8 - k; // 0 is probable, 8 is rare.
							int modulus = WordData.vocabulary.Length / 9; // will lose 0-8 words at end of list
							int n = rand.Next(modulus) + choice * modulus;
							if (n < 0 || n >= WordData.vocabulary.Length)
							{
								tb.Font = EnglishFont;
								tb.Text = " " + "Error in Random Choice[" + n + "].";
								CurrentState--; // to pick again
							}
							else
							{
								CurrentLineParts = WordData.vocabulary[n].Split('\t');
								if (CurrentLineParts.Length != 5)
								{
									tb.Font = EnglishFont;
									tb.Text = " " + "Error in WordData.vocabulary[" + n + "].";
									CurrentState--; // to pick again
								}
								else
								{
									// 2022 New vocabulary has some very long texts.
									// cut down CurrentLineParts[2] to say 50 chars.
									if (CurrentLineParts[2].Length > 50)
										CurrentLineParts[2] = CurrentLineParts[2].Substring(0, 50);

									// unused... CurrentState = 200; // to do Frequent words
									CurrentState = 100; // to do pointy words
								}
							}
						}
						break;


					case 100:
						// CurrentLineParts[3] = pointed hebrew
						tb.Font = HebrewFont;
						tb.Text = " " + CurrentLineParts[3];
						msToShow = 1000 + CurrentLineParts[3].Length * 600; // CurrentLineParts[3] = pointed hebrew
						break;

					case 200:
						// CurrentLineParts[0] = unpointed hebrew
						tb.Font = HebrewFont;
						tb.Text = " " + CurrentLineParts[0];
						msToShow = 1000 + CurrentLineParts[0].Length * 600; // CurrentLineParts[0] = unpointed hebrew
						break;

					case 101:
					case 201:
						// yes... tb.Font = EnglishFont;
						tb.Text = " " + ""; // short rest after pointed hebrew or frequency ordinal
						msToShow = 500; // short rest after pointed hebrew or frequency ordinal
						break;

					// Because of so many variant pronunciations on unpointed Hebrew,
					// I think the transliteration should be before unpointed Hebrew.

					case 102:
					case 202:
						// CurrentLineParts[1] = transliteration
						tb.Font = TranslitFont;
						tb.Text = " " + CurrentLineParts[1];
						msToShow = 1000 + CurrentLineParts[1].Length * 100; // CurrentLineParts[1] = transliteration
						break;

					case 103:
					case 203:
						// CurrentLineParts[0] = unpointed hebrew
						tb.Font = HebrewFont;
						tb.Text = " " + CurrentLineParts[0];
						msToShow = 1000 + CurrentLineParts[0].Length * 300; // CurrentLineParts[0] = unpointed hebrew
						break;

					case 104:
					case 204:
						// CurrentLineParts[2] = english
						tb.Font = EnglishFont;
						tb.Text = " " + CurrentLineParts[2];
						msToShow = 500 + CurrentLineParts[2].Length * 70; // CurrentLineParts[2] = english
						break;

					case 105:
					case 205:
						// CurrentLineParts[0] = unpointed hebrew
						tb.Font = HebrewFont;
						tb.Text = " " + CurrentLineParts[0];
						msToShow = 1000 + CurrentLineParts[0].Length * 300; // CurrentLineParts[0] = unpointed hebrew
						break;

					case 106:
					case 206:
						// CurrentLineParts[1] = transliteration
						tb.Font = TranslitFont;
						tb.Text = " " + CurrentLineParts[1];
						msToShow = 1000 + CurrentLineParts[1].Length * 100; // CurrentLineParts[1] = transliteration
						break;

					case 107:
					case 207:
						// CurrentLineParts[2] = english
						tb.Font = EnglishFont;
						tb.Text = " " + CurrentLineParts[2];
						msToShow = 500 + CurrentLineParts[2].Length * 70; // CurrentLineParts[2] = english
						break;

					case 108:
						// CurrentLineParts[3] = pointed hebrew -- reprise for less time
						tb.Font = HebrewFont;
						tb.Text = " " + CurrentLineParts[3];
						msToShow = 1000 + CurrentLineParts[3].Length * 400; // CurrentLineParts[3] = pointed hebrew -- reprise for less time
						break;

					case 208:
						// CurrentLineParts[0] = unpointed hebrew -- reprise for less time
						tb.Font = HebrewFont;
						tb.Text = " " + CurrentLineParts[0];
						msToShow = 1000 + CurrentLineParts[0].Length * 300; // CurrentLineParts[0] = unpointed hebrew
						break;

				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK);
			}

			repaintTimer.Interval = msToShow;
			repaintTimer.Start();
		}

	}
}
