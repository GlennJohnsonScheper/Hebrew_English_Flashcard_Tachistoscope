# Hebrew_English_Flashcard_Tachistoscope

Hebrew_English_Flashcard_Tachistoscope is a Windows application that I like to add to my list of Windows startup processes, so that it always runs all day long while I work.

It displays 9289 biblical Hebrew words, alternating among unpointed Hebrew, pointed Hebrew, pronunciations/transliterations, and their short definitions, for all-day long memorization.

The small black text is on a transparent background to display over other apps.

Clicking on the small text (but not on the clear background) will exit the app.

Easier, if the app has focus, that is, if a cursor appears left of the phrase, as in shown in the screenshot.png here, then any keystroke will exit the app.

Download a zip of the .exe at:
http://johnsonscheper.com/Hebrew_English_Flashcard_Tachistoscope.zip

Please install many Hebrew fonts to have a greater variety of random font choices.
Sorry - The list of Hebrew font names this application knows is several years old.

The code demonstrates making a single-instance program.

The code is all handwritten C#, which are added to an almost virgin Visual Studio Windows Form app project, which entire VS project is archived in this project.

The Hebrew English data derived from the following Hebrew lexicon is in the public domain:

It was downloaded at full URL: https://github.com/bibleforge/BibleForgeDB/tarball/master
Quoting their licence file found there:
"The texts of the King James Bible, the original Greek and Hebrew, the Greek and Hebrew Lexicons, and all other related data, such as the Strong's numbers and grammatical information, are in the public domain."

A file "Hebrew Lexicon data derivation Helper App - MySQL_review_lexicon_hebrew.cs"
is not part of the app, but is itself a separate one-file command line application.
