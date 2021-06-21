using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardSetting
{
    public int xSize, ySize;
    public Tile tileGO;
    public List<Sprite> tileSprite;

}

public class GameManager : MonoBehaviour {
    [Header ("Настройки игровой доски")]
    public BoardSetting boardSetting;

	// Use this for initialization
	void Start () {
        BoardController.instance.SetValue(Board.instance.SetValue(boardSetting.xSize, boardSetting.ySize, boardSetting.tileGO, boardSetting.tileSprite),
            boardSetting.xSize, boardSetting.ySize,
            boardSetting.tileSprite);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
