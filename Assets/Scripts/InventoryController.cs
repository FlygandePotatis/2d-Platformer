using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class InventoryController : MonoBehaviour
{
    [HideInInspector]
    private ItemGrid selectedItemGrid;

    public ItemGrid SelectedItemGrid { 
        get => selectedItemGrid; 
        set
        {
            selectedItemGrid = value;
            inventoryHighlight.SetParent(value);
        } 
    }



    InventoryItem selectdItem;
    InventoryItem overlapItem;
    RectTransform rectTransform;



    [SerializeField] List<ItemData> items;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] Transform canvasTransform;



    InventoryHighlight inventoryHighlight;



    void Awake()
    {
       inventoryHighlight = GetComponent<InventoryHighlight>(); 
    }



    private void Update()
    {
        ItemIconDrag();



        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (selectdItem == null)
            {
                CreateRandomItem();
            }
        }



        if (Input.GetKeyDown(KeyCode.W))
        {
            InsertRandomItem();
        }



        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }



        if (selectedItemGrid == null)
        {
            inventoryHighlight.Show(false);
            return;
        }



        HandleHighlight();



        if (Input.GetMouseButtonDown(0))
        {
            LeftMouseButtonPress();
        }
    }



    private void RotateItem()
    {
        if(selectdItem == null)
        {
            return;
        }



        selectdItem.Rotate();
    }



    private void InsertRandomItem()
    {
        if (selectedItemGrid == null)
        {
            return;
        }



        CreateRandomItem();
        InventoryItem itemToInsert = selectdItem;
        selectdItem = null;
        InsertItem(itemToInsert);
    }



    private void InsertItem(InventoryItem itemToInsert)
    {
        Vector2Int? posOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);



        if(posOnGrid == null)
        {
            return;
        }



        selectedItemGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
    }



    Vector2Int oldPosition;
    InventoryItem itemToHighlight;



    private void HandleHighlight()
    {
        Vector2Int positionOnGrid = GetTileGridPosition();

        Debug.Log(positionOnGrid);
        //fixa att man inte åker "ur grid med positions vektor Int x,y"

        if (positionOnGrid == oldPosition)
        {
            return;
        }



        oldPosition = positionOnGrid;



        if (selectdItem == null)
        {
            itemToHighlight = selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);



            if(itemToHighlight != null)
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(itemToHighlight);
                inventoryHighlight.SetPosition(selectedItemGrid, itemToHighlight);
            }
            else
            {
                inventoryHighlight.Show(false);
            }
        }
        else
        {
            inventoryHighlight.Show(selectedItemGrid.BoundryCheck(positionOnGrid.x, positionOnGrid.y, selectdItem.WIDTH, selectdItem.HEIGHT));
            inventoryHighlight.SetSize(selectdItem);
            inventoryHighlight.SetPosition(selectedItemGrid, selectdItem, positionOnGrid.x, positionOnGrid.y);
        }
    }



    private void CreateRandomItem()
    {
        InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
        selectdItem = inventoryItem;

        rectTransform = inventoryItem.GetComponent<RectTransform>();
        rectTransform.SetParent(canvasTransform);
        rectTransform.SetAsLastSibling();//fixa ui - layer 



        int selectedItemID = UnityEngine.Random.Range(0, items.Count);
        inventoryItem.Set(items[selectedItemID]);
    }



    private void LeftMouseButtonPress()
    {
        Vector2Int tileGridPosition = GetTileGridPosition();

        if (selectdItem == null)
        {
            PickUpItem(tileGridPosition);
        }
        else
        {
            PlaceItem(tileGridPosition);
        }
    }



    private Vector2Int GetTileGridPosition()
    {
        Vector2 position = Input.mousePosition;



        if (selectdItem != null)
        {
            position.x -= (selectdItem.WIDTH - 1) * ItemGrid.tileSizeWidth / 2;
            position.y += (selectdItem.HEIGHT - 1) * ItemGrid.tileSizeHeight / 2;
        }



        return selectedItemGrid.GetTileGridPosition(position);
    }



    private void PlaceItem(Vector2Int tileGridPosition)
    {
        bool complete = selectedItemGrid.PlaceItem(selectdItem, tileGridPosition.x, tileGridPosition.y, ref overlapItem);
        if (complete)
        {
            selectdItem = null;
            if (overlapItem != null)
            {
                selectdItem = overlapItem;
                overlapItem = null;
                rectTransform = selectdItem.GetComponent<RectTransform>();
                rectTransform.SetAsLastSibling();//fixa ui - layer
            }
        }
    }

    private void PickUpItem(Vector2Int tileGridPosition)
    {
        selectdItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
        if (selectdItem != null)
        {
            rectTransform = selectdItem.GetComponent<RectTransform>();
        }
    }

    private void ItemIconDrag()
    {
        if (selectdItem != null)
        {
            rectTransform.position = Input.mousePosition;
        }
    }
}
