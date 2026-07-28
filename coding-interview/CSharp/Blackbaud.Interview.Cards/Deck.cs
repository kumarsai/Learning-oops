namespace Blackbaud.Interview.Cards;

/// <summary>
/// A deck of cards
/// </summary>
public class Deck
{
    private readonly Stack<Card> _stackOfCards;

    /// <summary>
    /// Private constructor for a new deck of <paramref name="cards"/>.
    /// Use Deck.NewDeck() static factory method.
    /// </summary>
    /// <param name="cards"></param>
    private Deck(IEnumerable<Card> cards)
    {
        _stackOfCards = new Stack<Card>(cards);
    }

    /// <summary>
    /// Creates and returns a new deck of cards.
    /// </summary>
    /// <returns></returns>
    public static Deck NewDeck()
    {
        return new Deck(
            Enum.GetValues<Suit>().SelectMany(suit =>
                Enum.GetValues<Rank>().Select(rank =>
                    new Card(rank, suit))
        ));
    }

    /// <summary>
    /// The number of remaining cards in the deck
    /// </summary>
    public int RemainingCards => _stackOfCards.Count;

    /// <summary>
    /// Returns true if there are no remaining cards in the deck
    /// </summary>
    public bool Empty => RemainingCards == 0;

    /// <summary>
    /// Removes the next card from the deck.
    /// </summary>
    /// <returns>The next card from the deck.
    /// Returns null if no cards remain.</returns>
    public Card NextCard()
    {
        if (!Empty)
        {
            var nextCard = _stackOfCards.Pop();
            return nextCard;
        }
        else
        {
            return null;
        }
    }

    public void Shuffle(int shuffleCount)
    {
        for (int count = 0; count < shuffleCount; count++)
        {
            //var cards = _stackOfCards.ToList();
            //var random = new Random();
            //var shuffledCards = cards.OrderBy(x => random.Next()).ToList();
            //_stackOfCards.Clear();
            //foreach (var card in shuffledCards)
            //{
            //    _stackOfCards.Push(card);
            //}
            var random = new Random();
            var cards = _stackOfCards.ToList();
            for (int i = 0; i < RemainingCards; i++)
            {
                var j = random.Next(RemainingCards);
                var temp = cards[j];
                cards[j] = cards[i];
                cards[i] = temp;
            }


            shuffleCount--;

            //Console.WriteLine($"Shuffled attempt {shuffleCount}");

        }
    }

    public Dictionary<int,List<Card>> DealCards(int noOfPlayers, int noCardsPerPlayer)
    {
        var playerHands = new Dictionary<int, List<Card>>();

        for (int i = 0; i < noOfPlayers; i++)
        {
            playerHands[i] = new List<Card>();
        }

        for (int i = 0; i < noCardsPerPlayer; i++)
        {
            for (int j = 0; j < noOfPlayers; j++)
            {
                if (!Empty)
                {
                    var card = NextCard();
                    playerHands[j].Add(card);
                    Console.WriteLine($"{card.ToShortString()} - {card}");
                }
            }
        }

        return playerHands;
    }
}
