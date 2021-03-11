using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sokoban : MonoBehaviour
{

    public struct State : IState, IEquatable<State>
    {   
        //
        // To compare State
        //

        public bool Equals(State other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            bool isEquals = true;
            foreach (var box in other.boxes)
            {
                isEquals = isEquals && this.boxes.Contains(box);
            }
            return this.pos == other.pos && isEquals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (State)) return false;
            return Equals((State)obj);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 37 + pos.GetHashCode();
            foreach (var box in this.boxes)
            {
                hash = hash * 37 + box.GetHashCode();
            }
            return hash;
        } 

        //
        // Element of the custom GridWorld's State
        //

        public Vector2 pos;
        public List<Vector2> boxes;

        // Property implementation:
        public bool IsFinal { get; set; }
        public bool HasActions { get; set; }
        public List<int> PossibleActions { get; set; }

        public State(Vector2 playerPos, List<Vector2> boxesPos)
        {
            this.pos = playerPos;
            this.boxes = new List<Vector2>(boxesPos);
            
            bool isFinal = true;
            foreach (var goal in goals)
            {
                isFinal = isFinal && this.boxes.Contains(goal);
            }

            IsFinal = isFinal;
            HasActions = !walls.Contains(playerPos);

            PossibleActions = new List<int>();

            foreach (var action in actions)
            {
                Vector2 move = new Vector2(0, 0);

                switch(action)
                {
                    case (int)Actions.UP:
                        move = new Vector2(0, 1);
                        break;
                    case (int)Actions.LEFT:
                        move = new Vector2(-1, 0);
                        break;
                    case (int)Actions.DOWN:
                        move = new Vector2(0, -1);
                        break;
                    case (int)Actions.RIGHT:
                        move = new Vector2(1, 0);
                        break;
                }
                
                Vector2 tmp = playerPos + move;
                
                bool isValidBoxMove = true;

                for (int i = 0; i < boxesPos.Count; i++)
                {
                    if (tmp == boxesPos[i])
                    {
                        Vector2 movedBox = boxesPos[i] + move;

                        // On ne bouge pas la boite si elle est sur une autre boite ou sur un mur
                        isValidBoxMove = (!(boxesPos.Contains(movedBox) || walls.Contains(movedBox)));
                    }
                }

                // C'est un coup valid si le joueur n'est pas allé sur un mur
                bool isValidMove = (!walls.Contains(tmp)) && isValidBoxMove;

                if (isValidMove && area.ContainsKey(tmp)) PossibleActions.Add(action);
            }
        }
    }

    // Game state

    public enum Actions {
        IDLE = 0,
        UP = 1,
        LEFT = 2,
        DOWN = 3,
        RIGHT = 4,
    };

    private Vector2 player;
    private List<Vector2> boxes;
    private static List<Vector2> walls, goals;

    // Hard test
    // public const int WIDTH = 9, HEIGHT = 8;
    // private static List<int> grid = new List<int>() {
    //     1, 1, 1, 1, 1, 1, 1, 1,
    //     1, 0, 0, 0, 3, 0, 0, 1,
    //     1, 2, 0, 2, 2, 2, 3, 1,
    //     1, 0, 1, 0, 3, 0, 1, 1,
    //     1, 3, 1, 1, 2, 0, 1, 1, 
    //     1, 1, 1, 0, 2, 3, 1, 1, 
    //     1, 3, 4, 2, 0, 0, 1, 1, 
    //     1, 1, 1, 0, 0, 0, 1, 1, 
    //     1, 1, 1, 1, 1, 1, 1, 1, 
    // };

    // public readonly static int WIDTH = 3, HEIGHT = 6;
    // private readonly static List<int> grid = new List<int>() {
    //     1, 1, 1, 1, 1, 1, 
    //     1, 4, 0, 2, 3, 1,
    //     1, 1, 1, 1, 1, 1, 
    // };
    //public readonly static int WIDTH = 4, HEIGHT = 6;
    //private readonly static List<int> grid = new List<int>() {
    //     1, 1, 1, 1, 1, 1,
    //     1, 4, 0, 0, 0, 1,
    //     1, 0, 0, 2, 3, 1,
    //     1, 1, 1, 1, 1, 1,
    // };


    //easy test with multiple box
    public readonly static int WIDTH = 6, HEIGHT = 6;
    private readonly static List<int> grid = new List<int>() {
        1, 1, 1, 1, 1, 1,
        1, 4, 0, 0, 0, 1,
        1, 0, 0, 2, 3, 1,
        1, 0, 0, 2, 3, 1,
        1, 0, 0, 2, 3, 1,
        1, 1, 1, 1, 1, 1,
    };
    // Medium test with logic
    //public readonly static int WIDTH = 9, HEIGHT = 6;
    //private readonly static List<int> grid = new List<int>() {
    //    1, 1, 1, 1, 1, 1, 
    //    1, 4, 0, 0, 0, 1,
    //    1, 0, 0, 0, 0, 1,
    //    1, 0, 0, 1, 3, 1,
    //    1, 1, 2, 1, 1, 1,
    //    1, 0, 0, 0, 0, 1,
    //    1, 0, 0, 0, 0, 1,
    //    1, 0, 0, 0, 0, 1,
    //    1, 1, 1, 1, 1, 1, 
    //};
    private static Dictionary<Vector2, Cell> area;

    // Unity
    private GameObject goPlayer;
    public Button yourButton;
    public Sprite tileSprite, arrowUp, arrowLeft, arrowDown, arrowRight;
    public Text initText;
    public Text thinkText;

    // Markov
    private List<IState> states;
    private readonly static List<int> actions = new List<int>() { (int)Actions.UP, (int)Actions.LEFT, (int)Actions.DOWN, (int)Actions.RIGHT };
    private MonteCarlo markovIA;

    // Debug
    private static Utils.Logger logger = new Utils.Logger("GridWorld");

    void Start()
    {   
        //
        // Game state
        player = new Vector2(0, 0);
        walls = new List<Vector2>();
        boxes = new List<Vector2>();
        goals = new List<Vector2>();

        // Initialize area
        area = new Dictionary<Vector2, Cell>();
        for(int y = 0; y < HEIGHT; y++)
        {
            for(int x = 0; x < WIDTH; x++)
            {
                Vector2 pos = new Vector2(x, y);

                int value = grid[x * HEIGHT + y];
                if (value == 1) walls.Add(pos);
                if (value == 2) boxes.Add(pos);
                if (value == 3) goals.Add(pos);
                if (value == 4) player = pos;

                area.Add(pos, new Cell { value = -1 });
                
                // Draw default tile
                SpawnTile(x, y, 
                    (value == 1) ? 
                        new Color(0, 0, 0) : (value == 2) ? 
                            new Color(0.59f, 0.29f, 0.00f) : (value == 3) ? 
                                new Color(1, 0, 0) : new Color(1, 1, 1)
                );
            }
        }
        
        //
        // Unity
        goPlayer = GameObject.Find("Player");
        goPlayer.transform.position = player;

        Camera.main.transform.position = new Vector3(WIDTH / 2, HEIGHT / 2, -10);

        //
        // Markov
        var initialState = new State(player, boxes);
        states = new List<IState>() {
            initialState
        };

        var startTime = DateTime.Now;
        markovIA = new MonteCarlo(states, actions, Play);
        initText.text = (DateTime.Now - startTime).Milliseconds.ToString("f6");

        //
        // Debug
        Button btn = yourButton.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
    }   

    void TaskOnClick() {
        IState currentIState = new State(player, boxes);

        // try {
            var startTime = DateTime.Now;
            int act = markovIA.Think(currentIState);
            thinkText.text = (DateTime.Now - startTime).Milliseconds.ToString("f6");
            Debug.Log(act);

            // move player
            Play(currentIState, act, out currentIState);
            player = ((State)currentIState).pos;
            boxes = ((State)currentIState).boxes;

            // update rendering
            Render();
        // } catch (Exception e) {
        //     logger.WriteLine("Exception: " + e.Message);
        // }
	}

    public static Cell Play(IState iState, int action, out IState newIState)
    {
        // execute action
        State state = (State)iState;

        Vector2 move = new Vector2(0, 0);

        switch(action)
        {
            case (int)Actions.IDLE:
                break;
            case (int)Actions.UP:
                move = new Vector2(0, 1);
                break;
            case (int)Actions.LEFT:
                move = new Vector2(-1, 0);
                break;
            case (int)Actions.DOWN:
                move = new Vector2(0, -1);
                break;
            case (int)Actions.RIGHT:
                move = new Vector2(1, 0);
                break;
            default:
                Debug.Log("Unkonwn action.");
                break; 
        }

        Vector2 tmp = state.pos + move;
        
        // Mais a jour les boites, si une a été bougé
        Vector2 movedBox = new Vector2(0, 0);
        List<Vector2> tmpBoxes = new List<Vector2>(state.boxes);
        for (int i = 0; i < tmpBoxes.Count; i++)
        {
            if (tmp == tmpBoxes[i])
            {
                movedBox = tmpBoxes[i] + move;
                tmpBoxes[i] = movedBox;
            }
        }

        // define the new state
        newIState = new State(tmp, tmpBoxes);

        if (((State)newIState).IsFinal)
        {
            return new Cell { value = 1000 };
        }

        //return the cell data
        if (goals.Contains(movedBox))
        {
            return new Cell { value = 10 };
        }

        return area[tmp]; 
    }

    void Render()
    {   
        goPlayer.transform.position = new Vector3(player.x, player.y);

        for(int y = 0; y < HEIGHT; y++)
        {
            for(int x = 0; x < WIDTH; x++)
            {
                Vector2 pos = new Vector2(x, y);

                Color color = new Color(1, 1, 1);
                if (walls.Contains(pos)) color = new Color(0, 0, 0);
                if (goals.Contains(pos)) color = new Color(1, 0, 0);
                if (boxes.Contains(pos)) color = new Color(0.59f, 0.29f, 0.00f);

                SpawnTile(x, y, color);
            }
        }
    }

    //
    // Helpers
    //

    private void SpawnTile(int x, int y, Color color)
    {
        string name = x + ":" + y;
        GameObject g = GameObject.Find(name);
        if (g == null) {
            g = new GameObject (name);
        }
        g.transform.position = new Vector3(x, y);

        var tile = g.GetComponent<SpriteRenderer>();
        if (tile == null) {
            tile = g.AddComponent<SpriteRenderer>();
        }

        tile.sprite = tileSprite;
        tile.color = color;
    }

    private void SpawnArrow(int x, int y, int action)
    {
        Sprite sprite = tileSprite;
        switch(action)
        {
            case (int)Actions.UP:
                sprite = arrowUp;
                break;
            case (int)Actions.LEFT:
                sprite = arrowLeft;
                break;
            case (int)Actions.DOWN:
                sprite = arrowDown;
                break;
            case (int)Actions.RIGHT:
                sprite = arrowRight;
                break;
        }

        string name = x + ":" + y + "arrow";
        GameObject g = GameObject.Find(name);
        if (g == null) {
            g = new GameObject(name);
        }

        g.transform.position = new Vector3(x, y);
        g.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        var tile = g.GetComponent<SpriteRenderer>();
        if (tile == null) {
            tile = g.AddComponent<SpriteRenderer>();
        }

        tile.sprite = sprite;
    }
}
