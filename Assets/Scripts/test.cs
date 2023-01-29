Daniil ♥️, [2023-01-15 2:03 PM]
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
    public event Action<Environment> OnEnvironmentRequestedDecision;
    public event Action<Environment> OnEnvironmentGameCompleted;
    
    public readonly List<Card> drawCards = new List<Card>();
    public readonly List<Card> inPlayCards = new List<Card>();
    public readonly List<Card> discardedCards = new List<Card>();
    public List<DurakAgent> players;
    public Card playedCard;
    public CardSuit trumpSuit;
    public int numAttacks;
    public int numTurnsThisAttack;
    
    [SerializeField] private int defaultCardAmount = 6;
    [SerializeField] private int maxTurnsPerAttack = 100;
    [SerializeField] private int maxAttacksPerGame = 100;

    private Coroutine gameRoutine;
    private Coroutine attackRoutine;
    private bool attackerWon;

    private void Awake()
    {
        FillDrawWithAllCards();
    }

    public IEnumerator PlayAGame()
    {
        ShuffleDraw();
        DealCards(players);
        DurakAgent winner = null;
        trumpSuit = (CardSuit)Random.Range(0, 3);
        int attackingPlayerIndex = Random.Range(0, players.Count);
        int defendingPlayerIndex = NextPlayerIndex(attackingPlayerIndex);
        numAttacks = 0;
        
        while (numAttacks < maxAttacksPerGame && winner == null)
        {
            attackRoutine = StartCoroutine(PlayAnAttack(attackingPlayerIndex, defendingPlayerIndex));
            yield return new WaitUntil(() => attackRoutine == null);
            
            if (drawCards.Count < 1)
            {
                foreach (DurakAgent durakAgent in players)
                {
                    if (durakAgent.hand.Count < 1)
                    {
                        winner = durakAgent;
                    }
                }
            }
            else
            {
                DealCards(new List<DurakAgent>{players[attackingPlayerIndex], players[defendingPlayerIndex]});
            }
            
            if (!attackerWon)
            {
                attackingPlayerIndex = defendingPlayerIndex;
                defendingPlayerIndex = NextPlayerIndex(defendingPlayerIndex);
            }
            
            numAttacks++;
        }
        
        ReturnCardsToDraw();
        
        if (winner != null)
        {
            winner.AddReward(2f);
        }
        
        foreach (DurakAgent durakAgent in players)
        {
            durakAgent.AddReward(-1);
            durakAgent.EndEpisode();
            //Academy.Instance.EnvironmentStep();
            //yield return new WaitUntil(() => EnvironmentManager.academyStepped);
        }
        
        OnEnvironmentGameCompleted?.Invoke(this);
    }

Daniil ♥️, [2023-01-15 2:03 PM]
private IEnumerator PlayAnAttack(int attackerIndex, int defenderIndex)
    {
        int currentTurnPlayerIndex = attackerIndex;
        attackerWon = false;
        bool attackerGaveUp = false;
        numTurnsThisAttack = 0;
        
        while (numTurnsThisAttack < maxTurnsPerAttack && !attackerWon && !attackerGaveUp)
        {
            // do attack or defend
            if (currentTurnPlayerIndex == attackerIndex)
            {
                players[attackerIndex].Attack();
                OnEnvironmentRequestedDecision?.Invoke(this);
                yield return new WaitUntil(() => EnvironmentManager.academyStepped);
            }
            else
            {
                players[defenderIndex].Defend();
                OnEnvironmentRequestedDecision?.Invoke(this);
                yield return new WaitUntil(() => EnvironmentManager.academyStepped);
            }
           
            if (playedCard == null)
            {
                if (currentTurnPlayerIndex == attackerIndex)
                {
                    attackerGaveUp = true;
                    discardedCards.AddRange(inPlayCards);
                    inPlayCards.Clear();
                }
                else
                {
                    // Attacker can shed cards
                    bool attackerAddedCard = false;
                    
                    do
                    {
                        players[attackerIndex].FinishOff();
                        OnEnvironmentRequestedDecision?.Invoke(this);
                        yield return new WaitUntil(() => EnvironmentManager.academyStepped);
                        
                        attackerAddedCard = playedCard != null;

                        if (attackerAddedCard)
                        {
                            inPlayCards.Insert(0, playedCard);
                            playedCard = null;
                        }
                    } while (attackerAddedCard);
                    
                    attackerWon = true;
                    players[defenderIndex].hand.AddRange(inPlayCards);
                    inPlayCards.Clear();
                }
            }
            else
            {
                inPlayCards.Insert(0, playedCard);
                playedCard = null;
                
                if (currentTurnPlayerIndex == defenderIndex && players[defenderIndex].hand.Count < 1)
                {
                    attackerGaveUp = true;
                    discardedCards.AddRange(inPlayCards);
                    inPlayCards.Clear();
                }
                else
                {
                    currentTurnPlayerIndex = currentTurnPlayerIndex == attackerIndex ? defenderIndex : attackerIndex;
                }
            }
            
            numTurnsThisAttack++;
        }
        
        attackRoutine = null;
    }
    
    private void FillDrawWithAllCards()
    {
        foreach (CardSuit cardType in Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardValue cardValue in Enum.GetValues(typeof(CardValue)))
            {
                drawCards.Add(new Card(cardType, cardValue));
            }
        } 
    }

    private void ShuffleDraw()
    {
        int shuffledCardIndex = drawCards.Count; 
        
        while (shuffledCardIndex > 1) 
        {
            shuffledCardIndex--;  
            int shuffledCardNewIndex = Random.Range(0, shuffledCardIndex + 1);
            (drawCards[shuffledCardNewIndex], drawCards[shuffledCardIndex]) = (drawCards[shuffledCardIndex], drawCards[shuffledCardNewIndex]);
        }  
    }

    private void DealCards(List<DurakAgent> playersToDealCardsTo)
    {
        foreach (DurakAgent durakAgent in playersToDealCardsTo)
        {
            int cardsToDraw = defaultCardAmount - durakAgent.hand.Count;
            
            for (int i = 0; i < cardsToDraw; i++)
            {
                if (drawCards.Count > 0)
                {
                    durakAgent.hand.Add(drawCards[0]);
                    drawCards.RemoveAt(0);
                }
                else
                {
                    return;
                }
            }
        }
    }
    
private void ReturnCardsToDraw()
    {
        foreach (DurakAgent durakAgent in players)
        {
            drawCards.AddRange(durakAgent.hand);
            durakAgent.hand.Clear();
        }
        
        drawCards.AddRange(discardedCards);
        discardedCards.Clear();
        
        drawCards.AddRange(inPlayCards);
        inPlayCards.Clear();
    }
    
    private int NextPlayerIndex(int index)
    {
        int nextPlayerIndexCandidate = index + 1;
        return nextPlayerIndexCandidate < players.Count ? nextPlayerIndexCandidate : 0;
    }
}