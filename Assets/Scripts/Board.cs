using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
	public Tilemap tilemap { get; private set; }
	public Piece activePiece { get; private set; }
	public Piece nextPiece { get; private set; }
	public Piece savedPiece { get; private set; }

	public TetrominoData[] tetrominoes;

	public Vector2Int boardSize = new Vector2Int(10, 20);
	public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
	public Vector3Int previewPosition = new Vector3Int(7, 3, 0);
	public Vector3Int holdPosition = new Vector3Int(-9, 3, 0);

	public int currentLevel = 1;
	private int numLinesCleared = 0;
	public static float fallSpeed = 1f;

	public Text Score;
	public Text LevelUI;

	public int scoreOneLine = 40;
	public int scoreTwoLine = 100;
	public int scoreThreeLine = 300;
	public int scoreFourLine = 1200;


	private int numberOfRowsThisTurn = 0;
	public static int currentScore = 0;

	public void UpdateLevel()
	{
		currentLevel = numLinesCleared / 10;
	}

	public void UpdateSpeed()
	{
		fallSpeed -= 0.15f;
	}

	public void UpdateScore()
	{

		if (numberOfRowsThisTurn > 0)
		{
			if (numberOfRowsThisTurn == 1)
			{
				ClearedOneLine();
			} else if (numberOfRowsThisTurn == 2) {
				ClearedTwoLines();
			} else if (numberOfRowsThisTurn == 3) {
				ClearedThreeLines();
			} else if (numberOfRowsThisTurn == 4) {
				ClearedFourLines();
			}

			numberOfRowsThisTurn = 0;
		}
	}

	public void UpdateUI()
	{
		Score.text = "Score: " + currentScore.ToString();
		LevelUI.text = "Level: " + currentLevel.ToString();
	}

	public void ClearedOneLine()
	{
		currentScore += scoreOneLine + ((currentLevel-1) + 20);
		numLinesCleared++;
	}

	public void ClearedTwoLines()
	{
		currentScore += scoreTwoLine + ((currentLevel - 1) + 25);
		numLinesCleared += 2;
	}

	public void ClearedThreeLines()
	{
		currentScore += scoreThreeLine + ((currentLevel - 1) + 30);
		numLinesCleared += 3;
	}

	public void ClearedFourLines()
	{
		currentScore += scoreFourLine + ((currentLevel - 1) + 40);
		numLinesCleared += 4;
	}

	public RectInt Bounds
	{
		get
		{
			Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
			return new RectInt(position, boardSize);
		}
	}

	private void Awake()
	{
		tilemap = GetComponentInChildren<Tilemap>();
		activePiece = GetComponentInChildren<Piece>();

		nextPiece = gameObject.AddComponent<Piece>();
		nextPiece.enabled = false;

		savedPiece = gameObject.AddComponent<Piece>();
		savedPiece.enabled = false;

		for (int i = 0; i < tetrominoes.Length; i++)
		{
			tetrominoes[i].Initialize();
		}
	}

	private void Start()
	{
		SetNextPiece();
		SpawnPiece();
	}

	private void SetNextPiece()
	{
		// Clear the existing piece from the board
		if (nextPiece.cells != null)
		{
			Clear(nextPiece);
		}

		// Pick a random tetromino to use
		int random = Random.Range(0, tetrominoes.Length);
		TetrominoData data = tetrominoes[random];

		// Initialize the next piece with the random data
		// Draw it at the "preview" position on the board
		nextPiece.Initialize(this, previewPosition, data);
		Set(nextPiece);
	}

	public void SpawnPiece()
	{
		// Initialize the active piece with the next piece data
		activePiece.Initialize(this, spawnPosition, nextPiece.data);

		// Only spawn the piece if valid position otherwise game over
		if (IsValidPosition(activePiece, spawnPosition))
		{
			Set(activePiece);
		}
		else
		{
			GameOver();
		}

		// Set the next random piece
		SetNextPiece();
	}

	//public void SwapPiece()
	//{
	//	// Temporarily store the current saved data so we can swap
	//	TetrominoData savedData = savedPiece.data;


	//	// Clear the existing saved piece from the board
	//	if (savedData.cells != null)
	//	{
	//		Clear(savedPiece);
	//	}

	//	// В "Hold" записываем "Active"
	//	savedPiece.Initialize(this, holdPosition, activePiece.data);
	//	Set(savedPiece);


	//	Clear(activePiece);

	//	if (savedData.cells == null)
	//	{
	//		Clear(activePiece);
	//		activePiece.Initialize(this, spawnPosition, nextPiece.data);
	//		Set(activePiece);
			
	//	}
	//	else
	//	{
	//		Clear(activePiece);
	//		activePiece.Initialize(this, spawnPosition, savedPiece.data);
	//		Set(activePiece);

	//		// Гененрируем "Next"
	//		SetNextPiece();	
	//	}

		
	//}

	private void Update()
	{
		//if (Input.GetKeyDown(KeyCode.RightShift))
		//{
		//	SwapPiece();
		//}

		UpdateScore();
		UpdateUI();
		UpdateLevel();
		UpdateSpeed();
	}

	public void GameOver()
	{
		tilemap.ClearAllTiles();

		// Do anything else you want on game over here..
	}

	public void Set(Piece piece)
	{
		for (int i = 0; i < piece.cells.Length; i++)
		{
			Vector3Int tilePosition = piece.cells[i] + piece.position;
			tilemap.SetTile(tilePosition, piece.data.tile);
		}
	}

	public void Clear(Piece piece)
	{
		for (int i = 0; i < piece.cells.Length; i++)
		{
			Vector3Int tilePosition = piece.cells[i] + piece.position;
			tilemap.SetTile(tilePosition, null);
		}
	}

	public bool IsValidPosition(Piece piece, Vector3Int position)
	{
		RectInt bounds = Bounds;

		// The position is only valid if every cell is valid
		for (int i = 0; i < piece.cells.Length; i++)
		{
			Vector3Int tilePosition = piece.cells[i] + position;

			// An out of bounds tile is invalid
			if (!bounds.Contains((Vector2Int)tilePosition))
			{
				return false;
			}

			// A tile already occupies the position, thus invalid
			if (tilemap.HasTile(tilePosition))
			{
				return false;
			}
		}

		return true;
	}

	public void ClearLines()
	{
		RectInt bounds = Bounds;
		int row = bounds.yMin;

		// Clear from bottom to top
		while (row < bounds.yMax)
		{
			// Only advance to the next row if the current is not cleared
			// because the tiles above will fall down when a row is cleared
			if (IsLineFull(row))
			{
				LineClear(row);
			}
			else
			{
				row++;
			}
		}
	}

	public bool IsLineFull(int row)
	{
		RectInt bounds = Bounds;

		for (int col = bounds.xMin; col < bounds.xMax; col++)
		{
			Vector3Int position = new Vector3Int(col, row, 0);

			// The line is not full if a tile is missing
			if (!tilemap.HasTile(position))
			{
				return false;
			}
		}

		numberOfRowsThisTurn++;

		return true;
	}

	public void LineClear(int row)
	{
		RectInt bounds = Bounds;

		// Clear all tiles in the row
		for (int col = bounds.xMin; col < bounds.xMax; col++)
		{
			Vector3Int position = new Vector3Int(col, row, 0);
			tilemap.SetTile(position, null);
		}

		// Shift every row above down one
		while (row < bounds.yMax)
		{
			for (int col = bounds.xMin; col < bounds.xMax; col++)
			{
				Vector3Int position = new Vector3Int(col, row + 1, 0);
				TileBase above = tilemap.GetTile(position);

				position = new Vector3Int(col, row, 0);
				tilemap.SetTile(position, above);
			}

			row++;
		}
	}

}