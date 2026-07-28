namespace Blackbaud.Interview.Cards;

public static class Program
{
    static void Main()
    {
        // Part 1 Create a new deck and deal out all the cards

        Console.WriteLine("**********");
        Console.WriteLine("Part 1 - Create a new deck, shuffle, then deal out all the cards");

        // Create a new deck
        var deck = Deck.NewDeck();

      

        //deck.sh
        // TODO: shuffle the deck
        deck.Shuffle(3);
        Console.WriteLine("Shuffling...");

        //// Deal all the cards
        //while (!deck.Empty)
        //{
        //    var card = deck.NextCard();
        //    Console.WriteLine($"{card.ToShortString()} - {card}");
        //}

       var playerHands = deck.DealCards(4, 3);
        playerHands.ToList().ForEach(playerHand =>
        {
            Console.WriteLine($"Player {playerHand.Key} hand:");
            playerHand.Value.ForEach(card => Console.WriteLine($"  {card.ToShortString()} - {card}"));
        });

        AnnoinceHighCardPlayer(playerHands);


        Console.WriteLine();
        Console.WriteLine("**********");
        Console.WriteLine();
    }

    public static void AnnoinceHighCardPlayer(Dictionary<int, List<Card>> playerHands)
    {
        var highCardPlayer = playerHands.MaxBy(playerHand => playerHand.Value.Max(card => card.Rank));
        Console.WriteLine("player with the highest card is player " + highCardPlayer.Key + " with card " + highCardPlayer.Value.Max(card => card.Rank));
    }
}
