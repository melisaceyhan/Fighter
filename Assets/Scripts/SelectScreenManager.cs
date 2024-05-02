using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using static MySceneManager;

public class SelectScreenManager : MonoBehaviour
{
    public int numberOfPlayers = 1;
    public List <PlayerInterfaces> plInterfaces = new List<PlayerInterfaces> ();
    public PortraitInfo[] portraitPrefabs;
    public int maxRow;
    public int maxColumn;


    List<PortraitInfo> portraitList= new List<PortraitInfo> (); 

    PortraitInfo[,] charGrid;

    public GameObject portraitCanvas;

    bool loadLevel;
    public bool bothPlayerSelected;

    CharacterManager charManager;
    GameObject portraitPrefab;

    public static SelectScreenManager instance;
    public static SelectScreenManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        charManager= CharacterManager.GetInstance();
        numberOfPlayers = charManager.numberOfUsers;

        portraitPrefab = Resources.Load("portraitPrefab") as GameObject;
        CreatePortraits();

        charManager.solo = (numberOfPlayers == 1);

        charGrid=new PortraitInfo[maxRow,maxColumn];
        int x = 0;
        int y = 0;

        portraitPrefabs = portraitCanvas.GetComponentsInChildren<PortraitInfo>();

        for (int i = 0; i < portraitPrefabs.Length; i++)
        {
            portraitPrefabs[i].posX += x;
            portraitPrefabs[i].posY += y;

            charGrid[x,y]= portraitPrefabs[i];      

            if(x< maxRow - 1)
            {
                x++;    
            }
            else
            {
                x = 0;
                y++;
            }
            maxColumn = y;
        }
    }
    void CreatePortraits()
    {
        GridLayoutGroup group=portraitCanvas.GetComponent<GridLayoutGroup>();

        maxRow = group.constraintCount;
        int x = 0;
        int y=0;

        for (int i = 0; i < charManager.characterList.Count; i++)
        {
            CharacterBase c = charManager.characterList[i];

            GameObject go= Instantiate(portraitPrefab) as GameObject;
            go.transform.SetParent(portraitCanvas.transform);

            PortraitInfo p=go.GetComponent<PortraitInfo>();
            p.img.sprite = c.icon;
            p.characterId = c.charId;
            p.posX = x;
            p.posY = y;
            portraitList.Add(p);

            if(x<maxRow - 1)
            {
                x++;
            }

        }
    }

    void Update()
    {
        if (!loadLevel)
        {
            for (int i = 0; i < plInterfaces.Count; i++)
            { 
                if(i<numberOfPlayers)
                {

                    if (Input.GetButtonUp("Fire2" + charManager.players[i].inputId))
                    {

                        plInterfaces[i].playerBase.hasCharacter = false;
                    }

                    if (!charManager.players[i].hasCharacter)
                    {
                        plInterfaces[i].playerBase = charManager.players[i];

                        HandleSelectorPosition(plInterfaces[i]);    
                        HandleSelectScreenInput(plInterfaces[i], charManager.players[i].inputId);
                        HandleCharacterPreview(plInterfaces[i]);
                    }
                }
                else
                {
                    charManager.players[i].hasCharacter = true;    
                }
            }
        }
        if(bothPlayerSelected)
        {
            Debug.Log("loading");
            StartCoroutine("LoadLevel");
            loadLevel = true;  
        }
        else
        {
            if (charManager.players[0].hasCharacter
                && charManager.players[1].hasCharacter)
            {
                bothPlayerSelected = true;  
            }
        }
    }
    void HandleSelectScreenInput (PlayerInterfaces pl, string playerId)
    {
        float vertical = Input.GetAxis("Vertical" + playerId);

        if(vertical != 0)
        {
            if (!pl.hitInputOnce)
            {
                if (vertical > 0)
                {
                    pl.activeY = (pl.activeY > 0) ? pl.activeY - 1 : maxColumn - 1;
                }
                else
                {
                    pl.activeY= (pl.activeY<maxColumn-1) ? pl.activeY + 1 : 0;
                }

                pl.hitInputOnce = true;
            }
        }
        float horizontal = Input.GetAxis("Horizontal" + playerId);

        if(horizontal != 0)
        {
            if (!pl.hitInputOnce)
            {
                if(horizontal > 0)
                {
                    pl.activeX=(pl.activeX>0)? pl.activeX-1 : maxRow - 1;
                }
                else
                {
                    pl.activeX = (pl.activeX < maxRow - 1) ? pl.activeX + 1 : 0;
                }
                pl.timerToReset = 0;
                pl.hitInputOnce = true; 
            }
        }
        if(vertical==0 && horizontal == 0)
        {
            pl.hitInputOnce = false;
        }

        if (pl.hitInputOnce)
        {
            pl.timerToReset += Time.deltaTime;

            if(pl.timerToReset > 0.8f)
            {
                pl.hitInputOnce= false;
                pl.timerToReset = 0;
            }
        }

        if (Input.GetButtonUp("Fire1" + playerId))
        {
            pl.createdCharacter.GetComponentInChildren<Animator>().Play("Kick");

            pl.playerBase.playerPrefab =
                charManager.returnCharacterWithID(pl.activePortrait.characterId).prefab;

            pl.playerBase.hasCharacter = true;
        }

    }

    IEnumerator LoadLevel()
    {
        for (int i = 0; i<charManager.players.Count; i++)
        {
            if (charManager.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                if (charManager.players[i].playerPrefab == null)
                {
                    int ranValue= Random.Range (0, portraitPrefabs.Length);
                    charManager.players[i].playerPrefab= 
                        charManager.returnCharacterWithID(portraitPrefabs[ranValue].characterId).prefab;

                    Debug.Log(portraitPrefabs[ranValue].characterId);   

            
                }
            }
        }
        yield return new WaitForSeconds(2);

        if (charManager.solo)
        {
            MySceneManager.GetInstance().CreateProgression();
            MySceneManager.GetInstance().LoadNextOnProgression();

        }
        else
        {
            MySceneManager.GetInstance().RequestLevelLoad(SceneType.prog, "level_1");
        }
    }

    void HandleSelectorPosition(PlayerInterfaces pl)
    {
        pl.selector.SetActive(true);
        
        PortraitInfo pi=ReturnPortrait(pl.activeX, pl.activeY);
        if (pi != null)
        {
            pl.activePortrait = pi;
            Vector2 selectorPosition = pl.activePortrait.transform.localPosition;
            selectorPosition = selectorPosition + new Vector2(portraitCanvas.transform.localPosition.x,
                portraitCanvas.transform.localPosition.y);

            pl.selector.transform.localPosition = selectorPosition;


        }


    }
    PortraitInfo ReturnPortrait(int x, int y)
    {
        PortraitInfo r = null;
        for (int i = 0; i < portraitList.Count; i++)
        {
            if (portraitList[i].posX == x && portraitList[i].posY == y)
            {
                r= portraitList[i];
            }
        }
        return r;

    }
    void HandleCharacterPreview(PlayerInterfaces pl)
    {
        if (pl.previewPortrait != pl.activePortrait)
        {
            if(pl.createdCharacter != null)
            {
                Destroy(pl.createdCharacter);   
            }

            GameObject go =Instantiate(
                CharacterManager.GetInstance().returnCharacterWithID(pl.activePortrait.characterId).prefab,
                pl.charVisPos.position,
                Quaternion.identity) as GameObject;   
            pl.createdCharacter = go;
            pl.previewPortrait = pl.activePortrait;

            if(!string.Equals (pl.playerBase.playerId, charManager.players[0].playerId))
            {
                pl.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }
        }
    }


    [System.Serializable]
    public class PlayerInterfaces
    {
        public PortraitInfo activePortrait;
        public PortraitInfo previewPortrait;
        public GameObject selector;
        public Transform charVisPos;
        public GameObject createdCharacter;

        public int activeX;
        public int activeY;

        public bool hitInputOnce;
        public float timerToReset;

        public PlayerBase playerBase;
    }
}

