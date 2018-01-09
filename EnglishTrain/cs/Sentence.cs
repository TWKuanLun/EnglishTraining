using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishTrain.cs
{
    [Serializable]
    class Sentence
    {
        public Sentence(string chi, string eng, string w, string p, int i)
        {
            Chi = chi;
            Eng = eng;
            WordKey = w;
            PartOfSpeech = p;
            ChiMeaningIndex = i;
        }
        public readonly string Chi;
        public readonly string Eng;
        public readonly string WordKey;//屬於哪個單字的例句
        public readonly string PartOfSpeech;//屬於該單字在哪個詞性的例句
        public readonly int ChiMeaningIndex;//該單字在該詞性時屬於哪個中文意思的例句
    }
}
