using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LevelBuilder {
	public void Build(Game game) {
		var roomWidth = UnityEngine.Random.Range(3,5);
		var roomHeight = UnityEngine.Random.Range(3,5);

		var rx = (game.Width - roomWidth) / 2;
		var ry = (game.Height - roomHeight) / 2;
		for (var x = 0; x < roomWidth; x++)
			for (var y = 0; y < roomHeight; y++) {
				game.SetTile(rx + x, ry + y, Tile.Floor);
			}

		for (var i = 0; i < 30; i++)
			AddRoom(game, UnityEngine.Random.Range(5,7), UnityEngine.Random.Range(5,7));

		AddSouthConnections(game);
		AddEastConnections(game);

		for (var i = 0; i < 8; i++) {
			var x = -1;
			var y = -1;

			while (game.GetTile(x,y).BlocksMovement) {
				x = UnityEngine.Random.Range(0, game.Width);
				y = UnityEngine.Random.Range(0, game.Height);
			}

			if (game.Creatures.Any())
				game.Creatures.Add(game.Catalog.Skeleton(x,y));
			else
				game.Creatures.Add(game.Catalog.Player(x,y));
		}
		game.Player = game.Creatures[0];
	}

	void AddSouthConnections(Game game) {
		for (var i = 0; i < 20; i++) {
			var cx = UnityEngine.Random.Range(0, game.Width);
			var cy = UnityEngine.Random.Range(0, game.Height);

			if (game.GetTile(cx,cy) != Tile.Floor)
				continue;

			while (game.GetTile(cx,cy) == Tile.Floor)
				cy++;

			var length = 0;
			while (game.GetTile(cx,cy+length) == Tile.Wall)
				length++;

			if (game.GetTile(cx,cy+length) == Tile.Floor && length > 1 && length < 8) {
				for (var l = 0; l < length; l++)
					game.SetTile(cx,cy+l,Tile.Floor);
			}
		}
	}

	void AddEastConnections(Game game) {
		for (var i = 0; i < 20; i++) {
			var cx = UnityEngine.Random.Range(0, game.Width);
			var cy = UnityEngine.Random.Range(0, game.Height);

			if (game.GetTile(cx,cy) != Tile.Floor)
				continue;

			while (game.GetTile(cx,cy) == Tile.Floor)
				cx++;

			var length = 0;
			while (game.GetTile(cx+length,cy) == Tile.Wall)
				length++;

			if (game.GetTile(cx+length,cy) == Tile.Floor && length > 1 && length < 8) {
				for (var l = 0; l < length; l++)
					game.SetTile(cx+l,cy,Tile.Floor);
			}
		}
	}

	void AddRoom(Game game, int roomWidth, int roomHeight) {

		var candidates = new List<Point>();
		for (var x = 0; x < game.Width; x++) {
			for (var y = 0; y < game.Height; y++) {
				var ok = false;
				for (var xo = 1; xo < roomWidth-1; xo++) {
					if (game.GetTile(x + xo, y - 1) == Tile.Floor)
						ok = true;
				}
				for (var xo = 1; xo < roomWidth-1; xo++) {
					if (game.GetTile(x + xo, y + roomWidth) == Tile.Floor)
						ok = true;
				}
				for (var yo = 1; yo < roomHeight-1; yo++) {
					if (game.GetTile(x - 1, y + yo) == Tile.Floor)
						ok = true;
				}
				for (var yo = 1; yo < roomHeight-1; yo++) {
					if (game.GetTile(x + roomWidth, y + yo) == Tile.Floor)
						ok = true;
				}

				if (!ok)
					continue;

				for (var xo = 0; xo < roomWidth; xo++) {
					for (var yo = 0; yo < roomHeight; yo++) {
						if (game.GetTile(x + xo, y + yo) != Tile.Wall)
							ok = false;
					}
				}
				if (ok)
					candidates.Add(new Point(x, y));
			}
		}
		if (candidates.Any())
			AddRoom(game, roomWidth, roomHeight, candidates[UnityEngine.Random.Range(0, candidates.Count)]);
	}

	void AddRoom(Game game, int roomWidth, int roomHeight, Point position) {
		var nDoorCandidates = new List<Point>();
		var sDoorCandidates = new List<Point>();
		var wDoorCandidates = new List<Point>();
		var eDoorCandidates = new List<Point>();

		for (var xo = 0; xo < roomWidth; xo++) {
			for (var yo = 0; yo < roomHeight; yo++) {
				if (xo == 0 && yo == 0 || xo == 0 && yo == roomHeight - 1 
					|| xo == roomWidth - 1 && yo == 0 || xo == roomWidth - 1 && yo == roomHeight - 1)
					continue;

				if (xo == 0 || yo == 0 || xo == roomWidth - 1 || yo == roomHeight - 1) {
					game.SetTile(position.X + xo, position.Y + yo, Tile.Wall);

					if (yo == 0 && game.GetTile(position.X + xo, position.Y + yo - 1) == Tile.Floor)
						sDoorCandidates.Add(new Point(position.X + xo, position.Y + yo));
					if (yo == roomHeight-1 && game.GetTile(position.X + xo, position.Y + yo + 1) == Tile.Floor)
						sDoorCandidates.Add(new Point(position.X + xo, position.Y + yo));
					if (xo == 0 && game.GetTile(position.X + xo - 1, position.Y + yo) == Tile.Floor)
						wDoorCandidates.Add(new Point(position.X + xo, position.Y + yo));
					if (xo == roomWidth-1 && game.GetTile(position.X + xo + 1, position.Y + yo) == Tile.Floor)
						eDoorCandidates.Add(new Point(position.X + xo, position.Y + yo));
				} else {
					game.SetTile(position.X + xo, position.Y + yo, Tile.Floor);
				}
			}
		}

		if (nDoorCandidates.Any()) {
			AddDoor(game, nDoorCandidates[UnityEngine.Random.Range(0, nDoorCandidates.Count)]);
			if (UnityEngine.Random.value < 0.25f)
				AddDoor(game, nDoorCandidates[UnityEngine.Random.Range(0, nDoorCandidates.Count)]);
		}

		if (sDoorCandidates.Any()) {
			AddDoor(game, sDoorCandidates[UnityEngine.Random.Range(0, sDoorCandidates.Count)]);
			if (UnityEngine.Random.value < 0.25f)
				AddDoor(game, sDoorCandidates[UnityEngine.Random.Range(0, sDoorCandidates.Count)]);
		}

		if (wDoorCandidates.Any()) {
			AddDoor(game, wDoorCandidates[UnityEngine.Random.Range(0, wDoorCandidates.Count)]);
			if (UnityEngine.Random.value < 0.25f)
				AddDoor(game, wDoorCandidates[UnityEngine.Random.Range(0, wDoorCandidates.Count)]);
		}

		if (eDoorCandidates.Any()) {
			AddDoor(game, eDoorCandidates[UnityEngine.Random.Range(0, eDoorCandidates.Count)]);
			if (UnityEngine.Random.value < 0.25f)
				AddDoor(game, eDoorCandidates[UnityEngine.Random.Range(0, eDoorCandidates.Count)]);
		}
	}

	void AddDoor(Game game, Point candidate) {
		game.SetTile(candidate.X, candidate.Y, Tile.Floor);
	}
}