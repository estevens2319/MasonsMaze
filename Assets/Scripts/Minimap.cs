using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;

public class Minimap : MonoBehaviour
{
    public Canvas canvas;
    public Vector2 minimapOffset;
    public int minimapWidth;
    public int minimapHeight;
    public string pathToMinimapBackground;
    public string pathToMinimapFilter;
    public GameObject imgObject;
    private byte[] backgroundImage;
    private Texture2D wholeMap;
    public Color wallColor;
    public Color floorColor;
    private int tileScale = 16;
    private int wallThickness = 2;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void mapDraw(string[,][] grid)
    {
        mapDraw(-1, grid);
    }

    public void mapDraw(int floor, string[,][] grid)
    {
        imgObject = new GameObject("Minimap" + (floor < 0 ? "" : " " + floor));
        RectTransform trans = imgObject.AddComponent<RectTransform>();
        trans.transform.SetParent(canvas.transform); // setting parent
        trans.localScale = Vector3.one;
        trans.anchorMin = new Vector2(1.0f- (minimapWidth / Screen.width), 1.0f - (minimapHeight / Screen.height));
        trans.anchorMax = new Vector2(1.0f, 1.0f);
        trans.pivot = new Vector2(1.0f, 1.0f);
        trans.anchoredPosition = new Vector2(0, 0); // setting position, will be on center
        trans.sizeDelta = new Vector2(minimapWidth, minimapHeight); // custom size
        Image image = imgObject.AddComponent<Image>();
        byte[] imageData = File.ReadAllBytes(Application.dataPath + pathToMinimapBackground);
        backgroundImage = imageData;
        Texture2D tex = new Texture2D(2, 2);  // This width and height are overridden by LoadIMage()
        tex.LoadImage(imageData);
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        imgObject.transform.SetParent(canvas.transform);


        wholeMap = new Texture2D(grid.GetLength(0) * tileScale, grid.GetLength(1) * tileScale);
        for(int i = 0; i < grid.GetLength(0); i++)
        {
            for(int j = 0; j < grid.GetLength(1); j++)
            {
                Color tileColor = new Color(0, 0, 0, 0);
                if(grid[i, j][MazeGenerator.directionIndex(MazeGenerator.CENTER)].Equals(MazeGenerator.WALL)) { tileColor = wallColor; }
                else if(!grid[i, j][MazeGenerator.directionIndex(MazeGenerator.CENTER)].Equals(MazeGenerator.EMPTY)) { tileColor = floorColor; }
                
                // Center square and paths
                for (int k = 0; k < tileScale; k++)
                {
                    for(int l = 0; l < tileScale; l++)
                    {
                        wholeMap.SetPixel((tileScale * i) + k, (tileScale * j) + l, tileColor);
                    }
                }
                // Walled edges
                for(int k = 0; k < tileScale; k++)
                {
                    for (int l = 0; l < wallThickness; l++)
                    {
                        if (grid[i, j][MazeGenerator.directionIndex(MazeGenerator.UP)].Equals(MazeGenerator.WALL)) { wholeMap.SetPixel((tileScale * i) + k, (tileScale * j) + l + tileScale - wallThickness, wallColor); }
                        if (grid[i, j][MazeGenerator.directionIndex(MazeGenerator.DOWN)].Equals(MazeGenerator.WALL)) { wholeMap.SetPixel((tileScale * i) + k, (tileScale * j) + l, wallColor); }
                    }
                }
                for (int k = 0; k < wallThickness; k++)
                {
                    for (int l = 0; l < tileScale; l++)
                    {
                        if (grid[i, j][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL)) { wholeMap.SetPixel((tileScale * i) + k, (tileScale * j) + l, wallColor); }
                        if (grid[i, j][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL)) { wholeMap.SetPixel((tileScale * i) + k + tileScale - wallThickness, (tileScale * j) + l, wallColor); }
                    }
                }
            }
        }
        // Walled corners
        for(int i = 0; i < grid.GetLength(0); i++)
        {
            for(int j = 0; j < grid.GetLength(1); j++)
            {
                //Walled corners
                if (grid[i, j][MazeGenerator.directionIndex(MazeGenerator.UP)].Equals(MazeGenerator.WALL) &&
                    (i > 0) && (j < grid.GetLength(1) - 1) &&
                     grid[i - 1, j + 1][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL) &&
                    !grid[i - 1, j][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL) &&
                    !grid[i - 1, j][MazeGenerator.directionIndex(MazeGenerator.UP)].Equals(MazeGenerator.WALL))
                {
                    for (int k = -wallThickness; k < 0; k++)
                    {
                        for (int l = -wallThickness; l < 0; l++)
                        {
                            wholeMap.SetPixel((tileScale * i) + k, (tileScale * (j + 1)) + l, wallColor);
                        }
                    }
                }
                if (grid[i, j][MazeGenerator.directionIndex(MazeGenerator.UP)].Equals(MazeGenerator.WALL) &&
                    (i < grid.GetLength(0) - 1) && (j < grid.GetLength(1) - 1) &&
                     grid[i + 1, j + 1][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL) &&
                    !grid[i + 1, j][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL) &&
                    !grid[i + 1, j][MazeGenerator.directionIndex(MazeGenerator.UP)].Equals(MazeGenerator.WALL))
                {
                    for (int k = 0; k < wallThickness; k++)
                    {
                        for (int l = -wallThickness; l < 0; l++)
                        {
                            wholeMap.SetPixel((tileScale * (i + 1)) + k, (tileScale * (j + 1)) + l, wallColor);
                        }
                    }
                }
                if (grid[i, j][MazeGenerator.directionIndex(MazeGenerator.DOWN)].Equals(MazeGenerator.WALL) &&
                   (i > 0) && (j > 0) &&
                    grid[i - 1, j - 1][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL) &&
                   !grid[i - 1, j][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL) &&
                   !grid[i - 1, j][MazeGenerator.directionIndex(MazeGenerator.DOWN)].Equals(MazeGenerator.WALL))
                {
                    for (int k = -wallThickness; k < 0; k++)
                    {
                        for (int l = 0; l < wallThickness; l++)
                        {
                            wholeMap.SetPixel((tileScale * i) + k, (tileScale * j) + l, wallColor);
                        }
                    }
                }
                if (grid[i, j][MazeGenerator.directionIndex(MazeGenerator.DOWN)].Equals(MazeGenerator.WALL) &&
                   (i < grid.GetLength(0) - 1) && (j > 0) &&
                    grid[i + 1, j - 1][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL) &&
                   !grid[i + 1, j][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL) &&
                   !grid[i + 1, j][MazeGenerator.directionIndex(MazeGenerator.DOWN)].Equals(MazeGenerator.WALL))
                {
                    for (int k = 0; k < wallThickness; k++)
                    {
                        for (int l = 0; l < wallThickness; l++)
                        {
                            wholeMap.SetPixel((tileScale * (i + 1)) + k, (tileScale * j) + l, wallColor);
                        }
                    }
                }
            }
        }

        // Flip minimap for rendering purposes
        Texture2D newMap = new Texture2D(wholeMap.width, wholeMap.height);
        for(int i = 0; i < wholeMap.width; i++)
        {
            for(int j = 0; j < wholeMap.height; j++)
            {
                newMap.SetPixel(i, j, wholeMap.GetPixel(wholeMap.width - 1 - i, wholeMap.height - 1 - j));
            }
        }
        wholeMap = newMap;

        File.WriteAllBytes(Application.dataPath + "/Resources/" + imgObject.name + ".png", wholeMap.EncodeToPNG());
        image.sprite = Sprite.Create(wholeMap, new Rect(0.0f, 0.0f, wholeMap.width, wholeMap.height), new Vector2(0.5f, 0.5f));
        wholeMap.Apply();
        Debug.Log(wholeMap.width);
    }

    public void setVisible(bool visible)
    {
        imgObject.SetActive(visible);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
