using System.Collections;
using UnityEngine;
using ColorSwitchGame.Types;
using ColorSwitchGame;

namespace ColorSwitchGame
{
	/// <summary>
	/// This script defines a block, which can be touched by the player. If the player is the same color as the block, it passes through. If not, it dies.
	/// </summary>
	[ExecuteInEditMode]
	public class CSGBlock:MonoBehaviour
	{
		[Tooltip("The index of the color of this object. It corresponds to the color list defined in the gamecontroller ( 0 is the first color, 1 is the seconds coloe, etc)")]
		public int colorIndex = 0;

		// Various variables we will access often during the game, so we cache them
		internal Transform thisTransform;
		static CSGGameController GameController;
		static CSGPlayer PlayerObject;

		[Tooltip("The tag of the object that this block can touch")]
		public string touchTargetTag = "Player";
	
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			thisTransform = transform;

			// Register the game controller for easier access
			if ( GameController == null )    GameController = (CSGGameController) FindObjectOfType(typeof(CSGGameController));
			if ( PlayerObject == null )    PlayerObject = (CSGPlayer) FindObjectOfType(typeof(CSGPlayer));

			// Assign the color of this block from the list of colors in the gamecontroller
			if ( colorIndex < GameController.colorList.Length )    GetComponent<SpriteRenderer>().color = GameController.colorList[colorIndex];
		}

        private void OnValidate()
        {
            // Assign the color of this block from the list of colors in the gamecontroller
            if (GameController && colorIndex < GameController.colorList.Length) GetComponent<SpriteRenderer>().color = GameController.colorList[colorIndex];
        }

        /// <summary>
        /// Is executed when this obstacle touches another object with a trigger collider
        /// </summary>
        /// <param name="other"><see cref="Collider"/></param>
        void OnTriggerEnter2D(Collider2D other)
		{	
			// Check if the object that was touched has the correct tag
			if( other.tag == touchTargetTag )
			{
				// If the color of the player does not match the color of the block, kill it
				if ( PlayerObject.colorIndex != colorIndex )    other.SendMessage("Die", transform);
			}
		}

		void OnDrawGizmos()
		{
			// Register the game controller for easier access
			if ( GameController == null )    GameController = (CSGGameController) FindObjectOfType(typeof(CSGGameController));
		}
	}
}