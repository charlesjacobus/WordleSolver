using System.Text;

namespace WordleLibrary
{
    public class Letter
    {
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public enum CorrectnessLevel
        {
            NotScored,
            NotInWord,
            InWordDifferentPosition,
            InWordCorrectPosition
        }

        private Letter() { }

        public uint Position { get; private set; }

        public char Value { get; private set; }

        public CorrectnessLevel Correctness { get; internal set; }

        internal static Letter Create(char value, uint position, CorrectnessLevel correctness)
        {
            return new Letter
            {
                Value = value,
                Position = position,
                Correctness = correctness
            };
        }

        public static bool IsValid(char value) => Alphabet.IndexOf(value.ToString().ToUpper()) != -1;
    }

    public class LetterPositionOccurrence
    {
        private LetterPositionOccurrence() { }

        public char Letter { get; protected set; }

        public uint Count { get; protected set; }

        public uint Position { get; private set; }

        public virtual void AddOne(uint position)
        {
            Count++;

            Position = position;
        }

        public static LetterPositionOccurrence Create(char letter, uint position)
        {
            return new LetterPositionOccurrence
            {
                Count = 0,
                Letter = letter,
                Position = position
            };
        }
    }

    public class LetterPositionOccurrences
        : List<LetterPositionOccurrence>
    {
        private LetterPositionOccurrences()
        {
            Letter.Alphabet.ToCharArray().ToList().ForEach(c => {
                for (uint i = 1; i <= Word.LetterLimit; i++)
                {
                    Add(LetterPositionOccurrence.Create(c, i));
                }
            });
        }

        public void AddOne(Letter letter)
        {
            if (letter?.Value == null)
            {
                return;
            }

            var l = this.FirstOrDefault(o => o.Letter == letter.Value && o.Position == letter.Position);
            if (l != null)
            {
                l.AddOne(letter.Position);
            }
        }

        public string ToCsv()
        {
            var builder = new StringBuilder("Letter\t1\t2\t3\t4\t5\tTotal\n");
            if (!this.Any())
            {
                return builder.ToString();
            }

            var letters = Letter.Alphabet.ToCharArray();
            letters
                .ToList()
                .ForEach(a =>
                {
                    var positions = this.Where(l => l.Letter == a);

                    builder.Append($"{a}\t");
                    for (int i = 1; i <= Word.LetterLimit; i++)
                    {
                        var count = positions.FirstOrDefault(p => p.Letter == a && p.Position == i)?.Count ?? 0;

                        builder.Append($"{count:N0}\t");
                    }
                    builder.Append($"{positions.Sum(p => p.Count):N0}\n");
                });

            return builder.ToString();
        }

        public static LetterPositionOccurrences Create()
        {
            return new LetterPositionOccurrences();
        }
    }
}