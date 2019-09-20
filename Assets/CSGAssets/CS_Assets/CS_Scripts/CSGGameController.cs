#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ColorSwitchGame.Types;

namespace ColorSwitchGame
{
	/// <summary>
	/// This script controls the game, starting it, following game progress, and finishing it with game over.
	/// </summary>
	public class CSGGameController:MonoBehaviour 
	{
        [Tooltip("The player object, assigned from the scene")]
		public Transform playerObject;
		
		[Tooltip("The button for jumping. This is defined from the Input Manager and corresponds to the Mouse, Gamepad, Keyboard, and Touch")]
		public string jumpButton = "Fire1";

		[Tooltip("A list of all the colors in the game. This affects the blocks, color balls, and the player.")]
		public Color[] colorList;
		
		[Tooltip("Randomize the list of the colors at the start of the game. This will make your game a bit more varied")]
		public bool randomizeColors = true;

		// The game will continue forever after the last level
		internal bool isEndless = true;

		[Tooltip("The first section in the game. This is assigned from the scene, and all sections are created after it")]
		public Transform firstSection;

		// The distance between the current section and the next section
		internal float nextSectionGap;

		[Tooltip("A list of all sections spawned in the game")]
		public Spawn[] sections;
		internal Spawn[] sectionList;

		[Tooltip("The score of the game. Score is earned by collecting coins")]
		public float score = 0;
		internal float scoreCount = 0;
		
		[Tooltip("The text object that displays the score, assigned from the scene")]
		public Transform scoreText;
		internal float highScore = 0;
		internal float scoreMultiplier = 1;

		[Tooltip("The effect displayed before starting the game")]
		public Transform readyGoEffect;
		
		[Tooltip("How long to wait before starting gameplay. In this time we usually display the readyGoEffect")]
		public float startDelay = 1;

		// Is the game over?
		internal bool  isGameOver = false;

		[Tooltip("The level of the main menu that can be loaded after the game ends")]
		public string mainMenuLevelName = "CS_StartMenu";
		
		[Tooltip("The keyboard/gamepad button that will restart the game after game over")]
		public string confirmButton = "Submit";
		
		[Tooltip("The keyboard/gamepad button that pauses the game")]
		public string pauseButton = "Cancel";
		internal bool  isPaused = false;

		[Tooltip("Various canvases for the UI, assign them from the scene")]
		public Transform gameCanvas;
		public Transform pauseCanvas;
		public Transform gameOverCanvas;

		[Tooltip("The sound that plays when you lose the game")]
		public AudioClip soundGameOver;
		public string soundSourceTag = "Sound";
		internal GameObject soundSource;

		// A general use index
		internal int index = 0;

		void Awake()
		{
			// Activate the pause canvas early on, so it can detect info about sound volume state
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(true);

			// Randomize the colors in the list. This affects the color order in blocks, color balls, and the player.
			if ( randomizeColors )    colorList = Shuffle(colorList);
		}

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			//Update the score and enemy count
			UpdateScore();

			//Hide the game over and pause screens
			if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);

			//Get the highscore for the player
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			highScore = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "HighScore", 0);
			#else
			highScore = PlayerPrefs.GetFloat(Application.loadedLevelName + "HighScore", 0);
			#endif

//CALCULATING SECTION CHANCES
			// Calculate the chances for the objects to spawn
			int totalSpawns = 0;
			int totalSpawnsIndex = 0;
			
			// Calculate the total number of sections with their chances
			for( index = 0; index < sections.Length; index++)
			{
				totalSpawns += sections[index].spawnChance;
			}
			
			// Create a new list of the objects that can be dropped
			sectionList = new Spawn[totalSpawns];
			
			// Go through the list again and fill out each type of drop based on its drop chance
			for( index = 0; index < sections.Length; index++)
			{
				int sectionsChanceCount = 0;
				
				while( sectionsChanceCount < sections[index].spawnChance )
				{
					sectionList[totalSpawnsIndex] = sections[index];
					
					sectionsChanceCount++;
					
					totalSpawnsIndex++;
				}
			}

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

			// Create the ready?GO! effect
			if ( readyGoEffect )    Instantiate( readyGoEffect );

		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void  Update()
		{
			// Make the score count up to its current value
			if ( scoreCount < score )
			{
				// Count up to the courrent value
				scoreCount = Mathf.Lerp( scoreCount, score, Time.deltaTime * 10);
				
				// Update the score text
				UpdateScore();
			}

			// Delay the start of the game
			if ( startDelay > 0 )
			{
				startDelay -= Time.deltaTime;
			}
			else
			{
				//If the game is over, listen for the Restart and MainMenu buttons
				if ( isGameOver == true )
				{
					//The jump button restarts the game
					if ( Input.GetButtonDown(confirmButton) )
					{
						Restart();
					}
					
					//The pause button goes to the main menu
					if ( Input.GetButtonDown(pauseButton) )
					{
						MainMenu();
					}
				}
				else
				{
					//Toggle pause/unpause in the game
					if ( Input.GetButtonDown(pauseButton) )
					{
						if ( isPaused == true )    Unpause();
						else    Pause(true);
					}

					// If a player exists and the game isn't paused, we can press the JumpButton to make the player jump
					if ( playerObject && isPaused == false && !EventSystem.current.IsPointerOverGameObject() && Input.GetButtonDown(jumpButton) )
					{
						playerObject.SendMessage("Jump");
					}

					// If the player moves close enough to the current section, create another section
					if ( sectionList.Length > 0 && playerObject.position.y > firstSection.position.y + nextSectionGap - 10 )    SpawnObject(sectionList);
				}
			}
		}

		/// <summary>
		/// Changes the distance to the next section position
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		void ChangeGap( float changeValue )
		{
			nextSectionGap = changeValue;
		}

		/// <summary>
		/// Creates a new enemy at the end of a random lane 
		/// </summary>
		public Transform SpawnObject( Spawn[] currentSpawnList )
		{
			// Create a new random target from the target list
			Transform newSpawn = Instantiate( currentSpawnList[Mathf.FloorToInt(Random.Range(0,currentSpawnList.Length))].spawnObject ) as Transform;
            float scale = 0.8f;
            newSpawn.localScale = new Vector3(scale, scale, scale);
			
			// Place the target at a random position along the throw height
			newSpawn.position = new Vector2( 0, firstSection.position.y + nextSectionGap);

			firstSection = newSpawn;

			return newSpawn;
		}

		/// <summary>
		/// Change the score
		/// </summary>
		/// <param name="changeValue">Change value</param>
		void  ChangeScore( float changeValue )
		{
			score += changeValue * scoreMultiplier;

			//Update the score
			UpdateScore();
		}
		
		/// <summary>
		/// Updates the score value and checks if we got to the next level
		/// </summary>
		void  UpdateScore()
		{
			//Update the score text
			if ( scoreText )    scoreText.GetComponent<Text>().text = Mathf.CeilToInt(scoreCount).ToString();
		}

		/// <summary>
		/// Sends a SetSpeedMultiplier command to the player, which makes it either faster or slower
		/// </summary>
		void SetSpeedMultiplier( float setValue )
		{
			if ( playerObject )    playerObject.SendMessage("SetSpeedMultiplier", setValue);
		}
		
		/// <summary>
		/// Set the score multiplier
		/// </summary>
		void SetScoreMultiplier( int setValue )
		{
			// Set the score multiplier
			scoreMultiplier = setValue;
		}

		/// <summary>
		/// Shuffles the specified Color list, and returns it
		/// </summary>
		/// <param name="colors">A list of colors</param>
		Color[] Shuffle( Color[] colors )
		{
			// Go through all the colors and shuffle them
			for ( index = 0 ; index < colors.Length ; index++ )
			{
				// Hold the text in a temporary variable
				Color tempNumber = colors[index];
				
				// Choose a random index from the text list
				int randomIndex = UnityEngine.Random.Range( index, colors.Length);
				
				// Assign a random text from the list
				colors[index] = colors[randomIndex];
				
				// Assign the temporary text to the random question we chose
				colors[randomIndex] = tempNumber;
			}
			
			return colors;
		}

		/// <summary>
		/// Pause the game, and shows the pause menu
		/// </summary>
		/// <param name="showMenu">If set to <c>true</c> show menu.</param>
		public void  Pause( bool showMenu )
		{
			isPaused = true;
			
			//Set timescale to 0, preventing anything from moving
			Time.timeScale = 0;
			
			//Show the pause screen and hide the game screen
			if ( showMenu == true )
			{
				if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(true);
				if ( gameCanvas )    gameCanvas.gameObject.SetActive(false);
			}
		}
		
		/// <summary>
		/// Resume the game
		/// </summary>
		public void  Unpause()
		{
			isPaused = false;
			
			//Set timescale back to the current game speed
			Time.timeScale = 1;
			
			//Hide the pause screen and show the game screen
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);
			if ( gameCanvas )    gameCanvas.gameObject.SetActive(true);
		}

		/// <summary>
		/// Runs the game over event and shows the game over screen
		/// </summary>
		IEnumerator GameOver(float delay)
		{
            AdManager.instance.RequestFullscreenAd();

			isGameOver = true;

			yield return new WaitForSeconds(delay);
			
			//Remove the pause and game screens
			if ( pauseCanvas )    Destroy(pauseCanvas.gameObject);
			if ( gameCanvas )    Destroy(gameCanvas.gameObject);
			
			//Show the game over screen
			if ( gameOverCanvas )    
			{
				//Show the game over screen
				gameOverCanvas.gameObject.SetActive(true);
				
				//Write the score text
				gameOverCanvas.Find("Base/Texts/TextScore").GetComponent<Text>().text = "SCORE " + score.ToString();
				
				//Check if we got a high score
				if ( score > highScore )    
				{
					highScore = score;
					
					//Register the new high score
					#if UNITY_5_3 || UNITY_5_3_OR_NEWER
					PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", score);
					#else
					PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", score);
					#endif
				}
				
				//Write the high score text
				gameOverCanvas.Find("Base/Texts/TextHighScore").GetComponent<Text>().text = "HIGH SCORE " + highScore.ToString();

				// Get the current amount of money we have
				float currentMoney = PlayerPrefs.GetFloat("Money", 0);

				// Add the score to the money
				currentMoney += score;

				// Save the money again
				PlayerPrefs.SetFloat("Money", currentMoney);

				//If there is a source and a sound, play it from the source
				if ( soundSource && soundGameOver )    
				{
					soundSource.GetComponent<AudioSource>().pitch = 1;
					
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);
				}
			}
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  Restart()
		{
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			#else
			Application.LoadLevel(Application.loadedLevelName);
			#endif
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  MainMenu()
		{
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(mainMenuLevelName);
			#else
			Application.LoadLevel(mainMenuLevelName);
			#endif
		}
	}
}