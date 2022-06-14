N.B. This one-file command line application file is NOT part of the GUI application project.

/*
 * MySQL_review_lexicon_hebrew.cs
 * 
 * Step 1.
 * Peruse the lexicon_hebrew.sql.gz I just downloaded from https://github.com/bibleforge/BibleForgeDB/
 * Which data is in the public domain:
 * 
 * Quoting their licence file found there:
 * "The texts of the King James Bible, the original Greek and Hebrew, the Greek and Hebrew Lexicons,
 * and all other related data, such as the Strong's numbers and grammatical information, are in the public domain."
 * 
 * Download at full URL: https://github.com/bibleforge/BibleForgeDB/tarball/master
 * Open, Go 2 or 3 compression/folder levels down, extract file:
 * lexicon_hebrew.sql.gz
 * 
 * Unzip, Import to new MySQL database bibleforgedb using MySQL workbench. So, now it appears thus:
 * SELECT * FROM bibleforgedb.lexicon_hebrew;
 * 9289 table rows. No nullable fields:
 * id
 * 	int(10)unsigned
 * strongs
 * 	mediumint(8)unsigned
 * base_word
 * 	varchar(100)
 * data
 * 	text
 * usage
 * 	text
 * part_of_speech
 * 	varchar(20)
 * See the exploratory routines below.
 * 
 * 2. prepare public domain data for a new sharable version of Hebrew Flashcards.
 * 
 * My original hebrew study program has a table of data like:
 * // Any points in the 0x5b0-cf range follow AFTER the body glyph in the 0x5d0-ef range.
 *  string[] PointyWords = {
 *  	"אב	av	father	אָב	",
 *  	"אבד	a-vad	perish	אבד	",
 *  	"אבה	a-vah	consent	אבה	",
 *  Note the de-pointed Hebrew in col[0], then pronunciation, meaning, and possibly-pointed Hebrew in col[3], tab separated,final tab.
 * 
 */

// New console application, using Microsoft Visual Studio Community 2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

// VS: Tool, NuGet package Manager, Browse, MySql.Data, install:

using MySql.Data.MySqlClient;


namespace MySQL_review_lexicon_hebrew
{
	class MySQL_review_lexicon_hebrew
	{
		//=============== TO DO: OBLITERATE THESE LOGIN CREDENTIALS BEFORE SHARING ==================
		static string connStr = "server=localhost; database=bibleforgedb; user=XXXX; password=XXXX";

		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			try
			{
				doit();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception in doit: " + ex.ToString());
				Console.Error.WriteLine("Exception in doit: " + ex.ToString());
			}
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}

		static void ex_doit_1()
		{
			using (MySqlConnection sqlConnDoIt = new MySqlConnection(connStr))
			{
				try
				{
					sqlConnDoIt.Open();
				}
				catch (MySqlException ex)
				{
					Console.WriteLine("Exception in sqlConn.Open: " + ex.ToString());
					Console.Error.WriteLine("Exception in sqlConn.Open: " + ex.ToString());
					return;
				}

				if (sqlConnDoIt.State != ConnectionState.Open)
				{
					Console.WriteLine("Error in sqlConn.Open: sqlConn.State != ConnectionState.Open");
					Console.Error.WriteLine("Error in sqlConn.Open: sqlConn.State != ConnectionState.Open");
					return;
				}

				Console.WriteLine("Happy! I managed to get the ConnectionState.Open state");

				// You got me. Whatcha gonna do now?

				// Let's assess the whole glyph range in all base_word.

				// The Unicode Hebrew block extends from U + 0590 to U+05FF and from U+FB1D to U+FB4F.
				// The low block contains uncomposed forms:
				// https://en.wikipedia.org/wiki/Unicode_and_HTML_for_the_Hebrew_alphabet
				// The high block contains ligatures, composed forms:
				// https://en.wikipedia.org/wiki/Alphabetic_Presentation_Forms
				// Pointing:
				// https://en.wikipedia.org/wiki/Niqqud
				// -- which also shows a table of transliterations for composed glyphs

				Dictionary<int, int> glyphrange = new Dictionary<int, int>();

				using (MySqlCommand command = new MySqlCommand(
					"SELECT base_word FROM lexicon_hebrew;",
					sqlConnDoIt))
				{
					try
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								if (!(reader["base_word"] is System.DBNull))
								{
									string base_word = (string)reader["base_word"];
									foreach (char c in base_word)
									{
										int i = (int)c;
										if (glyphrange.ContainsKey(i))
											glyphrange[i]++;
										else
											glyphrange.Add(i, 1);
									}
								}
							}
						}
					}
					catch (MySqlException ex)
					{
						Console.WriteLine("Error ExecuteReader " + ex.ToString());
						Console.Error.WriteLine("Error ExecuteReader " + ex.ToString());
						return;
					}
				}

				List<string> ls = new List<string>();
				foreach (KeyValuePair<int, int> kvp in glyphrange)
				{
					string s = kvp.Key.ToString("X").PadLeft(6) + ": " + kvp.Value.ToString();
					ls.Add(s);
				}
				ls.Sort();
				File.WriteAllLines(@"C:\a\ao.txt", ls);

				/* Enquiring minds want to know:
					=== ascii spaces ===
					 20: 228
					=== here start the pointing... ===
					5B0: 3395
					5B1: 61
					5B2: 737
					5B3: 26
					5B4: 2581
					5B5: 1261
					5B6: 1732
					5B7: 4162
					5B8: 5849
					5B9: 1642
					5BA: 1
					5BB: 239
					5BC: 4461
					5BD: 2
					5BE: 48
					5C1: 1604
					5C2: 311
					5C7: 110
					=== here start the letters... ===
					5D0: 1803
					5D1: 1810
					5D2: 745
					5D3: 1256
					5D4: 2347
					5D5: 2171
					5D6: 584
					5D7: 1603
					5D8: 449
					5D9: 2821
					5DA: 189
					5DB: 769
					5DC: 2101
					5DD: 571
					5DE: 1991
					5DF: 839
					5E0: 1236
					5E1: 654
					5E2: 1664
					5E3: 225
					5E4: 935
					5E5: 147
					5E6: 685
					5E7: 999
					5E8: 2990
					5E9: 1916
					5EA: 1481
				 * Good: No RTL or LTR marks, neither puncts nor any ligatures in U+05XX nor anything at all in U+FXXX.
				 */
			}
		}

		static void ex_doit_2()
		{
			using (MySqlConnection sqlConnDoIt = new MySqlConnection(connStr))
			{
				try
				{
					sqlConnDoIt.Open();
				}
				catch (MySqlException ex)
				{
					Console.WriteLine("Exception in sqlConn.Open: " + ex.ToString());
					Console.Error.WriteLine("Exception in sqlConn.Open: " + ex.ToString());
					return;
				}

				if (sqlConnDoIt.State != ConnectionState.Open)
				{
					Console.WriteLine("Error in sqlConn.Open: sqlConn.State != ConnectionState.Open");
					Console.Error.WriteLine("Error in sqlConn.Open: sqlConn.State != ConnectionState.Open");
					return;
				}

				Console.WriteLine("Happy! I managed to get the ConnectionState.Open state");

				// What about the data field, which looks like JSON?
				// E.g., a short entry #2:
				// {"def":{"short":"father","long":["father"]},"deriv":"corresponding to H0001","pronun":{"ipa":"ʔɑb","ipa_mod":"ʔɑv","sbl":"ʾab","dic":"ab","dic_mod":"av"},"aramaic":1}

				List<string> ls = new List<string>();
				Dictionary<string, int> jsonPropertyNames = new Dictionary<string, int>();
				int[] jsonTokenTypes = new int[12]; // JsonTokenType enum runs from 0 to 11.

				using (MySqlCommand command = new MySqlCommand(
					"SELECT data FROM lexicon_hebrew;",
					sqlConnDoIt))
				{
					try
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								if (!(reader["data"] is System.DBNull))
								{
									string data = (string)reader["data"];
									// ls.Add(data); // says line in comment above
									byte[] bytes = Encoding.UTF8.GetBytes(data);
									var jsonReader = new Utf8JsonReader(bytes);
									while (jsonReader.Read())
									{
										// Console.WriteLine(jsonReader.TokenType); // says string, etc.
										jsonTokenTypes[(int)jsonReader.TokenType]++; // 0..11 or throws
										switch (jsonReader.TokenType)
										{
											case JsonTokenType.PropertyName:
												string propName = jsonReader.GetString();
												if (jsonPropertyNames.ContainsKey(propName))
													jsonPropertyNames[propName]++;
												else
													jsonPropertyNames.Add(propName, 1);
												break;
										}
									}
								}
							}
						}
					}
					catch (MySqlException ex)
					{
						Console.WriteLine("Error ExecuteReader " + ex.ToString());
						Console.Error.WriteLine("Error ExecuteReader " + ex.ToString());
						return;
					}
				}

				{
					string s = "";
					string prefix = "[";
					for (int i = 0; i < 12; i++)
					{
						s += prefix + jsonTokenTypes[i].ToString();
						prefix = ",";
					}
					s += "]";
					ls.Add(s);
				}
				foreach (KeyValuePair<string, int> kvp in jsonPropertyNames)
				{
					string s = kvp.Key + ": " + kvp.Value.ToString();
					ls.Add(s);
				}
				ls.Sort();
				File.WriteAllLines(@"C:\a\ao.txt", ls);

				/* Enquiring minds want to know:
				[0,27867,27867,14780,14780,96891,0,93183,692,0,0,0] = no nulls, no bools, no nones, no comments.
				aramaic: 692
				comment: 269
				def: 9289
				deriv: 9289
				dic_mod: 9289
				dic: 9289
				ipa_mod: 9289
				ipa: 9289
				lit: 2745
				long: 9289
				pronun: 9289
				sbl: 9289
				see: 295
				short: 9289

				So I will take each string following a short for definition, and either following sbl or dic for pronunciation
				 */
			}
		}

		static void doit()
		{
			// Finally, explorations are finished.
			// Now create the desired output data.
			List<string> ls = new List<string>();
			using (MySqlConnection sqlConnDoIt = new MySqlConnection(connStr))
			{
				try
				{
					sqlConnDoIt.Open();
				}
				catch (MySqlException ex)
				{
					Console.WriteLine("Exception in sqlConn.Open: " + ex.ToString());
					Console.Error.WriteLine("Exception in sqlConn.Open: " + ex.ToString());
					return;
				}

				if (sqlConnDoIt.State != ConnectionState.Open)
				{
					Console.WriteLine("Error in sqlConn.Open: sqlConn.State != ConnectionState.Open");
					Console.Error.WriteLine("Error in sqlConn.Open: sqlConn.State != ConnectionState.Open");
					return;
				}

				Console.WriteLine("Happy! I managed to get the ConnectionState.Open state");

				using (MySqlCommand command = new MySqlCommand(
					"SELECT base_word, data FROM lexicon_hebrew;",
					sqlConnDoIt))
				{
					try
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								string definition = "";
								string pronunciation = "";
								string pointedHebrew = "";
								string unpointHebrew = "";

								if (!(reader["base_word"] is System.DBNull))
								{
									pointedHebrew = (string)reader["base_word"];
									foreach(char c in pointedHebrew)
                                    {
										if((int)c >= 0x5d0)
                                        {
											unpointHebrew += c; // I coulda hada stringbuilder
										}
									}
								}
                                if (!(reader["data"] is System.DBNull))
								{
									string data = (string)reader["data"];
									byte[] bytes = Encoding.UTF8.GetBytes(data);
									var jsonReader = new Utf8JsonReader(bytes);

									bool armedDefinition = false;
									bool armedPronunciation = false;

									while (jsonReader.Read())
									{
										switch (jsonReader.TokenType)
										{
											case JsonTokenType.String:
												if (armedDefinition)
												{
													definition = jsonReader.GetString();
												}
												if (armedPronunciation)
												{
													pronunciation = jsonReader.GetString();
												}
												armedDefinition = false;
												armedPronunciation = false;
												break;
											case JsonTokenType.PropertyName:
												string propName = jsonReader.GetString();
												switch (propName)
                                                {
													case "short":
														armedDefinition = true;
														break;
													case "dic":
														armedPronunciation = true;
														break;
													default:
														armedDefinition = false;
														armedPronunciation = false;
														break;
												}
												break;
											default:
												armedDefinition = false;
												armedPronunciation = false;
												break;
										}
									}
								}

								// Generate the C code strings for my Hebrew Flashcard app revision:
								//  *  Note the de-pointed Hebrew in col[0], then pronunciation, meaning, and possibly-pointed Hebrew in col[3], tab separated,final tab.
								// Wait -- original data was from a word frequency list, app favors frequent words.
								// Therefore, prefix either a hebrew word length or the whole line length, to sort.

								string line = "\"" +
									unpointHebrew + "\t" +
									pronunciation + "\t" +
									definition + "\t" +
									pointedHebrew + "\t\",";
								string toSort = line.Length.ToString().PadLeft(10) + ":";
								ls.Add(toSort + line);
							}
						}
					}
					catch (MySqlException ex)
					{
						Console.WriteLine("Error ExecuteReader " + ex.ToString());
						Console.Error.WriteLine("Error ExecuteReader " + ex.ToString());
						return;
					}
				}
				ls.Sort();
				File.WriteAllLines(@"C:\a\ao.txt", ls); // do a Notepad++ column edit to remove prefixes.

			}
		}

	}
}