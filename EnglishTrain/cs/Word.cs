using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishTrain.cs
{
    //Yahoo爬下來的資料中，一個英文單字有多種詞性，一個詞性有0~N種中文意思，一個中文意思有多個例句
    [Serializable]
    /// <summary>單字類別，含詞性、中文意思、權重等。</summary>
    class Word
    {
        /// <summary>單字權重起始值，數字越大越不熟，0=非常熟，完全不會出現在單字練習。</summary>
        public int weight = 3;
        /// <summary>英文單字</summary>
        public readonly string word;//該英文單字
        /// <summary>音標</summary>
        public readonly string phoneticSymbol;
        /// <summary>給詞性Key獲得中文解釋List</summary>
        public Dictionary<string, List<string>> chineseMeaning = new Dictionary<string, List<string>>();
        /// <summary>備註</summary>
        public string remark { get; set; } = string.Empty;
        public Word(string word, string phonetic)
        {
            this.word = word;
            this.phoneticSymbol = phonetic;
        }
        public override string ToString()
        {
            return word;
        }
    }
}
