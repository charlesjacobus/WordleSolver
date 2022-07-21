using System.Collections;

namespace WordleLibrary
{
    public class Game : IEnumerable<Word>
    {
        public const uint WordLimit = 6;

        private List<Word> _words;
        private Word _solution;

        private Game()
        {
            _words = new List<Word>();
            _solution = GetSolution();
        }

        public IEnumerator<Word> GetEnumerator()
        {
            return ((IEnumerable<Word>)_words).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_words).GetEnumerator();
        }

        public GuessResult? Guess(string? word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return null;
            }

            var created = Word.Create(word);
            if (created == null || !created.IsValid())
            {
                return null;
            }

            return Guess(created);
        }

        public bool IsComplete() => _words.Count >= WordLimit;

        public bool IsSolved() => _words.Any(w => w.IsSolved());

        public Word Solution => _solution;

        public IEnumerable<Word> Words => _words;

        public static Game Create()
        {
            return new Game();
        }

        protected virtual Word GetSolution()
        {
            var random = WordsLibrary.Instance.Random(WordsLibrary.WordleDictionary.Solutions);

            var word = Word.Create(random);

            return word;
        }

        protected virtual GuessResult Guess(Word word)
        {
            if (_words.Count < WordLimit)
            {
                word = Word.Score(word, _solution);
                if (word == null)
                {
                    return GuessResult.Create(null);
                }

                _words.Add(word);
            }

            return GuessResult.Create(word, IsComplete() ? _solution : null);
        }
    }

    public class GamePlayResults
    {
        private List<Game> _games;

        public GamePlayResults()
        {
            _games = new List<Game>();
        }

        public IEnumerable<Game> Games => _games;

        public double WinLoss
        {
            get
            {
                var solved = (double)_games.Where(g => g.IsSolved()).Count();
                var total = (double)_games.Count();
                
                return solved / total * 100d;
            }
        }

        public double WordsPerSolvedGameAverage
        {
            get
            {
                var solved = _games.Where(g => g.IsSolved());
                var words = !solved.Any() ? Game.WordLimit : solved.Average(g => g.Words.Count());

                return words;
            }
        }

        public void AddGame(Game game)
        {
            if (game != null)
            {
                _games.Add(game);
            }
        }

        public static GamePlayResults Create()
        {
            return new GamePlayResults();
        }
    }

    public class GuessResult
    {
        private GuessResult() { }

        public Word? Word { get; private set; }

        public Word? Solution { get; private set; }

        internal static GuessResult Create(Word? word, Word? solution = null)
        {
            return new GuessResult
            {
                Word = word,
                Solution = solution
            };
        }
    }
}
