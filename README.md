# tashi_script_editor
C# .net core GUI for translating game scripts.
Currently supports Maten no Soumetsu.

# current features
* reads text script file (json metadata, #-prefixed comments, and translated strings
* edit comments / translated string
* save file using same format
* keyboard shortcuts to insert control codes
* filter string list by searching comments, Japanese string, and English string
* highlight translated strings (sorta buggy)

# future features
* add/delete strings

# script format
script is expected to be formatted:
  * one line with JSON object with string metadata
  * one or more comment lines prefixed with # (original text, comments)
  * one or more translated lines with no prefix

# translating workflow
1. open properly formatted script file
1. select untranslated string from "Select String" list
1. type translation in English textbox, using ctrl+# key to insert control codes
1. use ctrl+enter to advance to the next string
Notes:
  * changes are stored in memory as you type, but only written to the file when you click the "Save File" button
  * ctrl+pgup and pgdown can also be used to navigate strings (shortcuts only work when the English textbox has focus)

# screenshot
* teal background means the string has been translated
* search for Japanese or English text using the "String Search" input box
* Progress tracks how many strings have been translated against total string count

![tashi_script_editor](https://user-images.githubusercontent.com/44418517/102733932-2e42ec00-4304-11eb-9803-43a134b775b6.png)
