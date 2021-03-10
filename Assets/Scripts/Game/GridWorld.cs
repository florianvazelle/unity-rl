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

    // Markov
    private List<IState> states;
    private List<int> actions;
    private MarkovPolicy markovIA;

    // Debug
    private static Utils.Logger logger = new Utils.Logger("GridWorld");

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
            new Vector2(4, 4),
        };

        // Initialize area
        area = new Dictionary<Vector2, Cell>();
        for(int x = 0; x < WIDTH; x++)
        {
            for(int y = 0; y < HEIGHT; y++)
            {
                Cell c = new Cell { value = -1 };
                Vector2 pos = new Vector2(x, y);

                // Set cell specific value
                if (pos == player) c = new Cell { value = 0 }; 
                if (pos == goal) c = new Cell { value = 1000 }; 
                if (obstacles.Contains(pos)) c = new Cell { value = -1000 }; 

                area.Add(new Vector2(x, y), c);
            }
        }

        // Draw default tile
        foreach (var cell in area)
        {
            SpawnTile((int)cell.Key.x, (int)cell.Key.y, (cell.Value.value == -1) ? new Color(1, 1, 1) : (cell.Value.value == -1000) ? new Color(1, 0, 0) : new Color(0, 1, 0));
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
                Vector2 pos = new Vector2(x, y);

                if (!(obstacles.Contains(pos)))
                    states.Add(new State { pos = new Vector2(x, y) });
            }
        }

        actions = new List<int>() { (int)Actions.UP, (int)Actions.LEFT, (int)Actions.DOWN, (int)Actions.RIGHT };
        markovIA = new MarkovPolicy(states, actions, Play);

        // foreach(var state in states)
        // {
        //     State s = (State)state;

        //     if (s.pos == goal) continue;
        //     SpawnArrow((int)s.pos.x, (int)s.pos.y, markovIA.policy[s]);
        // }

        //
        // Debug
        Button btn = yourButton.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick() {
        IState currentState = new State { pos = player };

        try {
            int act = markovIA.Think(currentState);

            // move player
            Play(currentState, act, out currentState);
            player = ((State)currentState).pos;

            // update rendering
            Render();
            
            // Debug
            float max = 0, min = 1000;
            foreach(var state in states) 
            {
                min = Mathf.Min(min, markovIA.v_s[state]);
                max = Mathf.Max(max, markovIA.v_s[state]);
            }

            var range = max - min;

            foreach(var state in states)
            {
                State s = (State)state;

                if (s.pos == goal) continue;
                float value = ((float)((markovIA.v_s[s] - min))) / range;
                SpawnTile((int)s.pos.x, (int)s.pos.y, new Color(value, value, value));
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
