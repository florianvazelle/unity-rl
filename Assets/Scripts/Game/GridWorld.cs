using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridWorld : MonoBehaviour
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
            return this.pos == other.pos;
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
            return (this.pos != null ? this.pos.GetHashCode() : 0);
        } 

        //
        // Element of the custom GridWorld's State
        //

        public Vector2 pos;

        public bool isFinal() {
            return pos == goal ;
        }

        // TODO : make a isAction state
    }

    // Game state
    public const int WIDTH = 6, HEIGHT = 6;

    public enum Actions {
        IDLE = 0,
        UP = 1,
        LEFT = 2,
        DOWN = 3,
        RIGHT = 4,
    };

    private static Vector2 player, goal;
    private List<Vector2> obstacles;
    private static Dictionary<Vector2, Cell> area;

    // Unity
    private GameObject goPlayer, goGoal;
    public Button yourButton;
    public Sprite tileSprite, arrowUp, arrowLeft, arrowDown, arrowRight;
    public Font arial;

    // Markov
    private List<IState> states;
    private List<int> actions;
    private MarkovValue markovIA;

    // Debug
    private static Utils.Logger logger;

    void Start()
    {   
        //
        // Game state
        player = new Vector2(0, 1);
        goal = new Vector2(4, 3);
        obstacles = new List<Vector2>() {
            new Vector2(1, 1),
            new Vector2(2, 2),
            new Vector2(3, 3),
            new Vector2(0, 3),
            // new Vector2(4, 4),
        };

        // Initialize area
        area = new Dictionary<Vector2, Cell>();
        for(int x = 0; x < WIDTH; x++)
        {
            for(int y = 0; y < HEIGHT; y++)
            {
                area.Add(new Vector2(x, y), new Cell { value = -1, prob = ((float)(x + y) / (WIDTH + HEIGHT)) });
            }
        }

        // Set cell specific value
        area[player] = new Cell { value = 0, prob = 0f };
        area[goal] = new Cell { value = 1000, prob = area[goal].prob };
        foreach (var obstacle in obstacles) area[obstacle] = new Cell { value = -1000, prob = area[obstacle].prob };

        // Draw default tile
        foreach (var cell in area)
        {
            SpawnTile((int)cell.Key.x, (int)cell.Key.y, (cell.Value.value == -1) ? new Color(255, 255, 255) : (cell.Value.value == -1000) ? new Color(255, 0, 0) : new Color(0, 255, 0));
        }
        
        //
        // Unity
        goPlayer = GameObject.Find("Player");
        goPlayer.transform.position = player;

        goGoal = GameObject.Find("Goal");
        goGoal.transform.position = goal;

        Camera.main.transform.position = new Vector3(WIDTH / 2, HEIGHT / 2, -10);

        //
        // Markov
        states = new List<IState>();
        for(int x = 0; x < WIDTH; x++)
        {
            for(int y = 0; y < HEIGHT; y++)
            {
                states.Add(new State { pos = new Vector2(x, y) });
            }
        }

        actions = new List<int>() { (int)Actions.UP, (int)Actions.LEFT, (int)Actions.DOWN, (int)Actions.RIGHT };
        markovIA = new MarkovValue(states, actions, Play);

        //
        // Debug
        logger = new Utils.Logger("GridWorld");

        Button btn = yourButton.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick() {
        IState currentState = new State { pos = player }; // TODO remove player and use a gameState like in tictactoe

        try {
            int act = markovIA.Think(currentState);
            Debug.Log(ActionToString(act));
            logger.WriteLine("Final Action " + act);

            // move player
            Play(currentState, act, out currentState);
            player = ((State)currentState).pos;

            // update rendering
            Render();

            // Debug
            float max = 0;
            foreach(var state in states)
            {
                max = Mathf.Max(max, markovIA.v_s[state]);
            }

            foreach(var state in states)
            {
                State s = (State)state;
                // SpawnArrow((int)s.pos.x, (int)s.pos.y, markovIA.policy[state]);

                if (area[s.pos].value != -1000)
                {
                    float value = ((float)(markovIA.v_s[state] * 255)) / max;
                    SpawnTile((int)s.pos.x, (int)s.pos.y, new Color(value, value, value));
                }
            }

        } catch (Exception e) {
            logger.WriteLine("Exception: " + e.Message);
        }
	}

    public static Cell Play(IState iState, int action, out IState newIState)
    {
        // execute action
        State state = (State)iState;

        switch(action)
        {
            case (int)Actions.IDLE:
                break;
            case (int)Actions.UP:
                state.pos += new Vector2(0, 1);
                break;
            case (int)Actions.LEFT:
                state.pos += new Vector2(-1, 0);
                break;
            case (int)Actions.DOWN:
                state.pos += new Vector2(0, -1);
                break;
            case (int)Actions.RIGHT:
                state.pos += new Vector2(1, 0);
                break;
            default:
                Debug.Log("Unkonwn action.");
                break; 
        }

        // clamp position in the area
        state.pos.x = (int)Mathf.Clamp(state.pos.x, 0, WIDTH - 1);
        state.pos.y = (int)Mathf.Clamp(state.pos.y, 0, HEIGHT - 1);

        // define the new state
        newIState = new State { pos = state.pos };

        // return the cell data
        return area[state.pos]; 
    }

    void Render()
    {   
        goPlayer.transform.position = new Vector3(player.x, player.y);
    }

    //
    // Helpers
    //

    void SpawnTile(int x, int y, Color color)
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

    void SpawnArrow(int x, int y, int action)
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
            g = new GameObject (name);
        }

        g.transform.position = new Vector3(x, y);
        g.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        var tile = g.GetComponent<SpriteRenderer>();
        if (tile == null) {
            tile = g.AddComponent<SpriteRenderer>();
        }

        tile.sprite = sprite;
    }

    void SpawnText(int x, int y, string text)
    {
        string name = x + ":" + y + "text";
        GameObject g = GameObject.Find(name);
        if (g == null) {
            g = new GameObject (name);
        }

        g.transform.SetParent( GameObject.Find( "Canvas" ).transform );
        g.transform.localPosition = new Vector3(x*75, y*75);

        var t = g.GetComponent<Text>();
        if (t == null) {
            t = g.AddComponent<Text>();
        }

        t.font = arial;
        t.text = text;
    }

    string ActionToString(int action)
    {
        switch(action)
        {
            case 0:
                return "x";
            case (int)Actions.UP:
                return "^";
            case (int)Actions.LEFT:
                return "<";
            case (int)Actions.DOWN:
                return "v";
            case (int)Actions.RIGHT:
                return ">";
            default:
                Debug.Log("Unkonwn action.");
                return " "; 
        }
    }
}
