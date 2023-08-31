using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearningNlp
{
    class PorterStemmer
    {
        private char[] vowels = { 'a', 'e', 'i', 'o', 'u' };

        public string Stem(string word)
        {
            word = word.ToLower();
            if (word.Length <= 2)
            {
                return word;
            }

            if (vowels.Contains(word[0]))
            {
                return ProcessVowel(word);
            }
            else
            {
                return ProcessConsonant(word);
            }
        }

        private string ProcessVowel(string word)
        {
            if (word[word.Length - 1] == 's')
            {
                word = word.Substring(0, word.Length - 1);
            }
            return word;
        }

        private string ProcessConsonant(string word)
        {
            if (word[word.Length - 1] == 's')
            {
                word = word.Substring(0, word.Length - 1);
            }

            if (word.EndsWith("sses"))
            {
                word = word.Substring(0, word.Length - 2);
            }
            else if (word.EndsWith("ies"))
            {
                word = word.Substring(0, word.Length - 2) + "i";
            }
            else if (word.EndsWith("ss"))
            {
                // No change needed
            }
            else if (word.EndsWith("s"))
            {
                word = word.Substring(0, word.Length - 1);
            }

            if (CountConsonantGroups(word) > 0)
            {
                word = RemoveConsonantSuffix(word);
            }

            return word;
        }

        private int CountConsonantGroups(string word)
        {
            int count = 0;
            int i = 0;
            while (i < word.Length)
            {
                if (!vowels.Contains(word[i]))
                {
                    count++;
                    while (i < word.Length && !vowels.Contains(word[i]))
                    {
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }
            return count;
        }

        private string RemoveConsonantSuffix(string word)
        {
            if (word.EndsWith("eed"))
            {
                if (CountConsonantGroups(word.Substring(0, word.Length - 3)) > 0)
                {
                    return word.Substring(0, word.Length - 1);
                }
                else
                {
                    return word;
                }
            }
            else if (word.EndsWith("ed"))
            {
                if (ContainsVowel(word.Substring(0, word.Length - 2)))
                {
                    return ProcessConsonant(word.Substring(0, word.Length - 2));
                }
                else
                {
                    return word;
                }
            }
            else if (word.EndsWith("ing"))
            {
                if (ContainsVowel(word.Substring(0, word.Length - 3)))
                {
                    return ProcessConsonant(word.Substring(0, word.Length - 3));
                }
                else
                {
                    return word;
                }
            }
            return word;
        }

        private bool ContainsVowel(string word)
        {
            foreach (char vowel in vowels)
            {
                if (word.Contains(vowel))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
