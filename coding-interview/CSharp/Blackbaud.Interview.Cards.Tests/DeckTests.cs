using Xunit;

namespace Blackbaud.Interview.Cards.Tests;
public class DeckTests
{
    [Fact]
    public void CanCreateANewDeck()
    {
        var deck = Deck.NewDeck();
        Assert.Equal(52, deck.RemainingCards);
    }

    [Fact]
    public void CanShuffleDeck()
    {
        var deck = Deck.NewDeck();
        var originalOrder = deck.RemainingCards;
        deck.Shuffle(3);
        Assert.Equal(originalOrder, deck.RemainingCards);
    }
}
