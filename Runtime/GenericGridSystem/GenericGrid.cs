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

		#region Constructors

		public GenericGrid(int maxWidth, int maxHeight, Vector2 cellSize, Vector3 offset)
		{
			MaxWidth = maxWidth;
			MaxHeight = maxHeight;
			CellSize = cellSize;
			Offset = offset;

			cells = new Dictionary<Vector2Int, T>();
			reverseCells = new Dictionary<T, Vector2Int>();
		}

		public GenericGrid(int maxWidth, int maxHeight)
		{
			MaxWidth = maxWidth;
			MaxHeight = maxHeight;
			CellSize = Vector2.one;
			Offset = Vector2.zero;

			cells = new Dictionary<Vector2Int, T>();
			reverseCells = new Dictionary<T, Vector2Int>();
		}

		#endregion

		#region Position Helpers

		public Vector3 CalculateWorldPosition(int x, int y)
		{
			return new Vector3(x * CellSize.x, y * CellSize.y, 0f) + Offset;
		}

		public Vector3 CalculateWorldPosition(Vector2Int gridPosition)
		{
			return CalculateWorldPosition(gridPosition.x, gridPosition.y);
		}

		public Vector2Int CalculateGridPosition(Vector3 worldPosition)
		{
			return new Vector2Int(
				Mathf.FloorToInt((worldPosition.x - Offset.x) / CellSize.x),
				Mathf.FloorToInt((worldPosition.y - Offset.y) / CellSize.y)
			);
		}

		public Vector2Int GetGridPosition(T value)
		{
			return reverseCells.TryGetValue(value, out var gridPosition) ? gridPosition : default;
		}

		public Vector3 GetWorldPosition(T value)
		{
			return CalculateWorldPosition(GetGridPosition(value));
		}

		public bool IsPositionValid(int x, int y)
		{
			var validX = MaxWidth <= 0 || (x >= 0 && x < MaxWidth);
			var validY = MaxHeight <= 0 || (y >= 0 && y < MaxHeight);

			return validX && validY;
		}

		public bool IsPositionValid(Vector2Int gridPosition)
		{
			return IsPositionValid(gridPosition.x, gridPosition.y);
		}

		public bool IsPositionValid(Vector3 worldPosition)
		{
			return IsPositionValid(CalculateGridPosition(worldPosition));
		}

		#endregion

		#region Get / Set / Remove

		public void Set(int x, int y, T value)
		{
			var pos = new Vector2Int(x, y);

			if (cells.TryGetValue(pos, out var oldValue))
				reverseCells.Remove(oldValue);

			if (reverseCells.TryGetValue(value, out var oldPos))
				cells.Remove(oldPos);

			cells[pos] = value;
			reverseCells[value] = pos;
		}

		public void Set(Vector3 worldPosition, T value)
		{
			Set(CalculateGridPosition(worldPosition), value);
		}

		public void Set(Vector2Int gridPosition, T value)
		{
			Set(gridPosition.x, gridPosition.y, value);
		}

		public T Get(int x, int y)
		{
			cells.TryGetValue(new Vector2Int(x, y), out var value);
			return value;
		}

		public T Get(Vector3 worldPosition)
		{
			return Get(CalculateGridPosition(worldPosition));
		}

		public T Get(Vector2Int gridPosition)
		{
			return Get(gridPosition.x, gridPosition.y);
		}

		public void Remove(int x, int y)
		{
			var pos = new Vector2Int(x, y);
			if (cells.Remove(pos, out var value))
				reverseCells.Remove(value);
		}

		public void Remove(Vector3 worldPosition)
		{
			Remove(CalculateGridPosition(worldPosition));
		}

		public void Remove(Vector2Int gridPosition)
		{
			Remove(gridPosition.x, gridPosition.y);
		}

		public void Remove(T value)
		{
			Remove(GetGridPosition(value));
		}

		public bool Contains(int x, int y)
		{
			return cells.ContainsKey(new Vector2Int(x, y));
		}

		public bool Contains(Vector3 worldPosition)
		{
			return Contains(CalculateGridPosition(worldPosition));
		}

		public bool Contains(Vector2Int gridPosition)
		{
			return Contains(gridPosition.x, gridPosition.y);
		}

		public bool Contains(T value)
		{
			return reverseCells.ContainsKey(value);
		}

		#endregion

		#region Move

		public void Move(int fromX, int fromY, int toX, int toY)
		{
			var from = new Vector2Int(fromX, fromY);
			var to = new Vector2Int(toX, toY);

			if (!cells.TryGetValue(from, out var value) || from == to || cells.ContainsKey(to))
				return;

			cells.Remove(from);
			cells[to] = value;
			reverseCells[value] = to;
		}

		public void Move(Vector2Int fromGridPosition, Vector2Int toGridPosition)
		{
			Move(fromGridPosition.x, fromGridPosition.y, toGridPosition.x, toGridPosition.y);
		}

		public void Move(int fromX, int fromY, Vector2Int toGridPosition)
		{
			Move(fromX, fromY, toGridPosition.x, toGridPosition.y);
		}

		public void Move(Vector2Int fromGridPosition, int toX, int toY)
		{
			Move(fromGridPosition.x, fromGridPosition.y, toX, toY);
		}

		public void Move(Vector3 fromWorldPosition, Vector3 toWorldPosition)
		{
			Move(CalculateGridPosition(fromWorldPosition), CalculateGridPosition(toWorldPosition));
		}

		public void Move(Vector3 fromWorldPosition, Vector2Int toGridPosition)
		{
			Move(CalculateGridPosition(fromWorldPosition), toGridPosition);
		}

		public void Move(Vector2Int fromGridPosition, Vector3 toWorldPosition)
		{
			Move(fromGridPosition, CalculateGridPosition(toWorldPosition));
		}

		#endregion

		#region Move Direction

		public void MoveDirection(int fromX, int fromY, int directionX, int directionY)
		{
			Move(fromX, fromY, fromX + directionX, fromY + directionY);
		}

		public void MoveDirection(int fromX, int fromY, Vector2Int direction)
		{
			MoveDirection(fromX, fromY, direction.x, direction.y);
		}

		public void MoveDirection(Vector2Int fromGridPosition, Vector2Int direction)
		{
			MoveDirection(fromGridPosition.x, fromGridPosition.y, direction.x, direction.y);
		}

		public void MoveDirection(Vector3 fromWorldPosition, int directionX, int directionY)
		{
			MoveDirection(CalculateGridPosition(fromWorldPosition), new Vector2Int(directionX, directionY));
		}

		public void MoveDirection(Vector3 fromWorldPosition, Vector2Int direction)
		{
			MoveDirection(CalculateGridPosition(fromWorldPosition), direction);
		}

		public void MoveUp(int fromX, int fromY) => MoveDirection(fromX, fromY, 0, 1);
		public void MoveUp(Vector2Int fromGridPosition) => MoveUp(fromGridPosition.x, fromGridPosition.y);
		public void MoveUp(Vector3 fromWorldPosition) => MoveUp(CalculateGridPosition(fromWorldPosition));

		public void MoveDown(int fromX, int fromY) => MoveDirection(fromX, fromY, 0, -1);
		public void MoveDown(Vector2Int fromGridPosition) => MoveDown(fromGridPosition.x, fromGridPosition.y);
		public void MoveDown(Vector3 fromWorldPosition) => MoveDown(CalculateGridPosition(fromWorldPosition));

		public void MoveRight(int fromX, int fromY) => MoveDirection(fromX, fromY, 1, 0);
		public void MoveRight(Vector2Int fromGridPosition) => MoveRight(fromGridPosition.x, fromGridPosition.y);
		public void MoveRight(Vector3 fromWorldPosition) => MoveRight(CalculateGridPosition(fromWorldPosition));

		public void MoveLeft(int fromX, int fromY) => MoveDirection(fromX, fromY, -1, 0);
		public void MoveLeft(Vector2Int fromGridPosition) => MoveLeft(fromGridPosition.x, fromGridPosition.y);
		public void MoveLeft(Vector3 fromWorldPosition) => MoveLeft(CalculateGridPosition(fromWorldPosition));

		#endregion

		#region Enumeration

		public IEnumerable<KeyValuePair<Vector2Int, T>> GetAllKeyValuePairs() => cells;
		public IEnumerable<Vector2Int> GetAllPositions() => cells.Keys;
		public IEnumerable<T> GetAllCells() => cells.Values;

		#endregion

		public void Clear()
		{
			cells.Clear();
			reverseCells.Clear();
		}
	}
}
