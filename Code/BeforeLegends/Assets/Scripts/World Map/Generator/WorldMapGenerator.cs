﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class WorldMapGenerator : MonoBehaviour
{

    private static WorldMapGenerator _instance;

    public static WorldMapGenerator instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<WorldMapGenerator>();
            return _instance;
        }
    }

    public Material mapMaterial;
    public Vec2int size;
    public Vec2int chunkSize;
    public int seed;
    public float erosionScale;
    public float continentScale;
    public float moistureScale;
    public float temperatureScale;
    public float erosionWeight;
    public float moistureErosionWeight;
    public float moistureWeight;
    public float continentWeight;
    public float temperatureNoiseWeight;
    public float temperatureLocationWeight;
    public Texture2D[] tileTextures;

    public float[] heightLookup;
    public float[] moistureLookup;
    public float[] temperatureLookup;

    public List<DropChance> DropChances = new List<DropChance>();

    public int[] water;
    public int[] ice;
    public int[] ice_mountain;
    public int[] tundra;
    public int[] savana;
    public int[] dry_forest;
    public int[] dry_mountain;
    public int[] grassland;
    public int[] forest;
    public int[] forest_mountain;
    public int[] desert;
    public int[] desert_mountain;
    public int[] jungle;

    public Texture2D chunkTexture;

    public Texture2D chunkTextureNormal;

    public Rect[] chunkTextureRects;

    public List<Vector2[]> chunkUVs;

    public Vector2 continentSeed;
    public Vector2 temperatureSeed;
    public Vector2 moistureSeed;
    public Vector2 erosionSeed;

    public Dictionary<GameObject, GameObject> enemys;
    
    public FlatHexagon flatHex;

    void Start()
    {
        enemys = new Dictionary<GameObject, GameObject>();
        flatHex = new FlatHexagon(1);
        setSeeds();
        packTextures();
        generate();
        createChunks();
        spawnObjects();
        spawnCarriers();
        spwanRessources();
        spawnPlayer();
    }

    void setSeeds()
    {
        Random.seed = seed;
        temperatureSeed = new Vector2(Random.Range(0f, 65536f), Random.Range(0f, 65536f));
        continentSeed = new Vector2(Random.Range(0f, 65536f), Random.Range(0f, 65536f));
        moistureSeed = new Vector2(Random.Range(0f, 65536f), Random.Range(0f, 65536f));
        erosionSeed = new Vector2(Random.Range(0f, 65536f), Random.Range(0f, 65536f));
    }

    void packTextures()
    {
	    chunkTexture = new Texture2D(2048, 2048);
	    chunkTexture.filterMode = FilterMode.Trilinear;
	    chunkTextureRects = chunkTexture.PackTextures(tileTextures, 50, 2048);
	    chunkUVs = new List<Vector2[]>();
	    foreach(Rect rect in chunkTextureRects){
		    Vector2[] uvs = new Vector2[flatHex.uv.Length];
		    for(int i = 0; i < uvs.Length; i++){
                Vector2 uv = new Vector2();
			    uv.x = rect.x + rect.width * flatHex.uv[i].x;
			    uv.y = rect.y + rect.height * flatHex.uv[i].y;
			    uvs[i] = uv;
		    }
		    chunkUVs.Add(uvs);
	    }
	    chunkTexture.Apply();
	    mapMaterial.mainTexture = chunkTexture;
    }

    void spawnObjects()
    {
	    WorldMapData data = WorldMapData.instance;
	    foreach(Hexagon tile in data.tiles){
		    if(tile.traversable && Random.Range(0f, 1f) <= 0.025){
                MapObjectData obj = new MapObjectData();
			    obj.appearanceID = Random.Range(0f, 1f) >= 0.5 ? 0 : 1;
			    tile.mapObjects.Add(obj);
		    }
	    }	
    }

    void spawnCarriers(){
	    WorldMapData data = WorldMapData.instance;
	    foreach(Hexagon tile in data.tiles){
		    if(tile.mapObjects.Count != 0){
			    GameObject go = (GameObject)Instantiate(CharacterModelPrefabs.prefabs[tile.mapObjects[0].appearanceID], tile.position, Quaternion.identity);
			    go.transform.parent = transform;
			    go.GetComponentInChildren<MapObjectCarrier>().pos = tile.gridPos;
                go.GetComponentInChildren<MapObjectCarrier>().data = tile.mapObjects[0];
			    tile.gameObjectList.Add(go);
                enemys.Add(go, go.GetComponentInChildren<MapObjectCarrier>().gameObject);
                go.GetComponentInChildren<MapObjectCarrier>().gameObject.SetActive(false);
		    }
	    }	
    }

    void spwanRessources()
    {
	    WorldMapData data = WorldMapData.instance;
	    foreach(Hexagon tile in data.tiles){
		    DropChance chance = returnDropChance(tile.matID);
		    chance.overallDropChance();
		    tile.tileType = chance.tileType;

		    float spawnChance = Random.Range(0.0f, 1.0f);
		    if(chance.chance > spawnChance && tile.traversable == true){
			    int spawn = chance.returnDrop();
			    float randomX = Random.Range(0, 2);
			    if(randomX > 0.5f)
				    randomX = 0.4f;
			    else if(randomX < 0.5f)
				    randomX = -0.4f;
			    else
				    randomX = 0;

			    float randomY = Random.Range(0, 2);
			    if(randomY > 0.5f)
				    randomY = 0.4f;
			    else if(randomY < 0.5f)
				    randomY = -0.4f;
			    else
				    randomY = 0;
			
			    GameObject go = Instantiate(CharacterModelPrefabs.ressourcePrefabs[spawn], new Vector3(tile.position.x + randomX, tile.position.y + 10, tile.position.z + randomY) , CharacterModelPrefabs.ressourcePrefabs[(int)spawnChance].transform.rotation) as GameObject;
			    FogOfWar.instance.SetLayerRecursively(go, 11);
			    go.transform.parent = transform;
			    go.GetComponent<Ressource>().pos = tile.gridPos;
			    tile.gameObjectList.Add(go);
		    }
	    }
    }

    void createChunks()
    {
        Mesh chunkMesh = getChunkMesh();
	    var xChunks = size.x / chunkSize.x;
	    var yChunks = size.y / chunkSize.y;

	    for(var y = 0; y < yChunks; y++){
		    for(var x = 0; x < xChunks; x++){
			    createChunk(x, y, chunkMesh);
		    }
	    }
    }

    void createChunk(int xChunk, int yChunk, Mesh mesh)
    {
	    GameObject chunk = new GameObject("Chunk(" + xChunk + "|" + yChunk + ")");
	    MeshRenderer render = chunk.AddComponent<MeshRenderer>();
	    MeshFilter filter = chunk.AddComponent<MeshFilter>();
	    chunk.transform.position = new Vector3(xChunk * chunkSize.x * flatHex.size.x, 0, yChunk * chunkSize.y * flatHex.size.z * 0.75f);
	    Vector2[] uvs = new Vector2[mesh.uv.Length];
	    for(int y = 0; y < chunkSize.y; y++){
		    for(int x = 0; x < chunkSize.x; x++){
			    Vector2[] texUV = chunkUVs[WorldMapData.instance.tiles[chunkSize.x * xChunk + x, chunkSize.y * yChunk + y].matID];
			    int offset = (x + y * chunkSize.y) * flatHex.uv.Length;
			    for(int i = 0; i < flatHex.uv.Length; i++){
				    uvs[offset + i] = texUV[i];
			    }
		    }
	    }
	    filter.mesh = mesh;
	    filter.mesh.uv = uvs;
	    render.sharedMaterial = mapMaterial;
	    chunk.transform.parent = transform;
    }

    Mesh getChunkMesh(){
	    Mesh mesh = new Mesh();
	    List<CombineInstance> meshes = new List<CombineInstance>();
	    for(int y = 0; y < chunkSize.y; y++){
		    bool odd = y % 2 == 1;
		    for(var x = 0; x < chunkSize.x; x++){
			    Matrix4x4 matrix = new Matrix4x4();
			    matrix.SetTRS(new Vector3(x * flatHex.size.x + (odd ? flatHex.extents.x : 0), 0, y * flatHex.size.z * 0.75f), Quaternion.identity, Vector3.one);
			    var combInst = new CombineInstance();
			    combInst.mesh = flatHex.hexMesh;
			    combInst.transform = matrix;
			    meshes.Add(combInst);
		    }
	    }
        CombineInstance[] meshArray = meshes.ToArray();
	    mesh.CombineMeshes(meshArray, true);
	    mesh.RecalculateNormals();
	    mesh.RecalculateBounds();
	    return mesh;
    }

    DropChance returnDropChance(int matID) 
    {
 	    foreach(int e in ice) {
 		    if(e == matID)
 			    return DropChances[1];
 	    }
 	    foreach(int e in ice_mountain) {
 		    if(e == matID)
 			    return DropChances[2];
 	    }
 	    foreach(int e in tundra) {
 		    if(e == matID)
 			    return DropChances[3];
 	    }
 	    foreach(int e in savana) {
 		    if(e == matID)
 			    return DropChances[4];
 	    }
 	    foreach(int e in dry_forest) {
 		    if(e == matID)
 			    return DropChances[5];
 	    }
 	    foreach(int e in dry_mountain) {
 		    if(e == matID)
 			    return DropChances[6];
 	    }
 	    foreach(int e in grassland) {
 		    if(e == matID)
 			    return DropChances[7];
 	    }
 	    foreach(int e in forest) {
 		    if(e == matID)
 			    return DropChances[8];
 	    }
 	    foreach(int e in forest_mountain) {
 		    if(e == matID)
 			    return DropChances[9];
 	    }
 	    foreach(int e in desert) {
 		    if(e == matID)
 			    return DropChances[10];
 	    }
 	    foreach(int e in desert_mountain) {
 		    if(e == matID)
 			    return DropChances[11];
 	    }
 	    foreach(int e in jungle) {
 		    if(e == matID)
 			    return DropChances[12];
 	    }
        return DropChances[0];
    }

    void generate(){
	    WorldMapData data = WorldMapData.instance;
	    data.flatHex = flatHex;
	    data.size = size;
	    data.tiles = new Hexagon[size.x, size.y];
	
	    for(int y = 0; y < size.y; y++){
		    bool odd = y % 2 == 1;
		    for(var x = 0; x < size.x; x++){
			    data.tiles[x, y] = new Hexagon();
			    data.tiles[x, y].position = new Vector3(x * flatHex.size.x + (odd ? flatHex.extents.x : 0), 0, y * flatHex.size.z * 0.75f);
			    data.tiles[x, y].gridPos = new Vec2int(x, y);
			    data.tiles[x, y].generate(this);
		    }
	    }
	
	    foreach(Hexagon hex in data.tiles){
		    hex.assignMaterials(this);
	    }
    }

    void spawnPlayer()
    {
        WorldMapData worldData = WorldMapData.instance;
        bool playerSpawned = false;
        while (!playerSpawned)
        {
            int randX = Random.Range(0, size.x);
            int randY = Random.Range(0, size.y);

            var contains = false;
            foreach (int e in grassland)
            {
                if (e == worldData.tiles[randX, randY].matID)
                    contains = true;
            }
            foreach (int e in forest)
            {
                if (e == worldData.tiles[randX, randY].matID)
                    contains = true;
            }
            if (contains)
            {
                FogOfWar.instance.CheckTiles(new Vec2int(randX, randY), FogOfWar.instance.visionRange);
                bool spawn = true;
                foreach (Hexagon e in FogOfWar.instance.adjacent)
                {
                    if (e.tileType == "ice" || e.tileType == "desert" || e.tileType == "water")
                        spawn = false;
                }
                FogOfWar.instance.ClearLists();
                if (spawn && worldData.tiles[randX, randY].gameObjectList.Count <= 0)
                {
                    GameObject player = GameObject.Find("Olaf");
                    MapObjectCarrier objData = player.AddComponent<MapObjectCarrier>();
                    player.GetComponent<MoveOnClick>().objData = objData;
                    objData.setPosition(new Vec2int(randX, randY));
                    player.transform.position = worldData.tiles[randX, randY].position;
                    InterfaceData.instance.selectedCarrier = objData;

                    GameObject.Find("World Camera").GetComponent<MouseMovement>().nextPos = new Vector3(player.transform.position.x, 10.0f, player.transform.position.z - 10.0f);
                    //CameraTransitions.Instance.LerpCamera(player.transform);

                    playerSpawned = true;
                }
            }
        }
    }
}