using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {

    public static BoardController instance;

    private int xSize, ySize;
    private List<Sprite> tileSprite = new List<Sprite>();
    private Tile[,] tileArray;

    private Tile oldSelectTile;
    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    private bool isFindMatch = false;
    private bool isShift = false;
    private bool isSearchEmptyTile = false;

    public void SetValue(Tile[,] tileArray, int xSize, int ySize, List<Sprite> tileSprite)
    {

        this.tileArray = tileArray;
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileSprite = tileSprite;
    }

    private void Awake()
    {

        instance = this;
    }


	
	// Update is called once per frame
	void Update () {
        if (isSearchEmptyTile)
        {
            SearchEmptyTile();
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (ray != false)
            {
                CheckSelectTile(ray.collider.gameObject.GetComponent<Tile>());
            }
        }
	}
    #region(выделить тайл убрать проверить)
    private void SelectTile(Tile tile)
    {
        tile.isSelected = true;
        tile.spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        oldSelectTile = tile;
    }

    private void DeselectTile(Tile tile)
    {

        tile.isSelected = false;
        tile.spriteRenderer.color = new Color(1, 1, 1);
        oldSelectTile = null;
    }
    private void CheckSelectTile(Tile tile)
    {
        if (tile.isEmpty||isShift)
        {
            return;
        }
        if (tile.isSelected)
        {
            DeselectTile(tile);
        }
        else
        {
            //первое выделение
            if(!tile.isSelected&& oldSelectTile == null)
            {
                SelectTile(tile);
            }
            //попытка выбора второго тайла
            else
            {
                //если 2й выбранный тайл сосед предыдущего тайла
                if (AdjacentTiles().Contains(tile))
                {
                    SwapTwoTiles(tile);
                    FindAllMatch(tile);
                    DeselectTile(oldSelectTile);
                }
                //выделение нового, забытие старого
                else
                {
                    DeselectTile(oldSelectTile);
                    SelectTile(tile);
                }
               
            }
        }

    }
    #endregion
    #region(поиск совпадения, удаление спрайтов, движение тайлов, смена спрайтов у тайлов)
    private List<Tile> FindMatch(Tile tile, Vector2 dir)
    {
        List<Tile> cashFindTiles = new List<Tile>();
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir);
        while(hit.collider!=null 
            &&  hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            cashFindTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            hit = Physics2D.Raycast(hit.collider.gameObject.transform.position, dir);
        }
        return cashFindTiles;

    }

    private void DeleteSprite(Tile tile, Vector2[] dirArray)
    {
        List<Tile> cashFindSprite = new List<Tile>();
        for (int i = 0; i < dirArray.Length; i++)
        {
            cashFindSprite.AddRange(FindMatch(tile, dirArray[i]));
        }
        if(cashFindSprite.Count >= 2)
        {
            for(int i = 0; i < cashFindSprite.Count; i++)
            {
                cashFindSprite[i].spriteRenderer.sprite = null;
            }
            isFindMatch = true;
        }
    }

    private void FindAllMatch(Tile tile)
    {
        if (tile.isEmpty)
        {
            return;
        }
        DeleteSprite(tile, new Vector2[2] { Vector2.up, Vector2.down });
        DeleteSprite(tile, new Vector2[2] { Vector2.left, Vector2.right });
        if (isFindMatch)
        {
            isFindMatch = false;
            tile.spriteRenderer.sprite = null;
            isSearchEmptyTile = true;
        }

    }

    #endregion
    #region(смена тайлов, находка тайлов)
    private void SwapTwoTiles(Tile tile)
    {

        if(oldSelectTile.spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            return;
        }
        Sprite cashSprite = oldSelectTile.spriteRenderer.sprite;
        oldSelectTile.spriteRenderer.sprite = tile.spriteRenderer.sprite;
        tile.spriteRenderer.sprite = cashSprite;
        UI.instance.Moves(1);
    }
    private List<Tile> AdjacentTiles()
    {
        List<Tile> cashTiles = new List<Tile>();
        for(int i=0; i < dirRay.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(oldSelectTile.transform.position, dirRay[i]);
            if (hit.collider != null)
            {
             
                cashTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            }
            
        }
        return cashTiles;
    }
    #endregion
    #region(Поиск пустого тайла, новык тайлы)
    private void SearchEmptyTile()
    {
        for(int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tileArray[x, y].isEmpty)
                {
                    ShiftTileDown(x, y);
                    break;
                }
                if(x == xSize - 1 && y == ySize - 1)
                {
                    isSearchEmptyTile = false;
                }

            }
        }
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                FindAllMatch(tileArray[x, y]);

            }
        }
    }

    private void ShiftTileDown(int xPos, int yPos)
    {
        isShift = true;
        List<SpriteRenderer> cashRenderer = new List<SpriteRenderer>();
        int count = 0;
        for(int y = yPos; y < ySize; y++)
        {
            Tile tile = tileArray[xPos, y];
            if (tile.isEmpty)
            {
                count++;
            }
            cashRenderer.Add(tile.spriteRenderer);
        }
        for (int i = 0; i < count; i++)
        {
            UI.instance.Score(50);
            SetNewSprite(xPos, cashRenderer);
        }
        
        isShift = false;
    }

    private void SetNewSprite(int xPos, List<SpriteRenderer> renderer)
    {
        for(int y = 0; y < renderer.Count - 1; y++)
        {
            renderer[y].sprite = renderer[y + 1].sprite;
            renderer[y + 1].sprite = GetNewSprite(xPos, ySize - 1);
        }

    }
    private Sprite GetNewSprite(int xPos,int yPos)
    {
        List<Sprite> cashSprite = new List<Sprite>();
        cashSprite.AddRange(tileSprite);

        if (xPos > 0)
        {
            cashSprite.Remove(tileArray[xPos - 1, yPos].spriteRenderer.sprite);

        }
        if (xPos < xSize - 1)
        {
            cashSprite.Remove(tileArray[xPos + 1, yPos].spriteRenderer.sprite);
        }
        if (yPos > 0)
        {
            cashSprite.Remove(tileArray[xPos, yPos - 1].spriteRenderer.sprite);
        }
        return cashSprite[Random.Range(0, cashSprite.Count)];
    }
    #endregion
}
