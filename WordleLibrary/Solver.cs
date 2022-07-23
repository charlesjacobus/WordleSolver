namespace WordleLibrary
{
    public class Solver
    {
        private IEnumerable<Word> _dictionary;
        private WordsLibrary.WordleDictionary _source;

        public Solver(WordsLibrary.WordleDictionary source)
        {
            _source = source;

            var words = _source == WordsLibrary.WordleDictionary.Solutions 
                ? WordsLibrary.Instance.Solutions 
                : WordsLibrary.Instance.Complete;
            
            _dictionary = InitializeDictionary(words);
        }

        public Word? SolveOne(Game game)
        {
            if (game?.Words == null || !game.Words.Any())
            {
                return null;
            }

            if (game.IsSolved())
            {
                return game.Words.Last();
            }

            // It's pretty important that these filters be applied in order
            var matches = Enumerable.Empty<Word>();
            matches = Filter(game, matches, FilterWordsWithLettersInCorrectPosition);
            matches = Filter(game, matches, FilterWordsWithoutAnyMatchingLettersInDifferentPositions);
            matches = Filter(game, matches, FilterWordsWithAllMatchingLetters);
            matches = Filter(game, matches, FilterNotInWordLetters);
            matches = Filter(game, matches, FilterPreviouslyPlayedWords);
            matches = Filter(game, matches, FilterOneRemainingLetterInWord);

            return !matches.Any() ? 
                Word.Create(WordsLibrary.Instance.Random(_source)) : 
                matches.First();
        }

        protected virtual IEnumerable<Word> FilterOneRemainingLetterInWord(Game game, IEnumerable<Word> words)
        {
            var previous = game.Words.LastOrDefault();
            if (previous == null || previous.Letters.Count(l => l.Correctness == Letter.CorrectnessLevel.InWordCorrectPosition) != Word.LetterLimit - 1)
            {
                return words;
            }

            var remaining = previous.Letters.First(l => l.Correctness != Letter.CorrectnessLevel.InWordCorrectPosition);

            var wordCounts = new Dictionary<Word, uint>();
            foreach (var word in words)
            {
                var match = word.Letters[remaining.Position - 1];
                if (remaining.Value != match.Value)
                {
                    var occurrences = WordsLibrary.Instance.LetterOccurrences.First(o => o.Letter == match.Value).Count;

                    wordCounts.Add(word, occurrences);
                }
            }

            var matches = wordCounts.OrderByDescending(w => w.Value).Select(w => w.Key);

            return matches;
        }

        protected virtual IEnumerable<Word> FilterPreviouslyPlayedWords(Game game, IEnumerable<Word> words)
        {
            var matches = words.Where(m => !game.Words.Select(w => w.ToString()).Contains(m.ToString(), StringComparer.OrdinalIgnoreCase)).ToList();

            return matches;
        }

        protected virtual IEnumerable<Word> FilterNotInWordLetters(Game game, IEnumerable<Word> words)
        {
            // Filter words with letters previously played and marked as "not in word"

            var misplaced = game.Words
                .SelectMany(w => w.Letters)
                .Where(l => l.Correctness == Letter.CorrectnessLevel.NotInWord)
                .Distinct();

            var matches = new List<Word>();

            foreach (var word in words)
            {
                if (!word.HasAnyLetter(misplaced))
                {
                    matches.Add(word);
                }
            }

            return matches;
        }

        protected virtual IEnumerable<Word> FilterWordsWithAllMatchingLetters(Game game, IEnumerable<Word> words)
        {
            // Filter words that don't have all matching letters, whether in the correct position or not

            var letters = game.Words
                .SelectMany(w => w.Letters)
                .Where(l => l.Correctness == Letter.CorrectnessLevel.InWordCorrectPosition || l.Correctness == Letter.CorrectnessLevel.InWordDifferentPosition)
                .Distinct();

            var matches = new List<Word>();

            foreach (var word in words)
            {
                if (word.HasAllLetters(letters))
                {
                    matches.Add(word);
                }
            }

            return matches;
        }

        protected virtual IEnumerable<Word> FilterWordsWithLettersInCorrectPosition(Game game, IEnumerable<Word> words)
        {
            // Filter words with letters previously played and marked as "in word correct position"

            var correct = game.Words
                .SelectMany(w => w.Letters)
                .Where(l => l.Correctness == Letter.CorrectnessLevel.InWordCorrectPosition);

            var matches = new List<Word>();
            foreach (var word in _dictionary)
            {
                var match = true;
                foreach (var c in correct)
                {
                    if (!word.MatchesOn(c.Value, c.Position))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    matches.Add(word);
                }
            }

            return matches;
        }

        protected virtual IEnumerable<Word> FilterWordsWithoutAnyMatchingLettersInDifferentPositions(Game game, IEnumerable<Word> words)
        {
            // Filter words with letters previously played and marked as "in word different position" where the word includes one or more of those misplaced letters in the same position

            var misplaced = game.Words
                .SelectMany(w => w.Letters)
                .Where(l => l.Correctness == Letter.CorrectnessLevel.InWordDifferentPosition)
                .Distinct();

            var matches = new List<Word>();

            foreach (var word in words)
            {
                if (!word.HasAtLeastOneLetterInSamePosition(misplaced))
                {
                    matches.Add(word);
                }
            }

            return matches;
        }

        protected virtual IEnumerable<Word> InitializeDictionary(IEnumerable<Word> words)
        {
            if (words == null)
            {
                return Enumerable.Empty<Word>();
            }

            var dictionary = new Dictionary<Word, uint>();
            foreach (var word in words)
            {
                uint rank = 0;
                word.Letters.DistinctBy(l => l.Value).ToList().ForEach(l => {
                    var occurrences = WordsLibrary.Instance.LetterOccurrences.FirstOrDefault(l => l.Letter == l.Letter);
                    rank += occurrences != null ? occurrences.Count : 0;
                });

                dictionary.Add(word, rank);
            }

            var sorted = dictionary.OrderByDescending(w => w.Value).Select(w => w.Key);

            return sorted;
        }

        public static Solver Create(WordsLibrary.WordleDictionary dictionary = WordsLibrary.WordleDictionary.Complete)
        {
            return new Solver(dictionary);
        }

        private static IEnumerable<Word> Filter(Game game, IEnumerable<Word> words, Func<Game, IEnumerable<Word>, IEnumerable<Word>> filter)
        {
            if (game?.Words == null || words == null)
            {
                return Enumerable.Empty<Word>();
            }

            if (words.Count().Equals(1))
            {
                return words;
            }

            var matches = filter(game, words);

            return matches;
        }
    }
}
