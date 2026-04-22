using System;
using System.Collections.Generic;
using UnityEngine;

namespace MegaMulti.GenericGridSystem
{
	public class GenericGrid<T>
	{
		private readonly int maxWidth;
		private readonly int maxHeight;
		private readonly Vector2 cellSize;
		private readonly Vector3 offset;

		private readonly Dictionary<Vector2Int, T> cells;
		private readonly Dictionary<T, Vector2Int> reverseCells;

		public GenGrid(int maxWidth = 0, int maxHeight = 0, Vector2 cellSize = default, Vector3 offset = default)
		{
			if (cellSize == Vector2.zero)
				cellSize = Vector2.one;
			
			if (cellSize.x <= 0 || cellSize.y <= 0)
				throw new ArgumentException("MegaMulti.Grid: cellSize must be positive");
			
			this.maxWidth = maxWidth;
			this.maxHeight = maxHeight;
			this.cellSize = cellSize;
			this.offset = offset;

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

		public bool TryGetWorldPosition(int x, int y, out Vector3 worldPosition)
		{
			if (!IsPositionValid(x, y))
			{
				worldPosition = default;
				return false;
			}

			worldPosition = CalculateWorldPosition(x, y, cellSize, offset);
			return true;
		}

		public bool TryGetWorldPosition(Vector2Int gridPosition, out Vector3 worldPosition) =>
			TryGetWorldPosition(gridPosition.x, gridPosition.y, out worldPosition);

		public bool TryGetGridPosition(Vector3 worldPosition, out Vector2Int gridPosition)
		{
			if (!IsPositionValid(worldPosition))
			{
				gridPosition = default;
				return false;
			}

			gridPosition = CalculateGridPosition(worldPosition, cellSize, offset);
			return true;
		}

		public bool TryGetGridPosition(T value, out Vector2Int gridPosition) =>
			return reverseCells.TryGetValue(value, out gridPosition);
		
		public bool TrySet(int x, int y, T value)
		{
			if (!IsPositionValid(x, y))
				return false;

			var pos = new Vector2Int(x, y);

			// If something already exists at this position, remove old reverse mapping
			if (cells.TryGetValue(pos, out T oldValue))
			{
				reverseCells.Remove(oldValue);
			}

			// If the value already exists somewhere else, remove its old position
			if (reverseCells.TryGetValue(value, out Vector2Int oldPos))
			{
				cells.Remove(oldPos);
			}

			cells[pos] = value;
			reverseCells[value] = pos;

			return true;
		}
		
		public bool TrySet(Vector3 worldPosition, T value) =>
			TrySet(CalculateGridPosition(worldPosition, cellSize, offset), value);

		public bool TrySet(Vector2Int gridPosition, T value) =>
			TrySet(gridPosition.x, gridPosition.y, value);

		public bool TryGet(int x, int y, out T value)
		{
			if (!IsPositionValid(x, y))
			{
				value = default;
				return false;
			}

			return cells.TryGetValue(new Vector2Int(x, y), out value);
		}
		
		public bool TryGet(Vector3 worldPosition, out T value) =>
			TryGet(CalculateGridPosition(worldPosition, cellSize, offset), out value);

		public bool TryGet(Vector2Int gridPosition, out T value) =>
			TryGet(gridPosition.x, gridPosition.y, out value);

		public bool TryRemove(int x, int y)
		{
			if (!IsPositionValid(x, y))
				return false;

			var pos = new Vector2Int(x, y);

			if (!cells.TryGetValue(pos, out T value))
				return false;

			cells.Remove(pos);
			reverseCells.Remove(value);

			return true;
		}

		public bool TryRemove(Vector3 worldPosition) =>
			TryRemove(CalculateGridPosition(worldPosition, cellSize, offset));

		public bool TryRemove(Vector2Int gridPosition) =>
			TryRemove(gridPosition.x, gridPosition.y);

		public bool Contains(int x, int y)
		{
			if (!IsPositionValid(x, y))
			{
				return false;
			}

			return cells.ContainsKey(new Vector2Int(x, y));
		}
		
		public bool Contains(Vector3 worldPosition) =>
			Contains(CalculateGridPosition(worldPosition, cellSize, offset));

		public bool Contains(Vector2Int gridPosition) =>
			Contains(gridPosition.x, gridPosition.y);

		public bool IsPositionValid(int x, int y)
		{
			bool validX = maxWidth <= 0 || (x >= 0 && x < maxWidth);
			bool validY = maxHeight <= 0 || (y >= 0 && y < maxHeight);

			return validX && validY;
		}

		public bool IsPositionValid(Vector2Int gridPosition) =>
			IsPositionValid(gridPosition.x, gridPosition.y);

		public bool IsPositionValid(Vector3 worldPosition) =>
			IsPositionValid(CalculateGridPosition(worldPosition, cellSize, offset));

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
