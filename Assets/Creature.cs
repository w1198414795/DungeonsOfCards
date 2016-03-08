using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Creature {
	public bool Exists = true;
	public Point Position;
	public string SpriteName;
	public string TeamName;
	public AI Ai;

	public int AttackValue;
	public int MaximumAttackCards;
	public int DefenseValue;
	public int MaximumDefenseCards;
	public int MaximumHealth;
	public int CurrentHealth;
	public int MaximumHandCards;

	public List<Card> DrawStack = new List<Card>();
	public List<Card> AttackStack = new List<Card>();
	public List<Card> DefenseStack = new List<Card>();
	public List<Card> HandStack = new List<Card>();
	public List<Card> DiscardStack = new List<Card>();

	public void MoveBy(Game game, int mx, int my) {
		var next = Position + new Point(mx, my);

		var tile = game.GetTile(next.X, next.Y);

		if (tile == Tile.DoorClosed) {
			game.SetTile(next.X, next.Y, Tile.DoorOpen);
			EndTurn(game);
			return;
		} else if (tile.BlocksMovement) {
			EndTurn(game);
			return;
		}
		
		var other = game.GetCreature(next);
		if (other != null && other != this) {
			if (other.TeamName != TeamName)
				Attack(game, other);
		} else {
			Position += new Point(mx, my);
			var item = game.GetItem(Position);
			if (item != null) {
				item.Exists = false;
				if (item.Card != null) {
					if (this == game.Player)
						game.NewCards.Add(item.Card);
					DrawStack.Add(item.Card);
					game.Popups.Add(new TextPopup(item.Card.Name, item.Position, Vector3.zero));
				} else if (item.Pack != null) {
					var allCards = Util.Shuffle(item.Pack.Cards);
					if (this == game.Player)
						game.NewCards.AddRange(allCards);
					DrawStack.AddRange(allCards);
					game.Popups.Add(new TextPopup(item.Pack.Name, item.Position, Vector3.zero));
				}
			}
			Draw1Card(game);
		}

		EndTurn(game);
	}

	public void UseCard(Game game, Card card) {
		HandStack.Remove(card);
		card.DoAction(game, this, null, card.OnUse);
		game.Popups.Add(new TextPopup(card.Name, Position, new Vector3(0,4,0)));
		DiscardStack.Add(card);
	}

	void ReshuffleIntoDrawStack() {
		if (!DiscardStack.Any()) {
			DiscardStack.AddRange(AttackStack);
			AttackStack.Clear();
			DiscardStack.AddRange(DefenseStack);
			DefenseStack.Clear();
			DiscardStack.AddRange(HandStack);
			HandStack.Clear();
		}
		DrawStack.AddRange(Util.Shuffle(DiscardStack));
		DiscardStack.Clear();
	}

	public Card GetTopDrawCard() {
		if (!DrawStack.Any())
			ReshuffleIntoDrawStack();
		
		var pulledCard = DrawStack.Last();
		DrawStack.Remove(pulledCard);
		return pulledCard;
	}

	public void Draw1Card(Game game) {
		KeepCard(game, GetTopDrawCard());
	}

	public void KeepCard(Game game, Card pulledCard) {
		pulledCard.DoAction(game, this, null, pulledCard.OnDraw);

		if (pulledCard.CardType == CardType.Attack) {
			AttackStack.Add(pulledCard);
			while (AttackStack.Count > MaximumAttackCards) {
				var toDiscard = AttackStack[0];
				AttackStack.RemoveAt(0);
				DiscardStack.Add(toDiscard);
			}
		} else if (pulledCard.CardType == CardType.Defense) {
			DefenseStack.Add(pulledCard);
			while (DefenseStack.Count > MaximumDefenseCards) {
				var toDiscard = DefenseStack[0];
				DefenseStack.RemoveAt(0);
				DiscardStack.Add(toDiscard);
			}
		} else {
			HandStack.Add(pulledCard);
			pulledCard.DoAction(game, this, null, pulledCard.OnInHand);
			while (HandStack.Count > MaximumHandCards) {
				var toDiscard = HandStack[0];
				HandStack.RemoveAt(0);
				toDiscard.UndoAction(game, this, toDiscard.OnInHand);
				DiscardStack.Add(toDiscard);
			}
		}
	}

	void EndTurn(Game game) {
		while (AttackStack.Count > MaximumAttackCards) {
			var toDiscard = AttackStack[0];
			AttackStack.RemoveAt(0);
			DiscardStack.Add(toDiscard);
		}
		while (DefenseStack.Count > MaximumDefenseCards) {
			var toDiscard = DefenseStack[0];
			DefenseStack.RemoveAt(0);
			DiscardStack.Add(toDiscard);
		}
		while (HandStack.Count > MaximumHandCards) {
			var toDiscard = HandStack[0];
			HandStack.RemoveAt(0);
			toDiscard.UndoAction(game, this, toDiscard.OnInHand);
			DiscardStack.Add(toDiscard);
		}
	}

	public void TakeTurn(Game game) {
		Ai.TakeTurn(game, this);
	}

	public void Attack(Game game, Creature other) {
		new Combat(game, this, other).Resolve();
	}

	public void TakeDamage(int amount) {
		CurrentHealth -= amount;
		if (CurrentHealth <= 0)
			Die();
	}

	public void Die() {
		Exists = false;
	}
}