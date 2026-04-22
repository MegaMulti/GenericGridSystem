using System;
using System.Collections.Generic;
using UnityEngine;

namespace MegaMulti.GenericGridSystem
{
	public class GenericGrid<T>
	{
		public readonly int MaxWidth;
		public readonly int MaxHeight;
		public readonly Vector2 CellSize;
		public readonly Vector3 Offset;

		private readonly Dictionary<Vector2Int, T> cells;
		private readonly Dictionary<T, Vector2Int> reverseCells;

		public GenericGrid(int maxWidth = 0, int maxHeight = 0, Vector2 cellSize = default, Vector3 offset = default)
		{
			if (cellSize == Vector2.zero)
				cellSize = Vector2.one;

			if (cellSize.x <= 0 || cellSize.y <= 0)
				throw new ArgumentException("MegaMulti.Grid: cellSize must be positive");

			MaxWidth = maxWidth;
			MaxHeight = maxHeight;
			CellSize = cellSize;
			Offset = offset;

			cells = new Dictionary<Vector2Int, T>();
			reverseCells = new Dictionary<T, Vector2Int>();
		}

		#region Helpers

		public static Vector3 CalculateWorldPosition(int x, int y, Vector2 cellSize, Vector3 offset) =>
			new Vector3(x * cellSize.x, y * cellSize.y, 0f) + offset;

		public static Vector3 CalculateWorldPosition(Vector2Int gridPosition, Vector2 cellSize, Vector3 offset) =>
			CalculateWorldPosition(gridPosition.x, gridPosition.y, cellSize, offset);

		public static Vector2Int CalculateGridPosition(Vector3 worldPosition, Vector2 cellSize, Vector3 offset) =>
			new Vector2Int(
				Mathf.FloorToInt((worldPosition.x - offset.x) / cellSize.x),
				Mathf.FloorToInt((worldPosition.y - offset.y) / cellSize.y)
			);

		#endregion


		#region Basic Methods

		public Vector3? GetWorldPosition(int x, int y)
		{
			if (!IsPositionValid(x, y))
				return null;

			return CalculateWorldPosition(x, y, CellSize, Offset);
		}

		public Vector3? GetWorldPosition(Vector2Int gridPosition) =>
			GetWorldPosition(gridPosition.x, gridPosition.y);

		public Vector2Int? GetGridPosition(Vector3 worldPosition)
		{
			if (!IsPositionValid(worldPosition))
				return null;

			return CalculateGridPosition(worldPosition, CellSize, Offset);
		}

		public Vector2Int? GetGridPosition(T value) =>
			reverseCells.TryGetValue(value, out var gridPosition) ? gridPosition : null;

		public void Set(int x, int y, T value)
		{
			if (!IsPositionValid(x, y))
				return;

			var pos = new Vector2Int(x, y);

			if (cells.TryGetValue(pos, out T oldValue))
				reverseCells.Remove(oldValue);

			if (reverseCells.TryGetValue(value, out Vector2Int oldPos))
				cells.Remove(oldPos);

			cells[pos] = value;
			reverseCells[value] = pos;
		}

		public void Set(Vector3 worldPosition, T value) =>
			Set(CalculateGridPosition(worldPosition, CellSize, Offset), value);

		public void Set(Vector2Int gridPosition, T value) =>
			Set(gridPosition.x, gridPosition.y, value);

		public T Get(int x, int y)
		{
			if (!IsPositionValid(x, y))
				return default;

			cells.TryGetValue(new Vector2Int(x, y), out T value);
			return value;
		}

		public T Get(Vector3 worldPosition) =>
			Get(CalculateGridPosition(worldPosition, CellSize, Offset));

		public T Get(Vector2Int gridPosition) =>
			Get(gridPosition.x, gridPosition.y);

		public void Remove(int x, int y)
		{
			if (!IsPositionValid(x, y))
				return;

			var pos = new Vector2Int(x, y);

			if (cells.Remove(pos, out T value))
				reverseCells.Remove(value);
		}

		public void Remove(Vector3 worldPosition) =>
			Remove(CalculateGridPosition(worldPosition, CellSize, Offset));

		public void Remove(Vector2Int gridPosition) =>
			Remove(gridPosition.x, gridPosition.y);

		public void Remove(T value)
		{
			var pos = GetGridPosition(value);
			if (pos.HasValue)
				Remove(pos.Value);
		}

		public bool Contains(int x, int y)
		{
			if (!IsPositionValid(x, y))
				return false;

			return cells.ContainsKey(new Vector2Int(x, y));
		}

		public bool Contains(Vector3 worldPosition) =>
			Contains(CalculateGridPosition(worldPosition, CellSize, Offset));

		public bool Contains(Vector2Int gridPosition) =>
			Contains(gridPosition.x, gridPosition.y);

		public bool Contains(T value)
		{
			var pos = GetGridPosition(value);
			return pos.HasValue && Contains(pos.Value);
		}

		public bool IsPositionValid(int x, int y)
		{
			bool validX = MaxWidth <= 0 || (x >= 0 && x < MaxWidth);
			bool validY = MaxHeight <= 0 || (y >= 0 && y < MaxHeight);

			return validX && validY;
		}

		public bool IsPositionValid(Vector2Int gridPosition) =>
			IsPositionValid(gridPosition.x, gridPosition.y);

		public bool IsPositionValid(Vector3 worldPosition) =>
			IsPositionValid(CalculateGridPosition(worldPosition, CellSize, Offset));

		#endregion

		#region Move Methods

		public void Move(int fromX, int fromY, int toX, int toY)
		{
			if (!IsPositionValid(fromX, fromY) || !IsPositionValid(toX, toY))
				return;

			var from = new Vector2Int(fromX, fromY);
			var to = new Vector2Int(toX, toY);

			if (!cells.TryGetValue(from, out T value) || from == to || cells.ContainsKey(to))
				return;

			cells.Remove(from);
			cells[to] = value;
			reverseCells[value] = to;
		}

		public void Move(Vector2Int fromGridPosition, Vector2Int toGridPosition) =>
			Move(fromGridPosition.x, fromGridPosition.y, toGridPosition.x, toGridPosition.y);

		public void Move(int fromX, int fromY, Vector2Int toGridPosition) =>
			Move(fromX, fromY, toGridPosition.x, toGridPosition.y);

		public void Move(Vector2Int fromGridPosition, int toX, int toY) =>
			Move(fromGridPosition.x, fromGridPosition.y, toX, toY);

		public void Move(Vector3 fromWorldPosition, Vector3 toWorldPosition) =>
			Move(
				CalculateGridPosition(fromWorldPosition, CellSize, Offset),
				CalculateGridPosition(toWorldPosition, CellSize, Offset)
			);

		public void Move(Vector3 fromWorldPosition, Vector2Int toGridPosition) =>
			Move(CalculateGridPosition(fromWorldPosition, CellSize, Offset), toGridPosition);

		public void Move(Vector2Int fromGridPosition, Vector3 toWorldPosition) =>
			Move(fromGridPosition, CalculateGridPosition(toWorldPosition, CellSize, Offset));

		public void MoveDirection(int fromX, int fromY, int directionX, int directionY) =>
			Move(fromX, fromY, fromX + directionX, fromY + directionY);

		public void MoveDirection(int fromX, int fromY, Vector2Int direction) =>
			MoveDirection(fromX, fromY, direction.x, direction.y);

		public void MoveDirection(Vector2Int fromGridPosition, Vector2Int direction) =>
			MoveDirection(fromGridPosition.x, fromGridPosition.y, direction.x, direction.y);

		public void MoveDirection(Vector3 fromWorldPosition, int directionX, int directionY) =>
			MoveDirection(CalculateGridPosition(fromWorldPosition, CellSize, Offset), new Vector2Int(directionX, directionY));

		public void MoveDirection(Vector3 fromWorldPosition, Vector2Int direction) =>
			MoveDirection(CalculateGridPosition(fromWorldPosition, CellSize, Offset), direction);

		public void MoveUp(int fromX, int fromY) =>
			MoveDirection(fromX, fromY, 0, 1);

		public void MoveUp(Vector2Int fromGridPosition) =>
			MoveUp(fromGridPosition.x, fromGridPosition.y);

		public void MoveUp(Vector3 fromWorldPosition) =>
			MoveUp(CalculateGridPosition(fromWorldPosition, CellSize, Offset));

		public void MoveDown(int fromX, int fromY) =>
			MoveDirection(fromX, fromY, 0, -1);

		public void MoveDown(Vector2Int fromGridPosition) =>
			MoveDown(fromGridPosition.x, fromGridPosition.y);

		public void MoveDown(Vector3 fromWorldPosition) =>
			MoveDown(CalculateGridPosition(fromWorldPosition, CellSize, Offset));

		public void MoveRight(int fromX, int fromY) =>
			MoveDirection(fromX, fromY, 1, 0);

		public void MoveRight(Vector2Int fromGridPosition) =>
			MoveRight(fromGridPosition.x, fromGridPosition.y);

		public void MoveRight(Vector3 fromWorldPosition) =>
			MoveRight(CalculateGridPosition(fromWorldPosition, CellSize, Offset));

		public void MoveLeft(int fromX, int fromY) =>
			MoveDirection(fromX, fromY, -1, 0);

		public void MoveLeft(Vector2Int fromGridPosition) =>
			MoveLeft(fromGridPosition.x, fromGridPosition.y);

		public void MoveLeft(Vector3 fromWorldPosition) =>
			MoveLeft(CalculateGridPosition(fromWorldPosition, CellSize, Offset));

		#endregion

		#region Additional Methods

		public IEnumerable<KeyValuePair<Vector2Int, T>> GetAllKeyValuePairs() => cells;

		public IEnumerable<Vector2Int> GetAllPositions() => cells.Keys;

		public IEnumerable<T> GetAllCells() => cells.Values;

		public void Clear()
		{
			cells.Clear();
			reverseCells.Clear();
		}

		#endregion
	}
}
