using System.Reflection;

namespace WordleLibrary
{
    public sealed class WordsLibrary
    {
        private List<Word> _complete;
        private List<Word> _solutions;

        private LetterOccurrences _letterOccurrences;

        private static WordsLibrary? _instance;
        private Random _random;

        public enum WordleDictionary
        {
            Complete,
            Solutions
        }

        private WordsLibrary()
        {
            _random = new Random();

            _letterOccurrences = LetterOccurrences.Create();

            _complete = LoadEmbeddedResource(WordleDictionary.Complete);
            _solutions = LoadEmbeddedResource(WordleDictionary.Solutions);
        }

        public IEnumerable<Word> Complete => _complete;

        public LetterOccurrences LetterOccurrences => _letterOccurrences;

        public IEnumerable<Word> Solutions => _solutions;

        public bool Exists(Word word)
        {
            return word != null
                &&
                _complete.Select(w => w.ToString()).Contains(word.ToString());
        }

        public string Random(WordleDictionary identifier)
        {
            var max = identifier == WordleDictionary.Solutions ? _solutions.Count() : _complete.Count();
            var random = _random.Next(0, max - 1);

            return identifier == WordleDictionary.Solutions ? _solutions.ElementAt(random).ToString() : _complete.ElementAt(random).ToString();
        }

        public static WordsLibrary Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WordsLibrary();
                }

                return _instance;
            }
        }

        private List<Word> LoadEmbeddedResource(WordleDictionary identifier)
        {
            var words = new List<Word>();

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"WordleLibrary.Resources.Words.{identifier}.txt");
            if (stream != null)
            {
                var reader = new StreamReader(stream);
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var w = Word.Create(line, validate:false);
                        if (w != null)
                        {
                            words.Add(w);

                            w.Letters.ToList().ForEach(l => {
                                _letterOccurrences.AddOne(l);
                            });
                        }
                    }
                }
            }

            return words;
        }
    }
}
