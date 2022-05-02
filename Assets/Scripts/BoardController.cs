using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardController : MonoBehaviour
{
    [SerializeField] private Tilemap board;
    [SerializeField] private TileBase[] scTiles, pathTiles;
    private List<Vector3Int> tilePath = new List<Vector3Int>();
    private Vector3Int pos, prevSet, currentTile = new Vector3Int(100, 100, 0);
    private Vector3 mousePos;
    private TileBase cTileObj, preview, st;
    private List<Change> changes = new List<Change>();
    private int cInd = 0;
    // private int st = 0;
    private bool firstTile = true;
    
    private readonly Dictionary<string, TileBase> tilesTypes = new Dictionary<string, TileBase>();

    void Start()
    {
        // StartCoroutine("GenerateBoard");
        st = pathTiles[0];

        foreach (var pathTile in pathTiles)
        {
            tilesTypes.Add(pathTile.name, pathTile);
        }

        foreach (var tile in scTiles)
        {
            tilesTypes.Add(tile.name, tile);
        }
    }

    void Update(){
        if (Camera.main != null) mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos = board.WorldToCell(mousePos) + new Vector3Int(0, 0, 10);
        
        if(pos != currentTile && st){
            board.SetEditorPreviewTile(currentTile, (preview) ? preview : null);
            preview = board.GetEditorPreviewTile(pos);
            if((cTileObj = board.GetTile(pos)) && st.name == "path0" && (firstTile || (preview && preview.name == "pclearPath"))){
                // tile de caminho
                board.SetEditorPreviewTile(pos, st);
            }
            else if(cTileObj && cTileObj.name == "path0" && st.name != "path0" && !st.name.Contains("cenario")){
                // tiles no caminho
                board.SetEditorPreviewTile(pos, st);
            }
            else if(cTileObj && st.name == "cenario0" && cTileObj.name.Contains("path")){
                // eraser
                board.SetEditorPreviewTile(pos, st);
            } 
            else if (cTileObj && !st.name.Contains("path") && !cTileObj.name.Contains("path"))
            {
                board.SetEditorPreviewTile(pos, st);
            }
            currentTile = pos;
        }
        
        if(Input.GetMouseButtonDown(0) && board.HasEditorPreviewTile(pos) && board.GetEditorPreviewTile(pos).name == st.name){
            if(cInd > 0){
                changes.RemoveRange(changes.Count - cInd, cInd);
                cInd = 0;
            }
            changes.Add(new Change(pos, board.GetTile(pos).name, st.name));
            board.SetTile(pos, st);
            board.SetEditorPreviewTile(pos, null);
            if(st.name == "path0"){
                tilePath.Add(pos);
                LockTiles(pos);
            }
            else if(st.name == "cenario0"){
                EraseTile(tilePath.IndexOf(pos));
            }
        }
    }

    public void UndoAction(){
        if(cInd < changes.Count){
            cInd++;
            Change undo = changes[changes.Count - cInd];
            board.SetTile(undo.cPos, tilesTypes[undo.prevTile]);
            if(undo.newTile == "path0"){
                tilePath.RemoveAt(tilePath.Count - 1);
            }
            if(st.name == "path0"){
                board.ClearAllEditorPreviewTiles();
                firstTile = true;
                if(tilePath.Count > 1){
                    for (int i = 0; i < tilePath.Count - 1; i++){
                        LockTiles(tilePath[i], 7);
                    }
                }
                if(tilePath.Count > 0)
                    LockTiles(tilePath[tilePath.Count - 1]);
            }
        }
    }

    public void RedoAction(){
        if(cInd > 0){
            cInd--;
            Change redo = changes[changes.Count - 1 - cInd];
            board.SetTile(redo.cPos, tilesTypes[redo.newTile]);
            if(redo.newTile == "path0"){
                tilePath.Add(redo.cPos);
            }
            if(st.name == "path0"){
                board.SetEditorPreviewTile(redo.cPos, null);
                LockTiles(redo.cPos);
            }
        }
    }

    private void LockTiles(Vector3Int center, int state = 8){
        if(!firstTile && state == 8){
            LockTiles(prevSet, 7);
        }
        for(int i = 0; i < 6; i++){
            Vector3 rot = Quaternion.Euler(0, 0, 60 * i) * new Vector3(0, .5f, 0);
            var target = board.WorldToCell(board.CellToLocal(center) + rot);
            TileBase targetTile = board.GetTile(target);
            preview = board.GetEditorPreviewTile(target);
            if(targetTile && targetTile.name != "path0" && !(preview && preview.name == "pblockPath")){
                board.SetEditorPreviewTile(target, pathTiles[state]);
            }
        }
        preview = null;
        prevSet = center;
        firstTile = false;
    }

    private void EraseTile(int ind){
        for(int i = tilePath.Count - 1; i > ind; i--){
            board.SetTile(tilePath[i], st);
            board.SetEditorPreviewTile(tilePath[i], null);
            tilePath.RemoveAt(i);
        }
        tilePath.RemoveAt(ind);
    }

    // Change the selected Tile to add
    public void PathSelection(string tileName){
        if (tilePath.Count == 0)
        {
            firstTile = true;
        }
        
        if (tileName == "path0" && tilePath.Count > 0 && !(st && st.name == "path0"))
        {
            if (tilePath.Count > 1)
            {
                for (int i = 0; i < tilePath.Count - 1; i++)
                {
                    LockTiles(tilePath[i], 7);
                }
            }

            LockTiles(tilePath[tilePath.Count - 1]);
        }

        st = tilesTypes[tileName];
    }

    public void ScenarioSelection(string tileName)
    {
        st = tilesTypes[tileName];
        board.ClearAllEditorPreviewTiles();
    }

    public void GenerateScenario() => StartCoroutine("GenerateBoard");

    public void ClearPreviewTiles() {
        st = null;
        board.ClearAllEditorPreviewTiles();
    }    

    public IEnumerator GenerateBoard(){
        yield return new WaitForSeconds(.3f);
        int xMod = 4, yMod = 3, y, j, count;
        for(int i = 0; i < 10; i++){
            xMod -= (i % 2);
            yMod += 1 - (i % 2);
            j = 1 - (i % 2);
            y = xMod - yMod;
            count = 0;
            for(int x = xMod; x >= (xMod - 5 + (i % 2)); x--){
                for(; j < 2; j++){
                    if(++count > 10)
                        break;
                    var nextTile = board.GetTile(new Vector3Int(x, y, 0));
                    if(nextTile && nextTile.name != "path0"){
                        yield return new WaitForSeconds(.005f);
                        board.SetTile(new Vector3Int(x, y, 0), scTiles[Random.Range(2, scTiles.Length-1)]);
                    }
                    y++;
                }
                j = 0;
            }
        }
    }
    
    public void OpenPopup(GameObject popup) {
        popup.SetActive(true);
    }
    
    public void ClosePopup(GameObject popup) {
        popup.SetActive(false);
    }
}

public struct Change
{
    public Vector3Int cPos;
    public string prevTile, newTile;

    public Change(Vector3Int pos, string pt, string nt)
    {
        cPos = pos;
        prevTile = pt;
        newTile = nt;
    }
}