//Paul Reichling
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PetteiaGameController : MonoBehaviour
{
	[Header("Game Pieces")]
	public List<PetteiaMovePiece> playerPieces;
	public PetteiaEnemyAI enemyAI;

	public AudioSource moveSound;

	[Header("Board Positions")]
	public PetteiaColliderMover[] squaresRow0 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow1 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow2 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow3 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow4 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow5 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow6 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow7 = new PetteiaColliderMover[8];
	public int[,] positions = new int[8, 8];

	[Header("UI")]
	//public GameObject menuCanvas;
	//public GameObject endCanvas;
	//public Text waterText;
	//public Text foodText;
	public MiniGameInfoScreen mgScreen;
	public TavernaMiniGameDialog gameBarks;
	public Sprite gameIcon;
	public float barkChance = 0.25f;


	[TextArea(3, 40)]
	public string boardText = "This text will appear in a text area that automatically expands";


	[HideInInspector] public bool yourTurn;
	private int currentPiece;
	private string moveDir;
	private Vector2 oldPos, curPos;
	private Vector2 curPosArray, oldPosArray;
	private bool updateOld;
	private bool gameOver = false;

	//private Transform currentT;
	//public MovePiece mp;

	//Some variables are public for debugging and being able to be viewed in the inspector 
	// Start is called before the first frame update
	void Start() {
		//menuCanvas.SetActive(false);
		//endCanvas.SetActive(false);
		mgScreen.gameObject.SetActive(true);
		mgScreen.DisplayText("Petteia", "Taverna game", "Petteia is started, here's where stuff will go", gameIcon, MiniGameInfoScreen.MiniGame.TavernaStart);
		enemyAI = GetComponent<PetteiaEnemyAI>();
		moveDir = "";
		InitalStateSetup();
		updateOld = true;
		yourTurn = true;

		for (int i = 0; i < 8; i++) {
			BoardSquares[0, i] = squaresRow0[i];
			BoardSquares[1, i] = squaresRow1[i];
			BoardSquares[2, i] = squaresRow2[i];
			BoardSquares[3, i] = squaresRow3[i];
			BoardSquares[4, i] = squaresRow4[i];
			BoardSquares[5, i] = squaresRow5[i];
			BoardSquares[6, i] = squaresRow6[i];
			BoardSquares[7, i] = squaresRow7[i];
		}
	}

	// Update is called once per frame
	void Update() {
		
		//if (!menuCanvas.activeSelf) {

		//	if (yourTurn) {
		//		lastPieceMoved = 1;
		//		curPosArray = PosToArray((int)curPos.x, (int)curPos.y);
		//		oldPosArray = PosToArray((int)oldPos.x, (int)oldPos.y);
		//		if (Input.GetKeyDown(KeyCode.Q)) {
		//			PrintBoard();
		//		}

		//		if (Input.GetKeyUp(KeyCode.Mouse0)) {
		//			if (SetPiecePosition() == "m") {

		//				//Debug.Log("enemyturn");
		//				//mp.isMoving = true;
		//				yourTurn = false;
		//				moveSound.pitch = Random.Range(0.7f, 1.1f);
		//				moveSound.Play();
		//				PrintBoard();
						
		//			} else {
		//				//mp.isMoving = false;
		//			}
		//			updateOld = true;

		//		}
		//	}
		//	else {

		//		EnemyMove();

		//	}
		//}
	}

	public void PauseMinigame() 
	{
		mgScreen.gameObject.SetActive(true);
		Time.timeScale = 0;
		mgScreen.DisplayText("Petteia", "Taverna game", "Petteia is paused, here's where the controls will go", gameIcon, MiniGameInfoScreen.MiniGame.TavernaPause);
	}

	public void UnpauseMinigame() {
		mgScreen.gameObject.SetActive(false);
		Time.timeScale = 1;
	}

	public void ExitMinigame() 
	{
		TavernaController.BackToTavernaMenu();
	}

	public void RestartMinigame() 
	{
		TavernaController.ReloadTavernaGame("Petteia");
	}

	//TODO: no more reloading, we'll have to just reset everything's position and state
	//so delete all pieces and respawn them in their base positions?
	//and if needed mark given squares as occupied
	//and show the ui again
	private IEnumerator ReloadMinigame() {
		Scene current = SceneManager.GetActiveScene();
		string name = current.name;
		yield return SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
		yield return SceneManager.UnloadSceneAsync(current);
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
	}

	public IEnumerator SwitchTurn() 
	{
		PrintBoard();
		//Debug.Log("Switching turn");
		if (yourTurn) {
			Debug.Log("Ending player turn");
			yourTurn = false;
			yield return CheckCapture();
			Debug.Log("Done CheckCapture");
			CheckGameOver();
			curPosArray = PosToArray((int)curPos.x, (int)curPos.y);
			oldPosArray = PosToArray((int)oldPos.x, (int)oldPos.y);
			updateOld = true;

			enemyAI.StartEnemyTurn();
		}
		else {
			Debug.Log("Ending enemy turn");
			yourTurn = true;
			yield return CheckCapture();
			Debug.Log("Done CheckCapture");
			CheckGameOver();
			oldPosArray = Vector2.up;
			curPosArray = Vector2.up;
			oldPos = Vector2.up;
			curPos = Vector2.up;
			PetteiaMovePiece.showHighlight = true;
		}
	}

	public void PlayMoveSound() 
	{
		moveSound.pitch = Random.Range(0.7f, 1.1f);
		moveSound.Play();
	}

	void InitalStateSetup() {
		for (int i = 0; i < 8; i++) {

			positions[0, i] = 1;

			positions[7, i] = 2;

		}
	}

	string SetPiecePosition() {
		bool collide = false;
		//See if piece is off board - interviewer said my code was hard to read so I made it shorter
		if ((int)curPosArray.x > 7 || (int)curPosArray.y > 7 || (int)curPosArray.x < 0 || (int)curPosArray.y < 0) { collide = true; }


		//Checking each direction to see if the piece tries to move through another piece, which is not allowed
		if (collide == false) {
			if (moveDir == "down") {
				for (int i = (int)curPosArray.x; i > (int)oldPosArray.x; i--) {
					if (positions[i, (int)curPosArray.y] != 0) {
						collide = true;
					}
				}
			}
			if (moveDir == "up") {
				for (int i = (int)curPosArray.x; i < (int)oldPosArray.x; i++) {
					if (positions[i, (int)curPosArray.y] != 0) {
						collide = true;
					}
				}
			}

			if (moveDir == "right") {
				for (int i = (int)curPosArray.y; i > (int)oldPosArray.y; i--) {
					if (positions[(int)curPosArray.x, i] != 0) {
						collide = true;
					}
				}
			}
			if (moveDir == "left") {
				for (int i = (int)curPosArray.y; i < (int)oldPosArray.y; i++) {
					if (positions[(int)curPosArray.x, i] != 0) {
						collide = true;
					}
				}
			}
		}

		if (collide && !updateOld) {
			//MoveBack(currentT);

			//yourTurn = false;
			//Debug.Log("MOVEBACK TRIGGERED");
			moveSound.pitch = Random.Range(0.7f, 1.1f);
			moveSound.Play();
			return "mb";
		}
		else {
			try {
				//Transform inital, final;
				//send in original pos and new pos 
				positions[(int)curPosArray.x, (int)curPosArray.y] = currentPiece;
				positions[(int)oldPosArray.x, (int)oldPosArray.y] = 0;
			}
			catch {

			}
			if (oldPosArray != curPosArray) {
				return "m";
			} else {
				return "mb";
			}

		}
	}

	public IEnumerator CheckCapture() {
		for (int y = 0; y < 8; y++) {

			for (int x = 0; x < 8; x++) {
				
				if (x != 0 && x != 7) {
					
						if (positions[x, y] == 1
						&& positions[x + 1, y] == 2
						&& positions[x - 1, y] == 2) {
							Debug.Log("CAPTURE1");
							yield return DoCapturePiece(x, y);
							//Debug.Log(x.ToString() + y.ToString());
						}
					
					
						if (positions[x, y] == 2
					&& positions[x + 1, y] == 1
					&& positions[x - 1, y] == 1) {
							Debug.Log("CAPTURE2");
							yield return DoCapturePiece(x, y);
							//Debug.Log(x.ToString() + y.ToString());
						}
					
				}
				if (y != 0 && y != 7) {
					
						if (positions[x, y] == 1
					&& positions[x, y + 1] == 2
					&& positions[x, y - 1] == 2) {
							Debug.Log("CAPTURE3");
							yield return DoCapturePiece(x, y);
							//Debug.Log(x.ToString() + y.ToString());
						}
					
					
						if (positions[x, y] == 2
						&& positions[x, y + 1] == 1
						&& positions[x, y - 1] == 1) {
							Debug.Log("CAPTURE4");
							yield return DoCapturePiece(x, y);
							//Debug.Log(x.ToString() + y.ToString());
						
					}
				}


			}
		}
		//CheckGameOver();
		#region old code
		//Older function with semantics errors

		//for (int y = 0; y < 8; y++) {

		//	for (int x = 1; x < 7; x++) {

		//		if (positions[0, x] == 1 && positions[0, x - 1] == 2 && positions[0, x + 1] == 2) {
		//			//Debug.Log("CAPTUREo");
		//			StartCoroutine(CapturePiece(x, 0));
		//			//Debug.Log(x.ToString() + y.ToString());
		//		}
		//		if (positions[0, x] == 2 && positions[0, x - 1] == 1 && positions[0, x + 1] == 1) {
		//			//Debug.Log("CAPTUREo");
		//			StartCoroutine(CapturePiece(x, 0));
		//			//Debug.Log(x.ToString() + y.ToString());
		//		}
		//		if (y != 0 && y != 7) {
		//			////Debug.Log(positions[i - 1, j] + "," + positions[j, i] + "," + positions[i + 1, j]);
		//			if (positions[x, y] == 1 && positions[x - 1, y] == 2 && positions[x + 1, y] == 2) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());
		//			}
		//			if (positions[x, y] == 2 && positions[x - 1, y] == 1 && positions[x + 1, y] == 1) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());

		//			}

		//		//if (j != 0 || j != 7) {
		//			if (positions[x, y] == 1 && positions[x, y - 1] == 2 && positions[x, y + 1] == 2) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());

		//			}
		//			if (positions[x, y] == 2 && positions[x, y - 1] == 1 && positions[x, y + 1] == 1) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());

		//			}
		//		} else {
		//			if (positions[x, y] == 1 && positions[x - 1, y] == 2 && positions[x + 1, y] == 2) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());
		//			}
		//			if (positions[x, y] == 2 && positions[x - 1, y] == 1 && positions[x + 1, y] == 1) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());

		//			}
		//		}
		//	}
		//}
		#endregion
	}

	public void CheckGameOver() {
		Debug.Log($"Players: {playerPieces.Count} | Enemies: {enemyAI.pieces.Count}");
		if (enemyAI.pieces.Count <= 1) {
			mgScreen.gameObject.SetActive(true);
			mgScreen.DisplayText("Petteia Victory", "Taverna Game", "You won the game and now you get a reward.", gameIcon, MiniGameInfoScreen.MiniGame.TavernaEnd);
			gameOver = true;
		}

		if (playerPieces.Count <= 1) {
			mgScreen.gameObject.SetActive(true);
			mgScreen.DisplayText("Petteia Loss", "Taverna Game", "You lost the game and now you get nothing.", gameIcon, MiniGameInfoScreen.MiniGame.TavernaEnd);
			gameOver = true;
		}
	}

	private void CapturePiece(int i, int j) {
		StartCoroutine(DoCapturePiece(i, j));
	}

	private IEnumerator DoCapturePiece(int i, int j) 
	{
		Debug.Log("DoCapturePiece");
		Debug.Log("Line after DoCapturePiece");
		positions[i, j] = 0;
		Debug.Log("Set positions to 0");
		BoardSquares[i, j].DestroyPiece();
		Debug.Log("Destroyed piece");
		Debug.Log("enemyAI.CheckPieces");
		yield return StartCoroutine(enemyAI.CheckPieces());
		Debug.Log("Finished with enemyAI.CheckPieces");
		yield return null;

		//CheckGameOver();
		PrintBoard();
		//colliders[i, j].GetComponent<PetteiaColliderMover>().destroy = true;
		//colliders[i, j].SetActive(true);
		//Collider needs time to check for collisions
		//yield return new WaitForSeconds(0.2f);
		//colliders[i, j].SetActive(false);

		if (Random.Range(0f, 1f) < barkChance) {
			//player captures enemy
			if (positions[i, j] == 1) {
				gameBarks.DisplayInsult();
			}
			//enemy captures player
			else if (positions[i, j] == 2) {
				gameBarks.DisplayBragging();
			}
		}
	}

	//IEnumerator GetPiece(int i, int j) {
	//	positions[i, j] = 0;
	//	colliders[i, j].GetComponent<colliderMover>().destroy = false;
	//	colliders[i, j].SetActive(true);
		

	//	yield return new WaitForSeconds(0.2f);
	//	g = colliders[i, j].GetComponent<colliderMover>().go.transform;
	//	colliders[i, j].SetActive(false);
	//	MovePiece(g, "down", gpos, g.tag);
	//	yourTurn = true;


	//}

	public void PrintBoard() {
		string s = "  ";
		for (int i = 0; i < 8; i++) {
			s += "\n\n";
			for (int j = 0; j < 8; j++) {
				s += "  ";
				switch (positions[i, j]) {
					case 0:
						s += "-";
						break;
					case 1:
						s += "X";
						break;
					case 2:
						s += "O";
						break;
				}
				//s += positions[i, j];
			}
		}

		//Debug.Log(s);
		boardText = s;
	}

	public void MovePiece(Vector2Int oldPos, Vector2Int newPos, string tag) 
	{
		positions[oldPos.x, oldPos.y] = 0;
		positions[newPos.x, newPos.y] = tag == "PetteiaW" ? 2 : 1;
		currentPiece = tag == "PetteiaW" ? 2 : 1;
	}

	//public Vector3 MovePiece(Transform piece, string dir, Vector3 startPos, string tag) {

	//	//PrintBoard();

	//	moveDir = dir;
	//	if (updateOld == true) {
	//		currentT = piece;
	//		oldPos = new Vector2(startPos.z, startPos.x);

	//		updateOld = false;
	//	}
	//	if (dir == "up") {
	//		piece.position = startPos + Vector3.forward * 6.25f;
	//		//SetPiecePosition();
	//		curPos = new Vector2(piece.position.z, piece.position.x);

	//		if (tag == "PetteiaW") {
	//			currentPiece = 2;
	//		}
	//		else {
	//			currentPiece = 1;
	//		}
	//		return startPos + Vector3.forward * 6.25f;
	//	}
	//	if (dir == "down") {
	//		piece.position = startPos + Vector3.forward * -6.25f;
	//		//SetPiecePosition();
	//		curPos = new Vector2(piece.position.z, piece.position.x);

	//		if (tag == "PetteiaW") {
	//			currentPiece = 2;
	//		}
	//		else {
	//			currentPiece = 1;
	//		}
	//		return startPos + Vector3.forward * -6.25f;
	//	}
	//	if (dir == "left") {
	//		piece.position = startPos + Vector3.right * -6.25f;
	//		//SetPiecePosition();
	//		curPos = new Vector2(piece.position.z, piece.position.x);

	//		if (tag == "PetteiaW") {
	//			currentPiece = 2;
	//		}
	//		else {
	//			currentPiece = 1;
	//		}
	//		return startPos + Vector3.right * -6.25f;
	//	}
	//	if (dir == "right") {
	//		piece.position = startPos + Vector3.right * 6.25f;
	//		//SetPiecePosition();
	//		curPos = new Vector2(piece.position.z, piece.position.x);

	//		if (tag == "PetteiaW") {
	//			currentPiece = 2;
	//		}
	//		else {
	//			currentPiece = 1;
	//		}
	//		return startPos + Vector3.right * 6.25f;
	//	}

	//	else {
	//		//Debug.Log("SOMETHING HORRIBLE HAS HAPPENED \n Just kidding. Probably a typo(use right/left/up/down only");
	//		return startPos;

	//	}

	//}


	public Vector2 PosToArray(int y, int x) {
		return new Vector2(Mathf.Round(((y + 3.25f) / -6.25f) - 0), Mathf.Round(((x - 3) / 6.25f)) - 0);
		//converts the real world cordinates of the pieces to the value of the array that stores where the pieces are

	}

	void MoveBack(Transform g) {
		g.position = new Vector3(oldPos.y, 1, oldPos.x);
	}

    public PetteiaColliderMover[,] BoardSquares { get; } = new PetteiaColliderMover[8, 8];

	public bool GameOver 
	{
		get { return gameOver; }
	}
}
